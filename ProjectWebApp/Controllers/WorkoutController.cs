using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProjectWebApp.Data;
using ProjectWebApp.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic; // Add this if not already present
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
            // Get the current user
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                // Handle the case where the user is not authenticated
                return RedirectToAction("Login", "Account");
            }

            // Get user's calendar entries
            var userCalendars = _context.UserCalendars
                .Include(uc => uc.Workout)
                .Where(uc => uc.UserId == currentUser.Id)
                .OrderBy(uc => uc.ScheduledDateTime)
                .ToList();

            // Get the user's exercise focus
            var userExerciseFocus = currentUser.ExerciseFocus;

            // Get workouts from the database
            var workouts = _context.Workouts.ToList();

            // Set IsFeatured to true for workouts matching the user's exercise focus
            foreach (var workout in workouts)
            {
                workout.IsFeatured = userExerciseFocus.Contains(workout.Category);
            }

            // Save changes to the database (optional, only if you want to persist the IsFeatured changes)
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
            // Get the current user
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized(); // or handle as needed
            }

            try
            {
                // Check if the user has already favorited this workout
                var existingFavourite = await _context.UserFavouriteWorkouts
                    .Where(w => w.UserId == currentUser.Id && w.WorkoutId == workoutId)
                    .FirstOrDefaultAsync();

                if (existingFavourite != null)
                {
                    // User has already favorited this workout, remove it from favorites
                    _context.UserFavouriteWorkouts.Remove(existingFavourite);

                    // Save changes to the database
                    await _context.SaveChangesAsync();

                    // Redirect back to the workout details page or handle as needed
                    return RedirectToAction("Details", new { id = workoutId });
                }

                // Create a new UserFavouriteWorkout
                var newFavourite = new UserFavouriteWorkout
                {
                    UserId = currentUser.Id,
                    WorkoutId = workoutId
                };

                // Add the UserFavouriteWorkout to the context
                _context.UserFavouriteWorkouts.Add(newFavourite);

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Redirect back to the workout details page or handle as needed
                return RedirectToAction("Details", new { id = workoutId });
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
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

        private string GetCurrentUserId()
        {
            return User.Identity.IsAuthenticated ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
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
                    return Ok(); // or return a JSON response if needed
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

                // Assuming Points is the property in your ApplicationUser model
                var points = user?.Points ?? 0;

                return Json(points);
            }

            return Json(0); // Return 0 for unauthenticated users or handle it according to your logic
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
                // Check if there's any existing workout scheduled at the same time for the current user
                bool isTimeSlotAvailable = await IsTimeSlotAvailable(currentUser.Id, scheduleDate.Date + scheduleTime);

                if (!isTimeSlotAvailable)
                {
                    return Json(new { success = false, message = "A workout is already scheduled at this time. Please choose a different time." });
                }

                // Assuming you have a DbSet<UserCalendar> in your ApplicationDbContext
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
                    // Handle specific SQL Server exceptions
                    errorMessage += $" SQL Server Error Number: {sqlException.Number}. ";
                }

                return Json(new { success = false, message = errorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Unexpected error occurred: " + ex.Message });
            }
        }


        private async Task<bool> IsTimeSlotAvailable(string userId, DateTime scheduledDateTime)
        {
            // Check if there's any existing workout scheduled at the same time for the current user
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

                // Assuming you have a DbSet<UserCalendar> in your ApplicationDbContext
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
                    // Handle specific SQL Server exceptions
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

            // Retrieve the completed workout
            var completedWorkout = await _context.Workouts.FindAsync(workoutId);

            if (completedWorkout == null)
            {
                return NotFound(); // Handle appropriately
            }

            // Log completed workout details for debugging
            Console.WriteLine($"Completed Workout ID: {completedWorkout.WorkoutId}");
            Console.WriteLine($"Completed Workout Category: {completedWorkout.Category}");

            // Retrieve the user challenges associated with the completed workout for the current user
            var userChallenges = await _context.UserChallenges
                .Include(uc => uc.Challenge)
                .Where(uc =>
                    uc.UserId == currentUser.Id)
                .ToListAsync();

            foreach (var userChallenge in userChallenges)
            {
                if (userChallenge.Challenge.Type != completedWorkout.Category)
                {
                    continue; // Skip to the next iteration if there's no match
                }

                // Log challenge details for debugging
                Console.WriteLine($"Challenge Type: {userChallenge.Challenge.ChallengeType}");
                Console.WriteLine($"Measurement Criteria: {userChallenge.Challenge.MeasurementCriteria}");

                // Determine the date criteria for the challenge
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
                        // Handle unsupported challenge types
                        continue;
                }

                // Check if the completed workout is within the challenge date range
                // Update challenge progress based on the completed workout
                if (userChallenge.Challenge.MeasurementCriteria == MeasurementCriteria.TotalTime)
                {
                    // Update based on total time of the workout (adjust based on your actual model)
                    userChallenge.CountProgress += completedWorkout.DurationMinutes;
                }
                else if (userChallenge.Challenge.MeasurementCriteria == MeasurementCriteria.WorkoutCategoryCount)
                {
                    // Log category details for debugging
                    Console.WriteLine($"Completed Workout Category: {completedWorkout.Category}");

                    // Update based on count (add 1)
                    userChallenge.CountProgress++;
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Other logic or redirection as needed
            return Ok(new { success = true, message = "Workout marked as completed successfully." });
        }

    }

}


