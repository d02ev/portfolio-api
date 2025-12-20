using Application.Dto;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/contact")]
public class ContactController(IContactService contactService) : ControllerBase
{
  private readonly IContactService _contactService = contactService;

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] ContactDto contactDto)
  {
    var result = await _contactService.CreateContact(contactDto);
    return CreatedAtAction(nameof(Create), result);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var result = await _contactService.FetchContact();
    return Ok(result);
  }

  [HttpPatch("update/{id}")]
  public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateContactDto updateContactDto)
  {
    var result = await _contactService.UpdateContact(id, updateContactDto);
    return Ok(result);
  }
}