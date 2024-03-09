namespace ProjectWebApp.Models
{
    public class DashboardViewModel
    {
        public List<WeightEntry> WeightData { get; set; }
        public LoginStreak LoginStreak { get; set; }
        public List<StreakReward> Rewards { get; set; }
        public List<UserStreakReward> ClaimableRewards { get; set; }
    }
}
