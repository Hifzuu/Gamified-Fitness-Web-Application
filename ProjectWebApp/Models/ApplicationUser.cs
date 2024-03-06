using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic; // Add this for List

namespace ProjectWebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            // Initialize non-nullable properties in the constructor
            FirstName = "";
            LastName = "";
            Gender = "";
            Country = "";
            CustomUserName = string.Empty;
            ProfileImageUrl = "/Images/empty-pfp.png";
        }

        //Registration properties
        public String CustomUserName {  get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Now;
        public string Country { get; set; }
        public int Height { get; set; }
        //public double CurrentWeight { get; set; }
        public double GoalWeight { get; set; }
        public List<string> SelectedGoals { get; set; } = new List<string>(); 
        public List<string> ExerciseFocus { get; set; } = new List<string>();
        public List<WeightEntry> WeightEntries { get; set; } = new List<WeightEntry>();

        //Additional properties for rest of app
        public int Points { get; set; }
        public List<UserFavouriteWorkout>? FavouriteWorkouts { get; set; } = new List<UserFavouriteWorkout>();
        public List<UserCalendar> UserCalendars { get; set; } = new List<UserCalendar>();
        public string ProfileImageUrl { get; set; }
        public List<LoginStreak> LoginStreaks { get; set; } = new List<LoginStreak>();
        public List<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();
        public List<UserRaffleEntry> UserRaffleEntries { get; set; } = new List<UserRaffleEntry>();
    }
}