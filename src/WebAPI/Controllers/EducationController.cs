using Application.Dto;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/education")]
public class EducationController(IEducationService educationService, ILogger<EducationController> logger) : ControllerBase
{
  private readonly IEducationService _educationService = educationService;
  private readonly ILogger<EducationController> _logger = logger;

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] EducationDto educationDto)
  {
    _logger.LogInformation("Started {Action}.", nameof(Create));
    var result = await _educationService.CreateEducation(educationDto);
    _logger.LogInformation("Completed {Action}.", nameof(Create));
    return CreatedAtAction(nameof(Create), result);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    _logger.LogInformation("Started {Action}.", nameof(GetAll));
    var result = await _educationService.FetchEducation();
    _logger.LogInformation("Completed {Action}.", nameof(GetAll));
    return Ok(result);
  }

  [HttpPatch("update/{id}")]
  public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateEducationDto updateEducationDto)
  {
    _logger.LogInformation("Started {Action} for EducationId={EducationId}.", nameof(Update), id);
    var result = await _educationService.UpdateEducation(id, updateEducationDto);
    _logger.LogInformation("Completed {Action} for EducationId={EducationId}.", nameof(Update), id);
    return Ok(result);
  }
}
