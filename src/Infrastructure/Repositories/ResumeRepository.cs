using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class ResumeRepository : IResumeRepository
{
  private readonly IMongoCollection<Resume> _resumes;

  public ResumeRepository(IMongoDatabase database)
  {
    _resumes = database.GetCollection<Resume>(CollectionNames.Resume);
    var idx = Builders<Resume>.IndexKeys
      .Ascending(r => r.Name);
    var opts = new CreateIndexOptions<Resume>
    {
      Background = true,
      Unique = true
    };
    var idxModel = new CreateIndexModel<Resume>(idx, opts);

    _resumes.Indexes.CreateOne(idxModel);
  }

  public async Task CreateAsync(Resume resume)
  {
    await _resumes.InsertOneAsync(resume);
  }

  public async Task<Resume?> FetchAsync()
  {
    var filter = Builders<Resume>.Filter.Empty;
    return await _resumes
      .Find(filter)
      .FirstOrDefaultAsync();
  }

  public async Task<Resume?> FetchByIdAsync(string resumeId)
  {
    var filter = Builders<Resume>.Filter.Eq(r => r.Id, resumeId);
    return await _resumes
      .Find(filter)
      .FirstOrDefaultAsync();
  }

  public async Task<Resume?> FetchByNameAsync(string name)
  {
    var filter = Builders<Resume>.Filter.Eq(r => r.Name, name);
    return await _resumes
      .Find(filter)
      .FirstOrDefaultAsync();
  }

  public async Task UpdateAsync(string resumeId, string serializedChanges)
  {
    var changes = BsonDocument.Parse(serializedChanges);
    var filter = Builders<Resume>.Filter.Eq(r => r.Id, resumeId);
    var updates = MongoUpdateHelper.BuildUpdateDefinition<Resume>(changes);

    await _resumes.UpdateOneAsync(filter, updates);
  }
}