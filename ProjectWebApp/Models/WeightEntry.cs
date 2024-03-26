namespace ProjectWebApp.Models
{
    public class WeightEntry
    {
        public string UserId { get; set; }   
        public ApplicationUser User { get; set; } 

        public DateTime Date { get; set; }
        public double Weight { get; set; }
    }
}
