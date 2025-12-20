using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class Project : BaseEntity
{
  [BsonElement("displayName")]
  [BsonRepresentation(BsonType.String)]
  public string DisplayName { get; set; } = string.Empty;

  [BsonElement("shortDescription")]
  [BsonRepresentation(BsonType.String)]
  public string ShortDescription { get; set; } = string.Empty;

  [BsonElement("longDescription")]
  [BsonRepresentation(BsonType.String)]
  public string? LongDescription { get; set; } = null;

  [BsonElement("techStack")]
  public List<string> TechStack { get; set; } = [];

  [BsonElement("repoUrl")]
  [BsonRepresentation(BsonType.String)]
  public string RepoUrl { get; set; } = string.Empty;

  [BsonElement("liveUrl")]
  [BsonRepresentation(BsonType.String)]
  public string? LiveUrl { get; set; } = null;

  [BsonElement("sorter")]
  [BsonRepresentation(BsonType.Int32)]
  public int Sorter { get; set; } = 1;

  [BsonElement("isDeleted")]
  [BsonRepresentation(BsonType.Boolean)]
  public bool IsDeleted { get; set; } = false;

  [BsonElement("deletedAt")]
  [BsonRepresentation(BsonType.DateTime)]
  public DateTime? DeletedAt { get; set; } = null;
}