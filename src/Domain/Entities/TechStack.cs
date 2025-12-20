using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class TechStack : BaseEntity
{
  [BsonElement("languages")]
  [BsonRequired]
  public List<string> Languages { get; set; } = [];

  [BsonElement("frameworks")]
  [BsonRequired]
  public List<string> Frameworks { get; set; } = [];

  [BsonElement("databases")]
  [BsonRequired]
  public List<string> Databases { get; set; } = [];

  [BsonElement("tools")]
  [BsonRequired]
  public List<string> Tools { get; set; } = [];

  [BsonElement("cloud")]
  [BsonRequired]
  public List<string> Cloud { get; set; } = [];

  [BsonElement("ai")]
  [BsonRequired]
  public List<string> Ai { get; set; } = [];
}