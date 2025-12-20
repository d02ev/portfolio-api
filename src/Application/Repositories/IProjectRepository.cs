using Domain.Entities;

namespace Application.Repositories;

public interface IProjectRepository
{
  Task CreateAsync(Project project);

  Task<IList<Project>> FetchAllAsync();

  Task<IList<Project>> FetchAllDeletedAsync();

  Task<Project?> FetchByIdAsync(string projectId);

  Task<IList<Project?>> FetchByIdsAsync(IList<string> projectIds);

  Task<Project?> FetchByDisplayName(string displayName);

  Task UpdateAsync(string projectId, string serializedChanges);

  Task DeleteAsync(string projectId);
}