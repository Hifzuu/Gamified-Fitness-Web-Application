using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProjectWebApp.Data;
using ProjectWebApp.Models;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ProjectWebApp.Controllers
{
    public class ClanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ClanController> _logger; // Add logger

        public ClanController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<ClanController> logger) // Add logger parameter
        {
            _userManager = userManager;
            _context = context;
            _logger = logger; // Initialize logger
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // Retrieve clans where the current user is the creator
            var clansCreatedByUser = await _context.Clans
                .Where(c => c.CreatorId == userId)
                .Include(c => c.Creator)
                .Include(c => c.Members)
                .ToListAsync();

            // Retrieve clans where the current user is a member
            var clansForMember = await _context.Clans
                .Include(c => c.Creator)
                .Include(c => c.Members)
                .Where(c => c.Members.Any(u => u.Id == userId))
                .ToListAsync();

            // Combine the two lists
            var userClans = clansCreatedByUser.Concat(clansForMember).ToList();

            // Map Clan entities to ClanViewModel
            var clanViewModels = userClans.Select(clan => new ClanViewModel
            {
                ClanId = clan.ClanId,
                Name = clan.Name,
                CreatorUserName = clan.Creator.UserName,
                CreatorId = clan.Creator.Id,
                Members = clan.Members.ToList(),
                ClanPoints = clan.ClanPoints,
            }).ToList();

            return View(clanViewModels);
        }



        [HttpPost]
        public async Task<IActionResult> CreateClan(ClanViewModel model)
        {
            _logger.LogInformation($"Received request to create clan with name: {model.Name}");

            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    // Check for an empty clan name
                    if (string.IsNullOrWhiteSpace(model.Name))
                    {
                        return Json(new { success = false, message = "Clan name cannot be empty." });
                    }

                    // Check for duplicate clan name
                    if (IsDuplicateClanName(model.Name))
                    {
                        return Json(new { success = false, message = "Clan name is already taken. Please choose a different name." });
                    }

                    var userId = _userManager.GetUserId(User);

                    // Create a new Clan entity
                    var newClan = new Clan
                    {
                        Name = model.Name,
                        CreatorId = userId,
                        ClanPoints = 0,
                    };

                    // Save the new clan to the database
                    _context.Clans.Add(newClan);
                    await _context.Entry(newClan).Reference(c => c.Creator).LoadAsync();
                    await _context.SaveChangesAsync();

                    // Assign the ClanId to the ApplicationUser (user who created the clan)
                    var user = await _userManager.FindByIdAsync(userId);

                    // Add the user as the creator of the clan
                    newClan.Creator = user;

                    // Set ClanId for the creator
                    user.ClanId = newClan.ClanId;

                    // Save changes to the user
                    await _userManager.UpdateAsync(user);

                    // Save changes to the clan
                    await _context.SaveChangesAsync();

                    // Map ClanViewModel to the newly created clan
                    var clanViewModel = new ClanViewModel
                    {
                        ClanId = newClan.ClanId,
                        Name = newClan.Name,
                        CreatorUserName = newClan.Creator.UserName,
                        CreatorId = newClan.Creator.Id,
                        Members = newClan.Members.ToList(),
                        ClanPoints = newClan.ClanPoints,
                    };

                    return Json(new { success = true, message = "Clan created successfully!", clanId = newClan.ClanId });
                }
                else
                {
                    return Json(new { success = false, message = "User not authenticated. Please log in and try again." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                _logger.LogError(ex, "Error creating clan");

                return Json(new { success = false, message = "An error occurred during the clan creation process." });
            }
        }
        // Helper method to check for duplicate clan name
        private bool IsDuplicateClanName(string clanName)
        {
            try
            {
                // Use StringComparison.OrdinalIgnoreCase outside of the query
                var normalizedClanName = clanName.ToUpperInvariant();

                // Materialize the list before using LINQ-to-Objects
                var clans = _context.Clans.ToList();

                return clans.Any(c => string.Equals(c.Name, normalizedClanName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                _logger.LogError(ex, "Error checking duplicate clan name");

                // Propagate the exception
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> JoinClan(int clanId)
        {
            try
            {
                // Retrieve the current user
                var userId = _userManager.GetUserId(User);

                // Retrieve the clan to join
                var clanToJoin = await _context.Clans
                    .Include(c => c.Members)
                    .FirstOrDefaultAsync(c => c.ClanId == clanId);

                if (clanToJoin != null)
                {
                    // Check if the user is already a member of the clan
                    if (!clanToJoin.Members.Any(u => u.Id == userId))
                    {
                        // Check if the user is already a member of another clan
                        var userCurrentClan = await _context.Clans
                            .Include(c => c.Members)
                            .FirstOrDefaultAsync(c => c.Members.Any(u => u.Id == userId));

                        if (userCurrentClan != null)
                        {
                            // User is already a member of a different clan
                            return Json(new { success = false, message = "You are already a member of another clan. Please leave your current clan before joining a new one." });
                        }

                        // Add the user to the clan
                        var user = await _userManager.FindByIdAsync(userId);
                        clanToJoin.Members.Add(user);

                        // Update the database
                        await _context.SaveChangesAsync();

                        // Return a JSON response with success message
                        return Json(new { success = true, message = "Successfully joined the clan." });
                    }
                    else
                    {
                        // User is already a member of the clan
                        return Json(new { success = false, message = "You are already a member of the clan." });
                    }
                }
                else
                {
                    // Clan not found
                    return Json(new { success = false, message = "Clan not found." });
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return Json(new { success = false, message = "An error occurred while joining the clan." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> LeaveClan(int clanId)
        {
            try
            {
                // Retrieve the current user
                var userId = _userManager.GetUserId(User);

                // Retrieve the clan to leave
                var clanToLeave = await _context.Clans
                    .Include(c => c.Members)
                    .FirstOrDefaultAsync(c => c.ClanId == clanId);

                if (clanToLeave != null)
                {
                    // Check if the user is a member of the clan
                    var user = clanToLeave.Members.FirstOrDefault(u => u.Id == userId);

                    if (user != null)
                    {
                        // Remove the user from the clan
                        clanToLeave.Members.Remove(user);

                        // Update the database
                        await _context.SaveChangesAsync();

                        return Json(new { success = true, message = "Successfully left the clan." });
                    }
                    else
                    {
                        // User is not a member of the clan
                        return Json(new { success = false, message = "You are not a member of this clan." });
                    }
                }
                else
                {
                    // Clan not found
                    return Json(new { success = false, message = "Clan not found." });
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return Json(new { success = false, message = "An error occurred while leaving the clan." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteClan(int clanId)
        {
            try
            {
                // Retrieve the current user
                var userId = _userManager.GetUserId(User);

                // Retrieve the clan to delete
                var clanToDelete = await _context.Clans
                    .Include(c => c.Members)
                    .FirstOrDefaultAsync(c => c.ClanId == clanId);

                if (clanToDelete != null)
                {
                    // Check if the current user is the creator of the clan
                    if (clanToDelete.CreatorId == userId)
                    {
                        // Set ClanId to null for all members
                        foreach (var member in clanToDelete.Members)
                        {
                            member.ClanId = null;
                        }

                        // Delete the clan (including members)
                        _context.Clans.Remove(clanToDelete);

                        // Update the database
                        await _context.SaveChangesAsync();

                        return Json(new { success = true, message = "Successfully deleted the clan." });
                    }
                    else
                    {
                        // User is not the creator of the clan
                        return Json(new { success = false, message = "You are not authorized to delete this clan." });
                    }
                }
                else
                {
                    // Clan not found
                    return Json(new { success = false, message = "Clan not found." });
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return Json(new { success = false, message = "An error occurred while deleting the clan." });
            }
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


        [HttpPost]
        public async Task<IActionResult> UpdatePoints(int points)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                currentUser.Points -= points;
                var result = await _userManager.UpdateAsync(currentUser);

                if (result.Succeeded)
                {
                    return Ok(); // or return a JSON response if needed
                }
            }

            return BadRequest("Failed to update points");
        }


        public async Task<IActionResult> getAvailableClans()
        {
            try
            {
                var allClans = await _context.Clans
                    .ToListAsync();

                // Filter out the user's clans
                var userId = _userManager.GetUserId(User);
                var userClans = await GetUserClans(userId);

                var otherClans = allClans.Except(userClans).ToList();

                return Json(otherClans);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                _logger.LogError(ex, "Error fetching available clans");

                return Json(new { error = "An error occurred while fetching available clans." });
            }
        }

        private async Task<List<Clan>> GetUserClans(string userId)
        {
            var clansCreatedByUser = await _context.Clans
                .Where(c => c.CreatorId == userId)
                .ToListAsync();

            var clansForMember = await _context.Clans
                .Where(c => c.Members.Any(u => u.Id == userId))
                .ToListAsync();

            var userClans = clansCreatedByUser.Concat(clansForMember).ToList();
            return userClans;
        }




    }



}
