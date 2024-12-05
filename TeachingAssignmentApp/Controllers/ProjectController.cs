using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using TeachingAssignmentApp.Business.Project;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Controllers
{
    [Route("api/project")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost("filter")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<ProjectModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProjects(
            [FromBody] QueryModel queryModel)
        {
            var result = await _projectService.GetAllAsync(queryModel);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            var project = await _projectService.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return Ok(project);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddProject([FromBody] ProjectModel projectModel)
        {
            await _projectService.AddAsync(projectModel);
            return CreatedAtAction(nameof(GetProjectById), new { id = projectModel.Id }, projectModel);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] ProjectModel projectModel)
        {
            if (id != projectModel.Id)
            {
                return BadRequest("Project ID mismatch.");
            }

            var existingProject = await _projectService.GetByIdAsync(id);
            if (existingProject == null)
            {
                return NotFound();
            }

            await _projectService.UpdateAsync(projectModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var existingProject = await _projectService.GetByIdAsync(id);
            if (existingProject == null)
            {
                return NotFound();
            }

            await _projectService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportProjects(IFormFile file)
        {
            try
            {
                await _projectService.ImportProjectsAsync(file);
                return Ok(new { Success = true, Message = "Projects imported successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = ex.Message });
            }
        }
    }
}
