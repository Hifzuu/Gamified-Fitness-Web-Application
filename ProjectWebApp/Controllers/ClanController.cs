using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProjectWebApp.Data;
using ProjectWebApp.Data.Migrations;
using ProjectWebApp.Models;
using System;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ProjectWebApp.Controllers
{
    public class ClanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private static readonly Random random = new Random(Guid.NewGuid().GetHashCode());
        private readonly ILogger<ClanController> _logger; 

        public ClanController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<ClanController> logger) 
        {
            _userManager = userManager;
            _context = context;
            _logger = logger; 
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // Retrieve clans created by the user
                var clansCreatedByUser = await _context.Clans
                    .Where(c => c.CreatorId == userId)
                    .Include(c => c.Creator)
                    .Include(c => c.Members)
                    .ToListAsync();

                // Retrieve clans where the user is a member
                var clansForMember = await _context.Clans
                    .Include(c => c.Creator)
                    .Include(c => c.Members)
                    .Where(c => c.Members.Any(u => u.Id == userId))
                    .ToListAsync();

                // Combine the two lists
                var userClans = clansCreatedByUser.Concat(clansForMember).ToList();

                // Initialize variables for challenge info
                ClanChallenge activeClanChallenge = null;
                string formattedTimeForChallenge = string.Empty;
                double challengeProgressPercentage = 0;
                List<Workout> matchingWorkouts = new List<Workout>();

                if (userClans.Any())
                {
                    var userClanId = userClans.First().ClanId;

                    AssignClanChallengeForAllMembers(userClanId);

                    // retrieve current clan challenge
                    activeClanChallenge = GetClanChallenge(userClanId);
                    formattedTimeForChallenge = activeClanChallenge != null
                        ? FormatChallengeTime(activeClanChallenge)
                        : string.Empty;

                    int dailyCountProgress = activeClanChallenge?.CountProgress ?? 0;

                    if (activeClanChallenge != null && activeClanChallenge.Challenge.TargetCount > 0)
                    {
                        challengeProgressPercentage = (double)dailyCountProgress / activeClanChallenge.Challenge.TargetCount * 100;
                    }

                    var activeChallengeType = activeClanChallenge?.Challenge?.Type;

                    // retrieve workouts for the challenge type
                    matchingWorkouts = await _context.Workouts
                        .Where(w => w.Category == activeChallengeType)
                        .ToListAsync();
                }

                // populate clan view model
                var clanViewModels = userClans.Select(clan => new ClanViewModel
                {
                    ClanId = clan.ClanId,
                    Name = clan.Name,
                    CreatorUserName = clan.Creator.UserName,
                    CreatorId = clan.Creator.Id,
                    Members = clan.Members.ToList(),
                    ClanPoints = clan.ClanPoints,
                    bio = clan.bio,
                    ClanChallenge = activeClanChallenge,
                    ChallengeTime = formattedTimeForChallenge,
                    ChallengeProgressPercentage = challengeProgressPercentage,
                    Workouts = matchingWorkouts,
                }).ToList();

                _logger.LogInformation("Model count: {Count}", (clanViewModels != null ? clanViewModels.Count : 0));

                return View(clanViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching user clans.");
                throw; // Re-throw the exception for global exception handling
            }
        }

        private void UpdateClanChallengeParticipants(int clanId)
        {
            var clanChallenge = _context.ClanChallenges
                .Include(cc => cc.Participants)
                .FirstOrDefault(cc => cc.ClanId == clanId);

            if (clanChallenge != null)
            {
                var clan = _context.Clans.Include(c => c.Members).FirstOrDefault(c => c.ClanId == clanId);

                if (clan != null)
                {
                    var currentMembers = clan.Members.ToList();

                    // Remove participants who are no longer in the clan
                    var removedParticipants = clanChallenge.Participants
                        .Where(p => !currentMembers.Any(m => m.Id == p.Id))
                        .ToList();

                    foreach (var removedParticipant in removedParticipants)
                    {
                        clanChallenge.Participants.Remove(removedParticipant);
                    }

                    // Add new members as participants
                    foreach (var member in currentMembers)
                    {
                        if (!clanChallenge.Participants.Any(p => p.Id == member.Id))
                        {
                            clanChallenge.Participants.Add(member);
                        }
                    }

                    _context.SaveChanges();
                }
            }
        }

        private string FormatChallengeTime(ClanChallenge clanChallenge)
        {
            if (clanChallenge != null)
            {
                DateTime clanChallengeEnd = clanChallenge.EndDate;
                TimeSpan timeRemaining = clanChallengeEnd - DateTime.Now;
                int secondsRemaining = (int)Math.Max(0, timeRemaining.TotalSeconds);
                string formattedTime = DateTime.Now.AddSeconds(secondsRemaining).ToString("yyyy-MM-dd HH:mm:ssZ");
                return formattedTime;
            }
            else
            {
                return string.Empty; 
            }

        }

        private ClanChallenge GetClanChallenge(int? clanId)
        {
            try
            {
                DateTime currentDate = DateTime.Now.Date;
                DateTime startOfWeek = currentDate.StartOfWeek(DayOfWeek.Monday);

                // retrieve current weekly clan challenge
                var clanChallenge = _context.ClanChallenges
                    .Include(uc => uc.Challenge)
                        .ThenInclude(c => c.Workouts) 
                    .FirstOrDefault(uc => uc.ClanId == clanId
                                            && uc.StartDate.Date == startOfWeek.Date
                                            && uc.Challenge.ChallengeType == ChallengeType.Clan);

                // Update participants based on current clan members
                if (clanChallenge != null)
                {
                    var currentMembers = _context.Users.Where(u => u.ClanId == clanId).ToList();

                    // Remove participants who are no longer in the clan
                    var removedParticipants = clanChallenge.Participants
                        .Where(p => !currentMembers.Any(m => m.Id == p.Id))
                        .ToList();

                    foreach (var removedParticipant in removedParticipants)
                    {
                        clanChallenge.Participants.Remove(removedParticipant);
                    }

                    // Add new members as participants
                    foreach (var member in currentMembers)
                    {
                        if (!clanChallenge.Participants.Any(p => p.Id == member.Id))
                        {
                            clanChallenge.Participants.Add(member);
                        }
                    }

                    _context.SaveChanges(); 
                }

                return clanChallenge;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving clan challenge: {ex.Message}");
                return null;
            }
        }

        private void AssignClanChallengeForAllMembers(int clanId)
        {
            try
            {
                DateTime currentDate = DateTime.Now;
                DateTime startOfWeek = currentDate.StartOfWeek(DayOfWeek.Monday); // every week
                DateTime endOfWeek = startOfWeek.AddDays(6).EndOfDay();

                // check if challenge exists for this week
                bool hasChallenge = _context.ClanChallenges
                    .Any(uc => uc.ClanId == clanId
                                && uc.StartDate.Date >= startOfWeek.Date
                                && uc.EndDate.Date <= endOfWeek.Date
                                && uc.Challenge.ChallengeType == ChallengeType.Clan);

                if (!hasChallenge)
                {
                    // Get all available weekly challenges
                    var clanChallenges = _context.Challenges
                        .Where(c => c.ChallengeType == ChallengeType.Clan)
                        .ToList();

                    if (clanChallenges.Count > 0)
                    {
                        // Pick a random weekly challenge
                        var random = new Random();
                        Challenge randomChallenge = clanChallenges[random.Next(clanChallenges.Count)];

                        ClanChallenge clanChallenge = new ClanChallenge
                        {
                            ClanId = clanId,
                            ChallengeId = randomChallenge.ChallengeId,
                            StartDate = startOfWeek,
                            EndDate = endOfWeek,
                            CountProgress = 0
                        };

                        _context.ClanChallenges.Add(clanChallenge);
                        _context.SaveChanges();

                        // Update ClanChallengeId for all members of the clan
                        var clanMembers = _context.Users.Where(u => u.ClanId == clanId).ToList();
                        foreach (var member in clanMembers)
                        {
                            member.ClanChallengeId = clanChallenge.ClanChallengeId;
                        }

                        _context.SaveChanges();
                    }
                    else
                    {
                        Console.WriteLine("No clan challenges available.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error assigning clan challenge: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateClan(ClanViewModel model)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    // Check for empty clan name
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
                        bio = model.bio,
                    };

                    // Add new clan to the Clans database.
                    _context.Clans.Add(newClan);

                    // Load the Creator navigation property of the new clan 
                    await _context.Entry(newClan).Reference(c => c.Creator).LoadAsync();

                    // Save changes asynchronously to the database
                    await _context.SaveChangesAsync();

                    var user = await _userManager.FindByIdAsync(userId);
                    newClan.Creator = user;
                    user.ClanId = newClan.ClanId;
                    await _userManager.UpdateAsync(user);


                    await _context.SaveChangesAsync();

                    // assign challenge for all members of the new clan
                    AssignClanChallengeForAllMembers(newClan.ClanId);

                    // Map ClanViewModel to the new clan
                    var clanViewModel = new ClanViewModel
                    {
                        ClanId = newClan.ClanId,
                        Name = newClan.Name,
                        CreatorUserName = newClan.Creator.UserName,
                        CreatorId = newClan.Creator.Id,
                        Members = newClan.Members.ToList(),
                        ClanPoints = newClan.ClanPoints,
                        bio = newClan.bio,
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
                return Json(new { success = false, message = "An error occurred during the clan creation process." });
            }
        }

        // Helper method to check for duplicate clan name
        private bool IsDuplicateClanName(string clanName)
        {
            try
            {
                var normalizedClanName = clanName.ToUpperInvariant();
                var clans = _context.Clans.ToList();
                return clans.Any(c => string.Equals(c.Name, normalizedClanName, StringComparison.OrdinalIgnoreCase)); //case sensitive
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> JoinClan(int clanId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var clanToJoin = await _context.Clans
                    .Include(c => c.Members)
                    .FirstOrDefaultAsync(c => c.ClanId == clanId);

                if (clanToJoin != null)
                {
                    // if user is not part of this clan
                    if (!clanToJoin.Members.Any(u => u.Id == userId))
                    {
                        // check if there is space in clan
                        if (clanToJoin.Members.Count >= 30)
                        {
                            return Json(new { success = false, message = "This clan has reached its maximum capacity. No more members can join at the moment." });
                        }

                        // Check if the user is already a member of another clan
                        var userCurrentClan = await _context.Clans
                            .Include(c => c.Members)
                            .FirstOrDefaultAsync(c => c.Members.Any(u => u.Id == userId));

                        if (userCurrentClan != null)
                        {
                            return Json(new { success = false, message = "You are already a member of another clan. Please leave your current clan before joining a new one." });
                        }

                        var user = await _userManager.FindByIdAsync(userId);
                        clanToJoin.Members.Add(user);

                        UpdateClanChallengeParticipants(clanId);
                        await _context.SaveChangesAsync();
                        return Json(new { success = true, message = "Successfully joined the clan." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "You are already a member of the clan." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Clan not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while joining the clan." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> LeaveClan(int clanId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
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
                        UpdateClanChallengeParticipants(clanId);
                        await _context.SaveChangesAsync();
                        return Json(new { success = true, message = "Successfully left the clan." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "You are not a member of this clan." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Clan not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while leaving the clan." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteClan(int clanId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var clanToDelete = await _context.Clans
                    .Include(c => c.Members)
                    .Include(c => c.ClanChallenges)
                    .FirstOrDefaultAsync(c => c.ClanId == clanId);

                if (clanToDelete != null)
                {
                    // check if its the leader
                    if (clanToDelete.CreatorId == userId)
                    {
                        // Set ClanId to null for all members
                        foreach (var member in clanToDelete.Members)
                        {
                            member.ClanId = null;
                        }

                        // Delete associated clan challenges and clan with members
                        _context.ClanChallenges.RemoveRange(clanToDelete.ClanChallenges);
                        _context.Clans.Remove(clanToDelete);
                        await _context.SaveChangesAsync();
                        return Json(new { success = true, message = "Successfully deleted the clan." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "You are not authorized to delete this clan." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Clan not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while deleting the clan." });
            }
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

            return Json(0); // Return 0 for unauthenticated users 
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
                    return Ok(); 
                }
            }

            return BadRequest("Failed to update points");
        }


        public async Task<IActionResult> getAvailableClans()
        {
            try
            {
                // retroeve all clans except users current clan
                var allClans = await _context.Clans
                    .ToListAsync();
                var userId = _userManager.GetUserId(User);
                var userClans = await GetUserClans(userId);
                var otherClans = allClans.Except(userClans).ToList();
                return Json(otherClans);
            }
            catch (Exception ex)
            {
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

        public ActionResult Leaderboards()
        {
            var leaderboardsData = _context.Clans
                .OrderByDescending(clan => clan.ClanPoints)
                .Select(clan => new
                {
                    clan.Name,
                    Points = clan.ClanPoints
                })
                .ToList();

            return Json(leaderboardsData);
        }

        [HttpPost]
        public async Task<IActionResult> KickMember(string memberId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                // chekck if user is leader
                if (currentUser == null || currentUser.ClanId == null)
                {
                    return Json(new { success = false, message = "You are not authorized to kick members from a clan." });
                }


                var memberToKick = await _context.Users.FirstOrDefaultAsync(u => u.Id == memberId);
                if (memberToKick == null)
                {
                    return Json(new { success = false, message = "Member not found." });
                }

                var clan = await _context.Clans
                    .Include(c => c.Members)
                    .FirstOrDefaultAsync(c => c.ClanId == currentUser.ClanId);
                if (clan == null)
                {
                    return Json(new { success = false, message = "Clan not found." });
                }

                // Check if the current user is the leader of the clan
                if (clan.CreatorId != currentUser.Id)
                {
                    return Json(new { success = false, message = "You are not authorized to kick members from this clan." });
                }

                // Check if the member to be kicked is a member of the clan
                if (!clan.Members.Any(m => m.Id == memberId))
                {
                    return Json(new { success = false, message = "The specified member is not a member of this clan." });
                }

                clan.Members.Remove(memberToKick);
                UpdateClanChallengeParticipants(clan.ClanId);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Member kicked successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while kicking the member from the clan." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchClans(string searchText)
        {
            try
            {
                // Return an empty result if search text is null or empty
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return Json(new List<Clan>());
                }

                searchText = searchText.ToLower();
                var matchingClans = await _context.Clans
                    .Where(c => c.Name.ToLower().Contains(searchText))
                    .ToListAsync();

                return Json(matchingClans);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while searching for clans.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditClanBio(int clanId, string newBio)
        {
            var userId = _userManager.GetUserId(User);

            // Check if the current user is the leader of the clan
            var clan = await _context.Clans.FirstOrDefaultAsync(c => c.ClanId == clanId && c.CreatorId == userId);
            if (clan == null)
            {
                return Json(new { success = false, message = "You are not authorized to edit this clan's bio." });
            }

            // Update the clan bio
            clan.bio = newBio;
            _context.Clans.Update(clan);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Clan bio updated successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePointsChallenge(int points, int clanChallengeId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var clan = await _context.Clans
                    .Include(c => c.Members)
                    .FirstOrDefaultAsync(c => c.ClanId == currentUser.ClanId);

                if (clan != null)
                {
                    // Mark the challenge as claimed
                    var clanChallenge = _context.ClanChallenges.Find(clanChallengeId);
                    if (clanChallenge != null && !clanChallenge.IsRewardClaimed)
                    {
                        clan.ClanPoints += points;
                        clanChallenge.IsRewardClaimed = true;
                        _context.SaveChanges();
                        return Ok(); 
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
}


