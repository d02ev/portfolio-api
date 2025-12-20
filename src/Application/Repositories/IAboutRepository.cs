using Domain.Entities;

namespace Application.Repositories;

public interface IAboutRepository
{
  Task CreateAsync(About about);

  Task<About?> FetchAsync();

  Task<About?> FetchByIdAsync(string aboutId);

  Task<About?> FetchByNameAsync(string name);

  Task UpdateAsync(string aboutId, string serializedChanges);
}