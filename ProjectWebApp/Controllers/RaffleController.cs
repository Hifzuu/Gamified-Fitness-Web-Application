using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectWebApp.Data;
using ProjectWebApp.Models;

namespace ProjectWebApp.Controllers
{
    public class RaffleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RaffleController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var raffles = await _context.Raffles
                    .Select(r => new RaffleViewModel
                    {
                        RaffleId = r.RaffleId,
                        Name = r.Name,
                        Description = r.Description,
                        StartDate = r.StartDate,
                        EndDate = r.EndDate,
                        Cost = r.Cost,
                        ImageUrl = r.ImageUrl
                    })
                    .ToListAsync();
            return View(raffles);
        }

        [HttpPost]
        public async Task <IActionResult> CheckUserPoints(int raffleCost)
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);

            // Check if the user has enough points
            if (user.Points >= raffleCost)
            {
                // User has enough points, proceed with entering the raffle (add your logic here)
                return Json(new { success = true, message = "Raffle entered successfully!" + user.Points + "raff: " + raffleCost });
            }
            else
            {
                // User does not have enough points, send a friendly message
                return Json(new { success = false, message = "You do not have enough points to enter this raffle. Earn more points to participate!" });
            }
        }


    }
}
