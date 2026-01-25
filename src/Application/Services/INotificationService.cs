using Application.Requests;

namespace Application.Services;

public interface INotificationService
{
  Task SendNotificationAsync(SendMessageRequest sendMessageRequest);
}