using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeachingAssignmentApp.Business.Feedback;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Controllers
{
    [Route("api/feedback")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackReposiotry _feedbackReposiotry;
        public FeedbackController(IFeedbackReposiotry feedbackReposiotry)
        {
            _feedbackReposiotry = feedbackReposiotry;
        }


        [HttpPost("filter")]
        [Authorize]
        [ProducesResponseType(typeof(ResponsePagination<Feedback>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllFeedbacks(
            [FromBody] QueryModel queryModel)
        {
            var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role == null)
            {
                return Unauthorized("Role not found in the request.");
            }
            var result = await _feedbackReposiotry.GetAllAsync(queryModel, role);
            return Ok(result);
        }
        // GET: api/feedbacks/{id}
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(Feedback), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFeedbackById(Guid id)
        {
            var feedback = await _feedbackReposiotry.GetByIdAsync(id);
            if (feedback == null)
            {
                return NotFound(new { message = "Feedback not found" });
            }
            return Ok(feedback);
        }

        // POST: api/feedbacks
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(Feedback), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateFeedback([FromBody] Feedback feedback)
        {
            if (feedback == null)
            {
                return BadRequest(new { message = "Invalid feedback data" });
            }

            var createdFeedback = await _feedbackReposiotry.CreateAsync(feedback);
            return CreatedAtAction(nameof(GetFeedbackById), new { id = createdFeedback.Id }, createdFeedback);
        }

        // PUT: api/feedbacks/{id}
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(Feedback), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateFeedback(Guid id, [FromBody] Feedback feedback)
        {
            if (feedback == null || id != feedback.Id)
            {
                return BadRequest(new { message = "Feedback data is invalid" });
            }

            var updatedFeedback = await _feedbackReposiotry.UpdateAsync(id, feedback);
            if (updatedFeedback == null)
            {
                return NotFound(new { message = "Feedback not found" });
            }

            return Ok(updatedFeedback);
        }

        // DELETE: api/feedbacks/{id}
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFeedback(Guid id)
        {
            var isDeleted = await _feedbackReposiotry.DeleteAsync(id);
            if (!isDeleted)
            {
                return NotFound(new { message = "Feedback not found" });
            }

            return NoContent();
        }
    }
}
