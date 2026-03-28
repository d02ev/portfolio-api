using Application.Dto;
using Application.Integrations;
using Domain.Configurations;
using Domain.Entities.Postgres;
using Domain.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Supabase;
using Supabase.Postgrest;
using Supabase.Storage;

namespace Infrastructure.Integrations;

public class SupabaseIntegration : ISupabaseIntegration
{
  private readonly SupabaseSettings _supabaseSettings;
  private readonly Supabase.Client _client;
  private readonly ILogger<SupabaseIntegration> _logger;

  public SupabaseIntegration(IOptions<SupabaseSettings> options, ILogger<SupabaseIntegration> logger)
  {
    _logger = logger;
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
    _logger.LogInformation("Initialized Supabase client for {Integration}.", nameof(SupabaseIntegration));
  }

  public async Task<byte[]> DownloadFileAsBytesAsync(string filePath)
  {
    _logger.LogInformation("Started {Operation}. FilePath={FilePath}.", nameof(DownloadFileAsBytesAsync), filePath);
    try
    {
      var storage = _client.Storage;
      var bucket = storage.From(_supabaseSettings.Bucket);
      var result = await bucket.Download(filePath, (TransformOptions?)null);
      _logger.LogInformation("Completed {Operation}. FilePath={FilePath}.", nameof(DownloadFileAsBytesAsync), filePath);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}. FilePath={FilePath}.", nameof(DownloadFileAsBytesAsync), filePath);
      throw;
    }
  }

  public async Task<string?> DownloadFileAsStringAsync(string filename)
  {
    _logger.LogInformation("Started {Operation}. Filename={Filename}.", nameof(DownloadFileAsStringAsync), filename);
    try
    {
      var storage = _client.Storage;
      var bucket = storage.From(_supabaseSettings.Bucket);
      var fileBytes = await bucket.Download(filename, (TransformOptions?)null);
      var result = System.Text.Encoding.UTF8.GetString(fileBytes);
      _logger.LogInformation("Completed {Operation}. Filename={Filename}.", nameof(DownloadFileAsStringAsync), filename);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}. Filename={Filename}.", nameof(DownloadFileAsStringAsync), filename);
      throw;
    }
  }

  public async Task<long> InsertJobStatusAsync(string latexFileName, string? companyName = null)
  {
    _logger.LogInformation("Started {Operation}. LatexFileName={LatexFileName}, HasCompanyName={HasCompanyName}.", nameof(InsertJobStatusAsync), latexFileName, !string.IsNullOrWhiteSpace(companyName));
    try
    {
      var newResumeJobEntry = await _client.From<ResumeJob>().Insert(new ResumeJob { Status = "pending",  LatexFileName = latexFileName, CompanyName = companyName, Mode = companyName is not null ? "job_description" : "generic" }, new QueryOptions { Returning = QueryOptions.ReturnType.Representation });
      var jobId = newResumeJobEntry.Models.First().Id;
      _logger.LogInformation("Completed {Operation}. JobId={JobId}.", nameof(InsertJobStatusAsync), jobId);
      return jobId;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}. LatexFileName={LatexFileName}.", nameof(InsertJobStatusAsync), latexFileName);
      throw;
    }
  }

  public async Task<ResumeJob?> FetchJobStatusAsync(long jobId)
  {
    _logger.LogInformation("Started {Operation}. JobId={JobId}.", nameof(FetchJobStatusAsync), jobId);
    try
    {
      var response = await _client.From<ResumeJob>().Where(x => x.Id == jobId).Get();
      var result = response.Models.FirstOrDefault();
      _logger.LogInformation("Completed {Operation}. JobId={JobId}, Found={Found}.", nameof(FetchJobStatusAsync), jobId, result is not null);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}. JobId={JobId}.", nameof(FetchJobStatusAsync), jobId);
      throw;
    }
  }

  public async Task<string> FetchLatestPdfUrlAsync()
  {
    _logger.LogInformation("Started {Operation}.", nameof(FetchLatestPdfUrlAsync));
    try
    {
      var result = await _client.From<ResumeJob>()
        .Select("pdf_url")
        .Where(x => x.Status == "success" && x.Mode == "generic")
        .Order("created_at", Constants.Ordering.Descending)
        .Limit(1)
        .Get();
      var pdfUrl = result.Models.FirstOrDefault()?.PdfUrl ?? throw new NotFoundException("No PDF URL found");
      _logger.LogInformation("Completed {Operation}.", nameof(FetchLatestPdfUrlAsync));
      return pdfUrl;
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed {Operation}.", nameof(FetchLatestPdfUrlAsync));
      throw;
    }
  }
}
