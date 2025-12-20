namespace Domain.Configurations;

public class GithubSettings
{
  public string PersonalAccessToken { get; set; } = string.Empty;

  public string Owner { get; set; } = string.Empty;

  public string Repo { get; set; } = string.Empty;

  public string WorkflowFilePath { get; set; } = string.Empty;
}