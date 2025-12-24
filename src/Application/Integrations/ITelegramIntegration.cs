using Application.Common;

namespace Application.Integrations;

public interface ITelegramIntegration
{
  Task SendSuccessMessageAsync(string pdfUrl, string? companyName = null, string mode = ResumeModes.Generic);

  Task SendFailureMessageAsync(string errorMessage, string mode = ResumeModes.Generic);

  Task SendWorkflowStartedMessageAsync();
}