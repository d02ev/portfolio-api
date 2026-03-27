using Application.Common;
using Application.Dto;
using Application.Helpers;
using Application.Integrations;
using Application.Repositories;
using Application.Responses;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Newtonsoft.Json;
using RazorLight;
using System.Text.RegularExpressions;

namespace Application.Services;

public class ResumeService(IResumeRepository resumeRepository, IExperienceRepository experienceRepository, IProjectRepository projectRepository, ITechStackRepository techStackRepository, IEducationRepository educationRepository, IContactRepository contactRepository, ISupabaseIntegration supabaseIntegration, ITelegramIntegration telegramIntegration, IGithubIntegration githubIntegration, IMapper mapper) : IResumeService
{
  private static readonly Regex LatexReservedCharacterRegex = new(@"(?<!\\)([#%])", RegexOptions.Compiled);
  private readonly IResumeRepository _resumeRepository = resumeRepository;
  private readonly IExperienceRepository _experienceRepository = experienceRepository;
  private readonly IProjectRepository _projectRepository = projectRepository;
  private readonly ITechStackRepository _techStackRepository = techStackRepository;
  private readonly IEducationRepository _educationRepository = educationRepository;
  private readonly IContactRepository _contactRepository = contactRepository;
  private readonly ISupabaseIntegration _supabaseIntegration = supabaseIntegration;
  private readonly ITelegramIntegration _telegramIntegration = telegramIntegration;
  // private readonly IAiIntegration _aiIntegration = aiIntegration;
  private readonly IGithubIntegration _githubIntegration = githubIntegration;
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
    var projectTask = _projectRepository.FetchByIdsAsync(resume.ProjectIds);
    var techStackTask = _techStackRepository.FetchByIdAsync(resume.TechStackId);
    var educationTask = _educationRepository.FetchByIdAsync(resume.EducationId);
    var contactTask = _contactRepository.FetchByIdAsync(resume.ContactId);

    await Task.WhenAll(projectTask, techStackTask, educationTask, contactTask);

    var experiences = await _experienceRepository.FetchByIdsAsync(resume.ExperienceIds);
    experiences = [.. experiences.OrderByDescending(e => e?.StartDate)];

    var projects = projectTask.Result;
    var techStack = techStackTask.Result;
    var education = educationTask.Result;
    var contact = contactTask.Result;

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

  public async Task<CreateResourceResponse<ResumeGenerationResponse>> GenerateResume(GenerateResumeDto generateResumeDto)
  {
    var resumeData = generateResumeDto.ResumeData;
    var templateId = generateResumeDto.TemplateId;
    var resumeName = generateResumeDto.ResumeName;
    var companyName = generateResumeDto.CompanyName;

    // resumeData = await _aiIntegration.OptimiseGenericAsync(resumeData);
    EscapeResumeDataForLatex(resumeData);

    var template = await _supabaseIntegration.DownloadFileAsStringAsync(templateId);
    var engine = ConfigureRazorLightEngine();
    var result = await engine.CompileRenderStringAsync("ResumeTemplate", template, resumeData);
    result = System.Net.WebUtility.HtmlDecode(result);
    var pushedFileName = companyName is null ? $"ge-{Guid.NewGuid()}-{DateTime.UtcNow:yyyyMMdd}" : $"jd-{Guid.NewGuid()}-{DateTime.UtcNow:yyyyMMdd}";

    await _githubIntegration.PushToRepositoryAsync($"docs/{pushedFileName}.tex", result!);

    var jobId = await _supabaseIntegration.InsertJobStatusAsync(pushedFileName);

    await _githubIntegration.InitWorkflowAsync(jobId.ToString(), resumeName, pushedFileName);
    await _telegramIntegration.SendWorkflowStartedMessageAsync();

    return new CreateResourceResponse<ResumeGenerationResponse>(ResourceNames.Resume, new ResumeGenerationResponse
    {
      JobId = jobId,
      ResumeName = resumeName,
      LatexFileName = pushedFileName + ".tex"
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

  private static RazorLightEngine ConfigureRazorLightEngine()
  {
    return new RazorLightEngineBuilder()
      .UseEmbeddedResourcesProject(typeof(ResumeService))
      .SetOperatingAssembly(typeof(ResumeService).Assembly)
      .Build();
  }

  private static void EscapeResumeDataForLatex(FetchResumeDto resumeData)
  {
    resumeData.Name = EscapeLatexReservedChars(resumeData.Name);

    resumeData.Contact.Email = EscapeLatexReservedChars(resumeData.Contact.Email);
    resumeData.Contact.Mobile = EscapeLatexReservedChars(resumeData.Contact.Mobile);
    resumeData.Contact.Github = EscapeLatexReservedChars(resumeData.Contact.Github);
    resumeData.Contact.Linkedin = EscapeLatexReservedChars(resumeData.Contact.Linkedin);
    resumeData.Contact.Website = EscapeLatexReservedChars(resumeData.Contact.Website);

    resumeData.Education.Institute = EscapeLatexReservedChars(resumeData.Education.Institute);
    resumeData.Education.StartDate = EscapeLatexReservedChars(resumeData.Education.StartDate);
    resumeData.Education.EndDate = EscapeLatexReservedChars(resumeData.Education.EndDate);
    resumeData.Education.Degree = EscapeLatexReservedChars(resumeData.Education.Degree);
    resumeData.Education.Grade = EscapeLatexReservedChars(resumeData.Education.Grade);
    EscapeStringListForLatex(resumeData.Education.Coursework);

    EscapeStringListForLatex(resumeData.TechStack.Languages);
    EscapeStringListForLatex(resumeData.TechStack.FrameworksAndPlatforms);
    EscapeStringListForLatex(resumeData.TechStack.Databases);
    EscapeStringListForLatex(resumeData.TechStack.CloudAndDevOps);
    EscapeStringListForLatex(resumeData.TechStack.Others);

    foreach (var experience in resumeData.Experience)
    {
      experience.JobTitle = EscapeLatexReservedChars(experience.JobTitle);
      experience.CompanyName = EscapeLatexReservedChars(experience.CompanyName);
      experience.Location = EscapeLatexReservedChars(experience.Location);
      experience.StartDate = EscapeLatexReservedChars(experience.StartDate);
      if (experience.EndDate is not null)
      {
        experience.EndDate = EscapeLatexReservedChars(experience.EndDate);
      }
      EscapeStringListForLatex(experience.Description);
    }

    foreach (var project in resumeData.Projects)
    {
      project.DisplayName = EscapeLatexReservedChars(project.DisplayName);
      project.ShortDescription = EscapeLatexReservedChars(project.ShortDescription);
      project.LongDescription = EscapeLatexReservedChars(project.LongDescription);
      project.RepoUrl = EscapeLatexReservedChars(project.RepoUrl);
      project.LiveUrl = EscapeLatexReservedChars(project.LiveUrl);
      EscapeStringListForLatex(project.TechStack);
    }
  }

  private static void EscapeStringListForLatex(List<string> values)
  {
    for (int i = 0; i < values.Count; i++)
    {
      values[i] = EscapeLatexReservedChars(values[i]);
    }
  }

  private static string EscapeLatexReservedChars(string value)
  {
    return LatexReservedCharacterRegex.Replace(value, match => $@"\{match.Groups[1].Value}");
  }

  // public async Task<CreateResourceResponse<ResumeGenerationResponse>> GenerateResumeForJob(GenerateResumeForJobDto generateResumeForJobDto)
  // {
  //   var resumeData = ParseResumeDataJsonString(generateResumeForJobDto.ResumeData);
  //   var templateId = generateResumeForJobDto.TemplateId;
  //   var resumeName = generateResumeForJobDto.ResumeName;
  //   var jobDescription = ReadJobDescription(generateResumeForJobDto.JobDescription);
  //   var companyName = generateResumeForJobDto.CompanyName;
  //   var projects = await _projectRepository.FetchAllAsync();
  //   var fetchProjectDtos = _mapper.Map<List<FetchProjectDto>>(projects);

  //   resumeData = await _aiIntegration.OptimiseForJobAsync(resumeData, fetchProjectDtos, jobDescription);

  //   var template = await _supabaseIntegration.DownloadFileAsStringAsync(templateId);
  //   var engine = ConfigureRazorLightEngine();
  //   var result = await engine.CompileRenderStringAsync("ResumeTemplate", template, resumeData);
  //   result = System.Net.WebUtility.HtmlDecode(result);
  //   var pushedFileName = $"jd-{Guid.NewGuid()}-{DateTime.UtcNow:yyyyMMdd}";

  //   await _githubIntegration.PushToRepositoryAsync($"docs/{pushedFileName}.tex", result!);

  //   var jobId = await _supabaseIntegration.InsertJobStatusAsync(pushedFileName, companyName);

  //   await _githubIntegration.InitWorkflowAsync(jobId.ToString(), resumeName, pushedFileName);
  //   await _telegramIntegration.SendWorkflowStartedMessageAsync();

  //   return new CreateResourceResponse<ResumeGenerationResponse>(ResourceNames.Resume, new ResumeGenerationResponse
  //   {
  //     JobId = jobId,
  //     ResumeName = resumeName,
  //     LatexFileName = pushedFileName + ".tex"
  //   });
  // }

  // private string ReadJobDescription(IFormFile jobDescriptionFile)
  // {
  //   using var reader = new StreamReader(jobDescriptionFile.OpenReadStream());
  //   return reader.ReadToEnd();
  // }

  // private FetchResumeDto ParseResumeDataJsonString(string resumeDataJsonString)
  // {
  //   return JsonConvert.DeserializeObject<FetchResumeDto>(resumeDataJsonString) ?? new FetchResumeDto();
  // }

  // private async Task<(string? error, string? pdfUrl)> PollForJobStatus(long jobId)
  // {
  //   var maxAttempts = 20;
  //   var delay = 30000;

  //   for (int attempt = 0; attempt < maxAttempts; ++attempt)
  //   {
  //     var resumeJob = await _supabaseIntegration.FetchJobStatusAsync(jobId);

  //     if (resumeJob is not null && resumeJob.Status != "pending" && resumeJob.Status != "processing")
  //     {
  //       if (resumeJob.Status == "success") return (null, resumeJob.PdfUrl);
  //       else return (resumeJob.Error, null);
  //     }
  //     if (attempt < maxAttempts)
  //     {
  //       await Task.Delay(delay);
  //     }
  //   }

  //   return ("Job timed out", null);
  // }
}
