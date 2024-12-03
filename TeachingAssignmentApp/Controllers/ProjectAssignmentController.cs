using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
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
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<ProjectAssigment>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProjectAssignment(
            [FromBody] QueryModel queryModel)
        {
            var result = await _projectAssignmentRepository.GetAllAsync(queryModel);
            return Ok(result);
        }

        [HttpGet("not-assignment")]
        [Authorize]
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
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<AspirationModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProjectNotAssignment(
            [FromBody] QueryModel queryModel)
        {
            var result = await _projectAssignmentRepository.GetProjectNotAssignmentAsync(queryModel);
            return Ok(result);
        }

        [HttpGet("export")]
        [Authorize]
        public async Task<IActionResult> ExportProjectAssignment()
        {
            try
            {
                // Gọi service để lấy dữ liệu file Excel
                var fileContent = await _projectAssignmentRepository.ExportProjectAssignment();
                var fileName = "ProjectAssignmentss.xlsx";

                // Trả về file dưới dạng tải xuống
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
