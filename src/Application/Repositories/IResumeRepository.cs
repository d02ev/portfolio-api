using Domain.Entities;

namespace Application.Repositories;

public interface IResumeRepository
{
  Task CreateAsync(Resume resume);

  Task<Resume?> FetchAsync();

  Task<Resume?> FetchByIdAsync(string resumeId);

  Task<Resume?> FetchByNameAsync(string name);

  Task UpdateAsync(string resumeId, string serializedChanges);
}