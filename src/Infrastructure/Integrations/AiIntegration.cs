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

  public async Task<FetchResumeDto> OptimiseForJobAsync(FetchResumeDto resumeData, List<FetchProjectDto> projects, string jobDescription)
  {
    var systemPrompt = @"
    You are an AI resume optimization engine designed to rewrite resume content for ATS compliance and recruiter readability.

    Task: Rewrite values in the provided resume JSON so they align strongly with the supplied Job Description (JD), maximize ATS relevance, and strictly follow modern industry resume-writing standards for a software engineer with 3–5 years of experience. Apply all optimization rules uniformly across every section of the resume JSON, including experience, projects, skills, education, certifications, and links.

    Preserve exact JSON structure, keys, and factual meaning. Do NOT fabricate achievements. Do NOT change metrics, numbers, or dates. Do NOT modify fields with null dates. Do NOT add or remove keys (except _issues and score). Output only valid JSON.

    Experience Optimization Rules (STRICT): rewrite, reorder, and selectively remove bullet points within each experience role to maximize alignment with the JD; retain only the most JD-relevant bullets (typically 3–5 per role); prioritize bullets demonstrating direct skill, technology, or responsibility matches with the JD; deprioritize or remove generic, redundant, or weakly aligned bullets; ensure every retained bullet starts with a strong action verb and focuses on measurable or outcome-driven impact.

    Project Selection Rules (STRICT): you will be provided a list of candidate projects; include at most 2 projects that most strongly align with the JD based on technologies, problem domain, and responsibilities; rewrite selected project bullets for ATS strength and JD relevance; remove or ignore projects with weak or no alignment; do NOT invent new projects.

    Allowed: rewrite bullet points for clarity, impact, and JD alignment; add JD-relevant technical keywords, tools, and terminology without keyword stuffing; strengthen action verbs while preserving factual meaning.

    Industry Content Rules (STRICT): each bullet must start with a strong action verb; focus on achievements and outcomes rather than responsibilities; prefer metric-first phrasing when metrics already exist; keep bullets concise (typically 12–20 words); limit bullets per experience role to 3–5; limit bullets per project to 2–4; avoid filler words, repetition, passive voice, and vague language; maintain consistent tense (past for past roles, present for current roles).

    ATS Scoring: after optimization, add a numeric field named 'score' (0–100) representing the estimated ATS alignment accuracy with the JD.

    Issues Handling: if any field is unclear, missing, or malformed, include it in a top-level _issues array (empty if none).";
    var userPrompt = JsonConvert.SerializeObject(new
    {
      jd = JsonConvert.SerializeObject(jobDescription),
      resume = JsonConvert.SerializeObject(resumeData),
      projects_pool = JsonConvert.SerializeObject(projects)
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
    You are an AI resume optimization engine designed to rewrite resume content for ATS compliance and recruiter readability.

    Task: Rewrite values in the provided resume JSON to improve grammar, clarity, technical impact, and ATS strength while strictly following modern industry resume-writing standards for a software engineer with 3–5 years of experience. Preserve exact JSON structure, keys, and factual meaning. Apply all optimization rules uniformly across every section of the resume JSON, including but not limited to experience, projects, skills, education, certifications, and links.

    Do NOT change metrics, numbers, or dates; do NOT fabricate achievements; do NOT modify fields with null dates; do NOT add or remove keys (except _issues). Output only valid JSON.

    Allowed: rewrite bullet points for impact and clarity; add relevant technical and domain keywords; strengthen action verbs and phrasing while preserving meaning.

    Industry Content Rules (STRICT): each bullet must start with a strong action verb (e.g., Developed, Implemented, Optimized, Designed, Automated, Integrated, Deployed); focus on achievements and outcomes, not responsibilities; prefer metric-first phrasing when metrics already exist; keep bullet points concise (typically 12–20 words); limit bullets per experience role to 3–5; limit bullets per project to 2–4; avoid filler words, repetition, passive voice, and vague language; maintain consistent tense (past for past roles, present for current roles).

    Issues Handling: if any field is missing, unclear, or malformed, include it in a top-level _issues array (empty if none).";
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

    switch (token.Type)
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
        foreach (var item in (JArray)token)
        {
          EscapePercentHashInToken(item);
        }
        break;
      default:
        break;
    }
  }
}