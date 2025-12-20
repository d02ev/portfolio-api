using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class AboutRepository : IAboutRepository
{
  private readonly IMongoCollection<About> _abouts;

  public AboutRepository(IMongoDatabase database)
  {
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
  }

  public async Task CreateAsync(About about)
  {
    await _abouts.InsertOneAsync(about);
  }

  public async Task<About?> FetchAsync()
  {
    var filter = Builders<About>.Filter.Empty;
    return await _abouts
      .Find(filter)
      .FirstOrDefaultAsync();
  }

  public async Task<About?> FetchByIdAsync(string aboutId)
  {
    var filter = Builders<About>.Filter.Eq(a => a.Id, aboutId);
    return await _abouts
      .Find(filter)
      .FirstOrDefaultAsync();
  }

  public async Task<About?> FetchByNameAsync(string name)
  {
    var filter = Builders<About>.Filter.Eq(a => a.Bio.Name, name);
    return await _abouts
      .Find(filter)
      .FirstOrDefaultAsync();
  }

  public async Task UpdateAsync(string aboutId, string serializedChanges)
  {
    var changes = BsonDocument.Parse(serializedChanges);
    var filter = Builders<About>.Filter.Eq(a => a.Id, aboutId);
    var updates = MongoUpdateHelper.BuildUpdateDefinition<About>(changes);

    await _abouts.UpdateOneAsync(filter, updates);
  }
}