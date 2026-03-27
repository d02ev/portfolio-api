using Application.Dto;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/about")]
public class AboutController(IAboutService aboutService, ILogger<AboutController> logger) : ControllerBase
{
  private readonly IAboutService _aboutService = aboutService;
  private readonly ILogger<AboutController> _logger = logger;

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] AboutDto aboutDto)
  {
    _logger.LogInformation("Started {Action}.", nameof(Create));
    var result = await _aboutService.CreateAbout(aboutDto);
    _logger.LogInformation("Completed {Action}.", nameof(Create));
    return CreatedAtAction(nameof(Create), result);
  }

  [AllowAnonymous]
  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    _logger.LogInformation("Started {Action}.", nameof(GetAll));
    var result = await _aboutService.FetchAbout();
    _logger.LogInformation("Completed {Action}.", nameof(GetAll));
    return Ok(result);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetById([FromRoute] string id)
  {
    _logger.LogInformation("Started {Action} for AboutId={AboutId}.", nameof(GetById), id);
    var result = await _aboutService.FetchAboutById(id);
    _logger.LogInformation("Completed {Action} for AboutId={AboutId}.", nameof(GetById), id);
    return Ok(result);
  }

  [HttpPatch("update/{id}")]
  public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateAboutDto updateAboutDto)
  {
    _logger.LogInformation("Started {Action} for AboutId={AboutId}.", nameof(Update), id);
    var result = await _aboutService.UpdateAbout(id, updateAboutDto);
    _logger.LogInformation("Completed {Action} for AboutId={AboutId}.", nameof(Update), id);
    return Ok(result);
  }
}
