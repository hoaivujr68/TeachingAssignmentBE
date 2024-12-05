using Microsoft.AspNetCore.Mvc;
using TeachingAssignmentApp.Business.ETLGeneral;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;

namespace TeachingAssignmentApp.Controllers
{
    [Route("api/general")]
    [ApiController]
    public class ETLGeneralController : ControllerBase
    {
        private readonly IGeneralService _generalService;

        public ETLGeneralController(IGeneralService generalService)
        {
            _generalService = generalService;
        }

        [HttpPost()]
        [ProducesResponseType(typeof(ResponseObject<ETLGeneral>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromBody] string type)
        {
            var res = await _generalService.GetAllAync(type);
            return Ok(res);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ResponseObject<ETLGeneral>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshDataAsync()
        {
            var res = await _generalService.RefreshAsync();
            return Ok(res);
        }
    }
}
