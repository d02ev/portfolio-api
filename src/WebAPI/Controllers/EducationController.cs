using Application.Dto;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/education")]
public class EducationController(IEducationService educationService) : ControllerBase
{
  private readonly IEducationService _educationService = educationService;

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] EducationDto educationDto)
  {
    var result = await _educationService.CreateEducation(educationDto);
    return CreatedAtAction(nameof(Create), result);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var result = await _educationService.FetchEducation();
    return Ok(result);
  }

  [HttpPatch("update/{id}")]
  public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateEducationDto updateEducationDto)
  {
    var result = await _educationService.UpdateEducation(id, updateEducationDto);
    return Ok(result);
  }
}