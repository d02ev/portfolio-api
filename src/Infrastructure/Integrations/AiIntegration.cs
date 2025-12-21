using System.ClientModel;
using System.Text.RegularExpressions;
using Application.Dto;
using Application.Integrations;
using Domain.Configurations;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
      Temperature = 0.0f,
    };
  }

  public async Task<FetchResumeDto> OptimiseForJobAsync(FetchResumeDto resumeData, string jobDescription)
  {
    var systemPrompt = @"
    Task: Rewrite values in the provided resume JSON so they align strongly with the supplied Job Description (JD), maximize ATS relevance, and strictly follow modern industry resume standards. Preserve the exact JSON structure and keys. Apply all optimization rules uniformly across every section of the resume JSON, including but not limited to experience, projects, skills, education, certifications, and links.
    Do NOT change metrics, numbers, dates, fabricate achievements, modify null-date fields, or add/remove keys (except _issues and score). Output only valid JSON.

    Allowed: rewrite bullet points for JD alignment; add JD-relevant keywords, skills, tools, and terminology; enhance clarity, technical strength, and impact while preserving factual meaning.

    Industry Resume Rules (STRICT): Use strong action verbs and technical language; each bullet must start with a verb; prioritize metric-first phrasing when metrics already exist; keep bullet points concise (typically 12–20 words); avoid paragraphs and filler words; avoid repetition; ensure clear outcome or impact per bullet; follow STAR-style structure implicitly where applicable; limit bullet counts per role to industry norms (do not over-expand); maintain professional, ATS-safe formatting.

    ATS Scoring: After optimization, add a numeric field named 'score' (0–100) representing the resume's alignment accuracy with the JD.

    Issues Handling: If any field is unclear, missing, or malformed, include it in a top-level _issues array(empty if none).
    ";
    var userPrompt = JsonConvert.SerializeObject(new
    {
      jd = JsonConvert.SerializeObject(jobDescription),
      resume = JsonConvert.SerializeObject(resumeData)
    });
    var messages = new List<ChatMessage>()
    {
      new SystemChatMessage(systemPrompt),
      new UserChatMessage(userPrompt)
    };
    var response = await _client.CompleteChatAsync(messages, _requestOptions);
    var jdOptimisedResumeData = EscapePercentHashInJsonString(CleanResponse(response.Value.Content[0].Text));

    return JsonConvert.DeserializeObject<FetchResumeDto>(jdOptimisedResumeData)!;
  }

  public async Task<FetchResumeDto> OptimiseGenericAsync(FetchResumeDto resumeData)
  {
    var systemPrompt = @"
    Task: Rewrite values in the provided resume JSON to improve grammar, clarity, technical impact, and ATS strength while strictly following modern resume-writing standards. Preserve exact JSON structure, keys, and factual meaning.
    Apply all optimization rules uniformly across every section of the resume JSON, including but not limited to experience, projects, skills, education, certifications, and links.
    Do NOT change metrics, numbers, or dates; do NOT fabricate achievements; do NOT modify fields with null dates; do NOT add or remove keys (except _issues). Output only valid JSON.

    Allowed: rewrite bullet points for clarity and impact; add relevant technical and domain keywords; strengthen action verbs and phrasing.

    Industry Resume Rules (STRICT): each bullet must start with a strong action verb; keep bullets concise and impact-focused (typically 12–20 words); prefer metric-first phrasing when metrics already exist; avoid paragraphs, filler words, repetition, and passive voice; maintain professional, ATS-safe language and formatting; do not over-expand bullet counts beyond industry norms.

    Issues Handling: if any field is missing, unclear, or malformed, include it in a top-level _issues array (empty if none).
    ";
    var userPrompt = JsonConvert.SerializeObject(resumeData);
    var messages = new List<ChatMessage>()
    {
      new SystemChatMessage(systemPrompt),
      new UserChatMessage(userPrompt)
    };
    var response = await _client.CompleteChatAsync(messages, _requestOptions);
    var optimisedResumeData = EscapePercentHashInJsonString(CleanResponse(response.Value.Content[0].Text));

    return JsonConvert.DeserializeObject<FetchResumeDto>(optimisedResumeData)!;
  }

  private static string CleanResponse(string aiResponse)
  {
    if (string.IsNullOrEmpty(aiResponse))
    {
      return aiResponse;
    }

    string cleaned = aiResponse;

    cleaned = Regex.Replace(cleaned, @"^```json\s*", "", RegexOptions.Multiline);
    cleaned = Regex.Replace(cleaned, @"```\s*$", "", RegexOptions.Multiline);
    cleaned = Regex.Replace(cleaned, @"```", "");
    cleaned = cleaned.Trim();

    return cleaned;
  }

  private static string EscapePercentHashInJsonString(string stringifiedJson)
  {
    if (string.IsNullOrEmpty(stringifiedJson))
    {
      return stringifiedJson;
    }

    JToken token = JToken.Parse(stringifiedJson);

    EscapePercentHashInToken(token);

    var escapedJson = token.ToString(Formatting.None);
    return escapedJson;
  }

  private static void EscapePercentHashInToken(JToken token)
  {
    if (token == null) return;

    switch(token.Type)
    {
      case JTokenType.String:
        var stringValue = token.Value<string>();
        if (!string.IsNullOrEmpty(stringValue) && (stringValue.Contains("%") || stringValue.Contains("#")))
        {
          var escaped = stringValue
            .Replace("%", "\\%")
            .Replace("#", "\\#");

          if (token is JValue jValue)
          {
            jValue.Value = escaped;
          }
        }
        break;
      case JTokenType.Object:
        foreach (var property in ((JObject)token).Properties())
        {
          EscapePercentHashInToken(property.Value);
        }
        break;
      case JTokenType.Array:
        foreach(var item in (JArray)token)
        {
          EscapePercentHashInToken(item);
        }
        break;
      default:
        break;
    }
  }
}