using System.ClientModel;
using Application.Dto;
using Application.Integrations;
using Domain.Configurations;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;

namespace Infrastructure.Integrations;

public class AiIntegration : IAiIntegration
{
  private readonly AiSettings _aiSettings;
  private readonly ChatClient _client;
  private readonly ChatCompletionOptions _requestOptions;

  public AiIntegration(IOptions<AiSettings> options)
  {
    _aiSettings = options.Value;
    _client = new ChatClient(_aiSettings.Model, new ApiKeyCredential(_aiSettings.PersonalAccessToken), new OpenAIClientOptions
    {
      Endpoint = new Uri(_aiSettings.Url)
    });
    _requestOptions = new ChatCompletionOptions
    {
      Temperature = 1.0f,
    };
  }

  public Task<FetchResumeDto> OptimiseForJobAsync(FetchResumeDto resumeData, string jobDescription)
  {
    throw new NotImplementedException();
  }

  public async Task<FetchResumeDto> OptimiseGenericAsync(FetchResumeDto resumeData)
  {
    string systemPrompt = @"
    Task: Rewrite values in the provided resume JSON to improve grammar,
    clarity, technical impact, action verbs, and ATS strength. Keep exact JSON
    structure and keys. Do NOT change metrics, numbers, dates, fabricate achievements,
    modify fields with null dates, or add/remove keys. Output only valid JSON.
    Allowed: add relevant keywords, rewrite bullets, and enhance clarity while keeping
    meaning. If issues: include a top-level _issues array listing them (empty if none).
    ";
    string userPrompt = JsonConvert.SerializeObject(resumeData);
    var messages = new List<ChatMessage> ()
    {
      new SystemChatMessage(systemPrompt),
      new UserChatMessage(userPrompt)
    };

    var response = await _client.CompleteChatAsync(messages, _requestOptions);
    var optimisedResumeData = response.Value.Content[0].Text;

    return JsonConvert.DeserializeObject<FetchResumeDto>(optimisedResumeData)!;
  }
}