using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using TeachingAssignmentApp.Business.ProjectAssigment;
using TeachingAssignmentApp.Business.TeachingAssignment;
using TeachingAssignmentApp.Business.TeachingAssignment.Model;
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

        [HttpGet("result")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObject<ResultModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetResultModel()
        {
            var result = await _projectAssignmentRepository.GetResultAsync();
            return Ok(result);
        }

        [HttpGet("result-error")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObject<TeacherResultError>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetResultErrorModel()
        {
            var result = await _projectAssignmentRepository.GetMaxAsync();
            return Ok(result);
        }

        [HttpGet("range")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<ClassModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRangeGdTeaching()
        {
            var result = await _projectAssignmentRepository.GetRangeGdInstruct();
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

        [HttpPost("teacher-by-student")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<TeacherModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTeacherByStudentId(
    [FromBody] string studentId)
        {
            var result = await _projectAssignmentRepository.GetAvailableTeachersForStudentId(studentId);
            return Ok(result);
        }

        [HttpPost("total-gd")]
        [Authorize]
        public async Task<double> GetTotalGdTeachingByTeacherCode(
            [FromBody] string teacherCode)
        {
            return await _projectAssignmentRepository.GetTotalGdTeachingByTeacherCode(teacherCode);
        }

        [HttpPut("")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<ProjectAssigment>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProejctAssignment(
            [FromBody] ProjectAssigment projectAssignment)
        {
            var result = await _projectAssignmentRepository.UpdateAsync(projectAssignment.Id, projectAssignment);
            return Ok(result);
        }

        [HttpPost("swap-assignment")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SwapTeacherAssignmentAsync([FromBody] SwapModel swapModel)
        {
            // Kiểm tra đầu vào
            if (swapModel?.TeacherAssignmentIds == null || swapModel.TeacherAssignmentIds.Length != 2)
            {
                return BadRequest("Invalid input. Exactly two assignment IDs are required.");
            }

            try
            {
                // Lấy ID từ model
                var teacherAssignmentId1 = Guid.Parse(swapModel.TeacherAssignmentIds[0]);
                var teacherAssignmentId2 = Guid.Parse(swapModel.TeacherAssignmentIds[1]);

                // Gọi hàm xử lý hoán đổi
                await _projectAssignmentRepository.SwapTeacherAssignmentAsync(teacherAssignmentId1, teacherAssignmentId2);

                return Ok(new { message = "Teacher assignments swapped successfully." });
            }
            catch (FormatException ex)
            {
                // Xử lý lỗi khi không thể parse GUID
                return BadRequest($"Invalid ID format: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi chung
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
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

        [HttpGet("export-quota")]
        public async Task<IActionResult> ExportProjectAssignmentByQuota()
        {
            try
            {
                var fileContent = await _projectAssignmentRepository.ExportProjectAssignmentByQuota();
                var fileName = "ProjectAssignmentStatistical.xlsx";

                // Trả về file dưới dạng tải xuống
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("export-aspiration")]
        [Authorize]
        public async Task<IActionResult> ExportAspirationAssignment()
        {
            try
            {
                // Gọi service để lấy dữ liệu file Excel
                var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (role == null)
                {
                    return Unauthorized("Role not found in the request.");
                }
                var fileContent = await _projectAssignmentRepository.ExportAspirationAssignment(role);
                var fileName = "AspirationAssignmentss.xlsx";

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
