namespace ProjectWebApp.Models
{
    public class Clan
    {
        public int ClanId { get; set; }
        public string Name { get; set; }
        public string? bio {  get; set; }
        public int ClanPoints { get; set; } 

        public string CreatorId { get; set; }
        public ApplicationUser Creator { get; set; }

        public List<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();

        public List<ClanChallenge> ClanChallenges { get; set; } = new List<ClanChallenge>();
    }
}
