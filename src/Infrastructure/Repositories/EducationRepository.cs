using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class EducationRepository(IMongoDatabase database) : IEducationRepository
{
  private readonly IMongoCollection<Education> _educations = database.GetCollection<Education>(CollectionNames.Education);

  public async Task CreateAsync(Education education)
  {
    await _educations.InsertOneAsync(education);
  }

  public async Task<Education?> FetchAsync()
  {
    var filter = Builders<Education>.Filter.Empty;
    return await _educations.Find(filter).FirstOrDefaultAsync();
  }

  public async Task<Education?> FetchByIdAsync(string educationId)
  {
    var filter = Builders<Education>.Filter.Eq(e => e.Id, educationId);
    return await _educations.Find(filter).FirstOrDefaultAsync();
  }

  public async Task UpdateAsync(string educationId, string serializedChanges)
  {
    var changes = BsonDocument.Parse(serializedChanges);
    var filter = Builders<Education>.Filter.Eq(e => e.Id, educationId);
    var updates = MongoUpdateHelper.BuildUpdateDefinition<Education>(changes);

    await _educations.UpdateOneAsync(filter, updates);
  }
}