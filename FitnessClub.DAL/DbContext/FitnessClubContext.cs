using Microsoft.EntityFrameworkCore;
using FitnessClub.DAL.Entities;

namespace FitnessClub.DAL
{
    public class FitnessClubContext : DbContext
    {
        public FitnessClubContext(DbContextOptions<FitnessClubContext> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            ChangeTracker.AutoDetectChangesEnabled = true;
            ChangeTracker.LazyLoadingEnabled = true;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<ClassSchedule> ClassSchedules { get; set; }
        public DbSet<MembershipType> MembershipTypes { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MembershipType>()
                .Property(mt => mt.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

            modelBuilder.Entity<Membership>()
                .HasOne(m => m.User)
                .WithMany(u => u.Memberships)
                .HasForeignKey(m => m.UserId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.ClassSchedule)
                .WithMany(cs => cs.Bookings)
                .HasForeignKey(b => b.ClassScheduleId);

            modelBuilder.Entity<ClassSchedule>()
                .HasOne(cs => cs.Club)
                .WithMany(c => c.ClassSchedules)
                .HasForeignKey(cs => cs.ClubId);

            modelBuilder.Entity<Trainer>()
                .HasOne(t => t.Club)
                .WithMany(c => c.Trainers)
                .HasForeignKey(t => t.ClubId);
        }

        public override int SaveChanges() => base.SaveChanges();
    }
}