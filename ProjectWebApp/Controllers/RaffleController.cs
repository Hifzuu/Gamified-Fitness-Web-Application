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
            DateTime currentDate = DateTime.UtcNow;
            var user = await _userManager.GetUserAsync(User);
            var rafflesToUpdate = await _context.Raffles
                .Where(r => currentDate > r.EndDate)
                .ToListAsync();

            // update raffle start and end dates
            if (rafflesToUpdate.Any())
            {
                await SelectWinners();

                foreach (var raffle in rafflesToUpdate)
                {
                    raffle.StartDate = new DateTime(currentDate.Year, currentDate.Month, 1).Date;
                    DateTime startOfNextMonth = currentDate.AddMonths(1);
                    raffle.EndDate = new DateTime(startOfNextMonth.Year, startOfNextMonth.Month, 1).Date;
                }

                await _context.SaveChangesAsync();
            }

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
            var user = await _userManager.GetUserAsync(User);

            // Check if the user has enough points
            if (user.Points >= raffleCost)
            {
                return Json(new { success = true, message = "Raffle entered successfully!" + user.Points + "raff: " + raffleCost });
            }
            else
            {
                return Json(new { success = false, message = "You do not have enough points to enter this raffle. Earn more points to participate!" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddRaffleEntry(int raffleCost, int raffleId)
        {
            var user = await _userManager.GetUserAsync(User);

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
                    RaffleId = raffleId, 
                    EntryTimestamp = DateTime.UtcNow,
                    IsWinner = false,
                };

                _context.UserRaffleEntries.Add(newEntry);
                user.Points -= raffleCost;
                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Raffle entry added successfully!" });
            }
            else
            {
                return Json(new { success = false, message = "You do not have enough points to enter this raffle. Earn more points to participate!" });
            }
        }

        public async Task<IActionResult> SelectWinners()
        {
            DateTime currentDate = DateTime.UtcNow;
            DateTime startOfPreviousMonth = currentDate.AddMonths(-1).Date;

            // check raffles that need winners to be selected for the previous month
            var rafflesToSelectWinners = await _context.Raffles
                .Where(r => r.EndDate >= startOfPreviousMonth)
                .Include(r => r.UserRaffleEntries) 
                .ToListAsync();

            if (rafflesToSelectWinners.Any())
            {
                foreach (var raffle in rafflesToSelectWinners)
                {
                    if (!raffle.UserRaffleEntries.Any(e => e.IsWinner))
                    {
                        // Select random winners for the raffle
                        var random = new Random();
                        var winners = raffle.UserRaffleEntries
                            .Where(e => e.IsWinner == false && e.EntryTimestamp.Month == startOfPreviousMonth.Month)
                            .OrderBy(e => random.Next())
                            .Take(1)
                            .ToList();

                        foreach (var winner in winners)
                        {
                            winner.IsWinner = true;
                        }
                    }
                    else
                    {
                        return Json(new { success = true, message = "A winner has already been selected so no new winners can be awarded" });
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Winners selected successfully!" });
            }
            else
            {
                return Json(new { success = true, message = "No raffles need winners to be selected for the previous month." });
            }
        }
    }
}
