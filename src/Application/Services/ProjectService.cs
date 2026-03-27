using Application.Common;
using Application.Dto;
using Application.Helpers;
using Application.Repositories;
using Application.Responses;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Application.Services;

public class ProjectService(IProjectRepository projectRepository, IMapper mapper, ILogger<ProjectService> logger) : IProjectService
{
  private readonly IProjectRepository _projectRepository = projectRepository;
  private readonly IMapper _mapper = mapper;
  private readonly ILogger<ProjectService> _logger = logger;

  public async Task<CreateResourceResponse<IDictionary<string, string>>> CreateProject(ProjectDto projectDto)
  {
    _logger.LogInformation("Started {Operation}.", nameof(CreateProject));

    try
    {
      var existingProject = await _projectRepository.FetchByDisplayName(projectDto.DisplayName);
      if (existingProject is not null)
      {
        _logger.LogWarning("Duplicate project detected while creating project.");
        throw new BadRequestException(ResourceNames.Project, $"Project with name '{projectDto.DisplayName}' already exists.");
      }

      var project = _mapper.Map<Project>(projectDto);
      await _projectRepository.CreateAsync(project);

      var createdProject = await _projectRepository.FetchByDisplayName(projectDto.DisplayName);
      if (createdProject is null)
      {
        throw new InternalServerException(ResourceNames.Project, $"An error occurred while creating the project {projectDto.DisplayName}");
      }

      _logger.LogInformation("Completed {Operation}. ProjectId={ProjectId}.", nameof(CreateProject), createdProject.Id);
      return new CreateResourceResponse<IDictionary<string, string>>(ResourceNames.Project, new Dictionary<string, string>
      {
        { nameof(createdProject.Id).ToLowerInvariant(), createdProject.Id },
        { nameof(createdProject.DisplayName).ToLowerInvariant(), createdProject.DisplayName }
      });
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
      _logger.LogError(ex, "Unexpected error in {Operation}.", nameof(CreateProject));
      throw;
    }
  }

  public async Task<DeleteResourceResponse> DeleteProject(string projectId)
  {
    _logger.LogInformation("Started {Operation} for ProjectId={ProjectId}.", nameof(DeleteProject), projectId);
    try
    {
      var existingProject = await _projectRepository.FetchByIdAsync(projectId);
      if (existingProject is null)
      {
        _logger.LogWarning("Project not found for delete. ProjectId={ProjectId}.", projectId);
        throw new NotFoundException(ResourceNames.Project, projectId);
      }

      await _projectRepository.DeleteAsync(projectId);
      _logger.LogInformation("Completed {Operation} for ProjectId={ProjectId}.", nameof(DeleteProject), projectId);
      return new DeleteResourceResponse(ResourceNames.Project, projectId);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation} for ProjectId={ProjectId}.", nameof(DeleteProject), projectId);
      throw;
    }
  }

  public async Task<FetchResourceResponse<IList<FetchProjectDto>>> FetchAllDeletedProjects()
  {
    _logger.LogInformation("Started {Operation}.", nameof(FetchAllDeletedProjects));
    try
    {
      var projects = await _projectRepository.FetchAllDeletedAsync();
      var fetchProjectDtos = _mapper.Map<IList<FetchProjectDto>>(projects);
      _logger.LogInformation("Completed {Operation}. Count={Count}.", nameof(FetchAllDeletedProjects), fetchProjectDtos.Count);
      return new FetchResourceResponse<IList<FetchProjectDto>>(ResourceNames.Project, fetchProjectDtos);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}.", nameof(FetchAllDeletedProjects));
      throw;
    }
  }

  public async Task<FetchResourceResponse<IList<FetchProjectDto>>> FetchAllProjects()
  {
    _logger.LogInformation("Started {Operation}.", nameof(FetchAllProjects));
    try
    {
      var projects = await _projectRepository.FetchAllAsync();
      var fetchProjectDtos = _mapper.Map<IList<FetchProjectDto>>(projects);
      _logger.LogInformation("Completed {Operation}. Count={Count}.", nameof(FetchAllProjects), fetchProjectDtos.Count);
      return new FetchResourceResponse<IList<FetchProjectDto>>(ResourceNames.Project, fetchProjectDtos);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}.", nameof(FetchAllProjects));
      throw;
    }
  }

  public async Task<FetchResourceResponse<FetchProjectDto>> FetchProjectById(string projectId)
  {
    _logger.LogInformation("Started {Operation} for ProjectId={ProjectId}.", nameof(FetchProjectById), projectId);
    try
    {
      var project = await _projectRepository.FetchByIdAsync(projectId);
      if (project is null)
      {
        _logger.LogWarning("Project not found for ProjectId={ProjectId}.", projectId);
        throw new NotFoundException(ResourceNames.Project, projectId);
      }

      var fetchProjectDto = _mapper.Map<FetchProjectDto>(project);
      _logger.LogInformation("Completed {Operation} for ProjectId={ProjectId}.", nameof(FetchProjectById), projectId);
      return new FetchResourceResponse<FetchProjectDto>(ResourceNames.Project, fetchProjectDto);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation} for ProjectId={ProjectId}.", nameof(FetchProjectById), projectId);
      throw;
    }
  }

  public async Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateProject(string projectId, UpdateProjectDto updateProjectDto)
  {
    _logger.LogInformation("Started {Operation} for ProjectId={ProjectId}.", nameof(UpdateProject), projectId);
    try
    {
      var existingProject = await _projectRepository.FetchByIdAsync(projectId);
      if (existingProject is null)
      {
        _logger.LogWarning("Project not found for update. ProjectId={ProjectId}.", projectId);
        throw new NotFoundException(ResourceNames.Project, projectId);
      }

      var changes = UpdateObjectBuilderHelper.BuildUpdateObject<UpdateProjectDto>(updateProjectDto);
      var serializedChanges = JsonConvert.SerializeObject(changes);

      await _projectRepository.UpdateAsync(projectId, serializedChanges);

      _logger.LogInformation("Completed {Operation} for ProjectId={ProjectId}.", nameof(UpdateProject), projectId);
      return new UpdateResourceResponse<IDictionary<string, object>>(ResourceNames.Project, changes);
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation} for ProjectId={ProjectId}.", nameof(UpdateProject), projectId);
      throw;
    }
  }
}
