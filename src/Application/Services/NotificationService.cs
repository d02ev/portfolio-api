using Application.Integrations;
using Application.Requests;

namespace Application.Services;

public class NotificationService(ITelegramIntegration telegramIntegration) : INotificationService
{
  private readonly ITelegramIntegration _telegramIntegration = telegramIntegration;

  public async Task SendNotificationAsync(SendMessageRequest sendMessageRequest)
  {
    // if both pdfUrl and errorMessage are null, send the in progress message
    if (sendMessageRequest.PdfUrl is null && sendMessageRequest.ErrorMessage is null)
    {
      await _telegramIntegration.SendInProgressMessageAsync(sendMessageRequest.Mode);
    }

    // if errorMessage is not null, send the error message
    else if (sendMessageRequest.ErrorMessage is not null)
    {
      await _telegramIntegration.SendFailureMessageAsync(sendMessageRequest.ErrorMessage, sendMessageRequest.Mode);
    }

    // if pdfUrl is not null, send the success message
    else if (sendMessageRequest.PdfUrl is not null)
    {
      await _telegramIntegration.SendSuccessMessageAsync(sendMessageRequest.PdfUrl, sendMessageRequest.CompanyName, sendMessageRequest.Mode);
    }

    return;
  }
}