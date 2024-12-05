using Microsoft.AspNetCore.Mvc;
using TeachingAssignmentApp.Business.ETLTeacher;
using TeachingAssignmentApp.Business.ETLTeacher.Model;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;

namespace TeachingAssignmentApp.Controllers
{
    [Route("api/teacher-etl")]
    [ApiController]
    public class ETLTeacherController : ControllerBase
    {
        private readonly ITeacherETLService _teacherETLService;
        public ETLTeacherController (ITeacherETLService teacherETLService)
        {
            _teacherETLService = teacherETLService;
        }

        [HttpPost()]
        [ProducesResponseType(typeof(ResponseObject<ETLTeacher>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromBody] ETLTeacherQueryModel eTLTeacherQueryModel)
        {
            var res = await _teacherETLService.GetAllAync(eTLTeacherQueryModel);
            return Ok(res);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ResponseObject<ETLTeacher>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshDataAsync([FromBody] string role)
        {
            var res = await _teacherETLService.RefreshAsync(role);
            return Ok(res);
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(ResponseObject<ETLTeacher>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateAll()
        {
            await _teacherETLService.CreateAll();
            return Ok();
        }
    }
}
