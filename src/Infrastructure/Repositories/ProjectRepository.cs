using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
  private readonly IMongoCollection<Project> _projects;
  private readonly ILogger<ProjectRepository> _logger;

  public ProjectRepository(IMongoDatabase database, ILogger<ProjectRepository> logger)
  {
    _logger = logger;
    _projects = database.GetCollection<Project>(CollectionNames.Project);
    var idxGeneral = Builders<Project>.IndexKeys
      .Ascending(p => p.IsDeleted)
      .Descending(p => p.Sorter);
    var optsIdxGeneral = new CreateIndexOptions<Project>
    {
      Background = true,
    };
    var idxUnique = Builders<Project>.IndexKeys
      .Ascending(p => p.DisplayName);
    var optsIdxUnique = new CreateIndexOptions<Project>
    {
      Unique = true,
      Background = true,
      PartialFilterExpression = Builders<Project>.Filter.Eq(p => p.IsDeleted, false)
    };
    var idxModels = new List<CreateIndexModel<Project>>
    {
      new (idxGeneral, optsIdxGeneral),
      new (idxUnique, optsIdxUnique)
    };

    _projects.Indexes.CreateMany(idxModels);
    _logger.LogDebug("Created indexes for {Repository}.", nameof(ProjectRepository));
  }

  public async Task CreateAsync(Project project)
  {
    _logger.LogDebug("Started {Operation}.", nameof(CreateAsync));
    try
    {
      await _projects.InsertOneAsync(project);
      _logger.LogDebug("Completed {Operation}.", nameof(CreateAsync));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(CreateAsync));
      throw;
    }
  }

  public async Task DeleteAsync(string projectId)
  {
    _logger.LogDebug("Started {Operation} for ProjectId={ProjectId}.", nameof(DeleteAsync), projectId);
    try
    {
      var filter = Builders<Project>.Filter.Eq(p => p.Id, projectId);
      var updates = Builders<Project>.Update
        .Set(p => p.IsDeleted, true)
        .Set(p => p.DeletedAt, DateTime.UtcNow);

      await _projects.UpdateOneAsync(filter, updates);
      _logger.LogDebug("Completed {Operation} for ProjectId={ProjectId}.", nameof(DeleteAsync), projectId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for ProjectId={ProjectId}.", nameof(DeleteAsync), projectId);
      throw;
    }
  }

  public async Task<IList<Project>> FetchAllAsync()
  {
    _logger.LogDebug("Started {Operation}.", nameof(FetchAllAsync));
    try
    {
      var filter = Builders<Project>.Filter.Eq(p => p.IsDeleted, false);
      var sort = Builders<Project>.Sort.Descending(p => p.Sorter);
      var result = await _projects
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

  public async Task<IList<Project>> FetchAllDeletedAsync()
  {
    _logger.LogDebug("Started {Operation}.", nameof(FetchAllDeletedAsync));
    try
    {
      var filter = Builders<Project>.Filter.Eq(p => p.IsDeleted, true);
      var sort = Builders<Project>.Sort.Descending(p => p.Sorter);
      var result = await _projects
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

  public async Task<Project?> FetchByDisplayName(string displayName)
  {
    _logger.LogDebug("Started {Operation}.", nameof(FetchByDisplayName));
    try
    {
      var filterDefinitions = new List<FilterDefinition<Project>>
      {
        Builders<Project>.Filter.Eq(p => p.DisplayName, displayName),
        Builders<Project>.Filter.Eq(p => p.IsDeleted, false),
      };
      var filter = Builders<Project>.Filter.And(filterDefinitions);
      var result = await _projects
        .Find(filter)
        .FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation}. Found={Found}.", nameof(FetchByDisplayName), result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(FetchByDisplayName));
      throw;
    }
  }

  public async Task<Project?> FetchByIdAsync(string projectId)
  {
    _logger.LogDebug("Started {Operation} for ProjectId={ProjectId}.", nameof(FetchByIdAsync), projectId);
    try
    {
      var filterDefinitions = new List<FilterDefinition<Project>>
      {
        Builders<Project>.Filter.Eq(p => p.Id, projectId),
        Builders<Project>.Filter.Eq(p => p.IsDeleted, false),
      };
      var filter = Builders<Project>.Filter.And(filterDefinitions);
      var result = await _projects
        .Find(filter)
        .FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation} for ProjectId={ProjectId}. Found={Found}.", nameof(FetchByIdAsync), projectId, result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for ProjectId={ProjectId}.", nameof(FetchByIdAsync), projectId);
      throw;
    }
  }

  public async Task<IList<Project?>> FetchByIdsAsync(IList<string> projectIds)
  {
    _logger.LogDebug("Started {Operation}. Count={Count}.", nameof(FetchByIdsAsync), projectIds.Count);
    try
    {
      var projectTasks = projectIds.Select(FetchByIdAsync);
      var projects = await Task.WhenAll(projectTasks);
      var result = (IList<Project?>)[.. projects];
      _logger.LogDebug("Completed {Operation}. Count={Count}.", nameof(FetchByIdsAsync), result.Count);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(FetchByIdsAsync));
      throw;
    }
  }

  public async Task UpdateAsync(string projectId, string serializedChanges)
  {
    _logger.LogDebug("Started {Operation} for ProjectId={ProjectId}.", nameof(UpdateAsync), projectId);
    try
    {
      var changes = BsonDocument.Parse(serializedChanges);
      var filter = Builders<Project>.Filter.Eq(p => p.Id, projectId);
      var updates = MongoUpdateHelper.BuildUpdateDefinition<Project>(changes);

      await _projects.UpdateOneAsync(filter, updates);
      _logger.LogDebug("Completed {Operation} for ProjectId={ProjectId}.", nameof(UpdateAsync), projectId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for ProjectId={ProjectId}.", nameof(UpdateAsync), projectId);
      throw;
    }
  }
}
