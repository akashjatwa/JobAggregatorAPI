using JobAggregator.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobAggregator.Infrastructure.Configurations
{
    public class UserJobConfiguration : IEntityTypeConfiguration<UserJob>
    {
        public void Configure(EntityTypeBuilder<UserJob> builder)
        {
            builder.HasKey(uj => uj.Id);
            builder.Property(uj => uj.Status).IsRequired();
            builder.Property(uj => uj.Tags).HasMaxLength(255);
            builder.Property(uj => uj.Notes).HasMaxLength(2000);
            builder.Property(uj => uj.UpdatedAt).IsRequired();
            builder.HasIndex(uj => new { uj.UserId, uj.Status, uj.UpdatedAt });

            builder.HasOne(uj => uj.User)
                .WithMany(u => u.UserJobs)
                .HasForeignKey(uj => uj.UserId);

            builder.HasOne(uj => uj.Job)
                .WithMany(j => j.UserJobs)
                .HasForeignKey(uj => uj.JobId);
        }
    }
}
