using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class TechStackRepository(IMongoDatabase database) : ITechStackRepository
{
  private readonly IMongoCollection<TechStack> _techStacks = database.GetCollection<TechStack>(CollectionNames.TechStack);

  public async Task CreateAsync(TechStack techStack)
  {
    await _techStacks.InsertOneAsync(techStack);
  }

  public async Task<TechStack?> FetchAsync()
  {
    var filter = Builders<TechStack>.Filter.Empty;
    return await _techStacks.Find(filter).FirstOrDefaultAsync();
  }

  public async Task<TechStack?> FetchByIdAsync(string techStackId)
  {
    var filter = Builders<TechStack>.Filter.Eq(ts => ts.Id, techStackId);
    return await _techStacks.Find(filter).FirstOrDefaultAsync();
  }

  public async Task UpdateAsync(string techStackId, string serializedChanges)
  {
    var changes = BsonDocument.Parse(serializedChanges);
    var filter = Builders<TechStack>.Filter.Eq(ts => ts.Id, techStackId);
    var updates = MongoUpdateHelper.BuildUpdateDefinition<TechStack>(changes);

    await _techStacks.UpdateOneAsync(filter, updates);
  }
}