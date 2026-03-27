using System.Net;
using Application.Common;
using Application.Dto;
using Application.Integrations;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Postgres;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace PorfolioApi.Tests;

public class ResumeServiceTests
{
  private readonly Mock<IResumeRepository> _resumeRepositoryMock;
  private readonly Mock<IExperienceRepository> _experienceRepositoryMock;
  private readonly Mock<IProjectRepository> _projectRepositoryMock;
  private readonly Mock<ITechStackRepository> _techStackRepositoryMock;
  private readonly Mock<IEducationRepository> _educationRepositoryMock;
  private readonly Mock<IContactRepository> _contactRepositoryMock;
  private readonly Mock<ISupabaseIntegration> _supabaseIntegrationMock;
  private readonly Mock<ITelegramIntegration> _telegramIntegrationMock;
  private readonly Mock<IGithubIntegration> _githubIntegrationMock;
  private readonly TestLogger<ResumeService> _logger;
  private readonly IResumeService _resumeService;

  public ResumeServiceTests()
  {
    _resumeRepositoryMock = new Mock<IResumeRepository>();
    _experienceRepositoryMock = new Mock<IExperienceRepository>();
    _projectRepositoryMock = new Mock<IProjectRepository>();
    _techStackRepositoryMock = new Mock<ITechStackRepository>();
    _educationRepositoryMock = new Mock<IEducationRepository>();
    _contactRepositoryMock = new Mock<IContactRepository>();
    _supabaseIntegrationMock = new Mock<ISupabaseIntegration>();
    _telegramIntegrationMock = new Mock<ITelegramIntegration>();
    _githubIntegrationMock = new Mock<IGithubIntegration>();
    _logger = new TestLogger<ResumeService>();
    _resumeService = new ResumeService(
      _resumeRepositoryMock.Object,
      _experienceRepositoryMock.Object,
      _projectRepositoryMock.Object,
      _techStackRepositoryMock.Object,
      _educationRepositoryMock.Object,
      _contactRepositoryMock.Object,
      _supabaseIntegrationMock.Object,
      _telegramIntegrationMock.Object,
      _githubIntegrationMock.Object,
      TestMapperFactory.Create(),
      _logger);
  }

  [Fact]
  public async Task CreateResume_ShouldCreateResume_WhenNameDoesNotExist()
  {
    var resumeDto = new ResumeDto
    {
      Name = "default",
      ExperienceIds = ["exp-1"],
      ProjectIds = ["project-1"],
      TechStackId = "tech-1",
      ContactId = "contact-1",
      EducationId = "education-1"
    };

    _resumeRepositoryMock.SetupSequence(r => r.FetchByNameAsync(resumeDto.Name))
      .ReturnsAsync((Resume)null!)
      .ReturnsAsync(new Resume { Id = "resume-1", Name = resumeDto.Name });

    var response = await _resumeService.CreateResume(resumeDto);

    response.StatusCode.Should().Be((int)HttpStatusCode.Created);
    response.ResourceName.Should().Be(ResourceNames.Resume);
    _resumeRepositoryMock.Verify(r => r.CreateAsync(It.Is<Resume>(resume =>
      resume.Name == resumeDto.Name &&
      resume.ExperienceIds.SequenceEqual(resumeDto.ExperienceIds) &&
      resume.ProjectIds.SequenceEqual(resumeDto.ProjectIds))), Times.Once);
  }

  [Fact]
  public async Task CreateResume_ShouldThrowBadRequestException_WhenResumeAlreadyExists()
  {
    _resumeRepositoryMock.Setup(r => r.FetchByNameAsync("default"))
      .ReturnsAsync(new Resume { Id = "resume-1", Name = "default" });

    Func<Task> act = () => _resumeService.CreateResume(new ResumeDto { Name = "default" });

    await act.Should()
      .ThrowAsync<BadRequestException>()
      .Where(ex => ex.ResourceName == ResourceNames.Resume &&
                   ex.StatusCode == (int)HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task FetchResume_ShouldReturnFullyComposedResumeAndSortExperienceDescending()
  {
    _resumeRepositoryMock.Setup(r => r.FetchAsync())
      .ReturnsAsync(new Resume
      {
        Id = "resume-1",
        Name = "default",
        ExperienceIds = ["exp-1", "exp-2"],
        ProjectIds = ["project-1"],
        TechStackId = "tech-1",
        ContactId = "contact-1",
        EducationId = "education-1"
      });
    _projectRepositoryMock.Setup(r => r.FetchByIdsAsync(It.IsAny<IList<string>>()))
      .ReturnsAsync([new Project { Id = "project-1", DisplayName = "Portfolio API", RepoUrl = "repo" }]);
    _techStackRepositoryMock.Setup(r => r.FetchByIdAsync("tech-1"))
      .ReturnsAsync(new TechStack { Id = "tech-1", Languages = ["C#"] });
    _educationRepositoryMock.Setup(r => r.FetchByIdAsync("education-1"))
      .ReturnsAsync(new Education { Id = "education-1", Institute = "BITS" });
    _contactRepositoryMock.Setup(r => r.FetchByIdAsync("contact-1"))
      .ReturnsAsync(new Contact { Id = "contact-1", Email = "vikram@example.com" });
    _experienceRepositoryMock.Setup(r => r.FetchByIdsAsync(It.IsAny<IList<string>>()))
      .ReturnsAsync([
        new Experience { Id = "exp-1", CompanyName = "Older", JobTitle = "Developer", StartDate = new DateTime(2023, 1, 16) },
        new Experience { Id = "exp-2", CompanyName = "Newer", JobTitle = "Senior Developer", StartDate = new DateTime(2024, 1, 16) }
      ]);

    var response = await _resumeService.FetchResume();

    response.Data.Name.Should().Be("default");
    response.Data.Contact.Email.Should().Be("vikram@example.com");
    response.Data.TechStack.Languages.Should().ContainSingle().Which.Should().Be("C#");
    response.Data.Projects.Should().ContainSingle().Which.DisplayName.Should().Be("Portfolio API");
    response.Data.Experience.Select(e => e.CompanyName).Should().Equal("Newer", "Older");
  }

  [Fact]
  public async Task UpdateResume_ShouldPersistProvidedFields_WhenResumeExists()
  {
    _resumeRepositoryMock.Setup(r => r.FetchByIdAsync("resume-1"))
      .ReturnsAsync(new Resume { Id = "resume-1" });

    var response = await _resumeService.UpdateResume("resume-1", new UpdateResumeDto
    {
      Name = "updated",
      ProjectIds = ["project-1", "project-2"]
    });

    response.Data.Keys.Should().BeEquivalentTo(["Name", "ProjectIds"]);
    _resumeRepositoryMock.Verify(r => r.UpdateAsync("resume-1", It.Is<string>(s =>
      s.Contains("updated") && s.Contains("project-2"))), Times.Once);
  }

  [Fact]
  public async Task GenerateResume_ShouldRenderTemplateAndStartWorkflow_WhenInputsAreValid()
  {
    var resumeData = new FetchResumeDto
    {
      Name = "Base Resume"
    };
    var request = new GenerateResumeDto
    {
      ResumeData = resumeData,
      TemplateId = "template-1",
      ResumeName = "vikram-resume"
    };

    _supabaseIntegrationMock.Setup(s => s.DownloadFileAsStringAsync("template-1"))
      .ReturnsAsync("Hello @Model.Name");
    _supabaseIntegrationMock.Setup(s => s.InsertJobStatusAsync(It.IsAny<string>(), null))
      .ReturnsAsync(42);

    var response = await _resumeService.GenerateResume(request);

    response.StatusCode.Should().Be((int)HttpStatusCode.Created);
    response.Data.JobId.Should().Be(42);
    response.Data.ResumeName.Should().Be("vikram-resume");
    response.Data.LatexFileName.Should().StartWith("ge-").And.EndWith(".tex");
    _githubIntegrationMock.Verify(g => g.PushToRepositoryAsync(
      $"docs/{response.Data.LatexFileName}",
      It.Is<string>(content => content.Contains("Base Resume"))), Times.Once);
    _githubIntegrationMock.Verify(g => g.InitWorkflowAsync("42", "vikram-resume", response.Data.LatexFileName.Replace(".tex", string.Empty)), Times.Once);
    _telegramIntegrationMock.Verify(t => t.SendWorkflowStartedMessageAsync(), Times.Once);
    _logger.Entries.Should().Contain(e => e.Level == LogLevel.Information && e.Message.Contains("Started GenerateResume"));
    _logger.Entries.Should().Contain(e => e.Level == LogLevel.Information && e.Message.Contains("Downloading resume template"));
    _logger.Entries.Should().Contain(e => e.Level == LogLevel.Information && e.Message.Contains("Completed GenerateResume"));
  }

  [Fact]
  public async Task GenerateResume_ShouldEscapeUnescapedHashAndPercentAcrossResumeData()
  {
    var request = new GenerateResumeDto
    {
      ResumeData = new FetchResumeDto
      {
        Name = "Name #% ",
        Contact = new FetchContactDto
        {
          Email = "user#%mail.com",
          Mobile = "+1#%23",
          Github = "https://github.com/dev#%",
          Linkedin = "https://linkedin.com/in/dev#%",
          Website = "https://site.dev/#%"
        },
        Education = new FetchEducationDto
        {
          Institute = "Inst#%",
          StartDate = "Jan#% 2020",
          EndDate = "May#% 2024",
          Degree = "B.Tech#%",
          Grade = "9.1#%",
          Coursework = ["Algo#%"]
        },
        TechStack = new FetchTechStackDto
        {
          Languages = ["C#%"],
          FrameworksAndPlatforms = [".NET#%"],
          Databases = ["Postgres#%"],
          CloudAndDevOps = ["Azure#%"],
          Others = ["Docker#%"]
        },
        Experience =
        [
          new FetchExperienceDto
          {
            JobTitle = "Dev#%",
            CompanyName = "Comp#%",
            Location = "Loc#%",
            StartDate = "Jan#% 2022",
            EndDate = "Feb#% 2024",
            Description = ["Built#% things"]
          }
        ],
        Projects =
        [
          new FetchProjectDto
          {
            DisplayName = "Proj#%",
            ShortDescription = "Short#%",
            LongDescription = "Long#%",
            RepoUrl = "https://repo#%",
            LiveUrl = "https://live#%",
            TechStack = ["TS#%"]
          }
        ]
      },
      TemplateId = "template-1",
      ResumeName = "escaped-resume"
    };

    _supabaseIntegrationMock.Setup(s => s.DownloadFileAsStringAsync("template-1"))
      .ReturnsAsync("@Model.Name|@Model.Contact.Email|@Model.Education.Degree|@Model.TechStack.Languages[0]|@Model.Experience[0].Description[0]|@Model.Projects[0].DisplayName|@Model.Projects[0].TechStack[0]|@Model.Projects[0].LongDescription|@Model.Projects[0].RepoUrl");
    _supabaseIntegrationMock.Setup(s => s.InsertJobStatusAsync(It.IsAny<string>(), null))
      .ReturnsAsync(55);

    await _resumeService.GenerateResume(request);

    _githubIntegrationMock.Verify(g => g.PushToRepositoryAsync(
      It.IsAny<string>(),
      It.Is<string>(content =>
        content.Contains(@"Name \#\% ") &&
        content.Contains(@"user\#\%mail.com") &&
        content.Contains(@"B.Tech\#\%") &&
        content.Contains(@"C\#\%") &&
        content.Contains(@"Built\#\% things") &&
        content.Contains(@"Proj\#\%") &&
        content.Contains(@"TS\#\%") &&
        content.Contains(@"Long\#\%") &&
        content.Contains(@"https://repo\#\%"))), Times.Once);
  }

  [Fact]
  public async Task GenerateResume_ShouldNotDoubleEscapeAlreadyEscapedHashAndPercent()
  {
    var request = new GenerateResumeDto
    {
      ResumeData = new FetchResumeDto
      {
        Name = @"Value \# \% # %"
      },
      TemplateId = "template-1",
      ResumeName = "idempotent-resume"
    };

    _supabaseIntegrationMock.Setup(s => s.DownloadFileAsStringAsync("template-1"))
      .ReturnsAsync("@Model.Name");
    _supabaseIntegrationMock.Setup(s => s.InsertJobStatusAsync(It.IsAny<string>(), null))
      .ReturnsAsync(56);

    await _resumeService.GenerateResume(request);

    _githubIntegrationMock.Verify(g => g.PushToRepositoryAsync(
      It.IsAny<string>(),
      It.Is<string>(content =>
        content == @"Value \# \% \# \%" &&
        !content.Contains(@"\\#") &&
        !content.Contains(@"\\%"))), Times.Once);
  }

  [Fact]
  public async Task GenerateResume_ShouldUseJobPrefix_WhenCompanyNameIsProvided()
  {
    var resumeData = new FetchResumeDto
    {
      Name = "Company Resume"
    };
    var request = new GenerateResumeDto
    {
      ResumeData = resumeData,
      TemplateId = "template-1",
      ResumeName = "vikram-job",
      CompanyName = "OpenAI"
    };

    _supabaseIntegrationMock.Setup(s => s.DownloadFileAsStringAsync("template-1"))
      .ReturnsAsync("Hello @Model.Name");
    _supabaseIntegrationMock.Setup(s => s.InsertJobStatusAsync(It.IsAny<string>(), null))
      .ReturnsAsync(87);

    var response = await _resumeService.GenerateResume(request);

    response.StatusCode.Should().Be((int)HttpStatusCode.Created);
    response.Data.JobId.Should().Be(87);
    response.Data.LatexFileName.Should().StartWith("jd-").And.EndWith(".tex");
    _githubIntegrationMock.Verify(g => g.PushToRepositoryAsync(
      $"docs/{response.Data.LatexFileName}",
      It.Is<string>(content => content.Contains("Company Resume"))), Times.Once);
    _githubIntegrationMock.Verify(g => g.InitWorkflowAsync("87", "vikram-job", response.Data.LatexFileName.Replace(".tex", string.Empty)), Times.Once);
    _telegramIntegrationMock.Verify(t => t.SendWorkflowStartedMessageAsync(), Times.Once);
  }

  [Fact]
  public async Task FetchResumeJobRunStatus_ShouldReturnJobStatus_WhenJobExists()
  {
    _supabaseIntegrationMock.Setup(s => s.FetchJobStatusAsync(42))
      .ReturnsAsync(new ResumeJob
      {
        Id = 42,
        Status = "success",
        PdfUrl = "https://example.com/resume.pdf"
      });

    var response = await _resumeService.FetchResumeJobRunStatus(42);

    response.Status.Should().Be("success");
    response.PdfUrl.Should().Be("https://example.com/resume.pdf");
    response.Error.Should().BeNull();
  }

  [Fact]
  public async Task FetchResumeJobRunStatus_ShouldThrowNotFoundException_WhenJobDoesNotExist()
  {
    _supabaseIntegrationMock.Setup(s => s.FetchJobStatusAsync(42))
      .ReturnsAsync((ResumeJob)null!);

    Func<Task> act = () => _resumeService.FetchResumeJobRunStatus(42);

    await act.Should()
      .ThrowAsync<NotFoundException>()
      .Where(ex => ex.ResourceName == "ResumeJobRun" &&
                   ex.StatusCode == (int)HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task FetchLatestResumePdfUrl_ShouldReturnEmptyString_WhenProviderReturnsNull()
  {
    _supabaseIntegrationMock.Setup(s => s.FetchLatestPdfUrlAsync())
      .ReturnsAsync((string)null!);

    var response = await _resumeService.FetchLatestResumePdfUrl();

    response.ResourceName.Should().Be("ResumePdfUrl");
    response.Data["pdfUrl"].Should().BeEmpty();
  }
}
