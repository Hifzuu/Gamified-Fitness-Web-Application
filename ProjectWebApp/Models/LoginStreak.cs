namespace ProjectWebApp.Models
{
    public class LoginStreak
    {
        public string UserId { get; set; }
        public DateTime LastLoginTime { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }

        // Additional properties if needed

        // Navigation property to link back to ApplicationUser
        public ApplicationUser User { get; set; }
    }
}

