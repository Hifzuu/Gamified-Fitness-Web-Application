using System.ComponentModel.DataAnnotations;

namespace ProjectWebApp.Models
{
    public class UserWorkoutStats
    {
        public string UserWorkoutStatsId {  get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int WorkoutCount { get; set; }
        public int TotalWorkoutDuration { get; set; }

        public int CardioCompletedCount { get; set; }
        public int HIITCompletedCount { get; set; }
        public int StrengthTrainingCompletedCount { get; set; }
        public int RunningCompletedCount { get; set; }
        public int YogaCompletedCount { get; set; }
        public int PilatesCompletedCount { get; set; }
        public int BalancedWorkoutsCompletedCount { get; set; }
    }
}
