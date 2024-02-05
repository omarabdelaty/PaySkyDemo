using Microsoft.EntityFrameworkCore;

namespace PaySkyDemo.Models
{
    public class PaySkyDemoContext : DbContext
    {

        public PaySkyDemoContext(DbContextOptions<PaySkyDemoContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<Application> Applications { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Application>()
                .HasOne(a => a.Vacancy)
                .WithMany(v => v.Applications)
                .HasForeignKey(a => a.VacancyId)
                .OnDelete(DeleteBehavior.NoAction);

                modelBuilder.Entity<Vacancy>()
                .HasOne(v => v.Employer)
                .WithMany()
                .HasForeignKey(v => v.EmployerId)
                .OnDelete(DeleteBehavior.NoAction);


            base.OnModelCreating(modelBuilder);
        }
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }
        private void UpdateTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList();

            var currentTime = DateTime.UtcNow;

            foreach (var entry in entities)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = currentTime;
                }

                entry.Property("UpdatedAt").CurrentValue = currentTime;
            }
        }
    }

 
}
