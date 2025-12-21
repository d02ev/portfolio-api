using Application.Dto;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/project")]
public class ProjectController(IProjectService projectService) : ControllerBase
{
  private readonly IProjectService _projectService = projectService;

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] ProjectDto projectDto)
  {
    var result = await _projectService.CreateProject(projectDto);
    return CreatedAtAction(nameof(Create), result);
  }

  [AllowAnonymous]
  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var result = await _projectService.FetchAllProjects();
    return Ok(result);
  }

  [HttpGet("deleted")]
  public async Task<IActionResult> GetAllDeleted()
  {
    var result = await _projectService.FetchAllDeletedProjects();
    return Ok(result);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetById([FromRoute] string id)
  {
    var result = await _projectService.FetchProjectById(id);
    return Ok(result);
  }

  [HttpPatch("update/{id}")]
  public async Task<IActionResult> Update([FromBody] UpdateProjectDto updateProjectDto, [FromRoute] string id)
  {
    var result = await _projectService.UpdateProject(id, updateProjectDto);
    return Ok(result);
  }

  [HttpPatch("delete/{id}")]
  public async Task<IActionResult> Delete([FromRoute] string id)
  {
    var result = await _projectService.DeleteProject(id);
    return Ok(result);
  }

}