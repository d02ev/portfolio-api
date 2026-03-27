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
using Microsoft.Extensions.Logging;

namespace Infrastructure.Integrations;

public class AiIntegration : IAiIntegration
{
  private const int DefaultGenericOptimisationPasses = 3;
  private const int DefaultJobOptimisationPasses = 3;

  private readonly AiSettings _aiSettings;
  private readonly ChatClient _client;
  private readonly ChatCompletionOptions _requestOptions;
  private readonly ILogger<AiIntegration> _logger;

  public AiIntegration(IOptions<AiSettings> options, ILogger<AiIntegration> logger)
  {
    _logger = logger;
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
    _logger.LogInformation("Started {Operation}. ProjectCount={ProjectCount}.", nameof(OptimiseForJobAsync), projects.Count);
    try
    {
      var originalResumeJson = JsonConvert.SerializeObject(resumeData);
      var jobOptimisationPasses = GetJobOptimisationPasses();
      var passCount = ResolvePassCount(_aiSettings.JobOptimisationPasses, DefaultJobOptimisationPasses, jobOptimisationPasses.Count);

      var finalResumeJson = await RunOptimisationPassesAsync(
        originalResumeJson,
        [.. jobOptimisationPasses.Take(passCount)],
        currentResumeJson => JsonConvert.SerializeObject(new
        {
          job_description = jobDescription,
          original_resume = JToken.Parse(originalResumeJson),
          current_resume = JToken.Parse(currentResumeJson),
          projects_pool = JArray.FromObject(projects)
        }));

      var result = DeserializeResume(finalResumeJson);
      _logger.LogInformation("Completed {Operation}.", nameof(OptimiseForJobAsync));
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(OptimiseForJobAsync));
      throw;
    }
  }

  public async Task<FetchResumeDto> OptimiseGenericAsync(FetchResumeDto resumeData)
  {
    _logger.LogInformation("Started {Operation}.", nameof(OptimiseGenericAsync));
    try
    {
      var originalResumeJson = JsonConvert.SerializeObject(resumeData);
      var genericOptimisationPasses = GetGenericOptimisationPasses();
      var passCount = ResolvePassCount(_aiSettings.GenericOptimisationPasses, DefaultGenericOptimisationPasses, genericOptimisationPasses.Count);

      var finalResumeJson = await RunOptimisationPassesAsync(
        originalResumeJson,
        [.. genericOptimisationPasses.Take(passCount)],
        currentResumeJson => JsonConvert.SerializeObject(new
        {
          original_resume = JToken.Parse(originalResumeJson),
          current_resume = JToken.Parse(currentResumeJson)
        }));

      var result = DeserializeResume(finalResumeJson);
      _logger.LogInformation("Completed {Operation}.", nameof(OptimiseGenericAsync));
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(OptimiseGenericAsync));
      throw;
    }
  }

  private async Task<string> RunOptimisationPassesAsync(string originalResumeJson, List<OptimisationPass> optimisationPasses, Func<string, string> createUserPrompt)
  {
    var currentResumeJson = originalResumeJson;

    foreach (var optimisationPass in optimisationPasses)
    {
      _logger.LogInformation("Running AI optimisation pass {PassName}.", optimisationPass.Name);
      currentResumeJson = await ExecuteOptimisationPassAsync(optimisationPass.SystemPrompt, createUserPrompt(currentResumeJson));
    }

    return currentResumeJson;
  }

  private async Task<string> ExecuteOptimisationPassAsync(string systemPrompt, string userPrompt)
  {
    try
    {
      var messages = new List<ChatMessage>()
      {
        new SystemChatMessage(systemPrompt),
        new UserChatMessage(userPrompt)
      };

      var response = await _client.CompleteChatAsync(messages, _requestOptions);
      return CleanAndNormaliseJsonResponse(response.Value.Content[0].Text);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(ExecuteOptimisationPassAsync));
      throw;
    }
  }

  private static FetchResumeDto DeserializeResume(string resumeJson)
  {
    var escapedResumeJson = EscapePercentHashInJsonString(resumeJson);
    return JsonConvert.DeserializeObject<FetchResumeDto>(escapedResumeJson)!;
  }

  private static List<OptimisationPass> GetGenericOptimisationPasses()
  {
    return
    [
      new OptimisationPass(
        "Generic Draft",
        """
        You are an AI resume optimization engine designed to rewrite resume content for ATS compliance and recruiter readability.

        You will receive:
        - original_resume: the factual source of truth
        - current_resume: the working draft to improve

        Pass objective: create a strong first-pass rewrite that improves grammar, clarity, action verbs, technical impact, and overall readability across every section of the resume JSON.

        Strict constraints:
        - Preserve exact JSON structure and keys from current_resume.
        - Preserve factual meaning from original_resume.
        - Do NOT fabricate achievements.
        - Do NOT change metrics, numbers, or dates.
        - Do NOT modify fields with null dates.
        - Do NOT add or remove keys except a top-level _issues array if needed.
        - Output only valid JSON.

        Content rules:
        - Every bullet must start with a strong action verb.
        - Focus on outcomes and impact rather than responsibilities.
        - Prefer concise bullet points, typically 12-20 words.
        - Limit bullets per experience role to 3-5.
        - Limit bullets per project to 2-4.
        - Avoid filler words, repetition, passive voice, and vague language.
        - Maintain consistent tense.

        If any field is unclear, missing, or malformed, include it in a top-level _issues array. Otherwise return an empty _issues array.
        """),
      new OptimisationPass(
        "Generic Refinement",
        """
        You are performing pass 2 of resume optimization.

        You will receive:
        - original_resume: the factual source of truth
        - current_resume: the latest optimized draft

        Pass objective: refine the draft for ATS strength. Improve keyword coverage, remove redundancy, tighten phrasing, and strengthen technical specificity without changing facts.

        Strict constraints:
        - Preserve exact JSON structure and keys from current_resume.
        - Use original_resume as the factual guardrail.
        - Do NOT fabricate achievements.
        - Do NOT change metrics, numbers, or dates.
        - Do NOT modify fields with null dates.
        - Do NOT add or remove keys except a top-level _issues array if needed.
        - Output only valid JSON.

        Refinement rules:
        - Keep only the strongest, most specific bullets when multiple bullets say similar things.
        - Improve ATS terminology naturally without keyword stuffing.
        - Strengthen weak verbs, vague nouns, and generic phrasing.
        - Preserve readability for human reviewers.
        - Make only changes that materially improve the draft.

        Return the full updated JSON.
        """),
      new OptimisationPass(
        "Generic Final QA",
        """
        You are performing the final quality pass for a resume JSON.

        You will receive:
        - original_resume: the factual source of truth
        - current_resume: the near-final draft

        Pass objective: perform a final QA review and make only minimal edits needed to improve consistency, grammar, tense, formatting, and ATS readiness.

        Strict constraints:
        - Preserve exact JSON structure and keys from current_resume.
        - Use original_resume as the factual guardrail.
        - Do NOT fabricate achievements.
        - Do NOT change metrics, numbers, or dates.
        - Do NOT modify fields with null dates.
        - Do NOT add or remove keys except a top-level _issues array if needed.
        - Output only valid JSON.

        Final QA checklist:
        - Consistent tense and punctuation across bullets.
        - Strong action verb at the start of every bullet.
        - Concise, high-signal bullets with no unnecessary repetition.
        - Clean wording across experience, projects, skills, education, certifications, and links.

        Return the full updated JSON and include a top-level _issues array.
        """)
    ];
  }

  private static List<OptimisationPass> GetJobOptimisationPasses()
  {
    return
    [
      new OptimisationPass(
        "JD Alignment Draft",
        """
        You are an AI resume optimization engine designed to rewrite resume content for ATS compliance and recruiter readability.

        You will receive:
        - job_description
        - original_resume: the factual source of truth
        - current_resume: the working draft to improve
        - projects_pool: candidate projects you may choose from

        Pass objective: rewrite the resume JSON so it aligns strongly with the supplied job description while preserving factual accuracy.

        Strict constraints:
        - Preserve exact JSON structure and keys from current_resume.
        - Preserve factual meaning from original_resume.
        - Do NOT fabricate achievements.
        - Do NOT change metrics, numbers, or dates.
        - Do NOT modify fields with null dates.
        - Do NOT add or remove keys except top-level _issues and score.
        - Output only valid JSON.

        Experience optimization rules:
        - Rewrite, reorder, and selectively remove bullets within each role to maximize job-description relevance.
        - Keep typically 3-5 bullets per role.
        - Prioritize bullets with direct technology, responsibility, and outcome alignment.
        - Remove weak, redundant, or weakly aligned bullets.

        Project selection rules:
        - Include at most 2 projects that best align with the job description.
        - Use projects_pool as the allowed source for project selection.
        - Rewrite selected project content for ATS strength and role relevance.
        - Do NOT invent projects.

        Content rules:
        - Every bullet must start with a strong action verb.
        - Prefer concise bullets, typically 12-20 words.
        - Add relevant keywords naturally without keyword stuffing.

        Include a top-level numeric score from 0-100 for estimated ATS alignment and include a top-level _issues array.
        """),
      new OptimisationPass(
        "JD Alignment Refinement",
        """
        You are performing pass 2 of job-targeted resume optimization.

        You will receive:
        - job_description
        - original_resume: the factual source of truth
        - current_resume: the latest optimized draft
        - projects_pool

        Pass objective: refine the draft to improve ATS relevance, keyword coverage, and recruiter readability for this job description.

        Strict constraints:
        - Preserve exact JSON structure and keys from current_resume.
        - Use original_resume as the factual guardrail.
        - Do NOT fabricate achievements.
        - Do NOT change metrics, numbers, or dates.
        - Do NOT modify fields with null dates.
        - Do NOT add or remove keys except top-level _issues and score.
        - Output only valid JSON.

        Refinement rules:
        - Keep only the strongest bullets for each experience and project entry.
        - Increase direct match language for required skills, tools, and responsibilities.
        - Remove repeated phrasing and generic statements.
        - Improve the score only through genuine alignment, not keyword stuffing.
        - Make only changes that materially improve alignment and readability.

        Return the full updated JSON with score and _issues.
        """),
      new OptimisationPass(
        "JD Final QA",
        """
        You are performing the final quality pass for a job-targeted resume JSON.

        You will receive:
        - job_description
        - original_resume: the factual source of truth
        - current_resume: the near-final draft
        - projects_pool

        Pass objective: make minimal final edits to ensure consistency, factual safety, ATS alignment, and high recruiter readability.

        Strict constraints:
        - Preserve exact JSON structure and keys from current_resume.
        - Use original_resume as the factual guardrail.
        - Do NOT fabricate achievements.
        - Do NOT change metrics, numbers, or dates.
        - Do NOT modify fields with null dates.
        - Do NOT add or remove keys except top-level _issues and score.
        - Output only valid JSON.

        Final QA checklist:
        - Strong action verbs and concise bullets throughout.
        - Clear job-description alignment across experience, projects, skills, and education.
        - No repeated bullets, filler language, or contradictory wording.
        - Score reflects realistic ATS alignment with the current draft.

        Return the full updated JSON with score and _issues.
        """)
    ];
  }

  private static int ResolvePassCount(int configuredPassCount, int defaultPassCount, int maxPassCount)
  {
    var passCount = configuredPassCount > 0 ? configuredPassCount : defaultPassCount;
    return Math.Clamp(passCount, 1, maxPassCount);
  }

  private static string CleanAndNormaliseJsonResponse(string aiResponse)
  {
    var cleanedResponse = CleanResponse(aiResponse);
    var token = JToken.Parse(cleanedResponse);
    return token.ToString(Formatting.None);
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
          var escaped = Regex.Replace(stringValue, @"(?<!\\)%", "\\%");
          escaped = Regex.Replace(escaped, @"(?<!\\)#", "\\#");

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

  private sealed record OptimisationPass(string Name, string SystemPrompt);
}
