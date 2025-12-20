using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class Resume : BaseEntity
{
  [BsonElement("experienceIds")]
  [BsonRequired]
  public List<string> ExperienceIds { get; set; } = [];

  [BsonElement("projectIds")]
  [BsonRequired]
  public List<string> ProjectIds { get; set; } = [];

  [BsonElement("techStackId")]
  [BsonRepresentation(BsonType.ObjectId)]
  [BsonRequired]
  public string TechStackId { get; set; } = string.Empty;

  [BsonElement("name")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Name { get; set; } = string.Empty;

  [BsonElement("contact")]
  [BsonRequired]
  public Contact Contact { get; set; } = new Contact();

  [BsonElement("education")]
  [BsonRequired]
  public Education Education { get; set; } = new Education();
}

public class Contact
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

public class Education
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