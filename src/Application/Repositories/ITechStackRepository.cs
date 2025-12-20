using Domain.Entities;

namespace Application.Repositories;

public interface ITechStackRepository
{
  Task CreateAsync(TechStack techStack);

  Task<TechStack?> FetchAsync();

  Task<TechStack?> FetchByIdAsync(string techStackId);

  Task UpdateAsync(string techStackId, string serializedChanges);
}