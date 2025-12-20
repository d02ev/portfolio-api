using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Domain.Entities;

public class User : BaseEntity
{
  [BsonElement("username")]
  [BsonRequired]
  [BsonRepresentation(BsonType.String)]
  public string Username { get; set; } = string.Empty;

  [BsonElement("passwordHash")]
  [BsonRequired]
  [BsonRepresentation(BsonType.String)]
  public string PasswordHash { get; set; } = string.Empty;

  [BsonElement("refreshToken")]
  [BsonRepresentation(BsonType.String)]
  public string? RefreshToken { get; set; } = null;

  [BsonElement("role")]
  [BsonRepresentation(BsonType.String)]
  public string Role { get; set; } = "user";
}