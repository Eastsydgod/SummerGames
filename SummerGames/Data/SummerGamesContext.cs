using Microsoft.EntityFrameworkCore;
using SummerGames.Models;

namespace SummerGames.Data
{
    public class SummerGamesContext : DbContext
    {
        public SummerGamesContext(DbContextOptions<SummerGamesContext> options)
            : base(options)
        {
        }

        //To give access to IHttpContextAccessor for Audit Data with IAuditable
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Property to hold the UserName value
        public string UserName
        {
            get; private set;
        }

        public SummerGamesContext(DbContextOptions<SummerGamesContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            if (_httpContextAccessor.HttpContext != null)
            {
                //We have a HttpContext, but there might not be anyone Authenticated
                UserName = _httpContextAccessor.HttpContext?.User.Identity.Name;
                UserName ??= "Unknown";
            }
            else
            {
                //No HttpContext so seeding data
                UserName = "Seed Data";
            }
        }



        public DbSet<Athlete> Athletes { get; set; }
        public DbSet<Sport> Sports { get; set; }
        public DbSet<Contingent> Contingents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Added a unique index to the AthleteCode Number
            modelBuilder.Entity<Athlete>()
                .HasIndex(p => p.AthleteCode)
                .IsUnique();

            // Added unique constraints on Code property in Sport
            modelBuilder.Entity<Sport>()
                .HasIndex(p => p.Code)
                .IsUnique();

            // Added unique constraints on Code property in Contingent
            modelBuilder.Entity<Contingent>()
                .HasIndex(p => p.Code)
                .IsUnique();

            // Restricted Cascade Delete for Sport to Athlete relationship
            modelBuilder.Entity<Sport>()
                .HasMany(s => s.Athletes)
                .WithOne(a => a.Sport)
                .HasForeignKey(a => a.SportID)
                .OnDelete(DeleteBehavior.Restrict);

            // Restricted Cascade Delete for Contingent to Athlete relationship
            modelBuilder.Entity<Contingent>()
                .HasMany(c => c.Athletes)
                .WithOne(a => a.Contingent)
                .HasForeignKey(a => a.ContingentID)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditable trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;

                        case EntityState.Added:
                            trackable.CreatedOn = now;
                            trackable.CreatedBy = UserName;
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;
                    }
                }
            }
        }

    }
}
