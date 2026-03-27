using Application.Dto;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/techstack")]
public class TechStackController(ITechStackService techStackService, ILogger<TechStackController> logger) : ControllerBase
{
  private readonly ITechStackService _techStackService = techStackService;
  private readonly ILogger<TechStackController> _logger = logger;

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] TechStackDto techStackDto)
  {
    _logger.LogInformation("Started {Action}.", nameof(Create));
    var result = await _techStackService.CreateTechStack(techStackDto);
    _logger.LogInformation("Completed {Action}.", nameof(Create));
    return CreatedAtAction(nameof(Create), result);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    _logger.LogInformation("Started {Action}.", nameof(GetAll));
    var result = await _techStackService.FetchTechStack();
    _logger.LogInformation("Completed {Action}.", nameof(GetAll));
    return Ok(result);
  }

  [HttpPatch("update/{id}")]
  public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateTechStackDto updateTechStackDto)
  {
    _logger.LogInformation("Started {Action} for TechStackId={TechStackId}.", nameof(Update), id);
    var result = await _techStackService.UpdateTechStack(id, updateTechStackDto);
    _logger.LogInformation("Completed {Action} for TechStackId={TechStackId}.", nameof(Update), id);
    return Ok(result);
  }
}
