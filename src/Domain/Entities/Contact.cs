using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class Contact : BaseEntity
{
  [BsonElement("email")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Email { get; set; } = string.Empty;

  [BsonElement("mobile")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Mobile { get; set; } = string.Empty;

  [BsonElement("github")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Github { get; set; } = string.Empty;

  [BsonElement("linkedin")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Linkedin { get; set; } = string.Empty;

  [BsonElement("website")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Website { get; set; } = string.Empty;
}