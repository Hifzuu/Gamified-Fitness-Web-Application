using System.ComponentModel.DataAnnotations;

namespace ProjectWebApp.Models
{
    public class UserRaffleEntry
    {
        [Key]
        public int RaffleEntryId { get; set; }

        // Foreign key to link with ApplicationUser
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // Foreign key to link with Raffle
        public int RaffleId { get; set; }
        public Raffle Raffle { get; set; }

        // Additional attributes
        public DateTime EntryTimestamp { get; set; }
        public bool IsWinner { get; set; }

    }
}
