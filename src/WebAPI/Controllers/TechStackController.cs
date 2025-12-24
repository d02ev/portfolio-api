using Application.Dto;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/techstack")]
public class TechStackController(ITechStackService techStackService) : ControllerBase
{
  private readonly ITechStackService _techStackService = techStackService;

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] TechStackDto techStackDto)
  {
    var result = await _techStackService.CreateTechStack(techStackDto);
    return CreatedAtAction(nameof(Create), result);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var result = await _techStackService.FetchTechStack();
    return Ok(result);
  }

  [HttpPatch("update/{id}")]
  public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateTechStackDto updateTechStackDto)
  {
    var result = await _techStackService.UpdateTechStack(id, updateTechStackDto);
    return Ok(result);
  }
}