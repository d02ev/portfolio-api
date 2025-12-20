using System.Security.Cryptography;
using System.Text;
using Application.Common;
using Application.Dto;
using Application.Helpers;
using Application.Integrations;
using Application.Repositories;
using Application.Responses;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RazorLight;

namespace Application.Services;

public class ResumeService(IResumeRepository resumeRepository, IExperienceRepository experienceRepository, IProjectRepository projectRepository, ITechStackRepository techStackRepository, IEducationRepository educationRepository, IContactRepository contactRepository, ISupabaseIntegration supabaseIntegration, IAiIntegration aiIntegration,IGithubIntegration githubIntegration, IMemoryCache cache, IMapper mapper) : IResumeService
{
  private readonly IResumeRepository _resumeRepository = resumeRepository;
  private readonly IExperienceRepository _experienceRepository = experienceRepository;
  private readonly IProjectRepository _projectRepository = projectRepository;
  private readonly ITechStackRepository _techStackRepository = techStackRepository;
  private readonly IEducationRepository _educationRepository = educationRepository;
  private readonly IContactRepository _contactRepository = contactRepository;
  private readonly ISupabaseIntegration _supabaseIntegration = supabaseIntegration;
  private readonly IAiIntegration _aiIntegration = aiIntegration;
  private readonly IGithubIntegration _githubIntegration = githubIntegration;
  private readonly IMemoryCache _cache = cache;
  private readonly IMapper _mapper = mapper;

  public async Task<CreateResourceResponse> CreateResume(ResumeDto resumeDto)
  {
    var _ = await _resumeRepository.FetchByNameAsync(resumeDto.Name);
    if (_ is not null)
    {
      throw new BadRequestException(ResourceNames.Resume, $"Resume with name {resumeDto.Name} already exists.");
    }

    var resume = _mapper.Map<Resume>(resumeDto);
    await _resumeRepository.CreateAsync(resume);

    var newResume = await _resumeRepository.FetchByNameAsync(resumeDto.Name) ?? throw new InternalServerException(ResourceNames.Resume, "An error occurred while creating the resume.");

    return new CreateResourceResponse(ResourceNames.Resume);
  }

  public async Task<FetchResourceResponse<FetchResumeDto>> FetchResume()
  {
    var resume = await _resumeRepository.FetchAsync() ?? throw new NotFoundException(ResourceNames.Resume);
    var experienceTask = _experienceRepository.FetchByIdsAsync(resume.ExperienceIds);
    var projectTask = _projectRepository.FetchByIdsAsync(resume.ProjectIds);
    var techStackTask = _techStackRepository.FetchByIdAsync(resume.TechStackId);
    var educationTask = _educationRepository.FetchByIdAsync(resume.EducationId);
    var contactTask = _contactRepository.FetchByIdAsync(resume.ContactId);

    await Task.WhenAll(experienceTask, projectTask, techStackTask, educationTask, contactTask);

    var experiences = await experienceTask;
    experiences = [.. experiences.OrderByDescending(e => e.StartDate)];

    var projects = await projectTask;
    var techStack = await techStackTask;
    var education = await educationTask;
    var contact = await contactTask;

    var fetchResumeDto = _mapper.Map<FetchResumeDto>(resume);
    fetchResumeDto.Projects = _mapper.Map<List<FetchProjectDto>>(projects);
    fetchResumeDto.Experience = _mapper.Map<List<FetchExperienceDto>>(experiences);
    fetchResumeDto.TechStack = _mapper.Map<FetchTechStackDto>(techStack);
    fetchResumeDto.Contact = _mapper.Map<FetchContactDto>(contact);
    fetchResumeDto.Education = _mapper.Map<FetchEducationDto>(education);

    return new FetchResourceResponse<FetchResumeDto>(ResourceNames.Resume, fetchResumeDto);
  }

  public async Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateResume(string resumeId, UpdateResumeDto updateResumeDto)
  {
    var _ = await _resumeRepository.FetchByIdAsync(resumeId) ?? throw new NotFoundException(ResourceNames.Resume, resumeId);
    var changes = UpdateObjectBuilderHelper.BuildUpdateObject(updateResumeDto);
    var serializedChanges = JsonConvert.SerializeObject(changes);

    await _resumeRepository.UpdateAsync(resumeId, serializedChanges);
    return new UpdateResourceResponse<IDictionary<string, object>>(ResourceNames.Resume, changes);
  }

  public async Task<CreateResourceResponse<IDictionary<string, long>>> GenerateResume(GenerateResumeDto generateResumeDto)
  {
    var resumeData = generateResumeDto.ResumeData;
    var templateId = generateResumeDto.TemplateId;
    var resumeName = generateResumeDto.ResumeName;

    resumeData = await _aiIntegration.OptimiseGenericAsync(resumeData);

    var template = await _supabaseIntegration.DownloadFileAsStringAsync(templateId);
    var engine = ConfigureRazorLightEngine();
    var resumeDataHash = CreateResumeDataHash(resumeData);
    var result = await _cache.GetOrCreateAsync($"resume:{resumeDataHash}", async entry =>
    {
      entry.SetSlidingExpiration(TimeSpan.FromDays(7));
      return await engine.CompileRenderStringAsync("ResumeTemplate", template, resumeData);
    });
    result = System.Net.WebUtility.HtmlDecode(result);
    var pushedFileName = Guid.NewGuid().ToString();

    await _githubIntegration.PushToRepositoryAsync($"docs/{pushedFileName}.tex", result!);

    var jobId = await _supabaseIntegration.InsertJobStatusAsync();

    await _githubIntegration.InitWorkflowAsync(jobId.ToString(), resumeName, pushedFileName);

    return new CreateResourceResponse<IDictionary<string, long>>("JOB_RUN", new Dictionary<string, long>
    {
      ["jobId"] = jobId,
    });
  }

  public async Task<ResumeJobRunResponse> FetchResumeJobRunStatus(long jobId)
  {
    var resumeJob = await _supabaseIntegration.FetchJobStatusAsync(jobId) ?? throw new NotFoundException("ResumeJobRun", jobId);
    return new ResumeJobRunResponse
    {
      Status = resumeJob.Status,
      PdfUrl = resumeJob.PdfUrl,
      Error = resumeJob.Error,
    };
  }

  public async Task<FetchResourceResponse<IDictionary<string, string>>> FetchLatestResumePdfUrl()
  {
    var result = await _supabaseIntegration.FetchLatestPdfUrlAsync();
    return new FetchResourceResponse<IDictionary<string, string>>("ResumePdfUrl", new Dictionary<string, string>
    {
      ["pdfUrl"] = result ?? string.Empty,
    });
  }

  private RazorLightEngine ConfigureRazorLightEngine()
  {
    return new RazorLightEngineBuilder()
      .UseEmbeddedResourcesProject(typeof(ResumeService))
      .SetOperatingAssembly(typeof(ResumeService).Assembly)
      .Build();
  }

  private string CreateResumeDataHash(FetchResumeDto resumeData)
  {
    var json = JsonConvert.SerializeObject(resumeData, new JsonSerializerSettings
    {
      ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
      TypeNameHandling = TypeNameHandling.None
    });
    var bytes = Encoding.UTF8.GetBytes(json);
    var hash = SHA256.HashData(bytes);
    return Convert.ToBase64String(hash);
  }
}