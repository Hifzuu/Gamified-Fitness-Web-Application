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
                Console.WriteLine($"Error assigning challenges: {ex.Message}");
            }

            // users current daily challenge
            var userDailyChallenge = GetUserDailyChallenge(userId);
            string formattedTimeForDailyChallenge = userDailyChallenge != null
               ? FormatChallengeTime(userDailyChallenge)
               : string.Empty;

            int dailyCountProgress = userDailyChallenge?.CountProgress ?? 0;
            double dailyProgressPercentage = 0;

            if (userDailyChallenge != null && userDailyChallenge.Challenge.TargetCount > 0)
            {
                // daily progress percentage =  the daily count progress divided by the target count * 100
                dailyProgressPercentage = (double)dailyCountProgress / userDailyChallenge.Challenge.TargetCount * 100;
            }


            // users current weekly challenge
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

            // users current monthly challenge
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

            // get all associated workouts to the challenges
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

            //populate challenge view model with all the retrieved data
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
         
        // return users daily challenge (today)
        private UserChallenge GetUserDailyChallenge(string userId)
        {
            DateTime currentDate = DateTime.Now.Date;

            var userChallenge = _context.UserChallenges
                .Include(uc => uc.Challenge)
                    .ThenInclude(c => c.Workouts) 
                .FirstOrDefault(uc => uc.UserId == userId
                                       && uc.StartDate.Date == currentDate
                                       && uc.Challenge.ChallengeType == ChallengeType.Daily);

            return userChallenge;
        }

        // return users weekly challenge (start of week)
        private UserChallenge GetUserWeeklyChallenge(string userId)
        {
            DateTime currentDate = DateTime.Now.Date;
            DateTime startOfWeek = currentDate.StartOfWeek(DayOfWeek.Monday);

            var userChallenge = _context.UserChallenges
                .Include(uc => uc.Challenge)
                    .ThenInclude(c => c.Workouts)
                .FirstOrDefault(uc => uc.UserId == userId
                                       && uc.StartDate.Date == startOfWeek.Date
                                       && uc.Challenge.ChallengeType == ChallengeType.Weekly);

            return userChallenge;
        }

        // return users monthly challenge (start of month)
        private UserChallenge GetUserMonthlyChallenge(string userId)
        {
            DateTime currentDate = DateTime.Now.Date;
            DateTime startOfMonth = currentDate.StartOfMonth();

            var userChallenge = _context.UserChallenges
                .Include(uc => uc.Challenge)
                    .ThenInclude(c => c.Workouts) 
                .FirstOrDefault(uc => uc.UserId == userId
                                       && uc.StartDate.Date == startOfMonth.Date
                                       && uc.Challenge.ChallengeType == ChallengeType.Monthly);

            return userChallenge;
        }


        private void AssignDailyChallenge(string userId)
        {
            DateTime currentDate = DateTime.Now;
            DateTime endOfDay = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 23, 59, 59);

            // check if already assigned
            bool hasDailyChallenge = _context.UserChallenges
                .Any(uc => uc.UserId == userId
                           && uc.StartDate.Date == currentDate.Date
                           && uc.Challenge.ChallengeType == ChallengeType.Daily);

            if (!hasDailyChallenge)
            {
                // Get all daily challenges
                var dailyChallenges = _context.Challenges
                    .Where(c => c.ChallengeType == ChallengeType.Daily)
                    .ToList();

                if (dailyChallenges.Count > 0)
                {
                    // Pick a random daily challenge 
                    Challenge randomDailyChallenge = dailyChallenges[random.Next(dailyChallenges.Count)];

                    // assign the challenge
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
                    Console.WriteLine("No daily challenges available.");
                }
            }
        }

        private void AssignWeeklyChallenge(string userId)
        {
            DateTime currentDate = DateTime.Now;
            DateTime startOfWeek = currentDate.StartOfWeek(DayOfWeek.Monday); // monday is the start of the week
            DateTime endOfWeek = startOfWeek.AddDays(6).EndOfDay();

            bool hasWeeklyChallenge = _context.UserChallenges
                .Any(uc => uc.UserId == userId
                           && uc.StartDate.Date >= startOfWeek.Date
                           && uc.EndDate.Date <= endOfWeek.Date
                           && uc.Challenge.ChallengeType == ChallengeType.Weekly);

            if (!hasWeeklyChallenge)
            {
                // Get all weekly challenges
                var weeklyChallenges = _context.Challenges
                    .Where(c => c.ChallengeType == ChallengeType.Weekly)
                    .ToList();

                if (weeklyChallenges.Count > 0)
                {
                    Challenge randomWeeklyChallenge = weeklyChallenges[random.Next(weeklyChallenges.Count)];

                    // assign challenge
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
                    Console.WriteLine("No weekly challenges available.");
                }
            }
        }


        private void AssignMonthlyChallenge(string userId)
        {
            DateTime currentDate = DateTime.Now;
            DateTime startOfMonth = currentDate.StartOfMonth();
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1).EndOfDay();

            bool hasMonthlyChallenge = _context.UserChallenges
                .Any(uc => uc.UserId == userId
                           && uc.StartDate.Date >= startOfMonth.Date
                           && uc.EndDate.Date <= endOfMonth.Date
                           && uc.Challenge.ChallengeType == ChallengeType.Monthly);

            if (!hasMonthlyChallenge)
            {
                // Get all monthly challenges
                var monthlyChallenges = _context.Challenges
                    .Where(c => c.ChallengeType == ChallengeType.Monthly)
                    .ToList();

                if (monthlyChallenges.Count > 0)
                {
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
                    Console.WriteLine("No monthly challenges available.");
                }
            }
        }



        private string FormatChallengeTime(UserChallenge dailyChallenge)
        {
            // Retrieve end date of daily challenge.
            DateTime dailyChallengeEnd = dailyChallenge.EndDate;

            // Calculate time remaining until end date.
            TimeSpan timeRemaining = dailyChallengeEnd - DateTime.Now;

            // Ensure non-negative time remaining.
            int secondsRemaining = (int)Math.Max(0, timeRemaining.TotalSeconds);

            // Format current date and time plus remaining seconds.
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
                    var userChallenge = _context.UserChallenges.Find(userChallengeId);
                    if (userChallenge != null && !userChallenge.IsRewardClaimed)
                    {
                        currentUser.Points += points;
                        userChallenge.IsRewardClaimed = true;
                        _context.SaveChanges(); 

                        // update user entity
                        var userUpdateResult = await _userManager.UpdateAsync(currentUser);
                    }
                }

                return BadRequest("Failed to update points");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while updating points");
            }
        }
    }

    // Extension method for date time manipulation.
    public static class DateTimeExtensions
    {
        // Returns the start of the week
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.Date.AddDays(-1 * diff);
        }

        // Returns the end of the day 
        public static DateTime EndOfDay(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999);
        }

        // Returns the start of the month
        public static DateTime StartOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }
    }




}