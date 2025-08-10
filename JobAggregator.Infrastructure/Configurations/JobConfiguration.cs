using JobAggregator.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobAggregator.Infrastructure.Configurations
{
    public class JobConfiguration : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            builder.HasKey(j => j.Id);
            builder.Property(j => j.Source).IsRequired().HasMaxLength(100);
            builder.Property(j => j.ExternalId).IsRequired().HasMaxLength(100);
            builder.Property(j => j.Title).HasMaxLength(500);
            builder.Property(j => j.Company).HasMaxLength(255);
            builder.Property(j => j.Location).HasMaxLength(255);
            builder.Property(j => j.Url).HasMaxLength(2048);
            builder.HasIndex(j => new { j.Source, j.ExternalId }).IsUnique();
            builder.Property(j => j.CreatedAt).IsRequired();
        }
    }
}
