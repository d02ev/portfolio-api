using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class ContactRepository(IMongoDatabase database, ILogger<ContactRepository> logger) : IContactRepository
{
  private readonly IMongoCollection<Contact> _contacts = database.GetCollection<Contact>(CollectionNames.Contact);
  private readonly ILogger<ContactRepository> _logger = logger;

  public async Task CreateAsync(Contact contact)
  {
    _logger.LogDebug("Started {Operation}.", nameof(CreateAsync));
    try
    {
      await _contacts.InsertOneAsync(contact);
      _logger.LogDebug("Completed {Operation}.", nameof(CreateAsync));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(CreateAsync));
      throw;
    }
  }

  public async Task<Contact?> FetchAsync()
  {
    _logger.LogDebug("Started {Operation}.", nameof(FetchAsync));
    try
    {
      var filter = Builders<Contact>.Filter.Empty;
      var result = await _contacts.Find(filter).FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation}. Found={Found}.", nameof(FetchAsync), result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(FetchAsync));
      throw;
    }
  }

  public async Task<Contact?> FetchByIdAsync(string contactId)
  {
    _logger.LogDebug("Started {Operation} for ContactId={ContactId}.", nameof(FetchByIdAsync), contactId);
    try
    {
      var filter = Builders<Contact>.Filter.Eq(c => c.Id, contactId);
      var result = await _contacts.Find(filter).FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation} for ContactId={ContactId}. Found={Found}.", nameof(FetchByIdAsync), contactId, result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for ContactId={ContactId}.", nameof(FetchByIdAsync), contactId);
      throw;
    }
  }

  public async Task UpdateAsync(string contactId, string serializedChanges)
  {
    _logger.LogDebug("Started {Operation} for ContactId={ContactId}.", nameof(UpdateAsync), contactId);
    try
    {
      var changes = BsonDocument.Parse(serializedChanges);
      var filter = Builders<Contact>.Filter.Eq(c => c.Id, contactId);
      var updates = MongoUpdateHelper.BuildUpdateDefinition<Contact>(changes);

      await _contacts.UpdateOneAsync(filter, updates);
      _logger.LogDebug("Completed {Operation} for ContactId={ContactId}.", nameof(UpdateAsync), contactId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for ContactId={ContactId}.", nameof(UpdateAsync), contactId);
      throw;
    }
  }
}
