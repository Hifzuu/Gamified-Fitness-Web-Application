namespace ProjectWebApp.Models
{
    public class ClanChallenge
    {
        public int ClanChallengeId { get; set; }

        public int ClanId { get; set; }
        public Clan Clan { get; set; }

        public int ChallengeId { get; set; }
        public Challenge Challenge { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<Workout> Workouts { get; set; } = new List<Workout>();

        public int CountProgress { get; set; }

        public bool IsRewardClaimed { get; set; }

        public List<ApplicationUser> Participants { get; set; } = new List<ApplicationUser>();
    }

}
