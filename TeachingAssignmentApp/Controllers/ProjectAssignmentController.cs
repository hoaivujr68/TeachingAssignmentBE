using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TeachingAssignmentApp.Business.ProjectAssigment;
using TeachingAssignmentApp.Business.TeachingAssignment;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Controllers
{
    [Route("api/project-assignment")]
    [ApiController]
    public class ProjectAssignmentController : ControllerBase
    {
        private readonly IProjectAssignmentRepository _projectAssignmentRepository;
        public ProjectAssignmentController(IProjectAssignmentRepository projectAssignmentRepository)
        {
            _projectAssignmentRepository = projectAssignmentRepository;
        }

        [HttpGet("assignment")]
        [ProducesResponseType(typeof(ResponsePagination<ProjectAssigment>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProjectAssignment(
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            [FromQuery] string filter = "{ }")
        {
            var filterObject = JsonSerializer.Deserialize<QueryModel>(filter);
            filterObject.PageSize = size;
            filterObject.CurrentPage = page;
            var result = await _projectAssignmentRepository.GetAllAsync(filterObject);
            return Ok(result);
        }

        [HttpPost("filter-assignment")]
        [ProducesResponseType(typeof(ResponsePagination<ProjectAssigment>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProjectAssignment(
            [FromBody] QueryModel queryModel)
        {
            var result = await _projectAssignmentRepository.GetAllAsync(queryModel);
            return Ok(result);
        }

        [HttpGet("not-assignment")]
        [ProducesResponseType(typeof(ResponsePagination<AspirationModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProjectNotAssignment(
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            [FromQuery] string filter = "{ }")
        {
            var filterObject = JsonSerializer.Deserialize<QueryModel>(filter);
            filterObject.PageSize = size;
            filterObject.CurrentPage = page;
            var result = await _projectAssignmentRepository.GetProjectNotAssignmentAsync(filterObject);
            return Ok(result);
        }

        [HttpPost("filter-not-assignment")]
        [ProducesResponseType(typeof(ResponsePagination<AspirationModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProjectNotAssignment(
            [FromBody] QueryModel queryModel)
        {
            var result = await _projectAssignmentRepository.GetProjectNotAssignmentAsync(queryModel);
            return Ok(result);
        }
    }
}
