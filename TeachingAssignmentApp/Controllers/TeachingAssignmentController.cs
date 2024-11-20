using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TeachingAssignmentApp.Business.Assignment;
using TeachingAssignmentApp.Business.TeachingAssignment;
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

        [HttpGet("assignment")]
        [ProducesResponseType(typeof(ResponsePagination<TeachingAssignment>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTeachingAssignment(
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            [FromQuery] string filter = "{ }")
        {
            var filterObject = JsonSerializer.Deserialize<QueryModel>(filter);
            filterObject.PageSize = size;
            filterObject.CurrentPage = page;
            var result = await _teachingAssignmentRepository.GetAllAsync(filterObject);
            return Ok(result);
        }

        [HttpPost("filter-assignment")]
        [ProducesResponseType(typeof(ResponsePagination<TeachingAssignment>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTeachingAssignment(
            [FromBody] QueryModel queryModel)
        {
            var result = await _teachingAssignmentRepository.GetAllAsync(queryModel);
            return Ok(result);
        }

        [HttpGet("not-assignment")]
        [ProducesResponseType(typeof(ResponsePagination<ClassModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTeachingNotAssignment(
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            [FromQuery] string filter = "{ }")
        {
            var filterObject = JsonSerializer.Deserialize<QueryModel>(filter);
            filterObject.PageSize = size;
            filterObject.CurrentPage = page;
            var result = await _teachingAssignmentRepository.GetClassNotAssignmentAsync(filterObject);
            return Ok(result);
        }


        [HttpPost("filter-not-assignment")]
        [ProducesResponseType(typeof(ResponsePagination<ClassModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTeachingNotAssignment(
            [FromBody] QueryModel queryModel)
        {
            var result = await _teachingAssignmentRepository.GetClassNotAssignmentAsync(queryModel);
            return Ok(result);
        }
    }
}
