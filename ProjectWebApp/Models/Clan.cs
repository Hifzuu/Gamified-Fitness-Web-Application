namespace ProjectWebApp.Models
{
    public class Clan
    {
        public int ClanId { get; set; }
        public string Name { get; set; }

        // Foreign key for the creator
        public string CreatorId { get; set; }

        // Navigation property for the creator
        public ApplicationUser Creator { get; set; } 

        // List of users in the clan
        public List<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();
    }
}
