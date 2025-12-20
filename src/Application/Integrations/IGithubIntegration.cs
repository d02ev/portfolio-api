using Domain.Enums;

namespace Application.Integrations;

public interface IGithubIntegration
{
  Task PushToRepositoryAsync(string filePath, string fileContent);

  Task DeleteFromRepositoryAsync(string filePath);

  Task InitWorkflowAsync(string recordId, string compiledFileName, string pushedFileName);
}