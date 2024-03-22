using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectWebApp.Data;
using ProjectWebApp.Models;
using ProjectWebApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProjectWebApp.Controllers
{
    public class ChallengeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private static readonly Random random = new Random(Guid.NewGuid().GetHashCode());
        private readonly ILogger<ChallengeController> _logger;

        public ChallengeController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger<ChallengeController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return NotFound(); // User not found
            }

            var userId = currentUser.Id;

            try
            {
                AssignDailyChallenge(userId);
                AssignWeeklyChallenge(userId);
                AssignMonthlyChallenge(userId);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                // This prevents an exception from blocking the page load
                Console.WriteLine($"Error assigning challenges: {ex.Message}");
            }

            // Retrieve the user's current daily challenge for today
            var userDailyChallenge = GetUserDailyChallenge(userId);
            string formattedTimeForDailyChallenge = userDailyChallenge != null
               ? FormatChallengeTime(userDailyChallenge)
               : string.Empty;

            int dailyCountProgress = userDailyChallenge?.CountProgress ?? 0;
            double dailyProgressPercentage = 0;

            if (userDailyChallenge != null && userDailyChallenge.Challenge.TargetCount > 0)
            {
                dailyProgressPercentage = (double)dailyCountProgress / userDailyChallenge.Challenge.TargetCount * 100;
            }

            // Retrieve the user's current weekly challenge for the current week
            var userWeeklyChallenge = GetUserWeeklyChallenge(userId);
            string formattedTimeForWeeklyChallenge = userWeeklyChallenge != null
                ? FormatChallengeTime(userWeeklyChallenge)
                : string.Empty;

            int weeklyCountProgress = userWeeklyChallenge?.CountProgress ?? 0;
            double weeklyProgressPercentage = 0;

            if (userWeeklyChallenge != null && userWeeklyChallenge.Challenge.TargetCount > 0)
            {
                weeklyProgressPercentage = (double)weeklyCountProgress / userWeeklyChallenge.Challenge.TargetCount * 100;
            }

            // Retrieve the user's current monthly challenge for the current month
            var userMonthlyChallenge = GetUserMonthlyChallenge(userId);
            string formattedTimeForMonthlyChallenge = userMonthlyChallenge != null
                ? FormatChallengeTime(userMonthlyChallenge)
                : string.Empty;

            int monthlyCountProgress = userMonthlyChallenge?.CountProgress ?? 0;
            double monthlyProgressPercentage = 0;

            if (userMonthlyChallenge != null && userMonthlyChallenge.Challenge.TargetCount > 0)
            {
                monthlyProgressPercentage = (double)monthlyCountProgress / userMonthlyChallenge.Challenge.TargetCount * 100;
            }

            List<Workout> dailyChallengeWorkouts = new List<Workout>();
            dailyChallengeWorkouts = await _context.Workouts
                       .Where(w => w.Category == userDailyChallenge.Challenge.Type)
                       .ToListAsync();

            List<Workout> weeklyChallengeWorkouts = new List<Workout>();
            weeklyChallengeWorkouts = await _context.Workouts
                       .Where(w => w.Category == userWeeklyChallenge.Challenge.Type)
                       .ToListAsync();

            List<Workout> monthlyChallengeWorkouts = new List<Workout>();
            monthlyChallengeWorkouts = await _context.Workouts
                       .Where(w => w.Category == userMonthlyChallenge.Challenge.Type)
                       .ToListAsync();

            var model = new ChallengeViewModel
            {
                DailyChallenge = userDailyChallenge,
                DailyTime = formattedTimeForDailyChallenge,
                DailyProgressPercentage = dailyProgressPercentage,

                WeeklyChallenge = userWeeklyChallenge,
                WeeklyProgressPercentage = weeklyProgressPercentage,
                WeeklyTime = formattedTimeForWeeklyChallenge,

                MonthlyChallenge = userMonthlyChallenge,
                MonthlyProgressPercentage = monthlyProgressPercentage,
                MonthlyTime = formattedTimeForMonthlyChallenge,

                DailyChallengeWorkouts = dailyChallengeWorkouts,
                WeeklyChallengeWorkouts = weeklyChallengeWorkouts,
                MonthlyChallengeWorkouts = monthlyChallengeWorkouts
            };

            return View(model);
        }

        private UserChallenge GetUserDailyChallenge(string userId)
        {
            DateTime currentDate = DateTime.Now.Date;

            // Retrieve the user's current challenge for the specified type and date
            var userChallenge = _context.UserChallenges
                .Include(uc => uc.Challenge)
                    .ThenInclude(c => c.Workouts) // Include related data as needed
                .FirstOrDefault(uc => uc.UserId == userId
                                       && uc.StartDate.Date == currentDate
                                       && uc.Challenge.ChallengeType == ChallengeType.Daily);

            return userChallenge;
        }

        private UserChallenge GetUserWeeklyChallenge(string userId)
        {
            DateTime currentDate = DateTime.Now.Date;

            // Calculate the start of the week
            DateTime startOfWeek = currentDate.StartOfWeek(DayOfWeek.Monday);

            // Retrieve the user's current challenge for the specified type and date
            var userChallenge = _context.UserChallenges
                .Include(uc => uc.Challenge)
                    .ThenInclude(c => c.Workouts) // Include related data as needed
                .FirstOrDefault(uc => uc.UserId == userId
                                       && uc.StartDate.Date == startOfWeek.Date
                                       && uc.Challenge.ChallengeType == ChallengeType.Weekly);

            return userChallenge;
        }


        private UserChallenge GetUserMonthlyChallenge(string userId)
        {
            DateTime currentDate = DateTime.Now.Date;

            // Calculate the start of the month
            DateTime startOfMonth = currentDate.StartOfMonth();

            // Retrieve the user's current challenge for the specified type and date
            var userChallenge = _context.UserChallenges
                .Include(uc => uc.Challenge)
                    .ThenInclude(c => c.Workouts) // Include related data as needed
                .FirstOrDefault(uc => uc.UserId == userId
                                       && uc.StartDate.Date == startOfMonth.Date
                                       && uc.Challenge.ChallengeType == ChallengeType.Monthly);

            return userChallenge;
        }


        private void AssignDailyChallenge(string userId)
        {
            DateTime currentDate = DateTime.Now;
            DateTime endOfDay = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 23, 59, 59);

            // Check if a daily challenge is already assigned for the current day
            bool hasDailyChallenge = _context.UserChallenges
                .Any(uc => uc.UserId == userId
                           && uc.StartDate.Date == currentDate.Date
                           && uc.Challenge.ChallengeType == ChallengeType.Daily);

            if (!hasDailyChallenge)
            {
                // Get all available daily challenges
                var dailyChallenges = _context.Challenges
                    .Where(c => c.ChallengeType == ChallengeType.Daily)
                    .ToList();

                if (dailyChallenges.Count > 0)
                {
                    // Pick a random daily challenge using the static Random instance
                    Challenge randomDailyChallenge = dailyChallenges[random.Next(dailyChallenges.Count)];

                    UserChallenge userChallenge = new UserChallenge
                    {
                        UserId = userId,
                        ChallengeId = randomDailyChallenge.ChallengeId,
                        StartDate = currentDate,
                        EndDate = endOfDay,
                        CountProgress = 0,
                    };

                    _context.UserChallenges.Add(userChallenge);
                    _context.SaveChanges();
                }
                else
                {
                    // Log or handle the case where no daily challenges are available
                    Console.WriteLine("No daily challenges available.");
                }
            }
        }

        private void AssignWeeklyChallenge(string userId)
        {
            DateTime currentDate = DateTime.Now;
            DateTime startOfWeek = currentDate.StartOfWeek(DayOfWeek.Monday); // Assuming Monday is the start of the week
            DateTime endOfWeek = startOfWeek.AddDays(6).EndOfDay();

            // Check if a weekly challenge is already assigned for the current week
            bool hasWeeklyChallenge = _context.UserChallenges
                .Any(uc => uc.UserId == userId
                           && uc.StartDate.Date >= startOfWeek.Date
                           && uc.EndDate.Date <= endOfWeek.Date
                           && uc.Challenge.ChallengeType == ChallengeType.Weekly);

            if (!hasWeeklyChallenge)
            {
                // Get all available weekly challenges
                var weeklyChallenges = _context.Challenges
                    .Where(c => c.ChallengeType == ChallengeType.Weekly)
                    .ToList();

                if (weeklyChallenges.Count > 0)
                {
                    // Pick a random weekly challenge using the static Random instance
                    Challenge randomWeeklyChallenge = weeklyChallenges[random.Next(weeklyChallenges.Count)];

                    UserChallenge userChallenge = new UserChallenge
                    {
                        UserId = userId,
                        ChallengeId = randomWeeklyChallenge.ChallengeId,
                        StartDate = startOfWeek,
                        EndDate = endOfWeek,
                        CountProgress = 0,
                    };

                    _context.UserChallenges.Add(userChallenge);
                    _context.SaveChanges();
                }
                else
                {
                    // Log or handle the case where no weekly challenges are available
                    Console.WriteLine("No weekly challenges available.");
                }
            }
        }


        private void AssignMonthlyChallenge(string userId)
        {
            DateTime currentDate = DateTime.Now;
            DateTime startOfMonth = currentDate.StartOfMonth();
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1).EndOfDay();

            // Check if a monthly challenge is already assigned for the current month
            bool hasMonthlyChallenge = _context.UserChallenges
                .Any(uc => uc.UserId == userId
                           && uc.StartDate.Date >= startOfMonth.Date
                           && uc.EndDate.Date <= endOfMonth.Date
                           && uc.Challenge.ChallengeType == ChallengeType.Monthly);

            if (!hasMonthlyChallenge)
            {
                // Get all available monthly challenges
                var monthlyChallenges = _context.Challenges
                    .Where(c => c.ChallengeType == ChallengeType.Monthly)
                    .ToList();

                if (monthlyChallenges.Count > 0)
                {
                    // Pick a random monthly challenge using the static Random instance
                    Challenge randomMonthlyChallenge = monthlyChallenges[random.Next(monthlyChallenges.Count)];

                    UserChallenge userChallenge = new UserChallenge
                    {
                        UserId = userId,
                        ChallengeId = randomMonthlyChallenge.ChallengeId,
                        StartDate = startOfMonth,
                        EndDate = endOfMonth,
                        CountProgress = 0,
                        IsRewardClaimed = false,
                    };

                    _context.UserChallenges.Add(userChallenge);
                    _context.SaveChanges();
                }
                else
                {
                    // Log or handle the case where no monthly challenges are available
                    Console.WriteLine("No monthly challenges available.");
                }
            }
        }



        private string FormatChallengeTime(UserChallenge dailyChallenge)
        {
            // Use the UserChallenge's end date
            DateTime dailyChallengeEnd = dailyChallenge.EndDate;
            TimeSpan timeRemaining = dailyChallengeEnd - DateTime.Now;

            // Ensure the result is non-negative to avoid negative time remaining
            int secondsRemaining = (int)Math.Max(0, timeRemaining.TotalSeconds);

            // Include both date and time in the formatted output
            string formattedTime = DateTime.Now.AddSeconds(secondsRemaining).ToString("yyyy-MM-dd HH:mm:ssZ");

            return formattedTime;
        }


        public IActionResult GetUpdatedTimeRemainingForDailyChallenge(int userChallengeId)
        {
            var userChallenge = _context.UserChallenges.Find(userChallengeId);
            if (userChallenge == null)
            {
                return NotFound();
            }

            string formattedTime = FormatChallengeTime(userChallenge);

            return Json(new { formattedTime });
        }

        [HttpPost]
        public async Task<IActionResult> updatePointsChallenge(int points, int userChallengeId)
        {
            _logger.LogInformation($"Received points update request. Points: {points}, UserChallengeId: {userChallengeId}");
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser != null)
                {
                    // Mark the challenge as claimed
                    var userChallenge = _context.UserChallenges.Find(userChallengeId);
                    if (userChallenge != null && !userChallenge.IsRewardClaimed)
                    {
                        // Update user's points only if the reward is not already claimed
                        currentUser.Points += points;

                        // Update the IsRewardClaimed property
                        userChallenge.IsRewardClaimed = true;

                        // Save changes to the database
                        _context.SaveChanges(); // Save changes to the UserChallenge entity

                        // Save changes to the user entity
                        var userUpdateResult = await _userManager.UpdateAsync(currentUser);

                        if (userUpdateResult.Succeeded)
                        {
                            // Log successful points update
                            _logger.LogInformation($"Points updated successfully. User: {currentUser.UserName}, Points: {currentUser.Points}, ChallengeId: {userChallengeId}");

                            return Ok(); // or return a JSON response if needed
                        }
                    }
                }

                // Log failure to update points
                _logger.LogError($"Failed to update points. User: {currentUser?.UserName}, ChallengeId: {userChallengeId}");

                return BadRequest("Failed to update points");
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, $"An exception occurred while updating points. ChallengeId: {userChallengeId}");
                return BadRequest("An error occurred while updating points");
            }
        }


    }
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.Date.AddDays(-1 * diff);
        }

        public static DateTime EndOfDay(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999);
        }

        public static DateTime StartOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }


    }



}