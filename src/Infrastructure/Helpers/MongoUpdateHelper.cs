using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Helpers;

public static class MongoUpdateHelper
{
  public static UpdateDefinition<T>? BuildUpdateDefinition<T>(BsonDocument changes, List<string>? excludeFields = null)
  {
    var updateDefinitions = new List<UpdateDefinition<T>>();
    excludeFields ??= ["Id", "CreatedAt"];

    foreach (var change in changes)
    {
      if (excludeFields.Contains(change.Name)) continue;
      updateDefinitions.Add(Builders<T>.Update.Set(change.Name, change.Value));
    }

    updateDefinitions.Add(Builders<T>.Update.Set("UpdatedAt", DateTime.UtcNow));

    return updateDefinitions.Count != 0
      ? Builders<T>.Update.Combine(updateDefinitions)
      : null;
  }
}