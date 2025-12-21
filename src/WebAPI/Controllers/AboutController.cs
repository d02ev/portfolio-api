using Application.Dto;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/about")]
public class AboutController(IAboutService aboutService) : ControllerBase
{
  private readonly IAboutService _aboutService = aboutService;

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] AboutDto aboutDto)
  {
    var result = await _aboutService.CreateAbout(aboutDto);
    return CreatedAtAction(nameof(Create), result);
  }

  [AllowAnonymous]
  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var result = await _aboutService.FetchAbout();
    return Ok(result);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetById([FromRoute] string id)
  {
    var result = await _aboutService.FetchAboutById(id);
    return Ok(result);
  }

  [HttpPatch("update/{id}")]
  public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateAboutDto updateAboutDto)
  {
    var result = await _aboutService.UpdateAbout(id, updateAboutDto);
    return Ok(result);
  }
}