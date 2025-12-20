using Application.Dto;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/techstack")]
public class TechStackController(ITechStackService techStackService) : ControllerBase
{
  private readonly ITechStackService _techStackService = techStackService;

  [Authorize(Roles = "admin")]
  [HttpPost]
  public async Task<IActionResult> Create([FromBody] TechStackDto techStackDto)
  {
    var result = await _techStackService.CreateTechStack(techStackDto);
    return CreatedAtAction(nameof(Create), result);
  }

  [Authorize(Roles = "admin")]
  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var result = await _techStackService.FetchTechStack();
    return Ok(result);
  }

  [Authorize(Roles = "admin")]
  [HttpPatch("update/{id}")]
  public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateTechStackDto updateTechStackDto)
  {
    var result = await _techStackService.UpdateTechStack(id, updateTechStackDto);
    return Ok(result);
  }
}