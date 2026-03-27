using System.Net;
using Application.Common;
using Application.Dto;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace PorfolioApi.Tests;

public class AboutServiceTests
{
  private readonly Mock<IAboutRepository> _aboutRepositoryMock;
  private readonly Mock<ITechStackRepository> _techStackRepositoryMock;
  private readonly IAboutService _aboutService;

  public AboutServiceTests()
  {
    _aboutRepositoryMock = new Mock<IAboutRepository>();
    _techStackRepositoryMock = new Mock<ITechStackRepository>();
    _aboutService = new AboutService(_aboutRepositoryMock.Object, _techStackRepositoryMock.Object, TestMapperFactory.Create(), NullLogger<AboutService>.Instance);
  }

  [Fact]
  public async Task CreateAbout_ShouldCreateAbout_WhenNameDoesNotExist()
  {
    var aboutDto = new AboutDto
    {
      TechStackId = "tech-1",
      FunFact = "Fun fact",
      Bio = new BioDto
      {
        Name = "Vikram",
        Intro = "Intro",
        Experience = "5+ years",
        Company = "OpenAI",
        Highlights = [new BioHighlightDto { Icon = "rocket", Text = "Ships fast", Highlight = "Always" }]
      }
    };

    _aboutRepositoryMock.SetupSequence(r => r.FetchByNameAsync(aboutDto.Bio.Name))
      .ReturnsAsync((About)null!)
      .ReturnsAsync(new About { Id = "about-1", Bio = new Bio { Name = aboutDto.Bio.Name } });

    var response = await _aboutService.CreateAbout(aboutDto);

    response.StatusCode.Should().Be((int)HttpStatusCode.Created);
    response.ResponseCode.Should().Be(ResponseCodes.ResourceCreated);
    response.ResourceName.Should().Be(ResourceNames.About);
    _aboutRepositoryMock.Verify(r => r.CreateAsync(It.Is<About>(a =>
      a.TechStackId == aboutDto.TechStackId &&
      a.FunFact == aboutDto.FunFact &&
      a.Bio.Name == aboutDto.Bio.Name &&
      a.Bio.Company == aboutDto.Bio.Company &&
      a.Bio.Highlights.Single().Text == aboutDto.Bio.Highlights.Single().Text
    )), Times.Once);
  }

  [Fact]
  public async Task CreateAbout_ShouldThrowBadRequestException_WhenNameAlreadyExists()
  {
    var aboutDto = new AboutDto
    {
      Bio = new BioDto { Name = "Vikram" }
    };

    _aboutRepositoryMock.Setup(r => r.FetchByNameAsync(aboutDto.Bio.Name))
      .ReturnsAsync(new About { Id = "about-1", Bio = new Bio { Name = aboutDto.Bio.Name } });

    Func<Task> act = () => _aboutService.CreateAbout(aboutDto);

    await act.Should()
      .ThrowAsync<BadRequestException>()
      .Where(ex => ex.ResourceName == ResourceNames.About &&
                   ex.StatusCode == (int)HttpStatusCode.BadRequest &&
                   ex.Message.Contains("already exists"));
  }

  [Fact]
  public async Task CreateAbout_ShouldThrowInternalServerException_WhenCreatedAboutCannotBeFetched()
  {
    var aboutDto = new AboutDto
    {
      Bio = new BioDto { Name = "Vikram" }
    };

    _aboutRepositoryMock.SetupSequence(r => r.FetchByNameAsync(aboutDto.Bio.Name))
      .ReturnsAsync((About)null!)
      .ReturnsAsync((About)null!);

    Func<Task> act = () => _aboutService.CreateAbout(aboutDto);

    await act.Should()
      .ThrowAsync<InternalServerException>()
      .Where(ex => ex.ResourceName == ResourceNames.About &&
                   ex.StatusCode == (int)HttpStatusCode.InternalServerError);
  }

  [Fact]
  public async Task FetchAbout_ShouldReturnAboutWithTechStack_WhenAboutExists()
  {
    var about = new About
    {
      Id = "about-1",
      TechStackId = "tech-1",
      FunFact = "Fun fact",
      Bio = new Bio
      {
        Name = "Vikram",
        Intro = "Intro",
        Experience = "5+ years",
        Company = "OpenAI"
      }
    };
    var techStack = new TechStack
    {
      Id = "tech-1",
      Languages = ["C#"],
      FrameworksAndPlatforms = [".NET"],
      Databases = ["MongoDB"],
      CloudAndDevOps = ["Azure"],
      Others = ["Docker"]
    };

    _aboutRepositoryMock.Setup(r => r.FetchAsync()).ReturnsAsync(about);
    _techStackRepositoryMock.Setup(r => r.FetchByIdAsync(about.TechStackId)).ReturnsAsync(techStack);

    var response = await _aboutService.FetchAbout();

    response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    response.ResourceName.Should().Be(ResourceNames.About);
    response.Data.Id.Should().Be(about.Id);
    response.Data.Bio.Name.Should().Be(about.Bio.Name);
    response.Data.TechStack.Languages.Should().ContainSingle().Which.Should().Be("C#");
  }

  [Fact]
  public async Task FetchAboutById_ShouldThrowNotFoundException_WhenAboutDoesNotExist()
  {
    _aboutRepositoryMock.Setup(r => r.FetchByIdAsync("missing")).ReturnsAsync((About)null!);

    Func<Task> act = () => _aboutService.FetchAboutById("missing");

    await act.Should()
      .ThrowAsync<NotFoundException>()
      .Where(ex => ex.ResourceName == ResourceNames.About &&
                   ex.StatusCode == (int)HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UpdateAbout_ShouldOnlyPersistProvidedFields_WhenOptionalFieldsAreOmitted()
  {
    _aboutRepositoryMock.Setup(r => r.FetchByIdAsync("about-1"))
      .ReturnsAsync(new About { Id = "about-1" });

    var response = await _aboutService.UpdateAbout("about-1", new UpdateAboutDto());

    response.Data.Should().BeEmpty();
    _aboutRepositoryMock.Verify(r => r.UpdateAsync("about-1", "{}"), Times.Once);
  }

  [Fact]
  public async Task UpdateAbout_ShouldPersistNestedChanges_WhenValuesProvided()
  {
    _aboutRepositoryMock.Setup(r => r.FetchByIdAsync("about-1"))
      .ReturnsAsync(new About { Id = "about-1" });

    var updateAboutDto = new UpdateAboutDto
    {
      FunFact = "Updated fact",
      Bio = new UpdateBioDto
      {
        Company = "OpenAI"
      }
    };

    var response = await _aboutService.UpdateAbout("about-1", updateAboutDto);

    response.Data.Keys.Should().BeEquivalentTo(["Bio", "FunFact"]);
    ((UpdateBioDto)response.Data["Bio"]).Company.Should().Be("OpenAI");
    response.Data["FunFact"].Should().Be("Updated fact");
    _aboutRepositoryMock.Verify(r => r.UpdateAsync("about-1", It.Is<string>(s =>
      s.Contains("Updated fact") && s.Contains("OpenAI"))), Times.Once);
  }
}
