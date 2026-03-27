using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class ExperienceRepository : IExperienceRepository
{
  private readonly IMongoCollection<Experience> _experiences;
  private readonly ILogger<ExperienceRepository> _logger;

  public ExperienceRepository(IMongoDatabase database, ILogger<ExperienceRepository> logger)
  {
    _logger = logger;
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
    _logger.LogDebug("Created indexes for {Repository}.", nameof(ExperienceRepository));
  }

  public async Task CreateAsync(Experience experience)
  {
    _logger.LogDebug("Started {Operation}.", nameof(CreateAsync));
    try
    {
      await _experiences.InsertOneAsync(experience);
      _logger.LogDebug("Completed {Operation}.", nameof(CreateAsync));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(CreateAsync));
      throw;
    }
  }

  public async Task DeleteAsync(string experienceId)
  {
    _logger.LogDebug("Started {Operation} for ExperienceId={ExperienceId}.", nameof(DeleteAsync), experienceId);
    try
    {
      var filter = Builders<Experience>.Filter.Eq(e => e.Id, experienceId);
      var updates = Builders<Experience>.Update
        .Set(e => e.IsDeleted, true)
        .Set(e => e.DeletedAt, DateTime.UtcNow);

      await _experiences.UpdateOneAsync(filter, updates);
      _logger.LogDebug("Completed {Operation} for ExperienceId={ExperienceId}.", nameof(DeleteAsync), experienceId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for ExperienceId={ExperienceId}.", nameof(DeleteAsync), experienceId);
      throw;
    }
  }

  public async Task<IList<Experience>> FetchAllAsync()
  {
    _logger.LogDebug("Started {Operation}.", nameof(FetchAllAsync));
    try
    {
      var filter = Builders<Experience>.Filter.Eq(e => e.IsDeleted, false);
      var sort = Builders<Experience>.Sort.Descending(e => e.StartDate);
      var result = await _experiences
        .Find(filter)
        .Sort(sort)
        .ToListAsync();
      _logger.LogDebug("Completed {Operation}. Count={Count}.", nameof(FetchAllAsync), result.Count);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(FetchAllAsync));
      throw;
    }
  }

  public async Task<IList<Experience>> FetchAllDeletedAsync()
  {
    _logger.LogDebug("Started {Operation}.", nameof(FetchAllDeletedAsync));
    try
    {
      var filter = Builders<Experience>.Filter.Eq(e => e.IsDeleted, true);
      var sort = Builders<Experience>.Sort.Descending(e => e.StartDate);
      var result = await _experiences
        .Find(filter)
        .Sort(sort)
        .ToListAsync();
      _logger.LogDebug("Completed {Operation}. Count={Count}.", nameof(FetchAllDeletedAsync), result.Count);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(FetchAllDeletedAsync));
      throw;
    }
  }

  public async Task<Experience?> FetchByCompanyNameAndJobTitle(string companyName, string jobTitle)
  {
    _logger.LogDebug("Started {Operation}.", nameof(FetchByCompanyNameAndJobTitle));
    try
    {
      var filterDefinitions = new List<FilterDefinition<Experience>>
      {
        Builders<Experience>.Filter.Eq(e => e.CompanyName, companyName),
        Builders<Experience>.Filter.Eq(e => e.JobTitle, jobTitle)
      };
      var filter = Builders<Experience>.Filter.And(filterDefinitions);

      var result = await _experiences
        .Find(filter)
        .FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation}. Found={Found}.", nameof(FetchByCompanyNameAndJobTitle), result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(FetchByCompanyNameAndJobTitle));
      throw;
    }
  }

  public async Task<Experience?> FetchByIdAsync(string experienceId)
  {
    _logger.LogDebug("Started {Operation} for ExperienceId={ExperienceId}.", nameof(FetchByIdAsync), experienceId);
    try
    {
      var filterDefinitions = new List<FilterDefinition<Experience>>
      {
        Builders<Experience>.Filter.Eq(e => e.Id, experienceId),
        Builders<Experience>.Filter.Eq(e => e.IsDeleted, false),
      };
      var filter = Builders<Experience>.Filter.And(filterDefinitions);
      var result = await _experiences
        .Find(filter)
        .FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation} for ExperienceId={ExperienceId}. Found={Found}.", nameof(FetchByIdAsync), experienceId, result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for ExperienceId={ExperienceId}.", nameof(FetchByIdAsync), experienceId);
      throw;
    }
  }

  public async Task<IList<Experience?>> FetchByIdsAsync(IList<string> experienceIds)
  {
    _logger.LogDebug("Started {Operation}. Count={Count}.", nameof(FetchByIdsAsync), experienceIds.Count);
    try
    {
      var experienceTasks = experienceIds.Select(FetchByIdAsync);
      var experiences = await Task.WhenAll(experienceTasks);
      var result = (IList<Experience?>)[.. experiences];
      _logger.LogDebug("Completed {Operation}. Count={Count}.", nameof(FetchByIdsAsync), result.Count);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(FetchByIdsAsync));
      throw;
    }
  }

  public async Task UpdateAsync(string experienceId, string serializedChanges)
  {
    _logger.LogDebug("Started {Operation} for ExperienceId={ExperienceId}.", nameof(UpdateAsync), experienceId);
    try
    {
      var changes = BsonDocument.Parse(serializedChanges);
      var filter = Builders<Experience>.Filter.Eq(e => e.Id, experienceId);
      var updates = MongoUpdateHelper.BuildUpdateDefinition<Experience>(changes);

      await _experiences.UpdateOneAsync(filter, updates);
      _logger.LogDebug("Completed {Operation} for ExperienceId={ExperienceId}.", nameof(UpdateAsync), experienceId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for ExperienceId={ExperienceId}.", nameof(UpdateAsync), experienceId);
      throw;
    }
  }
}
