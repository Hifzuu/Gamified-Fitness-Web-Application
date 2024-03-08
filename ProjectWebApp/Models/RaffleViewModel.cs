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

        public List<RaffleViewModel> Raffles { get; set; }

        public List<RaffleViewModel> UserEnteredRaffles { get; set; }
        public List<RaffleViewModel> winners { get; set; }
        public DateTime EntryTimestamp { get; set; }
        public bool isWinner {  get; set; }
        public string userId { get; set; }

    }

}
