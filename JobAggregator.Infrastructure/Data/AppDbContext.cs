using JobAggregator.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using JobAggregator.Infrastructure;


namespace JobAggregator.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets will be added here in the future
        public DbSet<User> Users => Set<User>();
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<UserJob> UserJobs => Set<UserJob>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
