using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProjectWebApp.Data;
using ProjectWebApp.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic; 
using Newtonsoft.Json.Converters;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Data.SqlClient;
using ProjectWebApp.Data.Migrations;
using System.Collections.Immutable;

namespace ProjectWebApp.Controllers
{
    public class WorkoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkoutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get users calendar entries
            var userCalendars = _context.UserCalendars
                .Include(uc => uc.Workout)
                .Where(uc => uc.UserId == currentUser.Id)
                .OrderBy(uc => uc.ScheduledDateTime)
                .ToList();

            // Get users exercise focus
            var userExerciseFocus = currentUser.ExerciseFocus;

            // Get all workouts 
            var workouts = _context.Workouts.ToList();

            // Set set featured for workouts matching the users exercise focus
            foreach (var workout in workouts)
            {
                workout.IsFeatured = userExerciseFocus.Contains(workout.Category);
            }

            _context.SaveChanges();
            ViewData["UserCalendars"] = userCalendars;
            ViewData["Workouts"] = workouts;

            return View();
        }

        public IActionResult Details(int id)
        {
            var workout = _context.Workouts.FirstOrDefault(e => e.WorkoutId == id);
            return View(workout);
        }

        [HttpGet]
        public async Task<IActionResult> GetPoints(int workoutId)
        {
            var workout = await _context.Workouts.FindAsync(workoutId);
            if (workout != null)
            {
                return Json(workout.rewardPoints);
            }
            return Json(0);
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavourites(int workoutId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized(); 
            }

            try
            {
                // Check if the user has already favorited this workout
                var existingFavourite = await _context.UserFavouriteWorkouts
                    .Where(w => w.UserId == currentUser.Id && w.WorkoutId == workoutId)
                    .FirstOrDefaultAsync();

                // remove from favourite
                if (existingFavourite != null)
                {
                    _context.UserFavouriteWorkouts.Remove(existingFavourite);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = workoutId });
                }

                var newFavourite = new UserFavouriteWorkout
                {
                    UserId = currentUser.Id,
                    WorkoutId = workoutId
                };


                _context.UserFavouriteWorkouts.Add(newFavourite);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = workoutId });
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }


        }

        public async Task <IActionResult> IsFavourited(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized();
            }

            var isFavourited = await _context.UserFavouriteWorkouts
                .AnyAsync(w => w.UserId == currentUser.Id && w.WorkoutId == id);

            return Json(isFavourited);
        }


        [HttpPost]
        public async Task<IActionResult> UpdatePoints(int points)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                currentUser.Points += points;
                var result = await _userManager.UpdateAsync(currentUser);

                if (result.Succeeded)
                {
                    return Ok(); 
                }
            }
            return BadRequest("Failed to update points");
        }

        [HttpGet]
        public IActionResult GetCurrentUserPoints()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = _userManager.GetUserAsync(User).Result;
                var points = user?.Points ?? 0;
                return Json(points);
            }

            return Json(0); 
        }


        public async Task <IActionResult> GetUserCalendarEvents()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var userCalendars = _context.UserCalendars
                .Include(calendar => calendar.Workout)
                .Where(calendar => calendar.ScheduledDateTime != null && calendar.UserId == currentUser.Id)
                .ToList();
            
            var events = userCalendars
                .Select(calendar => new
                {
                    id = calendar.WorkoutId,
                    title = $"{calendar.Workout?.Name}",
                    start = calendar.ScheduledDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = calendar.ScheduledDateTime.AddHours(2).ToString("yyyy-MM-ddTHH:mm:ss"),
                    notes = calendar.Notes.ToString(),
                });

            return Json(events);
        }


        [HttpPost]
        public async Task<IActionResult> ScheduleWorkout(int workoutId, DateTime scheduleDate, TimeSpan scheduleTime, string scheduleNote)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            try
            {
                bool isTimeSlotAvailable = await IsTimeSlotAvailable(currentUser.Id, scheduleDate.Date + scheduleTime);

                if (!isTimeSlotAvailable)
                {
                    return Json(new { success = false, message = "A workout is already scheduled at this time. Please choose a different time." });
                }

                var userCalendarEntry = new UserCalendar
                {
                    UserId = currentUser.Id,
                    WorkoutId = workoutId,
                    ScheduledDateTime = scheduleDate.Date + scheduleTime, // Combine date and time
                    IsCompleted = false,
                    Notes = scheduleNote ?? string.Empty
                };

                _context.UserCalendars.Add(userCalendarEntry);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Schedule information saved successfully." });
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = "Error occurred while saving schedule information.";
                if (ex.InnerException is SqlException sqlException)
                {
                    errorMessage += $" SQL Server Error Number: {sqlException.Number}. ";
                }
                return Json(new { success = false, message = errorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Unexpected error occurred: " + ex.Message });
            }
        }

        // Check if there's any existing workout scheduled at the same time for the current user
        private async Task<bool> IsTimeSlotAvailable(string userId, DateTime scheduledDateTime)
        {
            return !await _context.UserCalendars.AnyAsync(uc =>
                uc.UserId == userId &&
                uc.ScheduledDateTime == scheduledDateTime);
        }

        [HttpPost]
        public async Task <IActionResult> RemoveScheduledWorkout(int workoutId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var userCalendarEntry = _context.UserCalendars.FirstOrDefault(u => u.WorkoutId == workoutId && u.UserId == currentUser.Id);

                if (userCalendarEntry == null)
                {
                    return Json(new { success = false, message = "Workout not found or unauthorized." });
                }

                _context.UserCalendars.Remove(userCalendarEntry);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Workout removed successfully." });
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = "Error occurred while removing workout.";

                if (ex.InnerException is SqlException sqlException)
                {
                    errorMessage += $" SQL Server Error Number: {sqlException.Number}. ";
                }
                return Json(new { success = false, message = errorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Unexpected error occurred: " + ex.Message });
            }
        }

        public async Task<IActionResult> MarkWorkoutAsCompleted(int workoutId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var completedWorkout = await _context.Workouts.FindAsync(workoutId);

            if (completedWorkout == null)
            {
                return NotFound();
            }

            var userWorkoutStats = await _context.UserWorkoutStats.FirstOrDefaultAsync(uws => uws.UserId == currentUser.Id);

            // If the user doesn't have workout stats, create a new entry
            if (userWorkoutStats == null)
            {
                var CardioCompletedCount = 0;
                var HIITCompletedCount = 0;
                var StrengthTrainingCompletedCount = 0;
                var RunningCompletedCount = 0;
                var YogaCompletedCount = 0;
                var PilatesCompletedCount = 0;
                var BalancedWorkoutsCompletedCount = 0;

                switch (completedWorkout.Category)
                {
                    case "Cardio":
                        CardioCompletedCount += 1;
                        break;
                    case "High-Intensity Interval Training":
                        HIITCompletedCount += 1;
                        break;
                    case "Strength Training":
                        StrengthTrainingCompletedCount += 1;
                        break;
                    case "Running":
                        RunningCompletedCount += 1;
                        break;
                    case "Yoga":
                        YogaCompletedCount += 1;
                        break;
                    case "Pilates":
                        PilatesCompletedCount += 1;
                        break;
                    case "Balanced Workouts":
                        BalancedWorkoutsCompletedCount += 1;
                        break;
                    default:
                        break;
                }

                userWorkoutStats = new UserWorkoutStats
                {
                    UserWorkoutStatsId = Guid.NewGuid().ToString(),
                    UserId = currentUser.Id,
                    WorkoutCount = 1,
                    TotalWorkoutDuration = completedWorkout.DurationMinutes, 
                    CardioCompletedCount = CardioCompletedCount,
                    HIITCompletedCount = HIITCompletedCount,
                    StrengthTrainingCompletedCount = StrengthTrainingCompletedCount,
                    RunningCompletedCount = RunningCompletedCount,
                    YogaCompletedCount = YogaCompletedCount,
                    PilatesCompletedCount = PilatesCompletedCount,
                    BalancedWorkoutsCompletedCount = BalancedWorkoutsCompletedCount,
                };

                _context.UserWorkoutStats.Add(userWorkoutStats);
            }
            else
            {
                // Increment workout count and update duration
                userWorkoutStats.WorkoutCount++;
                userWorkoutStats.TotalWorkoutDuration += completedWorkout.DurationMinutes;

                // Update the workout type count based on the completed workout category
                switch (completedWorkout.Category)
                {
                    case "Cardio":
                        userWorkoutStats.CardioCompletedCount++;
                        break;
                    case "High-Intensity Interval Training":
                        userWorkoutStats.HIITCompletedCount++;
                        break;
                    case "Strength Training":
                        userWorkoutStats.StrengthTrainingCompletedCount++;
                        break;
                    case "Running":
                        userWorkoutStats.RunningCompletedCount++;
                        break;
                    case "Yoga":
                        userWorkoutStats.YogaCompletedCount++;
                        break;
                    case "Pilates":
                        userWorkoutStats.PilatesCompletedCount++;
                        break;
                    case "Balanced Workouts":
                        userWorkoutStats.BalancedWorkoutsCompletedCount++;
                        break;
                    default:
                        break;
                }
            }

            // Retrieve the user challenges associated with the completed workout 
            var userChallenges = await _context.UserChallenges
                .Include(uc => uc.Challenge)
                .Where(uc => uc.UserId == currentUser.Id)
                .ToListAsync();

            foreach (var userChallenge in userChallenges)
            {
                if (userChallenge.Challenge.Type != completedWorkout.Category)
                {
                    continue; 
                }

                DateTime currentDate = DateTime.Now.Date;
                DateTime challengeStartDate;

                switch (userChallenge.Challenge.ChallengeType)
                {
                    case ChallengeType.Daily:
                        challengeStartDate = currentDate;
                        break;

                    case ChallengeType.Weekly:
                        challengeStartDate = currentDate.StartOfWeek(DayOfWeek.Monday);
                        break;

                    case ChallengeType.Monthly:
                        challengeStartDate = currentDate.StartOfMonth();
                        break;

                    default:
                        continue;
                }

                if (userChallenge.Challenge.MeasurementCriteria == MeasurementCriteria.TotalTime)
                {
                    userChallenge.CountProgress += completedWorkout.DurationMinutes;
                }
                else if (userChallenge.Challenge.MeasurementCriteria == MeasurementCriteria.WorkoutCategoryCount)
                {
                    userChallenge.CountProgress++;
                }
            }

            // Check if the user is part of a clan
            var userClan = await _context.Clans.FirstOrDefaultAsync(clan => clan.Members.Any(member => member.Id == currentUser.Id));

            if (userClan != null)
            {
                // Update clan challenge progress
                var clanChallenges = await _context.ClanChallenges
                    .Include(cc => cc.Challenge)
                    .Where(cc => cc.ClanId == userClan.ClanId)
                    .ToListAsync();

                foreach (var clanChallengeItem in clanChallenges)
                {
                    if (clanChallengeItem.Challenge.Type != completedWorkout.Category)
                    {
                        continue; 
                    }

                    if (clanChallengeItem.Challenge.MeasurementCriteria == MeasurementCriteria.TotalTime)
                    {
                        clanChallengeItem.CountProgress += completedWorkout.DurationMinutes;
                    }
                    else if (clanChallengeItem.Challenge.MeasurementCriteria == MeasurementCriteria.WorkoutCategoryCount)
                    {
                        clanChallengeItem.CountProgress++;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Workout marked as completed successfully." });
        }
    }
}


