using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TeachingAssignmentApp.Business.Class;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Controllers
{
    [Route("api/class")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classesService;

        public ClassController(IClassService classesService)
        {
            _classesService = classesService;
        }

        [HttpPost("filter")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<ClassModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllClasses(
            [FromBody] QueryModel queryModel)
        {
            var result = await _classesService.GetAllAsync(queryModel);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetClassById(Guid id)
        {
            var classes = await _classesService.GetByIdAsync(id);
            if (classes == null)
            {
                return NotFound();
            }
            return Ok(classes);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddClass([FromBody] ClassModel classesModel)
        {
            await _classesService.AddAsync(classesModel);
            return CreatedAtAction(nameof(GetClassById), new { id = classesModel.Id }, classesModel);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateClass(Guid id, [FromBody] ClassModel classesModel)
        {
            if (id != classesModel.Id)
            {
                return BadRequest("Class ID mismatch.");
            }

            var existingClass = await _classesService.GetByIdAsync(id);
            if (existingClass == null)
            {
                return NotFound();
            }

            await _classesService.UpdateAsync(classesModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteClass(Guid id)
        {
            var existingClass = await _classesService.GetByIdAsync(id);
            if (existingClass == null)
            {
                return NotFound();
            }

            await _classesService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportClasses(IFormFile file)
        {
            try
            {
                await _classesService.ImportClassAsync(file);
                return Ok(new { Success = true, Message = "Classes imported successfully." });
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
