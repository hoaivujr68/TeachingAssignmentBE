using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using TeachingAssignmentApp.Business.Assignment;
using TeachingAssignmentApp.Business.TeachingAssignment;
using TeachingAssignmentApp.Business.TeachingAssignment.Model;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Controllers
{
    [Route("api/teaching-assignment")]
    [ApiController]
    public class TeachingAssignmentController : ControllerBase
    {
        private readonly ITeachingAssignmentRepository _teachingAssignmentRepository;

        public TeachingAssignmentController(ITeachingAssignmentRepository teachingAssignmentRepository)
        {
            _teachingAssignmentRepository = teachingAssignmentRepository;
        }

        [HttpPost("filter-assignment")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<TeachingAssignment>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTeachingAssignment(
            [FromBody] QueryModel queryModel)
        {
            var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role == null)
            {
                return Unauthorized("Role not found in the request.");
            }
            var result = await _teachingAssignmentRepository.GetAllAsync(queryModel, role);
            return Ok(result);
        }


        [HttpGet("schedule")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<TeachingAssignment>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetScheduleByRole()
        {
            var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role == null)
            {
                return Unauthorized("Role not found in the request.");
            }
            var result = await _teachingAssignmentRepository.GetTimeTableByRole(role);
            return Ok(result);
        }

        [HttpPost("filter-not-assignment")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<ClassModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllClassNotAssignment(
            [FromBody] QueryModel queryModel)
        {
            var result = await _teachingAssignmentRepository.GetClassNotAssignmentAsync(queryModel);
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
                await _teachingAssignmentRepository.SwapTeacherAssignmentAsync(teacherAssignmentId1, teacherAssignmentId2);

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

        [HttpGet("range")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<ClassModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRangeGdTeaching()
        {
            var result = await _teachingAssignmentRepository.GetRangeGdTeaching();
            return Ok(result);
        }

        [HttpGet("result")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObject<ResultModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetResultModel()
        {
            var result = await _teachingAssignmentRepository.GetResultModel();
            return Ok(result);
        }

        [HttpPost("teacher-not-assignment")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<TeacherModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTeacherNotAssignment(
            [FromBody] QueryModel queryModel)
        {
            var result = await _teachingAssignmentRepository.GetTeacherNotAssignmentAsync(queryModel);
            return Ok(result);
        }

        [HttpPost("teacher-by-class")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<TeacherModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTeacherByClassId(
            [FromBody] string classId)
        {
            var result = await _teachingAssignmentRepository.GetAvailableTeachersForClass(classId);
            return Ok(result);
        }

        [HttpPost("total-gd")]
        [Authorize]
        public async Task<double> GetTotalGdTeachingByTeacherCode(
            [FromBody] string teacherCode)
        {
             return await _teachingAssignmentRepository.GetTotalGdTeachingByTeacherCode(teacherCode);         
        }

        [HttpPut("")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<TeachingAssignment>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTeachingAssignment(
            [FromBody] TeachingAssignment teachingAssignment)
        {
            var result = await _teachingAssignmentRepository.UpdateAsync(teachingAssignment.Id, teachingAssignment);
            return Ok(result);
        }

        [HttpGet("export")]
        [Authorize]
        public async Task<IActionResult> ExportTeachingAssignment()
        {
            try
            {
                // Gọi service để lấy dữ liệu file Excel
                var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (role == null)
                {
                    return Unauthorized("Role not found in the request.");
                }
                var fileContent = await _teachingAssignmentRepository.ExportTeachingAssignment(role);
                var fileName = "TeachingAssignmentss.xlsx";

                // Trả về file dưới dạng tải xuống
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("export-quota")]
        public async Task<IActionResult> ExportTeachingAssignmentQuota()
        {
            try
            {
                var fileContent = await _teachingAssignmentRepository.ExportTeacherAssignmentByQuota();
                var fileName = "TeachingAssignmentStatistical.xlsx";

                // Trả về file dưới dạng tải xuống
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("export-class")]
        [Authorize]
        public async Task<IActionResult> ExportClassAssignment()
        {
            try
            {
                // Gọi service để lấy dữ liệu file Excel
                var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (role == null)
                {
                    return Unauthorized("Role not found in the request.");
                }
                var fileContent = await _teachingAssignmentRepository.ExportClassAssignment(role);
                var fileName = "ClassAssignmentss.xlsx";

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
