using Application.Requests;
using Application.Responses;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/notification")]
public class NotificationController(INotificationService notificationService) : ControllerBase
{
  private readonly INotificationService _notificationService = notificationService;

  [HttpPost("send")]
  public async Task<IActionResult> SendNotification([FromBody] SendMessageRequest sendMessageRequest)
  {
    await _notificationService.SendNotificationAsync(sendMessageRequest);
    return Ok(new SendMessageResponse());
  }
}