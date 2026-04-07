using Application.Dto;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/experience")]
public class ExperienceController(IExperienceService experienceService, ILogger<ExperienceController> logger) : ControllerBase
{
  private readonly IExperienceService _experienceService = experienceService;
  private readonly ILogger<ExperienceController> _logger = logger;

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] ExperienceDto experienceDto)
  {
    _logger.LogInformation("Started {Action}.", nameof(Create));
    var result = await _experienceService.CreateExperience(experienceDto);
    _logger.LogInformation("Completed {Action}.", nameof(Create));
    return CreatedAtAction(nameof(Create), result);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    _logger.LogInformation("Started {Action}.", nameof(GetAll));
    var result = await _experienceService.FetchAllExperiences();
    _logger.LogInformation("Completed {Action}.", nameof(GetAll));
    return Ok(result);
  }

  [HttpGet("deleted")]
  public async Task<IActionResult> GetAllDeleted()
  {
    _logger.LogInformation("Started {Action}.", nameof(GetAllDeleted));
    var result = await _experienceService.FetchAllDeletedExperiences();
    _logger.LogInformation("Completed {Action}.", nameof(GetAllDeleted));
    return Ok(result);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetById([FromRoute] string id)
  {
    _logger.LogInformation("Started {Action} for ExperienceId={ExperienceId}.", nameof(GetById), id);
    var result = await _experienceService.FetchExperienceById(id);
    _logger.LogInformation("Completed {Action} for ExperienceId={ExperienceId}.", nameof(GetById), id);
    return Ok(result);
  }

  [HttpPatch("update/{id}")]
  public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateExperienceDto updateExperienceDto)
  {
    _logger.LogInformation("Started {Action} for ExperienceId={ExperienceId}.", nameof(Update), id);
    var result = await _experienceService.UpdateExperience(id, updateExperienceDto);
    _logger.LogInformation("Completed {Action} for ExperienceId={ExperienceId}.", nameof(Update), id);
    return Ok(result);
  }

  [HttpPatch("delete/{id}")]
  public async Task<IActionResult> Delete([FromRoute] string id)
  {
    _logger.LogInformation("Started {Action} for ExperienceId={ExperienceId}.", nameof(Delete), id);
    var result = await _experienceService.DeleteExperience(id);
    _logger.LogInformation("Completed {Action} for ExperienceId={ExperienceId}.", nameof(Delete), id);
    return Ok(result);
  }
}
