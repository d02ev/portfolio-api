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

public class EducationServiceTests
{
  private readonly Mock<IEducationRepository> _educationRepositoryMock;
  private readonly IEducationService _educationService;

  public EducationServiceTests()
  {
    _educationRepositoryMock = new Mock<IEducationRepository>();
    _educationService = new EducationService(_educationRepositoryMock.Object, TestMapperFactory.Create(), NullLogger<EducationService>.Instance);
  }

  [Fact]
  public async Task CreateEducation_ShouldCreateEducation_WhenEducationDoesNotExist()
  {
    var educationDto = new EducationDto
    {
      Institute = "BITS",
      StartDate = "2018",
      EndDate = "2022",
      Degree = "B.Tech",
      Grade = "9.1",
      Coursework = ["Algorithms"]
    };

    _educationRepositoryMock.Setup(r => r.FetchAsync()).ReturnsAsync((Education)null!);

    var response = await _educationService.CreateEducation(educationDto);

    response.StatusCode.Should().Be((int)HttpStatusCode.Created);
    response.ResourceName.Should().Be(ResourceNames.Education);
    _educationRepositoryMock.Verify(r => r.CreateAsync(It.Is<Education>(e =>
      e.Institute == educationDto.Institute &&
      e.Coursework.SequenceEqual(educationDto.Coursework))), Times.Once);
  }

  [Fact]
  public async Task CreateEducation_ShouldThrowBadRequestException_WhenEducationAlreadyExists()
  {
    _educationRepositoryMock.Setup(r => r.FetchAsync()).ReturnsAsync(new Education { Id = "education-1" });

    Func<Task> act = () => _educationService.CreateEducation(new EducationDto());

    await act.Should()
      .ThrowAsync<BadRequestException>()
      .Where(ex => ex.ResourceName == ResourceNames.Education &&
                   ex.StatusCode == (int)HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task FetchEducation_ShouldReturnEducation_WhenEducationExists()
  {
    _educationRepositoryMock.Setup(r => r.FetchAsync())
      .ReturnsAsync(new Education
      {
        Id = "education-1",
        Institute = "BITS",
        StartDate = "2018",
        EndDate = "2022",
        Degree = "B.Tech",
        Grade = "9.1",
        Coursework = ["Algorithms"]
      });

    var response = await _educationService.FetchEducation();

    response.Data.Id.Should().Be("education-1");
    response.Data.Institute.Should().Be("BITS");
    response.Data.Coursework.Should().ContainSingle().Which.Should().Be("Algorithms");
  }

  [Fact]
  public async Task UpdateEducation_ShouldReturnUpdatedFieldsAndIncludeId_WhenEducationExists()
  {
    _educationRepositoryMock.Setup(r => r.FetchByIdAsync("education-1"))
      .ReturnsAsync(new Education { Id = "education-1" });

    var response = await _educationService.UpdateEducation("education-1", new UpdateEducationDto
    {
      Degree = "M.Tech"
    });

    response.Data.Should().ContainKey("id").WhoseValue.Should().Be("education-1");
    response.Data.Should().ContainKey("Degree").WhoseValue.Should().Be("M.Tech");
    _educationRepositoryMock.Verify(r => r.UpdateAsync("education-1", It.Is<string>(s => s.Contains("M.Tech"))), Times.Once);
  }
}
