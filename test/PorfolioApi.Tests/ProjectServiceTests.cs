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

public class ProjectServiceTests
{
  private readonly Mock<IProjectRepository> _projectRepositoryMock;
  private readonly IProjectService _projectService;

  public ProjectServiceTests()
  {
    _projectRepositoryMock = new Mock<IProjectRepository>();
    _projectService = new ProjectService(_projectRepositoryMock.Object, TestMapperFactory.Create(), NullLogger<ProjectService>.Instance);
  }

  [Fact]
  public async Task CreateProject_ShouldCreateProject_WhenDisplayNameDoesNotExist()
  {
    var projectDto = new ProjectDto
    {
      DisplayName = "Portfolio API",
      Year = 2025,
      ShortDescription = "Short",
      LongDescription = "Long",
      TechStack = ["C#", ".NET"],
      RepoUrl = "https://github.com/example/repo",
      LiveUrl = "https://example.com"
    };

    _projectRepositoryMock.SetupSequence(r => r.FetchByDisplayName(projectDto.DisplayName))
      .ReturnsAsync((Project)null!)
      .ReturnsAsync(new Project { Id = "project-1", DisplayName = projectDto.DisplayName });

    var response = await _projectService.CreateProject(projectDto);

    response.StatusCode.Should().Be((int)HttpStatusCode.Created);
    response.ResourceName.Should().Be(ResourceNames.Project);
    response.Data["id"].Should().Be("project-1");
    response.Data["displayname"].Should().Be(projectDto.DisplayName);
    _projectRepositoryMock.Verify(r => r.CreateAsync(It.Is<Project>(p =>
      p.DisplayName == projectDto.DisplayName &&
      p.TechStack.SequenceEqual(projectDto.TechStack))), Times.Once);
  }

  [Fact]
  public async Task CreateProject_ShouldThrowBadRequestException_WhenProjectAlreadyExists()
  {
    _projectRepositoryMock.Setup(r => r.FetchByDisplayName("Portfolio API"))
      .ReturnsAsync(new Project { Id = "project-1", DisplayName = "Portfolio API" });

    Func<Task> act = () => _projectService.CreateProject(new ProjectDto { DisplayName = "Portfolio API" });

    await act.Should()
      .ThrowAsync<BadRequestException>()
      .Where(ex => ex.ResourceName == ResourceNames.Project &&
                   ex.StatusCode == (int)HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task CreateProject_ShouldThrowInternalServerException_WhenCreatedProjectCannotBeFetched()
  {
    _projectRepositoryMock.SetupSequence(r => r.FetchByDisplayName("Portfolio API"))
      .ReturnsAsync((Project)null!)
      .ReturnsAsync((Project)null!);

    Func<Task> act = () => _projectService.CreateProject(new ProjectDto { DisplayName = "Portfolio API" });

    await act.Should()
      .ThrowAsync<InternalServerException>()
      .Where(ex => ex.ResourceName == ResourceNames.Project &&
                   ex.StatusCode == (int)HttpStatusCode.InternalServerError);
  }

  [Fact]
  public async Task DeleteProject_ShouldDeleteProject_WhenProjectExists()
  {
    _projectRepositoryMock.Setup(r => r.FetchByIdAsync("project-1"))
      .ReturnsAsync(new Project { Id = "project-1" });

    var response = await _projectService.DeleteProject("project-1");

    response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    response.Data["resourceId"].Should().Be("project-1");
    _projectRepositoryMock.Verify(r => r.DeleteAsync("project-1"), Times.Once);
  }

  [Fact]
  public async Task FetchAllProjects_ShouldReturnMappedProjects_WhenProjectsExist()
  {
    _projectRepositoryMock.Setup(r => r.FetchAllAsync())
      .ReturnsAsync([
        new Project { Id = "project-1", DisplayName = "Portfolio API", RepoUrl = "repo-1" },
        new Project { Id = "project-2", DisplayName = "Portfolio Web", RepoUrl = "repo-2" }
      ]);

    var response = await _projectService.FetchAllProjects();

    response.Data.Should().HaveCount(2);
    response.Data.Select(p => p.DisplayName).Should().BeEquivalentTo(["Portfolio API", "Portfolio Web"]);
  }

  [Fact]
  public async Task UpdateProject_ShouldPersistProvidedFields_WhenProjectExists()
  {
    _projectRepositoryMock.Setup(r => r.FetchByIdAsync("project-1"))
      .ReturnsAsync(new Project { Id = "project-1" });

    var response = await _projectService.UpdateProject("project-1", new UpdateProjectDto
    {
      RepoUrl = "https://github.com/example/new-repo"
    });

    response.Data.Should().ContainSingle();
    response.Data.Should().ContainKey("RepoUrl").WhoseValue.Should().Be("https://github.com/example/new-repo");
    _projectRepositoryMock.Verify(r => r.UpdateAsync("project-1", It.Is<string>(s => s.Contains("new-repo"))), Times.Once);
  }
}
