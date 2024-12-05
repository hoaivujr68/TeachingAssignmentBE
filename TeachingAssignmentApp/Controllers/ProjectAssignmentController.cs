using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

        [HttpPost("filter-assignment")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<ProjectAssigment>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProjectAssignment(
            [FromBody] QueryModel queryModel)
        {
            var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role == null)
            {
                return Unauthorized("Role not found in the request.");
            }

            var result = await _projectAssignmentRepository.GetAllAsync(queryModel, role);
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

        [HttpPost("teacher-not-assignment")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<TeacherModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTeacherNotAssignment(
            [FromBody] QueryModel queryModel)
        {
            var result = await _projectAssignmentRepository.GetTeacherNotAssignmentAsync(queryModel);
            return Ok(result);
        }

        [HttpGet("export")]
        [Authorize]
        public async Task<IActionResult> ExportProjectAssignment()
        {
            try
            {
                // Gọi service để lấy dữ liệu file Excel
                var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (role == null)
                {
                    return Unauthorized("Role not found in the request.");
                }
                var fileContent = await _projectAssignmentRepository.ExportProjectAssignment(role);
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
