using Microsoft.AspNetCore.Mvc;
using TeachingAssignmentApp.Business.Assignment;
using TeachingAssignmentApp.Business.Assignment.Model;
using TeachingAssignmentApp.Helper;

namespace TeachingAssignmentApp.Controllers
{
    [Route("api/assignment")]
    [ApiController]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;

        public AssignmentController(IAssignmentService teacherAssignmentService)
        {
            _assignmentService = teacherAssignmentService;
        }

        [HttpGet("teachers")]
        [ProducesResponseType(typeof(ResponsePagination<TeacherInputModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTeacherInfo()
        {
            var result = await _assignmentService.GetAllTeacherInfo();
            return Ok(result);
        }

        [HttpGet("classes")]
        [ProducesResponseType(typeof(ResponsePagination<ClassInputModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllClassInfo()
        {
            var result = await _assignmentService.GetAllClassInfo();
            return Ok(result);
        }

        [HttpGet("aspirations")]
        [ProducesResponseType(typeof(ResponsePagination<AspirationInputModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAspirationInfo()
        {
            var result = await _assignmentService.GetAllAspirationInfo();
            return Ok(result);
        }

        [HttpGet("teaching")]
        [ProducesResponseType(typeof(ResponsePagination<SolutionModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> TeachingAssignment()
        {
            try
            {
                var result = await _assignmentService.TeachingAssignment();
                return Ok(result);
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

        [HttpGet("aspirating")]
        [ProducesResponseType(typeof(ResponsePagination<SolutionProjectModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ProjectAssignment()
        {
            try
            {
                var result = await _assignmentService.ProjectAssignment();
                return Ok(result);
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
