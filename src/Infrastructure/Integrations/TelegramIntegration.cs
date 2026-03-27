using Application.Common;
using Application.Integrations;
using Domain.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Infrastructure.Integrations;

public class TelegramIntegration(IOptions<TelegramSettings> options, ILogger<TelegramIntegration> logger) : ITelegramIntegration
{
  private readonly TelegramSettings _telegramSettings = options.Value;
  private readonly TelegramBotClient _botClient = new(options.Value.BotToken);
  private readonly ILogger<TelegramIntegration> _logger = logger;

  public async Task SendSuccessMessageAsync(string pdfUrl, string? companyName = null, string mode = ResumeModes.Generic)
  {
    _logger.LogInformation("Started {Operation}. Mode={Mode}, HasCompanyName={HasCompanyName}.", nameof(SendSuccessMessageAsync), mode, !string.IsNullOrWhiteSpace(companyName));
    var message = $"""
    Resume generation successful!

    Mode: {mode}
    Date Time (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}
    Company Name: {(companyName is not null ? companyName : "NA")}
    PDF_URL: {pdfUrl}
    """;
    await SendMessageAsync(message!);
    _logger.LogInformation("Completed {Operation}. Mode={Mode}.", nameof(SendSuccessMessageAsync), mode);
  }

  public async Task SendFailureMessageAsync(string errorMessage, string mode = ResumeModes.Generic)
  {
    _logger.LogInformation("Started {Operation}. Mode={Mode}.", nameof(SendFailureMessageAsync), mode);
    var message = $"""
    Resume generation failed!

    Mode: {mode}
    Date Time (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}
    ERROR: {errorMessage}
    """;
    await SendMessageAsync(message);
    _logger.LogInformation("Completed {Operation}. Mode={Mode}.", nameof(SendFailureMessageAsync), mode);
  }

  private async Task SendMessageAsync(string message)
  {
    _logger.LogInformation("Started {Operation}.", nameof(SendMessageAsync));
    try
    {
      await _botClient.SendMessage(_telegramSettings.ChatId, message, ParseMode.None);
      _logger.LogInformation("Completed {Operation}.", nameof(SendMessageAsync));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(SendMessageAsync));
      throw;
    }
  }

  public async Task SendWorkflowStartedMessageAsync()
  {
    _logger.LogInformation("Started {Operation}.", nameof(SendWorkflowStartedMessageAsync));
    var message = @"
    Workflow initiated!

    Date Time (UTC): " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + @"
    ";
    await SendMessageAsync(message);
    _logger.LogInformation("Completed {Operation}.", nameof(SendWorkflowStartedMessageAsync));
  }

  public async Task SendInProgressMessageAsync(string mode = ResumeModes.Generic)
  {
    _logger.LogInformation("Started {Operation}. Mode={Mode}.", nameof(SendInProgressMessageAsync), mode);
    var message = $"""
    Resume generation in progress...

    Mode: {mode}
    Date Time (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}
    """;
    await SendMessageAsync(message);
    _logger.LogInformation("Completed {Operation}. Mode={Mode}.", nameof(SendInProgressMessageAsync), mode);
  }
}
