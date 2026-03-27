using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
  private readonly IMongoCollection<User> _users;
  private readonly ILogger<UserRepository> _logger;

  public UserRepository(IMongoDatabase database, ILogger<UserRepository> logger)
  {
    _logger = logger;
    _users = database.GetCollection<User>(CollectionNames.User);
    var idxKeys = Builders<User>.IndexKeys
      .Ascending(u => u.Username);
    var idxModel = new CreateIndexModel<User>(idxKeys, new CreateIndexOptions<User> { Unique = true });

    _users.Indexes.CreateOne(idxModel);
    _logger.LogDebug("Created indexes for {Repository}.", nameof(UserRepository));
  }

  public async Task CreateAsync(User user)
  {
    _logger.LogDebug("Started {Operation}.", nameof(CreateAsync));
    try
    {
      await _users.InsertOneAsync(user);
      _logger.LogDebug("Completed {Operation}.", nameof(CreateAsync));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(CreateAsync));
      throw;
    }
  }

  public async Task<User?> FetchByIdAsync(string userId)
  {
    _logger.LogDebug("Started {Operation} for UserId={UserId}.", nameof(FetchByIdAsync), userId);
    try
    {
      var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
      var result = await _users.Find(filter).FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation} for UserId={UserId}. Found={Found}.", nameof(FetchByIdAsync), userId, result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for UserId={UserId}.", nameof(FetchByIdAsync), userId);
      throw;
    }
  }

  public async Task<User?> FetchByUsernameAsync(string username)
  {
    _logger.LogDebug("Started {Operation}.", nameof(FetchByUsernameAsync));
    try
    {
      var filter = Builders<User>.Filter.Eq(u => u.Username, username);
      var result = await _users.Find(filter).FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation}. Found={Found}.", nameof(FetchByUsernameAsync), result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(FetchByUsernameAsync));
      throw;
    }
  }

  public async Task UpdateAsync(string userId, string serializedChanges)
  {
    _logger.LogDebug("Started {Operation} for UserId={UserId}.", nameof(UpdateAsync), userId);
    try
    {
      var changes = BsonDocument.Parse(serializedChanges);
      var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
      var updates = MongoUpdateHelper.BuildUpdateDefinition<User>(changes);

      await _users.UpdateOneAsync(filter, updates);
      _logger.LogDebug("Completed {Operation} for UserId={UserId}.", nameof(UpdateAsync), userId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for UserId={UserId}.", nameof(UpdateAsync), userId);
      throw;
    }
  }
}
