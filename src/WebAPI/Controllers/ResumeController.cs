using Application.Dto;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/resume")]
public class ResumeController(IResumeService resumeService) : ControllerBase
{
  private readonly IResumeService _resumeService = resumeService;

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] ResumeDto resumeDto)
  {
    var result = await _resumeService.CreateResume(resumeDto);
    return CreatedAtAction(nameof(Create), result);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var result = await _resumeService.FetchResume();
    return Ok(result);
  }

  [HttpPatch("update/{id}")]
  public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateResumeDto updateResumeDto)
  {
    var result = await _resumeService.UpdateResume(id, updateResumeDto);
    return Ok(result);
  }

  [HttpPost("generate")]
  public async Task<IActionResult> Generate([FromBody] GenerateResumeDto generateResumeDto)
  {
    var result = await _resumeService.GenerateResume(generateResumeDto);
    return CreatedAtAction(nameof(Generate), result);
  }

  [HttpGet("status/{jobId}")]
  public async Task<IActionResult> GetJobStatus([FromRoute] string jobId)
  {
    var result = await _resumeService.FetchResumeJobRunStatus(long.Parse(jobId));
    return Ok(result);
  }

  [AllowAnonymous]
  [HttpGet("latest")]
  public async Task<IActionResult> GetLatestResume()
  {
    var result = await _resumeService.FetchLatestResumePdfUrl();
    return Ok(result);
  }
}