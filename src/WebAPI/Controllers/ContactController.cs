using Application.Dto;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/contact")]
public class ContactController(IContactService contactService, ILogger<ContactController> logger) : ControllerBase
{
  private readonly IContactService _contactService = contactService;
  private readonly ILogger<ContactController> _logger = logger;

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] ContactDto contactDto)
  {
    _logger.LogInformation("Started {Action}.", nameof(Create));
    var result = await _contactService.CreateContact(contactDto);
    _logger.LogInformation("Completed {Action}.", nameof(Create));
    return CreatedAtAction(nameof(Create), result);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    _logger.LogInformation("Started {Action}.", nameof(GetAll));
    var result = await _contactService.FetchContact();
    _logger.LogInformation("Completed {Action}.", nameof(GetAll));
    return Ok(result);
  }

  [HttpPatch("update/{id}")]
  public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateContactDto updateContactDto)
  {
    _logger.LogInformation("Started {Action} for ContactId={ContactId}.", nameof(Update), id);
    var result = await _contactService.UpdateContact(id, updateContactDto);
    _logger.LogInformation("Completed {Action} for ContactId={ContactId}.", nameof(Update), id);
    return Ok(result);
  }
}
