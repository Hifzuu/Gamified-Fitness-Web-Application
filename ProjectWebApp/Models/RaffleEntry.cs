namespace ProjectWebApp.Models
{
    public class RaffleEntry
    {
        public int RaffleEntryId { get; set; }

        // Foreign key to link with ApplicationUser
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // Foreign key to link with Raffle
        public int RaffleId { get; set; }
        public Raffle Raffle { get; set; }
    }
}
