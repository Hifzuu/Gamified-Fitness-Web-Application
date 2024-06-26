﻿namespace ProjectWebApp.Models
{
    public class LoginStreak
    {
        public string UserId { get; set; }
        public DateTime LastLoginTime { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public ApplicationUser User { get; set; }
        public List<UserStreakReward> UserStreakRewards { get; set; }
    }

    public class StreakReward
    {
        public int RewardId { get; set; } 
        public string MedalText { get; set; }
        public int Days { get; set; }
        public int Points { get; set; }
        public List<UserStreakReward> UserStreakRewards { get; set; }
    }

}

