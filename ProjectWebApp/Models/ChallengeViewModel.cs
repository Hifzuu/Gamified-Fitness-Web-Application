using System;
using System.Collections.Generic;
using ProjectWebApp.Models;

namespace ProjectWebApp.ViewModels
{
    public class ChallengeViewModel
    {
        public UserChallenge DailyChallenge { get; set; }
        public List<Workout> DailyWorkouts { get; set; }
        public string DailyTime { get; set; }
        public double DailyProgressPercentage { get; set; }

        public UserChallenge WeeklyChallenge { get; set; }
        public List<Workout> WeeklyWorkouts { get; set; }
        public string WeeklyTime { get; set; }
        public double WeeklyProgressPercentage { get; set; }

        public UserChallenge MonthlyChallenge { get; set; }
        public List<Workout> MonthlyWorkouts { get; set; }
        public string MonthlyTime { get; set; }
        public double MonthlyProgressPercentage { get; set; }


        public List<Workout> DailyChallengeWorkouts { get; set; } = new List<Workout>();
        public List<Workout> WeeklyChallengeWorkouts { get; set; } = new List<Workout>();
        public List<Workout> MonthlyChallengeWorkouts { get; set; } = new List<Workout>();



    }
}
