using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
  private readonly IMongoCollection<Project> _projects;

  public ProjectRepository(IMongoDatabase database)
  {
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
  }

  public async Task CreateAsync(Project project)
  {
    await _projects.InsertOneAsync(project);
  }

  public async Task DeleteAsync(string projectId)
  {
    var filter = Builders<Project>.Filter.Eq(p => p.Id, projectId);
    var updates = Builders<Project>.Update
      .Set(p => p.IsDeleted, true)
      .Set(p => p.DeletedAt, DateTime.UtcNow);

    await _projects.UpdateOneAsync(filter, updates);
  }

  public async Task<IList<Project>> FetchAllAsync()
  {
    var filter = Builders<Project>.Filter.Eq(p => p.IsDeleted, false);
    var sort = Builders<Project>.Sort.Descending(p => p.Sorter);
    return await _projects
      .Find(filter)
      .Sort(sort)
      .ToListAsync();
  }

  public async Task<IList<Project>> FetchAllDeletedAsync()
  {
    var filter = Builders<Project>.Filter.Eq(p => p.IsDeleted, true);
    var sort = Builders<Project>.Sort.Descending(p => p.Sorter);
    return await _projects
      .Find(filter)
      .Sort(sort)
      .ToListAsync();
  }

  public async Task<Project?> FetchByDisplayName(string displayName)
  {
    var filterDefinitions = new List<FilterDefinition<Project>>
    {
      Builders<Project>.Filter.Eq(p => p.DisplayName, displayName),
      Builders<Project>.Filter.Eq(p => p.IsDeleted, false),
    };
    var filter = Builders<Project>.Filter.And(filterDefinitions);
    return await _projects
      .Find(filter)
      .FirstOrDefaultAsync();
  }

  public async Task<Project?> FetchByIdAsync(string projectId)
  {
    var filterDefinitions = new List<FilterDefinition<Project>>
    {
      Builders<Project>.Filter.Eq(p => p.Id, projectId),
      Builders<Project>.Filter.Eq(p => p.IsDeleted, false),
    };
    var filter = Builders<Project>.Filter.And(filterDefinitions);
    return await _projects
      .Find(filter)
      .FirstOrDefaultAsync();
  }

  public async Task<IList<Project?>> FetchByIdsAsync(IList<string> projectIds)
  {
    var projectTasks = projectIds.Select(FetchByIdAsync);
    var projects = await Task.WhenAll(projectTasks);

    return [.. projects];
  }

  public async Task UpdateAsync(string projectId, string serializedChanges)
  {
    var changes = BsonDocument.Parse(serializedChanges);
    var filter = Builders<Project>.Filter.Eq(p => p.Id, projectId);
    var updates = MongoUpdateHelper.BuildUpdateDefinition<Project>(changes);

    await _projects.UpdateOneAsync(filter, updates);
  }
}