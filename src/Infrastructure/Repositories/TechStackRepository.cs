using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class TechStackRepository(IMongoDatabase database, ILogger<TechStackRepository> logger) : ITechStackRepository
{
  private readonly IMongoCollection<TechStack> _techStacks = database.GetCollection<TechStack>(CollectionNames.TechStack);
  private readonly ILogger<TechStackRepository> _logger = logger;

  public async Task CreateAsync(TechStack techStack)
  {
    _logger.LogDebug("Started {Operation}.", nameof(CreateAsync));
    try
    {
      await _techStacks.InsertOneAsync(techStack);
      _logger.LogDebug("Completed {Operation}.", nameof(CreateAsync));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(CreateAsync));
      throw;
    }
  }

  public async Task<TechStack?> FetchAsync()
  {
    _logger.LogDebug("Started {Operation}.", nameof(FetchAsync));
    try
    {
      var filter = Builders<TechStack>.Filter.Empty;
      var result = await _techStacks.Find(filter).FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation}. Found={Found}.", nameof(FetchAsync), result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(FetchAsync));
      throw;
    }
  }

  public async Task<TechStack?> FetchByIdAsync(string techStackId)
  {
    _logger.LogDebug("Started {Operation} for TechStackId={TechStackId}.", nameof(FetchByIdAsync), techStackId);
    try
    {
      var filter = Builders<TechStack>.Filter.Eq(ts => ts.Id, techStackId);
      var result = await _techStacks.Find(filter).FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation} for TechStackId={TechStackId}. Found={Found}.", nameof(FetchByIdAsync), techStackId, result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for TechStackId={TechStackId}.", nameof(FetchByIdAsync), techStackId);
      throw;
    }
  }

  public async Task UpdateAsync(string techStackId, string serializedChanges)
  {
    _logger.LogDebug("Started {Operation} for TechStackId={TechStackId}.", nameof(UpdateAsync), techStackId);
    try
    {
      var changes = BsonDocument.Parse(serializedChanges);
      var filter = Builders<TechStack>.Filter.Eq(ts => ts.Id, techStackId);
      var updates = MongoUpdateHelper.BuildUpdateDefinition<TechStack>(changes);

      await _techStacks.UpdateOneAsync(filter, updates);
      _logger.LogDebug("Completed {Operation} for TechStackId={TechStackId}.", nameof(UpdateAsync), techStackId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for TechStackId={TechStackId}.", nameof(UpdateAsync), techStackId);
      throw;
    }
  }
}
