using Application.Common;
using Application.Integrations;
using Domain.Configurations;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Infrastructure.Integrations;

public class TelegramIntegration(IOptions<TelegramSettings> options) : ITelegramIntegration
{
  private readonly TelegramSettings _telegramSettings = options.Value;
  private readonly TelegramBotClient _botClient = new(options.Value.BotToken);

  public async Task SendSuccessMessageAsync(string pdfUrl, string? companyName = null, string mode = ResumeModes.Generic)
  {
    var message = @"
    Resume generation successful!

    Mode: " + mode + @"
    Date Time (UTC): " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + @"
    Company Name: " + companyName is not null ? companyName : "NA" + @"
    PDF_URL: " + pdfUrl + @"
    ";
    await SendMessageAsync(message!);
  }

  public async Task SendFailureMessageAsync(string errorMessage, string mode = ResumeModes.Generic)
  {
    var message = @"
    Resume generation failed!

    Mode: " + mode + @"
    Date Time (UTC): " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + @"
    ERROR: " + errorMessage + @"
    ";
    await SendMessageAsync(message);
  }

  private async Task SendMessageAsync(string message)
  {
    await _botClient.SendMessage(_telegramSettings.ChatId, message, ParseMode.None);
  }

  public async Task SendWorkflowStartedMessageAsync()
  {
    var message = @"
    Workflow initiated!

    Date Time (UTC): " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + @"
    ";
    await SendMessageAsync(message);
  }
}