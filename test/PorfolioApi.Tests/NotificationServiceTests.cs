using Application.Common;
using Application.Integrations;
using Application.Requests;
using Application.Services;
using Moq;

namespace PorfolioApi.Tests;

public class NotificationServiceTests
{
  private readonly Mock<ITelegramIntegration> _telegramIntegrationMock;
  private readonly INotificationService _notificationService;

  public NotificationServiceTests()
  {
    _telegramIntegrationMock = new Mock<ITelegramIntegration>();
    _notificationService = new NotificationService(_telegramIntegrationMock.Object);
  }

  [Fact]
  public async Task SendNotificationAsync_ShouldSendInProgressMessage_WhenPdfUrlAndErrorAreMissing()
  {
    await _notificationService.SendNotificationAsync(new SendMessageRequest
    {
      Mode = ResumeModes.Generic
    });

    _telegramIntegrationMock.Verify(t => t.SendInProgressMessageAsync(ResumeModes.Generic), Times.Once);
  }

  [Fact]
  public async Task SendNotificationAsync_ShouldSendFailureMessage_WhenErrorMessageExists()
  {
    await _notificationService.SendNotificationAsync(new SendMessageRequest
    {
      ErrorMessage = "Failed",
      Mode = ResumeModes.JobDescription
    });

    _telegramIntegrationMock.Verify(t => t.SendFailureMessageAsync("Failed", ResumeModes.JobDescription), Times.Once);
  }

  [Fact]
  public async Task SendNotificationAsync_ShouldSendSuccessMessage_WhenPdfUrlExists()
  {
    await _notificationService.SendNotificationAsync(new SendMessageRequest
    {
      PdfUrl = "https://example.com/resume.pdf",
      CompanyName = "OpenAI",
      Mode = ResumeModes.JobDescription
    });

    _telegramIntegrationMock.Verify(t => t.SendSuccessMessageAsync("https://example.com/resume.pdf", "OpenAI", ResumeModes.JobDescription), Times.Once);
  }
}
