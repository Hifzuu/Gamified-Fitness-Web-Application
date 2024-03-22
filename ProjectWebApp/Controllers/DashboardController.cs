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

            // Retrieve all streak rewards
            var allStreakRewards = _context.StreakRewards
                .OrderBy(sr => sr.Points) // Order by Points, adjust as needed
                .ToList();

            // Check if it's a new year
            if (IsNewYear())
            {
                // Clear user streak rewards for the new year
                ClearUserStreakRewards(userId);

                // Update the last login time in login streak
                loginStreak.LastLoginTime = DateTime.Now;
                _context.SaveChanges();
            }

            // Check if the user's current streak matches any reward days
            var claimableRewards = allStreakRewards
                .Where(sr => loginStreak.CurrentStreak >= sr.Days)
                .ToList();


            // Fetch the user's streak rewards list
            var userStreaksRewardsList = _context.UserStreakRewards
                .Where(ur => ur.UserId == userId)
                .ToList();

            // Add UserStreakReward entries for new claimable rewards
            foreach (var reward in claimableRewards)
            {
                // Check if the user does not already have a claimable reward for the same RewardId (claimed or not)
                var existingClaimableReward = userStreaksRewardsList
                    .FirstOrDefault(ur => ur.RewardId == reward.RewardId);

                if (existingClaimableReward == null)
                {
                    var userStreakReward = new UserStreakReward
                    {
                        UserId = userId,
                        LastLoginTime = loginStreak.LastLoginTime,
                        RewardId = reward.RewardId,
                        Claimed = false // You may set it to true if you want to mark it as claimed initially
                    };

                    _context.UserStreakRewards.Add(userStreakReward);
                }
            }

            // Save changes to the database
            _context.SaveChanges();



            // Pass weight data, login streak, and all streak rewards to the view
            var viewModel = new DashboardViewModel
            {
                WeightData = weightData,
                LoginStreak = loginStreak,
                Rewards = allStreakRewards,
                ClaimableRewards= userStreaksRewardsList,
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

        // Helper method to check if it's a new year
        private bool IsNewYear()
        {
            var userId = _userManager.GetUserId(User);
            // Compare current year with the stored last login year
            var currentYear = DateTime.Now.Year;
            var lastLoginYear = _context.LoginStreaks
                .Where(ls => ls.UserId == userId)
                .Max(ls => ls.LastLoginTime.Year);

            return currentYear > lastLoginYear;
        }

        // Helper method to clear user streak rewards for the new year
        private void ClearUserStreakRewards(string userId)
        {
            var userStreakRewards = _context.UserStreakRewards
                .Where(ur => ur.UserId == userId)
                .ToList();

            _context.UserStreakRewards.RemoveRange(userStreakRewards);
            _context.SaveChanges();
        }


        [HttpPost]
        public IActionResult ClaimReward(int userStreakRewardId)
        {
            // Retrieve the user streak reward
            var userStreakReward = _context.UserStreakRewards.FirstOrDefault(ur => ur.UserStreakRewardId == userStreakRewardId);

            // Check if the userStreakReward exists and has not been claimed
            if (userStreakReward != null && !userStreakReward.Claimed)
            {
                // Find the associated reward based on RewardId
                var claimedReward = _context.StreakRewards.FirstOrDefault(sr => sr.RewardId == userStreakReward.RewardId);

                // Check if the reward is found
                if (claimedReward != null)
                {
                    // Mark the current reward as claimed
                    userStreakReward.Claimed = true;

                    // Retrieve the user associated with the reward
                    var user = _context.Users.FirstOrDefault(u => u.Id == userStreakReward.UserId);

                    // Update user points (add the points associated with the claimed reward)
                    user.Points += claimedReward.Points;

                    // Save changes to the database
                    _context.SaveChanges();

                    // Return JSON response indicating success
                    return Json(new { success = true, pointsClaimed = claimedReward.Points });
                }
            }

            // If the reward has already been claimed or doesn't exist, return JSON response indicating failure
            return Json(new { success = false });
        }

        // GET: Clan/Leaderboards
        public ActionResult Leaderboards()
        {
            // Retrieve leaderboards data from the database
            var leaderboardsData = _context.Users
                .OrderByDescending(user => user.Points) // Order by points in descending order
                .Select(user => new
                {
                    user.CustomUserName,
                    Points = user.Points,
                })
                .ToList();

            // Return the leaderboards data as JSON
            return Json(leaderboardsData);
        }


    }
}