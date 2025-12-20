using Application.Common;
using Application.Dto;
using Application.Helpers;
using Application.Repositories;
using Application.Responses;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Newtonsoft.Json;

namespace Application.Services;

public class ProjectService(IProjectRepository projectRepository, IMapper mapper) : IProjectService
{
  private readonly IProjectRepository _projectRepository = projectRepository;
  private readonly IMapper _mapper = mapper;

  public async Task<CreateResourceResponse<IDictionary<string, string>>> CreateProject(ProjectDto projectDto)
  {
    var _ = await _projectRepository.FetchByDisplayName(projectDto.DisplayName);
    if (_ is not null)
    {
      throw new BadRequestException(ResourceNames.Project, $"Project with name '{projectDto.DisplayName}' already exists.");
    }

    var project = _mapper.Map<Project>(projectDto);
    await _projectRepository.CreateAsync(project);

    var createdProject = await _projectRepository.FetchByDisplayName(projectDto.DisplayName) ?? throw new InternalServerException(ResourceNames.Project, $"An error occurred while creating the project {projectDto.DisplayName}");

    return new CreateResourceResponse<IDictionary<string, string>>(ResourceNames.Project, new Dictionary<string, string>
    {
      { nameof(createdProject.Id).ToLowerInvariant(), createdProject.Id },
      { nameof(createdProject.DisplayName).ToLowerInvariant(), createdProject.DisplayName }
    });
  }

  public async Task<DeleteResourceResponse> DeleteProject(string projectId)
  {
    var _ = await _projectRepository.FetchByIdAsync(projectId) ?? throw new NotFoundException(ResourceNames.Project, projectId);
    await _projectRepository.DeleteAsync(projectId);

    return new DeleteResourceResponse(ResourceNames.Project, projectId);
  }

  public async Task<FetchResourceResponse<IList<FetchProjectDto>>> FetchAllDeletedProjects()
  {
    var projects = await _projectRepository.FetchAllDeletedAsync();
    var fetchProjectDtos = _mapper.Map<IList<FetchProjectDto>>(projects);

    return new FetchResourceResponse<IList<FetchProjectDto>>(ResourceNames.Project, fetchProjectDtos);
  }

  public async Task<FetchResourceResponse<IList<FetchProjectDto>>> FetchAllProjects()
  {
    var projects = await _projectRepository.FetchAllAsync();
    var fetchProjectDtos = _mapper.Map<IList<FetchProjectDto>>(projects);

    return new FetchResourceResponse<IList<FetchProjectDto>>(ResourceNames.Project, fetchProjectDtos);
  }

  public async Task<FetchResourceResponse<FetchProjectDto>> FetchProjectById(string projectId)
  {
    var project = await _projectRepository.FetchByIdAsync(projectId) ?? throw new NotFoundException(ResourceNames.Project, projectId);
    var fetchProjectDto = _mapper.Map<FetchProjectDto>(project);

    return new FetchResourceResponse<FetchProjectDto>(ResourceNames.Project, fetchProjectDto);
  }

  public async Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateProject(string projectId, UpdateProjectDto updateProjectDto)
  {
    var _ = await _projectRepository.FetchByIdAsync(projectId) ?? throw new NotFoundException(ResourceNames.Project, projectId);
    var changes = UpdateObjectBuilderHelper.BuildUpdateObject<UpdateProjectDto>(updateProjectDto);
    var serializedChanges = JsonConvert.SerializeObject(changes);

    await _projectRepository.UpdateAsync(projectId, serializedChanges);

    return new UpdateResourceResponse<IDictionary<string, object>>(ResourceNames.Project, changes);
  }
}