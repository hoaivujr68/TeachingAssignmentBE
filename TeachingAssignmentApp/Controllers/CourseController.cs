using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TeachingAssignmentApp.Business.Course;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Controllers
{
    [Route("api/course")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetCourseById(Guid id)
        {
            var course = await _courseService.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return Ok(course);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddCourse([FromBody] CourseModel courseModel)
        {
            await _courseService.AddAsync(courseModel);
            return CreatedAtAction(nameof(GetCourseById), new { id = courseModel.Id }, courseModel);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] CourseModel courseModel)
        {
            if (id != courseModel.Id)
            {
                return BadRequest("Course ID mismatch.");
            }

            var existingCourse = await _courseService.GetByIdAsync(id);
            if (existingCourse == null)
            {
                return NotFound();
            }

            await _courseService.UpdateAsync(courseModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var existingCourse = await _courseService.GetByIdAsync(id);
            if (existingCourse == null)
            {
                return NotFound();
            }

            await _courseService.DeleteAsync(id);
            return NoContent();
        }
    }
}
