using Domain.Entities;

namespace Application.Repositories;

public interface IUserRepository
{
  Task CreateAsync(User user);

  Task<User?> FetchByIdAsync(string userId);

  Task<User?> FetchByUsernameAsync(string username);

  Task UpdateAsync(string userId, string serializedChanges);
}