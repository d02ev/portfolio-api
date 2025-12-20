using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class BaseEntity
{
  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  public string Id { get; set; } = string.Empty;

  [BsonElement("createdAt")]
  [BsonRepresentation(BsonType.DateTime)]
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  [BsonElement("updatedAt")]
  [BsonRepresentation(BsonType.DateTime)]
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}