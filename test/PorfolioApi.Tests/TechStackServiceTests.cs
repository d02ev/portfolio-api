using System.Net;
using Application.Common;
using Application.Dto;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace PorfolioApi.Tests;

public class TechStackServiceTests
{
  private readonly Mock<ITechStackRepository> _techStackRepositoryMock;
  private readonly ITechStackService _techStackService;

  public TechStackServiceTests()
  {
    _techStackRepositoryMock = new Mock<ITechStackRepository>();
    _techStackService = new TechStackService(_techStackRepositoryMock.Object, TestMapperFactory.Create());
  }

  [Fact]
  public async Task CreateTechStack_ShouldCreateTechStack_WhenNoneExists()
  {
    var techStackDto = new TechStackDto
    {
      Languages = ["C#"],
      FrameworksAndPlatforms = [".NET"],
      Databases = ["MongoDB"],
      CloudAndDevOps = ["Azure"],
      Others = ["Docker"]
    };

    _techStackRepositoryMock.Setup(r => r.FetchAsync()).ReturnsAsync((TechStack)null!);

    var response = await _techStackService.CreateTechStack(techStackDto);

    response.StatusCode.Should().Be((int)HttpStatusCode.Created);
    response.ResourceName.Should().Be(ResourceNames.TechStack);
    _techStackRepositoryMock.Verify(r => r.CreateAsync(It.Is<TechStack>(t =>
      t.Languages.SequenceEqual(techStackDto.Languages) &&
      t.Others.SequenceEqual(techStackDto.Others))), Times.Once);
  }

  [Fact]
  public async Task CreateTechStack_ShouldThrowBadRequestException_WhenTechStackAlreadyExists()
  {
    _techStackRepositoryMock.Setup(r => r.FetchAsync()).ReturnsAsync(new TechStack { Id = "tech-1" });

    Func<Task> act = () => _techStackService.CreateTechStack(new TechStackDto());

    await act.Should()
      .ThrowAsync<BadRequestException>()
      .Where(ex => ex.ResourceName == ResourceNames.TechStack &&
                   ex.StatusCode == (int)HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task FetchTechStack_ShouldThrowNotFoundException_WhenTechStackDoesNotExist()
  {
    _techStackRepositoryMock.Setup(r => r.FetchAsync()).ReturnsAsync((TechStack)null!);

    Func<Task> act = () => _techStackService.FetchTechStack();

    await act.Should()
      .ThrowAsync<NotFoundException>()
      .Where(ex => ex.ResourceName == ResourceNames.TechStack &&
                   ex.StatusCode == (int)HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UpdateTechStack_ShouldReturnUpdatedFieldsAndIncludeId_WhenTechStackExists()
  {
    _techStackRepositoryMock.Setup(r => r.FetchByIdAsync("tech-1"))
      .ReturnsAsync(new TechStack { Id = "tech-1" });

    var response = await _techStackService.UpdateTechStack("tech-1", new UpdateTechStackDto
    {
      Languages = ["Go", "Rust"]
    });

    response.Data.Should().ContainKey("id").WhoseValue.Should().Be("tech-1");
    response.Data.Should().ContainKey("Languages");
    _techStackRepositoryMock.Verify(r => r.UpdateAsync("tech-1", It.Is<string>(s => s.Contains("Go") && s.Contains("Rust"))), Times.Once);
  }
}
