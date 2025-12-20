using Domain.Entities;

namespace Application.Repositories;

public interface IExperienceRepository
{
  Task CreateAsync(Experience experience);

  Task<IList<Experience>> FetchAllAsync();

  Task<IList<Experience>> FetchAllDeletedAsync();

  Task<Experience?> FetchByIdAsync(string experienceId);

  Task<IList<Experience?>> FetchByIdsAsync(IList<string> experienceIds);

  Task<Experience?> FetchByCompanyNameAndJobTitle(string companyName, string jobTitle);

  Task UpdateAsync(string experienceId, string serializedChanges);

  Task DeleteAsync(string experienceId);
}