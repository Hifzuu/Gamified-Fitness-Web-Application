namespace ProjectWebApp.Models
{
    public class UserFavouriteWorkout
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int WorkoutId { get; set; }
        public Workout Workout { get; set; }
    }
}