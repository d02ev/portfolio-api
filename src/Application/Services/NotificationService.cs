using Application.Integrations;
using Application.Requests;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class NotificationService(ITelegramIntegration telegramIntegration, ILogger<NotificationService> logger) : INotificationService
{
  private readonly ITelegramIntegration _telegramIntegration = telegramIntegration;
  private readonly ILogger<NotificationService> _logger = logger;

  public async Task SendNotificationAsync(SendMessageRequest sendMessageRequest)
  {
    _logger.LogInformation(
      "Started {Operation}. Mode={Mode}, HasPdfUrl={HasPdfUrl}, HasErrorMessage={HasErrorMessage}.",
      nameof(SendNotificationAsync),
      sendMessageRequest.Mode,
      !string.IsNullOrWhiteSpace(sendMessageRequest.PdfUrl),
      !string.IsNullOrWhiteSpace(sendMessageRequest.ErrorMessage));

    try
    {
      // if both pdfUrl and errorMessage are null, send the in progress message
      if (sendMessageRequest.PdfUrl is null && sendMessageRequest.ErrorMessage is null)
      {
        _logger.LogInformation("Dispatching in-progress notification. Mode={Mode}.", sendMessageRequest.Mode);
        await _telegramIntegration.SendInProgressMessageAsync(sendMessageRequest.Mode);
      }

      // if errorMessage is not null, send the error message
      else if (sendMessageRequest.ErrorMessage is not null)
      {
        _logger.LogInformation("Dispatching failure notification. Mode={Mode}.", sendMessageRequest.Mode);
        await _telegramIntegration.SendFailureMessageAsync(sendMessageRequest.ErrorMessage, sendMessageRequest.Mode);
      }

      // if pdfUrl is not null, send the success message
      else if (sendMessageRequest.PdfUrl is not null)
      {
        _logger.LogInformation("Dispatching success notification. Mode={Mode}.", sendMessageRequest.Mode);
        await _telegramIntegration.SendSuccessMessageAsync(sendMessageRequest.PdfUrl, sendMessageRequest.CompanyName, sendMessageRequest.Mode);
      }

      _logger.LogInformation("Completed {Operation}. Mode={Mode}.", nameof(SendNotificationAsync), sendMessageRequest.Mode);
    }
    catch (BadRequestException)
    {
      throw;
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (InternalServerException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}. Mode={Mode}.", nameof(SendNotificationAsync), sendMessageRequest.Mode);
      throw;
    }
  }
}
