using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

// Make sure to import your Domain Entities namespace

namespace BookingService.Infrastructure
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        // DbSets for your entities
        public DbSet<MeetingRoom> MeetingRooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<EmailOutboxMessage> EmailOutbox { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Example: Configure relationships, primary keys, data types, etc.
            // If you are using Fluent API for configuration (recommended for complex setups)
            // you can configure your entities here using modelBuilder.Entity<YourEntity>()

            // Example: Configure Booking entity's relationships
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.MeetingRoom)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.MeetingRoomId);
            
             modelBuilder.Entity<Booking>()
                 .HasOne(b => b.User)
                 .WithMany(g => g.Bookings)
                .HasForeignKey(b => b.UserId);

            // IMPORTANT: If you are using Entity Framework Core's built-in Identity for Users/Auth,
            // it will have its own OnModelCreating configurations that you need to merge or call correctly.
            // For custom entities, you can use modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            // if you define your entity configurations in separate classes.
        }

        // Optional: Override SaveChangesAsync to add common logic like timestamps
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Example: Add timestamps to entities that implement an interface like IAuditableEntity
            // var entries = ChangeTracker
            //     .Entries()
            //     .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
            //
            // foreach (var entityEntry in entries)
            // {
            //     if (entityEntry.Entity is IAuditableEntity auditableEntity)
            //     {
            //         if (entityEntry.State == EntityState.Added)
            //         {
            //             auditableEntity.CreatedAt = DateTime.UtcNow;
            //         }
            //         auditableEntity.UpdatedAt = DateTime.UtcNow;
            //     }
            // }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
