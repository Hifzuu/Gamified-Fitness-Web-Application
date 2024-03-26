namespace ProjectWebApp.Models
{
    public class Challenge
    {
        public int ChallengeId { get; set; } 

        public string Name { get; set; } 
        public string Description { get; set; } 
        public string Type { get; set; } 
        public int TargetCount { get; set; }
        public string MeasurementType { get; set; } 


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
    }
}
