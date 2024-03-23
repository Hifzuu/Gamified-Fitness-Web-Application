using Microsoft.AspNetCore.Mvc;
using ProjectWebApp.Data;
using ProjectWebApp.Models;

namespace ProjectWebApp.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback([FromBody] Feedback feedback)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(feedback.Name))
                {
                    return BadRequest("Name is required. Please try again.");
                }

                if (string.IsNullOrWhiteSpace(feedback.Email))
                {
                    return BadRequest("Email is required. Please try again.");
                }

                if (string.IsNullOrWhiteSpace(feedback.FeedbackType))
                {
                    return BadRequest("Feedback type is required. Please select an option and try again.");
                }

                if (string.IsNullOrWhiteSpace(feedback.FeedbackDetails))
                {
                    return BadRequest("Feedback details are required. Please try again.");
                }

                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();
                return Ok("Feedback submitted successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
