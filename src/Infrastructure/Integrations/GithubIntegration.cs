using Octokit;
using Application.Integrations;
using Domain.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Domain.Enums;

namespace Infrastructure.Integrations;

public class GithubIntegration : IGithubIntegration
{
  private readonly GithubSettings _githubSettings;
  private readonly GitHubClient _client;
  private readonly ILogger<GithubIntegration> _logger;

  public GithubIntegration(IOptions<GithubSettings> options, ILogger<GithubIntegration> logger)
  {
    _logger = logger;
    _githubSettings = options.Value;
    _client = new GitHubClient(new ProductHeaderValue(_githubSettings.Owner))
    {
      Credentials = new Credentials(_githubSettings.PersonalAccessToken)
    };
  }

  public async Task DeleteFromRepositoryAsync(string filePath)
  {
    _logger.LogInformation("Started {Operation}. FilePath={FilePath}.", nameof(DeleteFromRepositoryAsync), filePath);
    try
    {
      var reference = await _client.Git.Reference.Get(_githubSettings.Owner, _githubSettings.Repo, $"heads/master");
      var currentCommit = await _client.Git.Commit.Get(_githubSettings.Owner, _githubSettings.Repo, reference.Object.Sha);
      var tree = new NewTree
      {
        BaseTree = currentCommit.Tree.Sha,
      };

      tree.Tree.Add(new NewTreeItem
      {
        Path = filePath,
        Mode = "100644",
        Type = TreeType.Blob,
        Sha = null
      });

      var treeResult = await _client.Git.Tree.Create(_githubSettings.Owner, _githubSettings.Repo, tree);
      var commit = new NewCommit($"delete {filePath}", treeResult.Sha, reference.Object.Sha);
      var commitResult = await _client.Git.Commit.Create(_githubSettings.Owner, _githubSettings.Repo, commit);

      await _client.Git.Reference.Update(_githubSettings.Owner, _githubSettings.Repo, $"heads/master", new ReferenceUpdate(commitResult.Sha));
      _logger.LogInformation("Completed {Operation}. FilePath={FilePath}.", nameof(DeleteFromRepositoryAsync), filePath);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}. FilePath={FilePath}.", nameof(DeleteFromRepositoryAsync), filePath);
      throw;
    }
  }

  public async Task InitWorkflowAsync(string recordId, string compiledFileName, string pushedFileName)
  {
    _logger.LogInformation("Started {Operation}. RecordId={RecordId}.", nameof(InitWorkflowAsync), recordId);
    try
    {
      var workflowDispatch = new CreateWorkflowDispatch("master")
      {
        Inputs = new Dictionary<string, object>
        {
          ["record_id"] = recordId,
          ["resume_name"] = compiledFileName,
          ["tex_file_path"] = $"docs/{pushedFileName}.tex",
        },
      };

      await _client.Actions.Workflows.CreateDispatch(_githubSettings.Owner, _githubSettings.Repo, "compile.yml", workflowDispatch);
      _logger.LogInformation("Completed {Operation}. RecordId={RecordId}.", nameof(InitWorkflowAsync), recordId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}. RecordId={RecordId}.", nameof(InitWorkflowAsync), recordId);
      throw;
    }
  }

  public async Task PushToRepositoryAsync(string filePath, string fileContent)
  {
    _logger.LogInformation("Started {Operation}. FilePath={FilePath}.", nameof(PushToRepositoryAsync), filePath);
    try
    {
      var reference = await _client.Git.Reference.Get(_githubSettings.Owner, _githubSettings.Repo, $"heads/master");
      var currentCommit = await _client.Git.Commit.Get(_githubSettings.Owner, _githubSettings.Repo, reference.Object.Sha);
      var blob = new NewBlob
      {
          Content = fileContent,
          Encoding = EncodingType.Utf8
      };
      var blobResult = await _client.Git.Blob.Create(_githubSettings.Owner, _githubSettings.Repo, blob);
      var tree = new NewTree
      {
          BaseTree = currentCommit.Tree.Sha
      };

      tree.Tree.Add(new NewTreeItem
      {
          Path = filePath,
          Mode = "100644",
          Type = TreeType.Blob,
          Sha = blobResult.Sha
      });

      var treeResult = await _client.Git.Tree.Create(_githubSettings.Owner, _githubSettings.Repo, tree);
      var commit = new NewCommit($"add/update {filePath}", treeResult.Sha, reference.Object.Sha);
      var commitResult = await _client.Git.Commit.Create(_githubSettings.Owner, _githubSettings.Repo, commit);

      await _client.Git.Reference.Update(_githubSettings.Owner, _githubSettings.Repo, $"heads/master", new ReferenceUpdate(commitResult.Sha));
      _logger.LogInformation("Completed {Operation}. FilePath={FilePath}.", nameof(PushToRepositoryAsync), filePath);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}. FilePath={FilePath}.", nameof(PushToRepositoryAsync), filePath);
      throw;
    }
  }
}
