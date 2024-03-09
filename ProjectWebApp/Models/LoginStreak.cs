namespace ProjectWebApp.Models
{
    public class LoginStreak
    {
        public string UserId { get; set; }
        public DateTime LastLoginTime { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }

        // Navigation property to link back to ApplicationUser
        public ApplicationUser User { get; set; }

        // Navigation property to link back to LoginStreakStreakReward
        public List<UserStreakReward> UserStreakRewards { get; set; }
    }

    public class StreakReward
    {
        public int RewardId { get; set; } // Primary key
        public string MedalText { get; set; }
        public int Days { get; set; }
        public int Points { get; set; }

        // Navigation property to link back to UserStreakReward
        public List<UserStreakReward> UserStreakRewards { get; set; }
    }

}

