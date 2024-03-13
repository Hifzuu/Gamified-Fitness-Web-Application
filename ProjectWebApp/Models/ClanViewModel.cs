// ClanViewModel.cs
namespace ProjectWebApp.Models
{
    public class ClanViewModel
    {
        public int ClanId { get; set; }
        public string Name { get; set; }
        public string CreatorUserName { get; set; }
        public List<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();
        // Add more properties as needed
    }
}
