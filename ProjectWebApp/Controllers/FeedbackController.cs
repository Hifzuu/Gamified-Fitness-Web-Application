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
