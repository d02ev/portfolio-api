using Domain.Entities;

namespace Application.Repositories;

public interface IContactRepository
{
  Task CreateAsync(Contact contact);

  Task<Contact?> FetchAsync();

  Task<Contact?> FetchByIdAsync(string contactId);

  Task UpdateAsync(string contactId, string serializedChanges);
}