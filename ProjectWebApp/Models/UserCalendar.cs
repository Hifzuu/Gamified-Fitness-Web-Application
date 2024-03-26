namespace ProjectWebApp.Models
{
    public class UserCalendar
    {
        public int UserCalendarId { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int WorkoutId { get; set; }
        public Workout Workout { get; set; }

        public DateTime ScheduledDateTime { get; set; }

        public bool IsCompleted { get; set; } = false;
        public string Notes { get; set; }
    }
}
