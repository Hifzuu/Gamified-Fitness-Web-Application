using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic; 

namespace ProjectWebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            FirstName = "";
            LastName = "";
            Gender = "";
            Country = "";
            CustomUserName = string.Empty;
            ProfileImageUrl = "/Images/empty-pfp.png";
        }

        public String CustomUserName {  get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Now;
        public string Country { get; set; }
        public int Height { get; set; }
        public double GoalWeight { get; set; }
        public List<string> SelectedGoals { get; set; } = new List<string>(); 
        public List<string> ExerciseFocus { get; set; } = new List<string>();
        public List<WeightEntry> WeightEntries { get; set; } = new List<WeightEntry>();

        // Foreign key for the clan the user belongs to
        public int? ClanId { get; set; }
        public Clan Clan { get; set; }

        public int Points { get; set; }
        public List<UserFavouriteWorkout>? FavouriteWorkouts { get; set; } = new List<UserFavouriteWorkout>();
        public List<UserCalendar> UserCalendars { get; set; } = new List<UserCalendar>();
        public string ProfileImageUrl { get; set; }
        public List<LoginStreak> LoginStreaks { get; set; } = new List<LoginStreak>();
        public List<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();
        public List<UserRaffleEntry> UserRaffleEntries { get; set; } = new List<UserRaffleEntry>();
    }
}