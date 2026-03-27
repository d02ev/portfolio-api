using Application.Requests;
using Application.Responses;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/notification")]
public class NotificationController(INotificationService notificationService, ILogger<NotificationController> logger) : ControllerBase
{
  private readonly INotificationService _notificationService = notificationService;
  private readonly ILogger<NotificationController> _logger = logger;

  [HttpPost("send")]
  public async Task<IActionResult> SendNotification([FromBody] SendMessageRequest sendMessageRequest)
  {
    _logger.LogInformation(
      "Started {Action}. Mode={Mode}, HasPdfUrl={HasPdfUrl}, HasErrorMessage={HasErrorMessage}.",
      nameof(SendNotification),
      sendMessageRequest.Mode,
      !string.IsNullOrWhiteSpace(sendMessageRequest.PdfUrl),
      !string.IsNullOrWhiteSpace(sendMessageRequest.ErrorMessage));
    await _notificationService.SendNotificationAsync(sendMessageRequest);
    _logger.LogInformation("Completed {Action}. Mode={Mode}.", nameof(SendNotification), sendMessageRequest.Mode);
    return Ok(new SendMessageResponse());
  }
}
