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

public class ExperienceServiceTests
{
  private readonly Mock<IExperienceRepository> _experienceRepositoryMock;
  private readonly IExperienceService _experienceService;

  public ExperienceServiceTests()
  {
    _experienceRepositoryMock = new Mock<IExperienceRepository>();
    _experienceService = new ExperienceService(_experienceRepositoryMock.Object, TestMapperFactory.Create(), NullLogger<ExperienceService>.Instance);
  }

  [Fact]
  public async Task CreateExperience_ShouldCreateExperience_WhenCombinationDoesNotExist()
  {
    var experienceDto = new ExperienceDto
    {
      CompanyName = "OpenAI",
      JobTitle = "Engineer",
      Location = "Remote",
      StartDate = "Jan 2024",
      EndDate = "Mar 2025",
      Description = ["Built things"]
    };

    _experienceRepositoryMock.SetupSequence(r => r.FetchByCompanyNameAndJobTitle(experienceDto.CompanyName, experienceDto.JobTitle))
      .ReturnsAsync((Experience)null!)
      .ReturnsAsync(new Experience
      {
        Id = "experience-1",
        CompanyName = experienceDto.CompanyName,
        JobTitle = experienceDto.JobTitle
      });

    var response = await _experienceService.CreateExperience(experienceDto);

    response.StatusCode.Should().Be((int)HttpStatusCode.Created);
    response.Data["companyname"].Should().Be("OpenAI");
    response.Data["jobtitle"].Should().Be("Engineer");
    _experienceRepositoryMock.Verify(r => r.CreateAsync(It.Is<Experience>(e =>
      e.CompanyName == experienceDto.CompanyName &&
      e.JobTitle == experienceDto.JobTitle &&
      e.StartDate == new DateTime(2024, 1, 16) &&
      e.EndDate == new DateTime(2025, 3, 16))), Times.Once);
  }

  [Fact]
  public async Task CreateExperience_ShouldThrowBadRequestException_WhenCombinationAlreadyExists()
  {
    _experienceRepositoryMock.Setup(r => r.FetchByCompanyNameAndJobTitle("OpenAI", "Engineer"))
      .ReturnsAsync(new Experience { Id = "experience-1" });

    Func<Task> act = () => _experienceService.CreateExperience(new ExperienceDto
    {
      CompanyName = "OpenAI",
      JobTitle = "Engineer"
    });

    await act.Should()
      .ThrowAsync<BadRequestException>()
      .Where(ex => ex.ResourceName == ResourceNames.Experience &&
                   ex.StatusCode == (int)HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task DeleteExperience_ShouldDeleteExperience_WhenRepositoryRemovesIt()
  {
    _experienceRepositoryMock.SetupSequence(r => r.FetchByIdAsync("experience-1"))
      .ReturnsAsync(new Experience { Id = "experience-1" })
      .ReturnsAsync((Experience)null!);

    var response = await _experienceService.DeleteExperience("experience-1");

    response.Message.Should().Be($"{ResourceNames.Experience} deleted successfully.");
    _experienceRepositoryMock.Verify(r => r.DeleteAsync("experience-1"), Times.Once);
  }

  [Fact]
  public async Task DeleteExperience_ShouldThrowInternalServerException_WhenExperienceStillExistsAfterDelete()
  {
    _experienceRepositoryMock.SetupSequence(r => r.FetchByIdAsync("experience-1"))
      .ReturnsAsync(new Experience { Id = "experience-1" })
      .ReturnsAsync(new Experience { Id = "experience-1" });

    Func<Task> act = () => _experienceService.DeleteExperience("experience-1");

    await act.Should()
      .ThrowAsync<InternalServerException>()
      .Where(ex => ex.ResourceName == ResourceNames.Experience &&
                   ex.StatusCode == (int)HttpStatusCode.InternalServerError);
  }

  [Fact]
  public async Task FetchAllExperiences_ShouldReturnMappedDtos_WhenExperiencesExist()
  {
    _experienceRepositoryMock.Setup(r => r.FetchAllAsync())
      .ReturnsAsync([
        new Experience { Id = "experience-1", CompanyName = "OpenAI", JobTitle = "Engineer", StartDate = new DateTime(2024, 1, 16) },
        new Experience { Id = "experience-2", CompanyName = "Example", JobTitle = "Developer", StartDate = new DateTime(2023, 5, 16) }
      ]);

    var response = await _experienceService.FetchAllExperiences();

    response.Data.Should().HaveCount(2);
    response.Data.First().StartDate.Should().Be("Jan 2024");
    response.Data.Last().StartDate.Should().Be("May 2023");
  }

  [Fact]
  public async Task UpdateExperience_ShouldConvertMonthYearStringsToDates_WhenDatesAreProvided()
  {
    _experienceRepositoryMock.Setup(r => r.FetchByIdAsync("experience-1"))
      .ReturnsAsync(new Experience { Id = "experience-1" });

    var response = await _experienceService.UpdateExperience("experience-1", new UpdateExperienceDto
    {
      StartDate = "Jan 2024",
      EndDate = "Feb 2024"
    });

    response.Data.Should().ContainKey("StartDate").WhoseValue.Should().Be(new DateTime(2024, 1, 16));
    response.Data.Should().ContainKey("EndDate").WhoseValue.Should().Be(new DateTime(2024, 2, 16));
    _experienceRepositoryMock.Verify(r => r.UpdateAsync("experience-1", It.Is<string>(s =>
      s.Contains("2024-01-16") && s.Contains("2024-02-16"))), Times.Once);
  }
}
