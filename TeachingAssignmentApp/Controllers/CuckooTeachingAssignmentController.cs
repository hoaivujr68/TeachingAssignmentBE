using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeachingAssignmentApp.Business.Assignment;
using TeachingAssignmentApp.Business.CuckooAssignment;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Controllers
{
    [Route("api/cuckoo-teaching-assignment")]
    [ApiController]
    public class CuckooTeachingAssignmentController : ControllerBase
    {
        private readonly ICuckooTeachingRepository _cuckooAssignmentRepository;
        public CuckooTeachingAssignmentController(ICuckooTeachingRepository cuckooAssignmentRepository)
        {
            _cuckooAssignmentRepository = cuckooAssignmentRepository;
        }

        [HttpPost("filter-assignment")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<CuckooTeachingAssignment>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTeachingAssignment(
            [FromBody] QueryModel queryModel)
        {
            var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role == null)
            {
                return Unauthorized("Role not found in the request.");
            }
            var result = await _cuckooAssignmentRepository.GetAllAsync(queryModel, role);
            return Ok(result);
        }

        [HttpPost("filter-not-assignment")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<ClassModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllClassNotAssignment(
            [FromBody] QueryModel queryModel)
        {
            var result = await _cuckooAssignmentRepository.GetClassNotAssignmentAsync(queryModel);
            return Ok(result);
        }

        [HttpGet("range")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<ClassModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRangeGdTeaching()
        {
            var result = await _cuckooAssignmentRepository.GetRangeGdTeaching();
            return Ok(result);
        }

        [HttpPost("teacher-not-assignment")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<TeacherModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTeacherNotAssignment(
            [FromBody] QueryModel queryModel)
        {
            var result = await _cuckooAssignmentRepository.GetTeacherNotAssignmentAsync(queryModel);
            return Ok(result);
        }

        [HttpPost("teacher-by-class")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<TeacherModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTeacherByClassId(
            [FromBody] string classId)
        {
            var result = await _cuckooAssignmentRepository.GetAvailableTeachersForClass(classId);
            return Ok(result);
        }

        [HttpPost("total-gd")]
        [Authorize]
        public async Task<double> GetTotalGdTeachingByTeacherCode(
            [FromBody] string teacherCode)
        {
            return await _cuckooAssignmentRepository.GetTotalGdTeachingByTeacherCode(teacherCode);
        }

        [HttpPut("")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<CuckooTeachingAssignment>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTeachingAssignment(
            [FromBody] CuckooTeachingAssignment teachingAssignment)
        {
            var result = await _cuckooAssignmentRepository.UpdateAsync(teachingAssignment.Id, teachingAssignment);
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
                var fileContent = await _cuckooAssignmentRepository.ExportTeachingAssignment(role);
                var fileName = "TeachingAssignmentss.xlsx";

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
                var fileContent = await _cuckooAssignmentRepository.ExportClassAssignment(role);
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
