using Application.Dto;
using Application.Responses;

namespace Application.Services;

public interface IProjectService
{
  Task<CreateResourceResponse<IDictionary<string, string>>> CreateProject(ProjectDto projectDto);

  Task<FetchResourceResponse<IList<FetchProjectDto>>> FetchAllProjects();

  Task<FetchResourceResponse<IList<FetchProjectDto>>> FetchAllDeletedProjects();

  Task<FetchResourceResponse<FetchProjectDto>> FetchProjectById(string projectId);

  Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateProject(string projectId, UpdateProjectDto updateProjectDto);

  Task<DeleteResourceResponse> DeleteProject(string projectId);
}