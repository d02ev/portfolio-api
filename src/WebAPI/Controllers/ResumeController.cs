using Application.Dto;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/resume")]
public class ResumeController(IResumeService resumeService, ILogger<ResumeController> logger) : ControllerBase
{
  private readonly IResumeService _resumeService = resumeService;
  private readonly ILogger<ResumeController> _logger = logger;

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] ResumeDto resumeDto)
  {
    _logger.LogInformation("Started {Action}.", nameof(Create));
    var result = await _resumeService.CreateResume(resumeDto);
    _logger.LogInformation("Completed {Action}.", nameof(Create));
    return CreatedAtAction(nameof(Create), result);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    _logger.LogInformation("Started {Action}.", nameof(GetAll));
    var result = await _resumeService.FetchResume();
    _logger.LogInformation("Completed {Action}.", nameof(GetAll));
    return Ok(result);
  }

  [HttpPatch("update/{id}")]
  public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateResumeDto updateResumeDto)
  {
    _logger.LogInformation("Started {Action} for ResumeId={ResumeId}.", nameof(Update), id);
    var result = await _resumeService.UpdateResume(id, updateResumeDto);
    _logger.LogInformation("Completed {Action} for ResumeId={ResumeId}.", nameof(Update), id);
    return Ok(result);
  }

  [HttpPost("generate")]
  public async Task<IActionResult> Generate([FromBody] GenerateResumeDto generateResumeDto)
  {
    _logger.LogInformation(
      "Started {Action}. ResumeName={ResumeName}, TemplateId={TemplateId}, HasCompanyName={HasCompanyName}.",
      nameof(Generate),
      generateResumeDto.ResumeName,
      generateResumeDto.TemplateId,
      !string.IsNullOrWhiteSpace(generateResumeDto.CompanyName));
    var result = await _resumeService.GenerateResume(generateResumeDto);
    _logger.LogInformation("Completed {Action}. JobId={JobId}.", nameof(Generate), result.Data.JobId);
    return CreatedAtAction(nameof(Generate), result);
  }

  [HttpGet("status/{jobId}")]
  public async Task<IActionResult> GetJobStatus([FromRoute] string jobId)
  {
    _logger.LogInformation("Started {Action} for JobId={JobId}.", nameof(GetJobStatus), jobId);
    var result = await _resumeService.FetchResumeJobRunStatus(long.Parse(jobId));
    _logger.LogInformation("Completed {Action} for JobId={JobId}.", nameof(GetJobStatus), jobId);
    return Ok(result);
  }

  [AllowAnonymous]
  [HttpGet("latest")]
  public async Task<IActionResult> GetLatestResume()
  {
    _logger.LogInformation("Started {Action}.", nameof(GetLatestResume));
    var result = await _resumeService.FetchLatestResumePdfUrl();
    _logger.LogInformation("Completed {Action}.", nameof(GetLatestResume));
    return Ok(result);
  }
}
