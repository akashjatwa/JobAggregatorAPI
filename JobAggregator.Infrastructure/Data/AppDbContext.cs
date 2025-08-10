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
    }
}
