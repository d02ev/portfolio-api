using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class Education : BaseEntity
{
  [BsonElement("institute")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Institute { get; set; } = string.Empty;

  [BsonElement("startDate")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string StartDate { get; set; } = string.Empty;

  [BsonElement("endDate")]
  [BsonRepresentation(BsonType.String)]
  public string EndDate { get; set; } = string.Empty;

  [BsonElement("degree")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Degree { get; set; } = string.Empty;

  [BsonElement("grade")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Grade { get; set; } = string.Empty;

  [BsonElement("coursework")]
  [BsonRequired]
  public List<string> Coursework { get; set; } = [];
}