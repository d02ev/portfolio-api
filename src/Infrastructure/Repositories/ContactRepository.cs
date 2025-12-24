using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class ContactRepository(IMongoDatabase database) : IContactRepository
{
  private readonly IMongoCollection<Contact> _contacts = database.GetCollection<Contact>(CollectionNames.Contact);

  public async Task CreateAsync(Contact contact)
  {
    await _contacts.InsertOneAsync(contact);
  }

  public async Task<Contact?> FetchAsync()
  {
    var filter = Builders<Contact>.Filter.Empty;
    return await _contacts.Find(filter).FirstOrDefaultAsync();
  }

  public async Task<Contact?> FetchByIdAsync(string contactId)
  {
    var filter = Builders<Contact>.Filter.Eq(c => c.Id, contactId);
    return await _contacts.Find(filter).FirstOrDefaultAsync();
  }

  public async Task UpdateAsync(string contactId, string serializedChanges)
  {
    var changes = BsonDocument.Parse(serializedChanges);
    var filter = Builders<Contact>.Filter.Eq(c => c.Id, contactId);
    var updates = MongoUpdateHelper.BuildUpdateDefinition<Contact>(changes);

    await _contacts.UpdateOneAsync(filter, updates);
  }
}