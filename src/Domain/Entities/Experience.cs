using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class Experience : BaseEntity
{
  [BsonElement("jobTitle")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string JobTitle { get; set; } = string.Empty;

  [BsonElement("companyName")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string CompanyName { get; set; } = string.Empty;

  [BsonElement("location")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Location { get; set; } = string.Empty;

  [BsonElement("startDate")]
  [BsonRepresentation(BsonType.DateTime)]
  [BsonRequired]
  [BsonIgnoreIfDefault]
  public DateTime StartDate { get; set; }

  [BsonElement("endDate")]
  [BsonRepresentation(BsonType.DateTime)]
  [BsonIgnoreIfDefault]
  public DateTime? EndDate { get; set; } = null;

  [BsonElement("description")]
  [BsonRequired]
  public List<string> Description { get; set; } = [];

  [BsonElement("isDeleted")]
  [BsonRepresentation(BsonType.Boolean)]
  public bool IsDeleted { get; set; } = false;

  [BsonElement("deletedAt")]
  [BsonRepresentation(BsonType.DateTime)]
  public DateTime? DeletedAt { get; set; } = null;
}