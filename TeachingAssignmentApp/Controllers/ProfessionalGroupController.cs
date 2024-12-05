using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TeachingAssignmentApp.Business.ProfessionalGroup;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Controllers
{
    [Route("api/professional-group")]
    [ApiController]
    public class ProfessionalGroupController : ControllerBase
    {
        private readonly IProfessionalGroupService _professionalGroupService;

        public ProfessionalGroupController(IProfessionalGroupService professionalGroupService)
        {
            _professionalGroupService = professionalGroupService;
        }

        [HttpPost("filter")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<ProfessionalGroupModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProfessionalGroups(
            [FromBody] QueryModel queryModel)
        {
            var result = await _professionalGroupService.GetAllAsync(queryModel);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetProfessionalGroupById(Guid id)
        {
            var professionalGroup = await _professionalGroupService.GetByIdAsync(id);
            if (professionalGroup == null)
            {
                return NotFound();
            }
            return Ok(professionalGroup);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddProfessionalGroup([FromBody] ProfessionalGroupModel professionalGroupModel)
        {
            await _professionalGroupService.AddAsync(professionalGroupModel);
            return CreatedAtAction(nameof(GetProfessionalGroupById), new { id = professionalGroupModel.Id }, professionalGroupModel);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProfessionalGroup(Guid id, [FromBody] ProfessionalGroupModel professionalGroupModel)
        {
            if (id != professionalGroupModel.Id)
            {
                return BadRequest("ProfessionalGroup ID mismatch.");
            }

            var existingProfessionalGroup = await _professionalGroupService.GetByIdAsync(id);
            if (existingProfessionalGroup == null)
            {
                return NotFound();
            }

            await _professionalGroupService.UpdateAsync(professionalGroupModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProfessionalGroup(Guid id)
        {
            var existingProfessionalGroup = await _professionalGroupService.GetByIdAsync(id);
            if (existingProfessionalGroup == null)
            {
                return NotFound();
            }

            await _professionalGroupService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportProfessionalGroups(IFormFile file)
        {
            try
            {
                await _professionalGroupService.ImportProfessionalGroupsAsync(file);
                return Ok(new { Success = true, Message = "Professional Group imported successfully." });
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
