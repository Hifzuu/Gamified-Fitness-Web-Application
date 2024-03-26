using System.ComponentModel.DataAnnotations;

namespace ProjectWebApp.Models
{
    public class UserRaffleEntry
    {
        [Key]
        public int RaffleEntryId { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int RaffleId { get; set; }
        public Raffle Raffle { get; set; }

        public DateTime EntryTimestamp { get; set; }
        public bool IsWinner { get; set; }

    }
}
