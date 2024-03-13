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
    public class CommunityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CommunityController> _logger; // Add logger

        public CommunityController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<CommunityController> logger) // Add logger parameter
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
                Members = clan.Members.ToList(),
                // Map other properties as needed
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
                    var userId = _userManager.GetUserId(User);

                    // Create a new Clan entity
                    var newClan = new Clan
                    {
                        Name = model.Name,
                        CreatorId = userId,
                    };

                    // Save the new clan to the database
                    _context.Clans.Add(newClan);
                    await _context.Entry(newClan).Reference(c => c.Creator).LoadAsync();
                    await _context.SaveChangesAsync();

                    // Map Clan entity to ClanViewModel
                    var clanViewModel = new ClanViewModel
                    {
                        ClanId = newClan.ClanId,
                        Name = newClan.Name,
                        CreatorUserName = newClan.Creator.UserName, 
                        Members = newClan.Members.ToList(),
                        // Map other properties as needed
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








        public async Task<IActionResult> getAvailableClans()
        {
            // Retrieve all clans
            var allClans = await _context.Clans
                .ToListAsync();

            // Return JSON data
            return Json(allClans);
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









    }



}
