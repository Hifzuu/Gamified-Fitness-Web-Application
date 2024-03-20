// ClanViewModel.cs
namespace ProjectWebApp.Models
{
    public class ClanViewModel
    {
        public int ClanId { get; set; }
        public string Name { get; set; }
        public string bio { get; set; }
        public string CreatorUserName { get; set; }
        public int ClanPoints { get; set; }
        public string CreatorId { get; set; }
        public List<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();
        public ClanChallenge? ClanChallenge { get; set; }
        public string? ChallengeTime { get; set; }
        public double ChallengeProgressPercentage { get; set; }
    }
}
