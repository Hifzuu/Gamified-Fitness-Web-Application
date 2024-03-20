namespace ProjectWebApp.Models
{
    public class Challenge
    {
        public int ChallengeId { get; set; } // Primary Key

        public string Name { get; set; } // Name of the challenge
        public string Description { get; set; } // Description of the challenge
        public string Type { get; set; } // Type of challenge (e.g., cardio, strength, etc.)
        public int TargetCount { get; set; } // Target count for the challenge (e.g., number of workouts to complete)
        public string MeasurementType { get; set; } // Type of measurement for progress (e.g., duration, repetitions, etc.)

        // Navigation property for the many-to-many relationship
        public List<Workout> Workouts { get; set; } = new List<Workout>();

        public ChallengeType ChallengeType { get; set; }
        public MeasurementCriteria MeasurementCriteria { get; set; }
    }

    public enum ChallengeType
    {
        Daily,
        Weekly,
        Monthly,
        Clan
    }

    public enum MeasurementCriteria
    {
        TotalTime,
        WorkoutCategoryCount
        // Add more as needed
    }
}
