using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
  private readonly IMongoCollection<User> _users;

  public UserRepository(IMongoDatabase database)
  {
    _users = database.GetCollection<User>(CollectionNames.User);
    var idxKeys = Builders<User>.IndexKeys
      .Ascending(u => u.Username);
    var idxModel = new CreateIndexModel<User>(idxKeys, new CreateIndexOptions<User> { Unique = true });

    _users.Indexes.CreateOne(idxModel);
  }

  public async Task CreateAsync(User user)
  {
    await _users.InsertOneAsync(user);
  }

  public async Task<User?> FetchByIdAsync(string userId)
  {
    var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
    return await _users.Find(filter).FirstOrDefaultAsync();
  }

  public async Task<User?> FetchByUsernameAsync(string username)
  {
    var filter = Builders<User>.Filter.Eq(u => u.Username, username);
    return await _users.Find(filter).FirstOrDefaultAsync();
  }

  public async Task UpdateAsync(string userId, string serializedChanges)
  {
    var changes = BsonDocument.Parse(serializedChanges);
    var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
    var updates = MongoUpdateHelper.BuildUpdateDefinition<User>(changes);

    await _users.UpdateOneAsync(filter, updates);
  }
}