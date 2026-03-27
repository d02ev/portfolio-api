using Application.Common;
using Application.Integrations;
using Application.Requests;
using Application.Services;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;

namespace PorfolioApi.Tests;

public class NotificationServiceTests
{
  private readonly Mock<ITelegramIntegration> _telegramIntegrationMock;
  private readonly TestLogger<NotificationService> _logger;
  private readonly INotificationService _notificationService;

  public NotificationServiceTests()
  {
    _telegramIntegrationMock = new Mock<ITelegramIntegration>();
    _logger = new TestLogger<NotificationService>();
    _notificationService = new NotificationService(_telegramIntegrationMock.Object, _logger);
  }

  [Fact]
  public async Task SendNotificationAsync_ShouldSendInProgressMessage_WhenPdfUrlAndErrorAreMissing()
  {
    await _notificationService.SendNotificationAsync(new SendMessageRequest
    {
      Mode = ResumeModes.Generic
    });

    _telegramIntegrationMock.Verify(t => t.SendInProgressMessageAsync(ResumeModes.Generic), Times.Once);
    _logger.Entries.Should().Contain(e => e.Level == LogLevel.Information && e.Message.Contains("Started SendNotificationAsync"));
    _logger.Entries.Should().Contain(e => e.Level == LogLevel.Information && e.Message.Contains("Dispatching in-progress notification"));
    _logger.Entries.Should().Contain(e => e.Level == LogLevel.Information && e.Message.Contains("Completed SendNotificationAsync"));
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
    _logger.Entries.Should().Contain(e => e.Level == LogLevel.Information && e.Message.Contains("Dispatching failure notification"));
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
    _logger.Entries.Should().Contain(e => e.Level == LogLevel.Information && e.Message.Contains("Dispatching success notification"));
  }
}
