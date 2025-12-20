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

  [JsonProperty("name")]
  public string Name { get; set; } = string.Empty;

  [JsonProperty("contact")]
  public ContactDto Contact { get; set; } = new ContactDto();

  [JsonProperty("education")]
  public EducationDto Education { get; set; } = new EducationDto();
}

public class ContactDto
{
  [JsonProperty("email")]
  public string Email { get; set; } = string.Empty;

  [JsonProperty("mobile")]
  public string Mobile { get; set; } = string.Empty;

  [JsonProperty("github")]
  public string Github { get; set; } = string.Empty;

  [JsonProperty("linkedin")]
  public string Linkedin { get; set; } = string.Empty;

  [JsonProperty("website")]
  public string Website { get; set; } = string.Empty;
}

public class EducationDto
{
  [JsonProperty("institute")]
  public string Institute { get; set; } = string.Empty;

  [JsonProperty("startDate")]
  public string StartDate { get; set; } = string.Empty;

  [JsonProperty("endDate")]
  public string EndDate { get; set; } = string.Empty;

  [JsonProperty("degree")]
  public string Degree { get; set; } = string.Empty;

  [JsonProperty("grade")]
  public string Grade { get; set; } = string.Empty;

  [JsonProperty("coursework")]
  public List<string> Coursework { get; set; } = [];
}

public class FetchResumeDto
{
  [JsonProperty("id")]
  public string Id { get; set; } = string.Empty;

  [JsonProperty("name")]
  public string Name { get; set; } = string.Empty;

  [JsonProperty("contact")]
  public ContactDto Contact { get; set; } = new ContactDto();

  [JsonProperty("education")]
  public EducationDto Education { get; set; } = new EducationDto();

  [JsonProperty("experience")]
  public List<FetchExperienceDto> Experience { get; set; } = [];

  [JsonProperty("projects")]
  public List<FetchProjectDto> Projects { get; set; } = [];

  [JsonProperty("techStack")]
  public ResumeTechStackDto TechStack { get; set; } = new ResumeTechStackDto();
}

public class UpdateResumeDto
{
  [JsonProperty("experienceIds")]
  public List<string>? ExperienceIds { get; set; } = null;

  [JsonProperty("projectIds")]
  public List<string>? ProjectIds { get; set; } = null;

  [JsonProperty("techStackId")]
  public string? TechStackId { get; set; } = null;

  [JsonProperty("name")]
  public string? Name { get; set; } = null;

  [JsonProperty("contact")]
  public UpdateContactDto? Contact { get; set; } = null;

  [JsonProperty("education")]
  public UpdateEducationDto? Education { get; set; } = null;
}

public class UpdateContactDto
{
  [JsonProperty("email")]
  public string? Email { get; set; } = null;

  [JsonProperty("mobile")]
  public string? Mobile { get; set; } = null;

  [JsonProperty("github")]
  public string? Github { get; set; } = null;

  [JsonProperty("linkedin")]
  public string? Linkedin { get; set; } = null;

  [JsonProperty("website")]
  public string? Website { get; set; } = null;
}

public class UpdateEducationDto
{
  [JsonProperty("institute")]
  public string? Institute { get; set; } = null;

  [JsonProperty("startDate")]
  public string? StartDate { get; set; } = null;

  [JsonProperty("endDate")]
  public string? EndDate { get; set; } = null;

  [JsonProperty("degree")]
  public string? Degree { get; set; } = null;

  [JsonProperty("grade")]
  public string? Grade { get; set; } = null;

  [JsonProperty("coursework")]
  public List<string>? Coursework { get; set; } = null;
}

public class ResumeTechStackDto
{
  [JsonProperty("id")]
  public string Id { get; set; } = string.Empty;

  [JsonProperty("languages")]
  public List<string> Languages { get; set; } = [];

  [JsonProperty("techAndTools")]
  public List<string> TechAndTools { get; set; } = [];
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