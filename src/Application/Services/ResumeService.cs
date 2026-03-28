using Application.Common;
using Application.Dto;
using Application.Helpers;
using Application.Integrations;
using Application.Repositories;
using Application.Responses;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RazorLight;
using System.Text.RegularExpressions;

namespace Application.Services;

public class ResumeService(IResumeRepository resumeRepository, IExperienceRepository experienceRepository, IProjectRepository projectRepository, ITechStackRepository techStackRepository, IEducationRepository educationRepository, IContactRepository contactRepository, ISupabaseIntegration supabaseIntegration, ITelegramIntegration telegramIntegration, IGithubIntegration githubIntegration, IMapper mapper, ILogger<ResumeService> logger) : IResumeService
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
  private readonly ILogger<ResumeService> _logger = logger;

  public async Task<CreateResourceResponse> CreateResume(ResumeDto resumeDto)
  {
    _logger.LogInformation("Started {Operation}. ResumeName={ResumeName}.", nameof(CreateResume), resumeDto.Name);
    try
    {
      var existingResume = await _resumeRepository.FetchByNameAsync(resumeDto.Name);
      if (existingResume is not null)
      {
        _logger.LogWarning("Duplicate resume detected while creating resume.");
        throw new BadRequestException(ResourceNames.Resume, $"Resume with name {resumeDto.Name} already exists.");
      }

      var resume = _mapper.Map<Resume>(resumeDto);
      await _resumeRepository.CreateAsync(resume);

      var newResume = await _resumeRepository.FetchByNameAsync(resumeDto.Name);
      if (newResume is null)
      {
        throw new InternalServerException(ResourceNames.Resume, "An error occurred while creating the resume.");
      }

      _logger.LogInformation("Completed {Operation}. ResourceName={ResourceName}.", nameof(CreateResume), ResourceNames.Resume);
      return new CreateResourceResponse(ResourceNames.Resume);
    }
    catch (BadRequestException)
    {
      throw;
    }
    catch (InternalServerException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}. ResumeName={ResumeName}.", nameof(CreateResume), resumeDto.Name);
      throw;
    }
  }

  public async Task<FetchResourceResponse<FetchResumeDto>> FetchResume()
  {
    _logger.LogInformation("Started {Operation}.", nameof(FetchResume));
    try
    {
      var resume = await _resumeRepository.FetchAsync();
      if (resume is null)
      {
        _logger.LogWarning("Resume not found.");
        throw new NotFoundException(ResourceNames.Resume);
      }

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

      _logger.LogInformation("Completed {Operation}.", nameof(FetchResume));
      return new FetchResourceResponse<FetchResumeDto>(ResourceNames.Resume, fetchResumeDto);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}.", nameof(FetchResume));
      throw;
    }
  }

  public async Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateResume(string resumeId, UpdateResumeDto updateResumeDto)
  {
    _logger.LogInformation("Started {Operation} for ResumeId={ResumeId}.", nameof(UpdateResume), resumeId);
    try
    {
      var existingResume = await _resumeRepository.FetchByIdAsync(resumeId);
      if (existingResume is null)
      {
        _logger.LogWarning("Resume not found for update. ResumeId={ResumeId}.", resumeId);
        throw new NotFoundException(ResourceNames.Resume, resumeId);
      }

      var changes = UpdateObjectBuilderHelper.BuildUpdateObject<UpdateResumeDto>(updateResumeDto);
      var serializedChanges = JsonConvert.SerializeObject(changes);

      await _resumeRepository.UpdateAsync(resumeId, serializedChanges);
      _logger.LogInformation("Completed {Operation} for ResumeId={ResumeId}.", nameof(UpdateResume), resumeId);
      return new UpdateResourceResponse<IDictionary<string, object>>(ResourceNames.Resume, changes);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation} for ResumeId={ResumeId}.", nameof(UpdateResume), resumeId);
      throw;
    }
  }

  public async Task<CreateResourceResponse<ResumeGenerationResponse>> GenerateResume(GenerateResumeDto generateResumeDto)
  {
    var templateId = generateResumeDto.TemplateId;
    var resumeName = generateResumeDto.ResumeName;
    var companyName = generateResumeDto.CompanyName;

    _logger.LogInformation(
      "Started {Operation}. TemplateId={TemplateId}, ResumeName={ResumeName}, HasCompanyName={HasCompanyName}.",
      nameof(GenerateResume),
      templateId,
      resumeName,
      !string.IsNullOrWhiteSpace(companyName));

    try
    {
      var resumeData = generateResumeDto.ResumeData;

      // resumeData = await _aiIntegration.OptimiseGenericAsync(resumeData);
      EscapeResumeDataForLatex(resumeData);
      _logger.LogInformation("Escaped resume data for LaTeX in {Operation}.", nameof(GenerateResume));

      _logger.LogInformation("Downloading resume template. TemplateId={TemplateId}.", templateId);
      var template = await _supabaseIntegration.DownloadFileAsStringAsync(templateId);

      _logger.LogInformation("Rendering resume template for ResumeName={ResumeName}.", resumeName);
      var engine = ConfigureRazorLightEngine();
      var result = await engine.CompileRenderStringAsync("ResumeTemplate", template, resumeData);
      result = System.Net.WebUtility.HtmlDecode(result);
      var pushedFileName = companyName is null ? $"ge-{Guid.NewGuid()}-{DateTime.UtcNow:yyyyMMdd}" : $"jd-{Guid.NewGuid()}-{DateTime.UtcNow:yyyyMMdd}";
      var latexFilePath = $"docs/{pushedFileName}.tex";

      _logger.LogInformation("Pushing rendered LaTeX to repository. FilePath={FilePath}.", latexFilePath);
      await _githubIntegration.PushToRepositoryAsync(latexFilePath, result!);

      _logger.LogInformation("Inserting resume job status for LatexFileName={LatexFileName}.", pushedFileName);
      var jobId = await _supabaseIntegration.InsertJobStatusAsync(pushedFileName, companyName);
      _logger.LogInformation("Inserted resume job status. JobId={JobId}.", jobId);

      _logger.LogInformation("Initializing workflow dispatch for JobId={JobId}.", jobId);
      await _githubIntegration.InitWorkflowAsync(jobId.ToString(), resumeName, pushedFileName);

      _logger.LogInformation("Sending workflow started notification for JobId={JobId}.", jobId);
      await _telegramIntegration.SendWorkflowStartedMessageAsync();

      _logger.LogInformation("Completed {Operation}. JobId={JobId}, LatexFileName={LatexFileName}.", nameof(GenerateResume), jobId, pushedFileName + ".tex");
      return new CreateResourceResponse<ResumeGenerationResponse>(ResourceNames.Resume, new ResumeGenerationResponse
      {
        JobId = jobId,
        ResumeName = resumeName,
        LatexFileName = pushedFileName + ".tex"
      });
    }
    catch (BadRequestException)
    {
      throw;
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (InternalServerException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(
        ex,
        "Unexpected error in {Operation}. TemplateId={TemplateId}, ResumeName={ResumeName}.",
        nameof(GenerateResume),
        templateId,
        resumeName);
      throw;
    }
  }

  public async Task<ResumeJobRunResponse> FetchResumeJobRunStatus(long jobId)
  {
    _logger.LogInformation("Started {Operation} for JobId={JobId}.", nameof(FetchResumeJobRunStatus), jobId);
    try
    {
      var resumeJob = await _supabaseIntegration.FetchJobStatusAsync(jobId);
      if (resumeJob is null)
      {
        _logger.LogWarning("Resume job run not found for JobId={JobId}.", jobId);
        throw new NotFoundException("ResumeJobRun", jobId);
      }

      _logger.LogInformation("Completed {Operation} for JobId={JobId}. Status={Status}.", nameof(FetchResumeJobRunStatus), jobId, resumeJob.Status);
      return new ResumeJobRunResponse
      {
        Status = resumeJob.Status,
        PdfUrl = resumeJob.PdfUrl,
        Error = resumeJob.Error,
      };
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation} for JobId={JobId}.", nameof(FetchResumeJobRunStatus), jobId);
      throw;
    }
  }

  public async Task<FetchResourceResponse<IDictionary<string, string>>> FetchLatestResumePdfUrl()
  {
    _logger.LogInformation("Started {Operation}.", nameof(FetchLatestResumePdfUrl));
    try
    {
      var result = await _supabaseIntegration.FetchLatestPdfUrlAsync();
      _logger.LogInformation("Completed {Operation}. HasPdfUrl={HasPdfUrl}.", nameof(FetchLatestResumePdfUrl), !string.IsNullOrWhiteSpace(result));
      return new FetchResourceResponse<IDictionary<string, string>>("ResumePdfUrl", new Dictionary<string, string>
      {
        ["pdfUrl"] = result ?? string.Empty,
      });
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}.", nameof(FetchLatestResumePdfUrl));
      throw;
    }
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
