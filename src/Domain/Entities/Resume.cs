using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class Resume : BaseEntity
{
  [BsonElement("experienceIds")]
  [BsonRepresentation(BsonType.Array)]
  [BsonRequired]
  public List<string> ExperienceIds { get; set; } = [];

  [BsonElement("projectIds")]
  [BsonRepresentation(BsonType.Array)]
  [BsonRequired]
  public List<string> ProjectIds { get; set; } = [];

  [BsonElement("techStackId")]
  [BsonRepresentation(BsonType.ObjectId)]
  [BsonRequired]
  public string TechStackId { get; set; } = string.Empty;

  [BsonElement("contactId")]
  [BsonRepresentation(BsonType.ObjectId)]
  [BsonRequired]
  public string ContactId { get; set; } = string.Empty;

  [BsonElement("educationId")]
  [BsonRepresentation(BsonType.ObjectId)]
  [BsonRequired]
  public string EducationId { get; set; } = string.Empty;

  [BsonElement("name")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Name { get; set; } = string.Empty;
}
