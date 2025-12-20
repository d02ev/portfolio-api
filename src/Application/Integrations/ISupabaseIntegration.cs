using Domain.Entities.Postgres;

namespace Application.Integrations;

public interface ISupabaseIntegration
{
  Task<string?> DownloadFileAsStringAsync(string filename);

  Task<byte[]> DownloadFileAsBytesAsync(string filename);

  Task<long> InsertJobStatusAsync();

  Task<ResumeJob?> FetchJobStatusAsync(long jobId);

  Task<string> FetchLatestPdfUrlAsync();
}