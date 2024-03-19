namespace ProjectWebApp.Models
{
    public class Workout
    {
        public int WorkoutId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public string ImageUrl { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsFavourited { get; set; } = false;
        public bool IsFeatured { get; set; } = false;
        public string difficulty { get; set; }
        public string TargetMuscle { get; set; }
        public int rewardPoints { get; set; }
        public List<UserFavouriteWorkout> UsersFavourited { get; set; } = new List<UserFavouriteWorkout>();
        public List<Challenge> Challenges { get; set; } = new List<Challenge>();
        public List<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();
    }
}
