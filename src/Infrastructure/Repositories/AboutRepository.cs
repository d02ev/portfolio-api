using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class AboutRepository : IAboutRepository
{
  private readonly IMongoCollection<About> _abouts;
  private readonly ILogger<AboutRepository> _logger;

  public AboutRepository(IMongoDatabase database, ILogger<AboutRepository> logger)
  {
    _logger = logger;
    _abouts = database.GetCollection<About>(CollectionNames.About);
    var Idx = Builders<About>.IndexKeys
      .Ascending(a => a.Bio.Name);
    var opts = new CreateIndexOptions
    {
      Background = true,
      Unique = true,
    };
    var idxModel = new CreateIndexModel<About>(Idx);

    _abouts.Indexes.CreateOne(idxModel);
    _logger.LogDebug("Created indexes for {Repository}.", nameof(AboutRepository));
  }

  public async Task CreateAsync(About about)
  {
    _logger.LogDebug("Started {Operation}.", nameof(CreateAsync));
    try
    {
      await _abouts.InsertOneAsync(about);
      _logger.LogDebug("Completed {Operation}.", nameof(CreateAsync));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(CreateAsync));
      throw;
    }
  }

  public async Task<About?> FetchAsync()
  {
    _logger.LogDebug("Started {Operation}.", nameof(FetchAsync));
    try
    {
      var filter = Builders<About>.Filter.Empty;
      var result = await _abouts
        .Find(filter)
        .FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation}. Found={Found}.", nameof(FetchAsync), result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(FetchAsync));
      throw;
    }
  }

  public async Task<About?> FetchByIdAsync(string aboutId)
  {
    _logger.LogDebug("Started {Operation} for AboutId={AboutId}.", nameof(FetchByIdAsync), aboutId);
    try
    {
      var filter = Builders<About>.Filter.Eq(a => a.Id, aboutId);
      var result = await _abouts
        .Find(filter)
        .FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation} for AboutId={AboutId}. Found={Found}.", nameof(FetchByIdAsync), aboutId, result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for AboutId={AboutId}.", nameof(FetchByIdAsync), aboutId);
      throw;
    }
  }

  public async Task<About?> FetchByNameAsync(string name)
  {
    _logger.LogDebug("Started {Operation}.", nameof(FetchByNameAsync));
    try
    {
      var filter = Builders<About>.Filter.Eq(a => a.Bio.Name, name);
      var result = await _abouts
        .Find(filter)
        .FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation}. Found={Found}.", nameof(FetchByNameAsync), result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(FetchByNameAsync));
      throw;
    }
  }

  public async Task UpdateAsync(string aboutId, string serializedChanges)
  {
    _logger.LogDebug("Started {Operation} for AboutId={AboutId}.", nameof(UpdateAsync), aboutId);
    try
    {
      var changes = BsonDocument.Parse(serializedChanges);
      var filter = Builders<About>.Filter.Eq(a => a.Id, aboutId);
      var updates = MongoUpdateHelper.BuildUpdateDefinition<About>(changes);

      await _abouts.UpdateOneAsync(filter, updates);
      _logger.LogDebug("Completed {Operation} for AboutId={AboutId}.", nameof(UpdateAsync), aboutId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for AboutId={AboutId}.", nameof(UpdateAsync), aboutId);
      throw;
    }
  }
}
