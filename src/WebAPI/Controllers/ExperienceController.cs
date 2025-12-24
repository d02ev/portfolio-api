using Application.Dto;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/experience")]
public class ExperienceController(IExperienceService experienceService) : ControllerBase
{
  private readonly IExperienceService _experienceService = experienceService;

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] ExperienceDto experienceDto)
  {
    var result = await _experienceService.CreateExperience(experienceDto);
    return CreatedAtAction(nameof(Create), result);
  }

  [AllowAnonymous]
  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var result = await _experienceService.FetchAllExperiences();
    return Ok(result);
  }

  [HttpGet("deleted")]
  public async Task<IActionResult> GetAllDeleted()
  {
    var result = await _experienceService.FetchAllDeletedExperiences();
    return Ok(result);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetById([FromRoute] string id)
  {
    var result = await _experienceService.FetchExperienceById(id);
    return Ok(result);
  }

  [HttpPatch("update/{id}")]
  public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateExperienceDto updateExperienceDto)
  {
    var result = await _experienceService.UpdateExperience(id, updateExperienceDto);
    return Ok(result);
  }

  [HttpPatch("delete/{id}")]
  public async Task<IActionResult> Delete([FromRoute] string id)
  {
    var result = await _experienceService.DeleteExperience(id);
    return Ok(result);
  }
}