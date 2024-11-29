using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TeachingAssignmentApp.Business.Aspiration;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Controllers
{
    [Route("api/aspiration")]
    [ApiController]
    public class AspirationController : ControllerBase
    {
        private readonly IAspirationService _aspirationService;

        public AspirationController(IAspirationService aspirationService)
        {
            _aspirationService = aspirationService;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<AspirationModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAspirations(
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            [FromQuery] string filter = "{ }")
        {
            var filterObject = JsonSerializer.Deserialize<QueryModel>(filter);
            filterObject.PageSize = size;
            filterObject.CurrentPage = page;
            var result = await _aspirationService.GetAllAsync(filterObject);
            return Ok(result);
        }

        [HttpPost("filter")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<AspirationModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAspirations(
            [FromBody] QueryModel queryModel)
        {
            var result = await _aspirationService.GetAllAsync(queryModel);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAspirationById(Guid id)
        {
            var aspiration = await _aspirationService.GetByIdAsync(id);
            if (aspiration == null)
            {
                return NotFound();
            }
            return Ok(aspiration);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddAspiration([FromBody] AspirationModel aspirationModel)
        {
            await _aspirationService.AddAsync(aspirationModel);
            return CreatedAtAction(nameof(GetAspirationById), new { id = aspirationModel.Id }, aspirationModel);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAspiration(Guid id, [FromBody] AspirationModel aspirationModel)
        {
            if (id != aspirationModel.Id)
            {
                return BadRequest("Aspiration ID mismatch.");
            }

            var existingAspiration = await _aspirationService.GetByIdAsync(id);
            if (existingAspiration == null)
            {
                return NotFound();
            }

            await _aspirationService.UpdateAsync(aspirationModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAspiration(Guid id)
        {
            var existingAspiration = await _aspirationService.GetByIdAsync(id);
            if (existingAspiration == null)
            {
                return NotFound();
            }

            await _aspirationService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("import")]
        [Authorize]
        public async Task<IActionResult> ImportAspirations(IFormFile file)
        {
            try
            {
                await _aspirationService.ImportAspirationsAsync(file);
                return Ok(new { Success = true, Message = "Aspirations imported successfully." });
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
