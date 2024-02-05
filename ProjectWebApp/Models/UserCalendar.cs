namespace ProjectWebApp.Models
{
    public class UserCalendar
    {
        public int UserCalendarId { get; set; }

        // Foreign Key to link with ApplicationUser
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int WorkoutId { get; set; }
        public Workout Workout { get; set; }

        public DateTime ScheduledDateTime { get; set; }

        // Additional properties you may want to include
        public bool IsCompleted { get; set; } = false;
        public string Notes { get; set; }
    }
}
