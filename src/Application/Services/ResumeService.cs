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

public class ResumeService(IResumeRepository resumeRepository, IExperienceRepository experienceRepository, IProjectRepository projectRepository, ITechStackRepository techStackRepository, IEducationRepository educationRepository, IContactRepository contactRepository, ISupabaseIntegration supabaseIntegration, ITelegramIntegration telegramIntegration,IAiIntegration aiIntegration, IGithubIntegration githubIntegration, IMemoryCache cache, IMapper mapper) : IResumeService
{
  private readonly IResumeRepository _resumeRepository = resumeRepository;
  private readonly IExperienceRepository _experienceRepository = experienceRepository;
  private readonly IProjectRepository _projectRepository = projectRepository;
  private readonly ITechStackRepository _techStackRepository = techStackRepository;
  private readonly IEducationRepository _educationRepository = educationRepository;
  private readonly IContactRepository _contactRepository = contactRepository;
  private readonly ISupabaseIntegration _supabaseIntegration = supabaseIntegration;
  private readonly ITelegramIntegration _telegramIntegration = telegramIntegration;
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
    var changes = UpdateObjectBuilderHelper.BuildUpdateObject<UpdateResumeDto>(updateResumeDto);
    var serializedChanges = JsonConvert.SerializeObject(changes);

    await _resumeRepository.UpdateAsync(resumeId, serializedChanges);
    return new UpdateResourceResponse<IDictionary<string, object>>(ResourceNames.Resume, changes);
  }

  public async Task<CreateResourceResponse<IDictionary<string, string>>> GenerateResume(GenerateResumeDto generateResumeDto)
  {
    var resumeData = generateResumeDto.ResumeData;
    var templateId = generateResumeDto.TemplateId;
    var resumeName = generateResumeDto.ResumeName;

    resumeData = await _aiIntegration.OptimiseGenericAsync(resumeData);

    var template = await _supabaseIntegration.DownloadFileAsStringAsync(templateId);
    var engine = ConfigureRazorLightEngine();
    var result = await engine.CompileRenderStringAsync("ResumeTemplate", template, resumeData);
    result = System.Net.WebUtility.HtmlDecode(result);
    var pushedFileName = $"ge-{Guid.NewGuid()}-{DateTime.UtcNow:yyyyMMdd}";

    await _githubIntegration.PushToRepositoryAsync($"docs/{pushedFileName}.tex", result!);

    var jobId = await _supabaseIntegration.InsertJobStatusAsync();

    await _githubIntegration.InitWorkflowAsync(jobId.ToString(), resumeName, pushedFileName);

    await _telegramIntegration.SendWorkflowStartedMessageAsync();

    var (error, pdfUrl) = await PollForJobStatus(jobId);
    if (error is not null)
    {
      await _telegramIntegration.SendFailureMessageAsync(error, ResumeModes.Generic);
      throw new InternalServerException(ResourceNames.JobRun, error);
    }

    await _telegramIntegration.SendSuccessMessageAsync(pdfUrl!, ResumeModes.Generic);

    return new CreateResourceResponse<IDictionary<string, string>>(ResourceNames.Resume, new Dictionary<string, string>
    {
      ["mode"] = ResumeModes.Generic,
      ["resumeName"] = resumeName,
      ["pdfUrl"] = pdfUrl!,
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

  public async Task<CreateResourceResponse<IDictionary<string, string>>> GenerateResumeForJob(GenerateResumeForJobDto generateResumeForJobDto)
  {
    var resumeData = generateResumeForJobDto.ResumeData;
    var templateId = generateResumeForJobDto.TemplateId;
    var resumeName = generateResumeForJobDto.ResumeName;
    var jobDescription = generateResumeForJobDto.JobDescription;
    var projects = await _projectRepository.FetchAllAsync();
    var fetchProjectDtos = _mapper.Map<List<FetchProjectDto>>(projects);

    resumeData = await _aiIntegration.OptimiseForJobAsync(resumeData, fetchProjectDtos, jobDescription);

    var template = await _supabaseIntegration.DownloadFileAsStringAsync(templateId);
    var engine = ConfigureRazorLightEngine();
    var result = await engine.CompileRenderStringAsync("ResumeTemplate", template, resumeData);
    result = System.Net.WebUtility.HtmlDecode(result);
    var pushedFileName = $"jd-{Guid.NewGuid()}-{DateTime.UtcNow:yyyyMMdd}";

    await _githubIntegration.PushToRepositoryAsync($"docs/jd_{pushedFileName}_{DateTime.UtcNow}.tex", result!);

    var jobId = await _supabaseIntegration.InsertJobStatusAsync();

    await _githubIntegration.InitWorkflowAsync(jobId.ToString(), resumeName, pushedFileName);
    await _telegramIntegration.SendWorkflowStartedMessageAsync();

    var (error, pdfUrl) = await PollForJobStatus(jobId);
    if (error is not null)
    {
      await _telegramIntegration.SendFailureMessageAsync(error, ResumeModes.JobDescription);
      throw new InternalServerException(ResourceNames.JobRun, error);
    }

    await _telegramIntegration.SendSuccessMessageAsync(pdfUrl!, ResumeModes.JobDescription);
    return new CreateResourceResponse<IDictionary<string, string>>(ResourceNames.Resume, new Dictionary<string, string>
    {
      ["mode"] = ResumeModes.JobDescription,
      ["resumeName"] = resumeName,
      ["pdfUrl"] = pdfUrl!
    });
  }

  private RazorLightEngine ConfigureRazorLightEngine()
  {
    return new RazorLightEngineBuilder()
      .UseEmbeddedResourcesProject(typeof(ResumeService))
      .SetOperatingAssembly(typeof(ResumeService).Assembly)
      .Build();
  }

  private async Task<(string? error, string? pdfUrl)> PollForJobStatus(long jobId)
  {
    var maxAttempts = 20;
    var delay = 30000;

    for (int attempt = 0; attempt < maxAttempts; ++attempt)
    {
      var resumeJob = await _supabaseIntegration.FetchJobStatusAsync(jobId);

      if (resumeJob is not null && resumeJob.Status != "pending" && resumeJob.Status != "processing")
      {
        if (resumeJob.Status == "success") return (null, resumeJob.PdfUrl);
        else return (resumeJob.Error, null);
      }
      if (attempt < maxAttempts)
      {
        await Task.Delay(delay);
      }
    }

    return ("Job timed out", null);
  }
}