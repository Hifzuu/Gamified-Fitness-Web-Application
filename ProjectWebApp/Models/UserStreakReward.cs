namespace ProjectWebApp.Models
{
    public class UserStreakReward
    {
        public int UserStreakRewardId { get; set; }
        public string UserId { get; set; }
        public DateTime LastLoginTime { get; set; }
        public int RewardId { get; set; }
        public bool Claimed { get; set; }

        public LoginStreak LoginStreak { get; set; }
        public StreakReward StreakReward { get; set; }
    }
}
