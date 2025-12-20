using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class About : BaseEntity
{
  [BsonElement("techStackId")]
  [BsonRepresentation(BsonType.ObjectId)]
  [BsonRequired]
  public string TechStackId { get; set; } = string.Empty;

  [BsonElement("bio")]
  [BsonRequired]
  public Bio Bio { get; set; } = new Bio();

  [BsonElement("funFact")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string FunFact { get; set; } = string.Empty;
}

public class Bio
{
  [BsonElement("name")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Name { get; set; } = string.Empty;

  [BsonElement("intro")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Intro { get; set; } = string.Empty;

  [BsonElement("experience")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Experience { get; set; } = string.Empty;

  [BsonElement("company")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Company { get; set; } = string.Empty;

  [BsonElement("highlights")]
  [BsonRequired]
  public List<BioHighlight> Highlights { get; set; } = [];
}

public class BioHighlight
{
  [BsonElement("icon")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Icon { get; set; } = string.Empty;

  [BsonElement("text")]
  [BsonRepresentation(BsonType.String)]
  [BsonRequired]
  public string Text { get; set; } = string.Empty;

  [BsonElement("highlight")]
  [BsonRepresentation(BsonType.String)]
  public string? Highlight { get; set; } = null;
}