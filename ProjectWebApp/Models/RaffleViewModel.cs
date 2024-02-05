namespace ProjectWebApp.Models
{
    public class RaffleViewModel
    {
        public int RaffleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Cost { get; set; }
        public string ImageUrl { get; set; }
    }

}
