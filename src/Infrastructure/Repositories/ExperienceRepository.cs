using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class ExperienceRepository : IExperienceRepository
{
  private readonly IMongoCollection<Experience> _experiences;

  public ExperienceRepository(IMongoDatabase database)
  {
    _experiences = database.GetCollection<Experience>(CollectionNames.Experience);
    var idxGeneral = Builders<Experience>.IndexKeys
      .Ascending(e => e.IsDeleted)
      .Descending(e => e.StartDate);
    var idxIsDeleted = Builders<Experience>.IndexKeys
      .Ascending(e => e.IsDeleted);
    var idxCompanyNameJobTitle = Builders<Experience>.IndexKeys
      .Ascending(e => e.CompanyName)
      .Ascending(e => e.JobTitle);
    var opts = new CreateIndexOptions<Experience>
    {
      Background = true,
    };
    var idxModels = new List<CreateIndexModel<Experience>>
    {
      new (idxGeneral, opts),
      new (idxIsDeleted, opts),
      new (idxCompanyNameJobTitle, new CreateIndexOptions<Experience>
      {
        Background = true,
        PartialFilterExpression = Builders<Experience>.Filter.Eq(e => e.IsDeleted, false),
      }),
    };

    _experiences.Indexes.CreateMany(idxModels);
  }

  public async Task CreateAsync(Experience experience)
  {
    await _experiences.InsertOneAsync(experience);
  }

  public async Task DeleteAsync(string experienceId)
  {
    var filter = Builders<Experience>.Filter.Eq(e => e.Id, experienceId);
    var updates = Builders<Experience>.Update
      .Set(e => e.IsDeleted, true)
      .Set(e => e.DeletedAt, DateTime.UtcNow);

    await _experiences.UpdateOneAsync(filter, updates);
  }

  public async Task<IList<Experience>> FetchAllAsync()
  {
    var filter = Builders<Experience>.Filter.Eq(e => e.IsDeleted, false);
    var sort = Builders<Experience>.Sort.Descending(e => e.StartDate);
    return await _experiences
      .Find(filter)
      .Sort(sort)
      .ToListAsync();
  }

  public async Task<IList<Experience>> FetchAllDeletedAsync()
  {
    var filter = Builders<Experience>.Filter.Eq(e => e.IsDeleted, true);
    var sort = Builders<Experience>.Sort.Descending(e => e.StartDate);
    return await _experiences
      .Find(filter)
      .Sort(sort)
      .ToListAsync();
  }

  public async Task<Experience?> FetchByCompanyNameAndJobTitle(string companyName, string jobTitle)
  {
    var filterDefinitions = new List<FilterDefinition<Experience>>
    {
      Builders<Experience>.Filter.Eq(e => e.CompanyName, companyName),
      Builders<Experience>.Filter.Eq(e => e.JobTitle, jobTitle)
    };
    var filter = Builders<Experience>.Filter.And(filterDefinitions);

    return await _experiences
      .Find(filter)
      .FirstOrDefaultAsync();
  }

  public async Task<Experience?> FetchByIdAsync(string experienceId)
  {
    var filterDefinitions = new List<FilterDefinition<Experience>>
    {
      Builders<Experience>.Filter.Eq(e => e.Id, experienceId),
      Builders<Experience>.Filter.Eq(e => e.IsDeleted, false),
    };
    var filter = Builders<Experience>.Filter.And(filterDefinitions);
    return await _experiences
      .Find(filter)
      .FirstOrDefaultAsync();
  }

  public async Task<IList<Experience?>> FetchByIdsAsync(IList<string> experienceIds)
  {
    var experienceTasks = experienceIds.Select(FetchByIdAsync);
    var experiences = await Task.WhenAll(experienceTasks);

    return [.. experiences];
  }

  public async Task UpdateAsync(string experienceId, string serializedChanges)
  {
    var changes = BsonDocument.Parse(serializedChanges);
    var filter = Builders<Experience>.Filter.Eq(e => e.Id, experienceId);
    var updates = MongoUpdateHelper.BuildUpdateDefinition<Experience>(changes);

    await _experiences.UpdateOneAsync(filter, updates);
  }
}