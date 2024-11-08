﻿using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;
using TeachingAssignmentApp.Services;

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

        [HttpGet]
        [ProducesResponseType(typeof(ResponsePagination<CourseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCourses(
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            [FromQuery] string filter = "{ }")
        {
            var filterObject = JsonSerializer.Deserialize<CourseQueryModel>(filter);
            filterObject.PageSize = size;
            filterObject.CurrentPage = page;
            var result = await _courseService.GetAllAsync(filterObject);
            return Ok(result);
        }

        [HttpGet("{id}")]
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
        public async Task<IActionResult> AddCourse([FromBody] CourseModel courseModel)
        {
            await _courseService.AddAsync(courseModel);
            return CreatedAtAction(nameof(GetCourseById), new { id = courseModel.Id }, courseModel);
        }

        [HttpPut("{id}")]
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
