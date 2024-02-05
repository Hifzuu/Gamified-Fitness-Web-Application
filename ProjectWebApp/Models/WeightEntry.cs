namespace ProjectWebApp.Models
{
    public class WeightEntry
    {
        public string UserId { get; set; }      // Foreign key to ApplicationUser
        public ApplicationUser User { get; set; } // Navigation property to ApplicationUser

        public DateTime Date { get; set; }
        public double Weight { get; set; }
    }
}
