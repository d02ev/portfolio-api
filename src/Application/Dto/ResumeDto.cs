using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Application.Dto;

public class ResumeDto
{
  [JsonProperty("experienceIds")]
  public List<string> ExperienceIds { get; set; } = [];

  [JsonProperty("projectIds")]
  public List<string> ProjectIds { get; set; } = [];

  [JsonProperty("techStackId")]
  public string TechStackId { get; set; } = string.Empty;

  [JsonProperty("contactId")]
  public string ContactId { get; set; } = string.Empty;

  [JsonProperty("educationId")]
  public string EducationId { get; set; } = string.Empty;

  [JsonProperty("name")]
  public string Name { get; set; } = string.Empty;
}

public class FetchResumeDto
{
  [JsonProperty("id")]
  public string Id { get; set; } = string.Empty;

  [JsonProperty("name")]
  public string Name { get; set; } = string.Empty;

  [JsonProperty("contact")]
  public FetchContactDto Contact { get; set; } = new FetchContactDto();

  [JsonProperty("education")]
  public FetchEducationDto Education { get; set; } = new FetchEducationDto();

  [JsonProperty("experience")]
  public List<FetchExperienceDto> Experience { get; set; } = [];

  [JsonProperty("projects")]
  public List<FetchProjectDto> Projects { get; set; } = [];

  [JsonProperty("techStack")]
  public FetchTechStackDto TechStack { get; set; } = new FetchTechStackDto();
}

public class UpdateResumeDto
{
  [JsonProperty("experienceIds")]
  public List<string>? ExperienceIds { get; set; } = null;

  [JsonProperty("projectIds")]
  public List<string>? ProjectIds { get; set; } = null;

  [JsonProperty("techStackId")]
  public string? TechStackId { get; set; } = null;

  [JsonProperty("contactId")]
  public string? ContactId { get; set; } = null;

  [JsonProperty("educationId")]
  public string? EducationId { get; set; } = null;

  [JsonProperty("name")]
  public string? Name { get; set; } = null;
}

public class GenerateResumeDto
{
  [JsonProperty("resumeData")]
  public FetchResumeDto ResumeData { get; set; } = new();

  [JsonProperty("templateId")]
  public string TemplateId { get; set; } = string.Empty;

  [JsonProperty("resumeName")]
  public string ResumeName { get; set; } = string.Empty;
}

public class GenerateResumeForJobDto
{
  [JsonProperty("resumeData")]
  public string ResumeData { get; set; } = string.Empty;

  [JsonProperty("templateId")]
  public string TemplateId { get; set; } = string.Empty;

  [JsonProperty("resumeName")]
  public string ResumeName { get; set; } = string.Empty;

  [JsonProperty("companyName")]
  public string CompanyName { get; set; } = string.Empty;

  [JsonProperty("jobDescription")]
  public IFormFile JobDescription { get; set; }
}