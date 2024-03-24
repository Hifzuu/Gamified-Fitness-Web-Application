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
                .OrderBy(sr => sr.Points) 
                .ToList();

            // if new year, clear users streak rewards
            if (IsNewYear())
            {
                ClearUserStreakRewards(userId);
                loginStreak.LastLoginTime = DateTime.Now;
                _context.SaveChanges();
            }

            // Check if the users current streak matches any reward days
            var claimableRewards = allStreakRewards
                .Where(sr => loginStreak.CurrentStreak >= sr.Days)
                .ToList();

            // Fetch the users streak rewards list
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
                        Claimed = false 
                    };
                    _context.UserStreakRewards.Add(userStreakReward);
                }
            }

            // Retrieve the current users workout stats
            var userWorkoutStats = _context.UserWorkoutStats
                .FirstOrDefault(uws => uws.UserId == userId);

            string mostFrequentWorkoutType = "";
            if (userWorkoutStats != null)
            {
                // Get the maximum completed count out of all workout types
                int maxCompletedCount = Math.Max(
                    userWorkoutStats.CardioCompletedCount,
                    Math.Max(
                        userWorkoutStats.HIITCompletedCount,
                        Math.Max(
                            userWorkoutStats.StrengthTrainingCompletedCount,
                            Math.Max(
                                userWorkoutStats.RunningCompletedCount,
                                Math.Max(
                                    userWorkoutStats.YogaCompletedCount,
                                    Math.Max(
                                        userWorkoutStats.PilatesCompletedCount,
                                        userWorkoutStats.BalancedWorkoutsCompletedCount
                                    )
                                )
                            )
                        )
                    )
                );

                if (maxCompletedCount == userWorkoutStats.CardioCompletedCount)
                {
                    mostFrequentWorkoutType = "Cardio";
                }
                else if (maxCompletedCount == userWorkoutStats.HIITCompletedCount)
                {
                    mostFrequentWorkoutType = "HIIT";
                }
                else if (maxCompletedCount == userWorkoutStats.StrengthTrainingCompletedCount)
                {
                    mostFrequentWorkoutType = "Strength Training";
                }
                else if (maxCompletedCount == userWorkoutStats.RunningCompletedCount)
                {
                    mostFrequentWorkoutType = "Running";
                }
                else if (maxCompletedCount == userWorkoutStats.YogaCompletedCount)
                {
                    mostFrequentWorkoutType = "Yoga";
                }
                else if (maxCompletedCount == userWorkoutStats.PilatesCompletedCount)
                {
                    mostFrequentWorkoutType = "Pilates";
                }
                else if (maxCompletedCount == userWorkoutStats.BalancedWorkoutsCompletedCount)
                {
                    mostFrequentWorkoutType = "Balanced Workouts";
                }
            }

                _context.SaveChanges();

            var viewModel = new DashboardViewModel
            {
                WeightData = weightData,
                LoginStreak = loginStreak,
                Rewards = allStreakRewards,
                ClaimableRewards= userStreaksRewardsList,
                UserWorkoutStats = userWorkoutStats,
                MostFrequentWorkoutType = mostFrequentWorkoutType,
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

            _context.WeightEntries.Add(newWeightEntry);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // method to check if it's a new year
        private bool IsNewYear()
        {
            var userId = _userManager.GetUserId(User);
            var currentYear = DateTime.Now.Year;
            var lastLoginYear = _context.LoginStreaks
                .Where(ls => ls.UserId == userId)
                .Max(ls => ls.LastLoginTime.Year);

            return currentYear > lastLoginYear;
        }

        // method to clear user streak rewards for the new year
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
            var userStreakReward = _context.UserStreakRewards.FirstOrDefault(ur => ur.UserStreakRewardId == userStreakRewardId);

            // Check if the userStreakReward exists and has not been claimed
            if (userStreakReward != null && !userStreakReward.Claimed)
            {
                var claimedReward = _context.StreakRewards.FirstOrDefault(sr => sr.RewardId == userStreakReward.RewardId);

                if (claimedReward != null)
                {
                    userStreakReward.Claimed = true;
                    var user = _context.Users.FirstOrDefault(u => u.Id == userStreakReward.UserId);
                    user.Points += claimedReward.Points;
                    _context.SaveChanges();
                    return Json(new { success = true, pointsClaimed = claimedReward.Points });
                }
            }

            return Json(new { success = false });
        }

        public ActionResult Leaderboards()
        {
            var leaderboardsData = _context.Users
                .OrderByDescending(user => user.Points) 
                .Select(user => new
                {
                    user.CustomUserName,
                    Points = user.Points,
                })
                .ToList();

            return Json(leaderboardsData);
        }


    }
}