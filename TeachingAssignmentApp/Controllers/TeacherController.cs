using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        [ProducesResponseType(typeof(ResponsePagination<TeacherModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTeachers(
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            [FromQuery] string filter = "{ }")
        {
            var filterObject = JsonSerializer.Deserialize<QueryModel>(filter);
            filterObject.PageSize = size;
            filterObject.CurrentPage = page;
            var result = await _teacherService.GetAllAsync(filterObject);
            return Ok(result);
        }

        [HttpPost("filter")]
        [ProducesResponseType(typeof(ResponsePagination<TeacherModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTeachers(
            [FromBody] QueryModel queryModel)
        {
            var result = await _teacherService.GetAllAsync(queryModel);
            return Ok(result);
        }

        [HttpGet("{id}")]
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
        public async Task<IActionResult> AddTeacher([FromBody] TeacherModel teacherModel)
        {
            await _teacherService.AddAsync(teacherModel);
            return CreatedAtAction(nameof(GetTeacherById), new { id = teacherModel.Id }, teacherModel);
        }

        [HttpPut("{id}")]
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
