using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ProjectWebApp.Data;
using ProjectWebApp.Models;

namespace ProjectWebApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DashboardController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            // Retrieve weight data
            var weightData = _context.WeightEntries
                .Where(w => w.UserId == userId)
                .OrderBy(w => w.Date)
                .ToList();

            // Retrieve login streak information
            var loginStreak = _context.LoginStreaks
                .Where(ls => ls.UserId == userId)
                .OrderByDescending(ls => ls.LastLoginTime)
                .FirstOrDefault();

            // Pass both weight data and login streak information to the view
            var viewModel = new DashboardViewModel
            {
                WeightData = weightData,
                LoginStreak = loginStreak
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddWeightEntry(double newWeight)
        {
            var userId = _userManager.GetUserId(User);

            // Create a new UserWeight object with the entered data and the current date
            var newWeightEntry = new WeightEntry
            {
                UserId = userId,
                Date = DateTime.Now,
                Weight = newWeight
            };

            // Add the new entry to the database
            _context.WeightEntries.Add(newWeightEntry);
            _context.SaveChanges();

            // Redirect back to the dashboard with the updated data
            return RedirectToAction("Index");
        }


    }
}