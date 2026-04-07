using Application.Dto;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/project")]
public class ProjectController(IProjectService projectService, ILogger<ProjectController> logger) : ControllerBase
{
  private readonly IProjectService _projectService = projectService;
  private readonly ILogger<ProjectController> _logger = logger;

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] ProjectDto projectDto)
  {
    _logger.LogInformation("Started {Action}.", nameof(Create));
    var result = await _projectService.CreateProject(projectDto);
    _logger.LogInformation("Completed {Action}.", nameof(Create));
    return CreatedAtAction(nameof(Create), result);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    _logger.LogInformation("Started {Action}.", nameof(GetAll));
    var result = await _projectService.FetchAllProjects();
    _logger.LogInformation("Completed {Action}.", nameof(GetAll));
    return Ok(result);
  }

  [HttpGet("deleted")]
  public async Task<IActionResult> GetAllDeleted()
  {
    _logger.LogInformation("Started {Action}.", nameof(GetAllDeleted));
    var result = await _projectService.FetchAllDeletedProjects();
    _logger.LogInformation("Completed {Action}.", nameof(GetAllDeleted));
    return Ok(result);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetById([FromRoute] string id)
  {
    _logger.LogInformation("Started {Action} for ProjectId={ProjectId}.", nameof(GetById), id);
    var result = await _projectService.FetchProjectById(id);
    _logger.LogInformation("Completed {Action} for ProjectId={ProjectId}.", nameof(GetById), id);
    return Ok(result);
  }

  [HttpPatch("update/{id}")]
  public async Task<IActionResult> Update([FromBody] UpdateProjectDto updateProjectDto, [FromRoute] string id)
  {
    _logger.LogInformation("Started {Action} for ProjectId={ProjectId}.", nameof(Update), id);
    var result = await _projectService.UpdateProject(id, updateProjectDto);
    _logger.LogInformation("Completed {Action} for ProjectId={ProjectId}.", nameof(Update), id);
    return Ok(result);
  }

  [HttpPatch("delete/{id}")]
  public async Task<IActionResult> Delete([FromRoute] string id)
  {
    _logger.LogInformation("Started {Action} for ProjectId={ProjectId}.", nameof(Delete), id);
    var result = await _projectService.DeleteProject(id);
    _logger.LogInformation("Completed {Action} for ProjectId={ProjectId}.", nameof(Delete), id);
    return Ok(result);
  }

}
