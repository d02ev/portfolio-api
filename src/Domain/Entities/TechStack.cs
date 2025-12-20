using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class TechStack : BaseEntity
{
  [BsonElement("languages")]
  [BsonRequired]
  public List<string> Languages { get; set; } = [];

  [BsonElement("frameworksAndPlatforms")]
  [BsonRequired]
  public List<string> FrameworksAndPlatforms { get; set; } = [];

  [BsonElement("databases")]
  [BsonRequired]
  public List<string> Databases { get; set; } = [];

  [BsonElement("cloudAndDevOps")]
  [BsonRequired]
  public List<string> CloudAndDevOps { get; set; } = [];

  [BsonElement("others")]
  [BsonRequired]
  public List<string> Others { get; set; } = [];
}