using System.ComponentModel.DataAnnotations;

namespace ProjectWebApp.Models
{
    public class UserChallenge
    {
        public int UserChallengeId { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int ChallengeId { get; set; }
        public Challenge Challenge { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        public List<Workout> Workouts { get; set; } = new List<Workout>();

        public int CountProgress { get; set; }

        public bool IsRewardClaimed { get; set; }


    }


}
