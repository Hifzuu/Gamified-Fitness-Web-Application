using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectWebApp.Models; // Make sure to include the namespace where ApplicationUser is defined

namespace ProjectWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> // Provide ApplicationUser as the generic argument
    {
        public DbSet<Workout> Workouts { get; set; }
        public DbSet<UserFavouriteWorkout> UserFavouriteWorkouts { get; set; }
        public DbSet<WeightEntry> WeightEntries { get; set; }
        public DbSet<UserCalendar> UserCalendars { get; set; }
        public DbSet<LoginStreak> LoginStreaks { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<UserChallenge> UserChallenges { get; set; }
        public DbSet<Raffle> Raffles { get; set; }
        public DbSet<UserRaffleEntry> UserRaffleEntries { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure many-to-many relationship for user favourited workouts
            modelBuilder.Entity<UserFavouriteWorkout>()
                .HasKey(ufw => new { ufw.UserId, ufw.WorkoutId });

            modelBuilder.Entity<UserFavouriteWorkout>()
                .HasOne(ufw => ufw.User)
                .WithMany(u => u.FavouriteWorkouts)
                .HasForeignKey(ufw => ufw.UserId);

            modelBuilder.Entity<UserFavouriteWorkout>()
                .HasOne(ufw => ufw.Workout)
                .WithMany(w => w.UsersFavourited)
                .HasForeignKey(ufw => ufw.WorkoutId);

            //weight entries
            modelBuilder.Entity<WeightEntry>()
           .HasKey(we => new { we.UserId, we.Date });

            modelBuilder.Entity<WeightEntry>()
                .HasOne(we => we.User)
                .WithMany(u => u.WeightEntries)
                .HasForeignKey(we => we.UserId)
                .IsRequired();

            // Configure one-to-many relationship for user calendar entries
            modelBuilder.Entity<UserCalendar>()
               .HasKey(uc => new { uc.UserCalendarId });

            modelBuilder.Entity<UserCalendar>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserCalendars)
                .HasForeignKey(uc => uc.UserId)
                .IsRequired();

            modelBuilder.Entity<UserCalendar>()
                .HasOne(uc => uc.Workout)
                .WithMany()
                .HasForeignKey(uc => uc.WorkoutId)
                .IsRequired();

            // Configure the relationship between ApplicationUser and Streak
            modelBuilder.Entity<LoginStreak>()
            .HasKey(ls => new { ls.UserId, ls.LastLoginTime });

            modelBuilder.Entity<LoginStreak>()
                .HasOne(ls => ls.User)
                .WithMany(u => u.LoginStreaks)
                .HasForeignKey(ls => ls.UserId)
                .IsRequired();

            //challenges
            modelBuilder.Entity<Challenge>()
                 .HasKey(c => c.ChallengeId);

            // Many-to-many relationship between Challenge and Workout
            modelBuilder.Entity<Challenge>()
                .HasMany(c => c.Workouts)
                .WithMany(w => w.Challenges)
                .UsingEntity(j => j.ToTable("ChallengeWorkout"));

            modelBuilder.Entity<Challenge>()
               .Property(c => c.ChallengeType)
               .HasConversion<string>();

            modelBuilder.Entity<Challenge>()
                .Property(c => c.MeasurementCriteria)
                .HasConversion<string>();

            modelBuilder.Entity<UserChallenge>()
                .HasKey(uc => uc.UserChallengeId);

            modelBuilder.Entity<UserChallenge>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserChallenges)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserChallenge>()
                .HasOne(uc => uc.Challenge)
                .WithMany()
                .HasForeignKey(uc => uc.ChallengeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-many relationship between UserChallenge and Workout
            modelBuilder.Entity<UserChallenge>()
                .HasMany(uc => uc.Workouts)
                .WithMany(w => w.UserChallenges)
                .UsingEntity(j => j.ToTable("UserChallengeWorkout"));

            // raffles
            modelBuilder.Entity<UserRaffleEntry>()
             .HasKey(re => re.RaffleEntryId);

            modelBuilder.Entity<UserRaffleEntry>()
                .HasOne(re => re.User)
                .WithMany(u => u.UserRaffleEntries)
                .HasForeignKey(re => re.UserId);

            modelBuilder.Entity<UserRaffleEntry>()
                .HasOne(re => re.Raffle)
                .WithMany(r => r.UserRaffleEntries)
                .HasForeignKey(re => re.RaffleId);
        }
    }
}