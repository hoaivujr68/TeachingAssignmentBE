using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using TeachingAssignmentApp.Business.Teacher;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Controllers
{
    [Route("api/teacher")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeacherController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [HttpPost("filter")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<TeacherModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTeachers(
            [FromBody] QueryModel queryModel)
        {
            var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role == null)
            {
                return Unauthorized("Role not found in the request.");
            }
            var result = await _teacherService.GetAllAsync(queryModel, role);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTeacherById(Guid id)
        {
            var teacher = await _teacherService.GetByIdAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(teacher);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddTeacher([FromBody] TeacherModel teacherModel)
        {
            await _teacherService.AddAsync(teacherModel);
            return CreatedAtAction(nameof(GetTeacherById), new { id = teacherModel.Id }, teacherModel);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTeacher(Guid id, [FromBody] TeacherModel teacherModel)
        {
            if (id != teacherModel.Id)
            {
                return BadRequest("Teacher ID mismatch.");
            }

            var existingTeacher = await _teacherService.GetByIdAsync(id);
            if (existingTeacher == null)
            {
                return NotFound();
            }

            await _teacherService.UpdateAsync(teacherModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTeacher(Guid id)
        {
            var existingTeacher = await _teacherService.GetByIdAsync(id);
            if (existingTeacher == null)
            {
                return NotFound();
            }

            await _teacherService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportTeachers(IFormFile file)
        {
            try
            {
                await _teacherService.ImportTeachersAsync(file);
                return Ok(new { Success = true, Message = "Teachers imported successfully." });
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
