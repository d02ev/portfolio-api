using Application.Repositories;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class EducationRepository(IMongoDatabase database, ILogger<EducationRepository> logger) : IEducationRepository
{
  private readonly IMongoCollection<Education> _educations = database.GetCollection<Education>(CollectionNames.Education);
  private readonly ILogger<EducationRepository> _logger = logger;

  public async Task CreateAsync(Education education)
  {
    _logger.LogDebug("Started {Operation}.", nameof(CreateAsync));
    try
    {
      await _educations.InsertOneAsync(education);
      _logger.LogDebug("Completed {Operation}.", nameof(CreateAsync));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(CreateAsync));
      throw;
    }
  }

  public async Task<Education?> FetchAsync()
  {
    _logger.LogDebug("Started {Operation}.", nameof(FetchAsync));
    try
    {
      var filter = Builders<Education>.Filter.Empty;
      var result = await _educations.Find(filter).FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation}. Found={Found}.", nameof(FetchAsync), result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(FetchAsync));
      throw;
    }
  }

  public async Task<Education?> FetchByIdAsync(string educationId)
  {
    _logger.LogDebug("Started {Operation} for EducationId={EducationId}.", nameof(FetchByIdAsync), educationId);
    try
    {
      var filter = Builders<Education>.Filter.Eq(e => e.Id, educationId);
      var result = await _educations.Find(filter).FirstOrDefaultAsync();
      _logger.LogDebug("Completed {Operation} for EducationId={EducationId}. Found={Found}.", nameof(FetchByIdAsync), educationId, result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for EducationId={EducationId}.", nameof(FetchByIdAsync), educationId);
      throw;
    }
  }

  public async Task UpdateAsync(string educationId, string serializedChanges)
  {
    _logger.LogDebug("Started {Operation} for EducationId={EducationId}.", nameof(UpdateAsync), educationId);
    try
    {
      var changes = BsonDocument.Parse(serializedChanges);
      var filter = Builders<Education>.Filter.Eq(e => e.Id, educationId);
      var updates = MongoUpdateHelper.BuildUpdateDefinition<Education>(changes);

      await _educations.UpdateOneAsync(filter, updates);
      _logger.LogDebug("Completed {Operation} for EducationId={EducationId}.", nameof(UpdateAsync), educationId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation} for EducationId={EducationId}.", nameof(UpdateAsync), educationId);
      throw;
    }
  }
}
