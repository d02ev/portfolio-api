using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class ResumeRepository : IResumeRepository
{
  private readonly IMongoCollection<Resume> _resumes;
  private readonly ILogger<ResumeRepository> _logger;

  public ResumeRepository(IMongoDatabase database, ILogger<ResumeRepository> logger)
  {
    _logger = logger;
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
    _logger.LogDebug("Created indexes for {Repository}.", nameof(ResumeRepository));
  }

  public async Task CreateAsync(Resume resume)
  {
    _logger.LogDebug("Started {Operation}.", nameof(CreateAsync));
    try
    {
      await _resumes.InsertOneAsync(resume);
      _logger.LogDebug("Completed {Operation}.", nameof(CreateAsync));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(CreateAsync));
      throw;
    }
  }

  public async Task<Resume?> FetchAsync()
  {
    _logger.LogDebug("Started {Operation}.", nameof(FetchAsync));
    try
    {
      var filter = Builders<Resume>.Filter.Empty;
      var result = await _resumes
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

  public async Task<Resume?> FetchByIdAsync(string resumeId)
  {
    _logger.LogDebug("Started {Operation} for ResumeId={ResumeId}.", nameof(FetchByIdAsync), resumeId);
    try
    {
      var filter = Builders<Resume>.Filter.Eq(r => r.Id, resumeId);
      var result = await _resumes
        .Find(filter)
        .FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation} for ResumeId={ResumeId}. Found={Found}.", nameof(FetchByIdAsync), resumeId, result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for ResumeId={ResumeId}.", nameof(FetchByIdAsync), resumeId);
      throw;
    }
  }

  public async Task<Resume?> FetchByNameAsync(string name)
  {
    _logger.LogDebug("Started {Operation}.", nameof(FetchByNameAsync));
    try
    {
      var filter = Builders<Resume>.Filter.Eq(r => r.Name, name);
      var result = await _resumes
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

  public async Task UpdateAsync(string resumeId, string serializedChanges)
  {
    _logger.LogDebug("Started {Operation} for ResumeId={ResumeId}.", nameof(UpdateAsync), resumeId);
    try
    {
      var changes = BsonDocument.Parse(serializedChanges);
      var filter = Builders<Resume>.Filter.Eq(r => r.Id, resumeId);
      var updates = MongoUpdateHelper.BuildUpdateDefinition<Resume>(changes);

      await _resumes.UpdateOneAsync(filter, updates);
      _logger.LogDebug("Completed {Operation} for ResumeId={ResumeId}.", nameof(UpdateAsync), resumeId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for ResumeId={ResumeId}.", nameof(UpdateAsync), resumeId);
      throw;
    }
  }
}
