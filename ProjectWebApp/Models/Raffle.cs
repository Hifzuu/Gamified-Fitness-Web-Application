using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectWebApp.Models
{
    public class Raffle
    {
        public int RaffleId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Cost { get; set; }
        public string ImageUrl { get; set; }

        // Navigation property for users who entered this raffle
        public List<UserRaffleEntry> UserRaffleEntries { get; set; } = new List<UserRaffleEntry>();
    }
}
