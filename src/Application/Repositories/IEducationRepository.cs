using Domain.Entities;

namespace Application.Repositories;

public interface IEducationRepository
{
  Task CreateAsync(Education education);

  Task<Education?> FetchAsync();

  Task<Education?> FetchByIdAsync(string educationId);

  Task UpdateAsync(string educationId, string serializedChanges);
}