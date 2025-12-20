using Application.Integrations;
using Domain.Configurations;
using Domain.Entities.Postgres;
using Domain.Exceptions;
using Microsoft.Extensions.Options;
using Supabase;
using Supabase.Postgrest;
using Supabase.Storage;

namespace Infrastructure.Integrations;

public class SupabaseIntegration : ISupabaseIntegration
{
  private readonly SupabaseSettings _supabaseSettings;
  private readonly Supabase.Client _client;

  public SupabaseIntegration(IOptions<SupabaseSettings> options)
  {
    _supabaseSettings = options.Value;
    _client = new Supabase.Client(
      _supabaseSettings.ProjectUrl,
      _supabaseSettings.ServiceRoleKey,
      new SupabaseOptions
      {
        AutoConnectRealtime = false,
      }
    );
    _client.InitializeAsync().Wait();
  }

  public async Task<byte[]> DownloadFileAsBytesAsync(string filePath)
  {
    var storage = _client.Storage;
    var bucket = storage.From(_supabaseSettings.Bucket);
    return await bucket.Download(filePath, (TransformOptions?)null);
  }

  public async Task<string?> DownloadFileAsStringAsync(string filename)
  {
    var storage = _client.Storage;
    var bucket = storage.From(_supabaseSettings.Bucket);
    var fileBytes = await bucket.Download(filename, (TransformOptions?)null);

    return System.Text.Encoding.UTF8.GetString(fileBytes);
  }

  public async Task<long> InsertJobStatusAsync()
  {
    var newResumeJobEntry = await _client.From<ResumeJob>().Insert(new ResumeJob { Status = "pending" }, new QueryOptions { Returning = QueryOptions.ReturnType.Representation });
    return newResumeJobEntry.Models.First().Id;
  }

  public async Task<ResumeJob?> FetchJobStatusAsync(long jobId)
  {
    var response = await _client.From<ResumeJob>().Where(x => x.Id == jobId).Get();
    return response.Models.FirstOrDefault();
  }

  public async Task<string> FetchLatestPdfUrlAsync()
  {
    var result = await _client.From<ResumeJob>()
      .Select("pdf_url")
      .Where(x => x.Status == "success")
      .Order("created_at", Constants.Ordering.Descending)
      .Limit(1)
      .Get();
    return result.Models.FirstOrDefault()?.PdfUrl ?? throw new NotFoundException("No PDF URL found");
  }
}