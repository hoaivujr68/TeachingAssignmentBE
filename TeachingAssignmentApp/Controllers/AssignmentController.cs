﻿using Microsoft.AspNetCore.Authorization;
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
        private readonly ICuckooAssignmentService _cuckooAssignmentService;

        public AssignmentController(IAssignmentService teacherAssignmentService, ICuckooAssignmentService cuckooAssignmentService)
        {
            _assignmentService = teacherAssignmentService;
            _cuckooAssignmentService = cuckooAssignmentService;
        }

        [HttpGet("teachers")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<TeacherInputModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTeacherInfo()
        {
            var result = await _assignmentService.GetAllTeacherInfo();
            return Ok(result);
        }

        [HttpGet("classes")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<ClassInputModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllClassInfo()
        {
            var result = await _assignmentService.GetAllClassInfo();
            return Ok(result);
        }

        [HttpGet("aspirations")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<AspirationInputModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAspirationInfo()
        {
            var result = await _assignmentService.GetAllAspirationInfo();
            return Ok(result);
        }

        [HttpGet("teaching")]
        [Authorize]
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

        [HttpGet("cuckoo-teaching")]
        [ProducesResponseType(typeof(ResponsePagination<SolutionModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CuckooTeachingAssignment()
        {
            try
            {
                var result = await _cuckooAssignmentService.TeachingAssignmentCuckooSearch();
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
        [Authorize]
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
