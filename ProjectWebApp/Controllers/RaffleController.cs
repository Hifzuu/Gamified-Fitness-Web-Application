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
            // Get the current date
            DateTime currentDate = DateTime.UtcNow;

            // Get the current user
            var user = await _userManager.GetUserAsync(User);

            // Get raffles that need to be updated
            var rafflesToUpdate = await _context.Raffles
                .Where(r => currentDate > r.EndDate)
                .ToListAsync();

            // Check if there are raffles that need to be updated
            if (rafflesToUpdate.Any())
            {
                await SelectWinners();

                // Update start and end dates for the selected raffles to the next month
                foreach (var raffle in rafflesToUpdate)
                {
                    raffle.StartDate = new DateTime(currentDate.Year, currentDate.Month, 1).Date;

                    // Calculate the start of the next month
                    DateTime startOfNextMonth = currentDate.AddMonths(1);

                    // Set EndDate to the start of the next month
                    raffle.EndDate = new DateTime(startOfNextMonth.Year, startOfNextMonth.Month, 1).Date;
                }

                // Save changes to the database
                await _context.SaveChangesAsync();
            }

            // Retrieve raffles with view model
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

            // Populate the UserEnteredRaffles property for the current user
            var userEnteredRaffles = await _context.UserRaffleEntries
                .Where(re => re.UserId == user.Id)
                .Select(re => new RaffleViewModel
                {
                    RaffleId = re.RaffleId,
                    Name = re.Raffle.Name,
                    EntryTimestamp = re.EntryTimestamp,
                    EndDate = re.Raffle.EndDate,
                    isWinner = re.IsWinner,
                    userId = re.UserId,
                    ImageUrl = re.Raffle.ImageUrl,
                })
                .ToListAsync();

            // Populate the winners property for all users
            var winners = await _context.UserRaffleEntries
                .Where(re => re.IsWinner == true)
                .Select(re => new RaffleViewModel
                {
                    RaffleId = re.RaffleId,
                    Name = re.Raffle.Name,
                    EntryTimestamp = re.EntryTimestamp,
                    EndDate = re.Raffle.EndDate,
                    isWinner = re.IsWinner,
                    userId = re.UserId,
                    ImageUrl=re.Raffle.ImageUrl,
                })
                .ToListAsync();

            // Iterate through the raffles and mark the ones entered by the user
            foreach (var raffle in raffles)
            {
                raffle.UserEnteredRaffles = userEnteredRaffles
                    .Where(enteredRaffle => enteredRaffle.RaffleId == raffle.RaffleId)
                    .ToList();
            }

            // Create an instance of the view model and set the UserEnteredRaffles property
            var viewModel = new RaffleViewModel
            {
                Raffles = raffles,
                UserEnteredRaffles = userEnteredRaffles,
                winners = winners,
            };

            return View(viewModel);
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

        [HttpPost]
        public async Task<IActionResult> AddRaffleEntry(int raffleCost, int raffleId)
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);

            // Check if the user has enough points
            if (user.Points >= raffleCost)
            {
                var alreadyEntered = await _context.UserRaffleEntries
                    .AnyAsync(re => re.UserId == user.Id
                         && re.RaffleId == raffleId
                         && re.EntryTimestamp.Month == DateTime.UtcNow.Month);

                if (alreadyEntered)
                {
                    return Json(new { success = true, alreadyEntered = true, message = "You are already entered in this raffle for the current month." });
                }

                // Create a new raffle entry
                var newEntry = new UserRaffleEntry
                {
                    UserId = user.Id,
                    RaffleId = raffleId, // Use the passed raffleId parameter
                    EntryTimestamp = DateTime.UtcNow,
                    IsWinner = false,
                };

                // Add the new raffle entry to the database
                _context.UserRaffleEntries.Add(newEntry);

                // Update user points (subtract the raffle cost)
                user.Points -= raffleCost;
                await _userManager.UpdateAsync(user);

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Raffle entry added successfully, send a success message
                return Json(new { success = true, message = "Raffle entry added successfully!" });
            }
            else
            {
                // User does not have enough points, send an error message
                return Json(new { success = false, message = "You do not have enough points to enter this raffle. Earn more points to participate!" });
            }
        }

        public async Task<IActionResult> SelectWinners()
        {
            // Get the current date
            DateTime currentDate = DateTime.UtcNow;

            // Get the start date of the previous month
            DateTime startOfPreviousMonth = currentDate.AddMonths(-1).Date;

            // Get raffles that need winners to be selected for the previous month
            var rafflesToSelectWinners = await _context.Raffles
                .Where(r => r.EndDate >= startOfPreviousMonth)
                .Include(r => r.UserRaffleEntries) // Include entries for each raffle
                .ToListAsync();

            // Check if there are raffles that need winners to be selected
            if (rafflesToSelectWinners.Any())
            {
                foreach (var raffle in rafflesToSelectWinners)
                {
                    // Check if there is already a winner for the raffle
                    if (!raffle.UserRaffleEntries.Any(e => e.IsWinner))
                    {
                        // Select random winners for the raffle
                        var random = new Random();
                        var winners = raffle.UserRaffleEntries
                            .Where(e => e.IsWinner == false && e.EntryTimestamp.Month == startOfPreviousMonth.Month)
                            .OrderBy(e => random.Next())
                            .Take(1)
                            .ToList();

                        // Mark the winners as winners
                        foreach (var winner in winners)
                        {
                            winner.IsWinner = true;
                        }
                    }
                    else
                    {
                        // No raffles need winners to be selected
                        return Json(new { success = true, message = "A winner has already been selected so no new winners can be awarded" });
                    }
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Return a success message
                return Json(new { success = true, message = "Winners selected successfully!" });
            }
            else
            {
                // No raffles need winners to be selected
                return Json(new { success = true, message = "No raffles need winners to be selected for the previous month." });
            }
        }


       



    }
}
