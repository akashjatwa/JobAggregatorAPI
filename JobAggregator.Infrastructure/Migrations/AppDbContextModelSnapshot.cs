using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using JobAggregator.Infrastructure.Data;

#nullable disable

namespace JobAggregator.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            modelBuilder.Entity("JobAggregator.Infrastructure.Entities.Job", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("Company")
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                b.Property<string>("Description")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("ExternalId")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Location")
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                b.Property<DateTimeOffset?>("PostedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("RawJson")
                    .HasColumnType("nvarchar(max)");

                b.Property<decimal?>("SalaryMax")
                    .HasColumnType("decimal(18,2)");

                b.Property<decimal?>("SalaryMin")
                    .HasColumnType("decimal(18,2)");

                b.Property<string>("Source")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Title")
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.Property<string>("Url")
                    .HasMaxLength(2048)
                    .HasColumnType("nvarchar(2048)");

                b.HasKey("Id");

                b.HasIndex("Source", "ExternalId").IsUnique();

                b.ToTable("Jobs");
            });

            modelBuilder.Entity("JobAggregator.Infrastructure.Entities.User", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("Email")
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                b.Property<string>("PasswordHash")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.HasIndex("Email").IsUnique();

                b.ToTable("Users");
            });

            modelBuilder.Entity("JobAggregator.Infrastructure.Entities.UserJob", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTimeOffset?>("AppliedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<Guid>("JobId")
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTimeOffset?>("LikedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("Notes")
                    .HasMaxLength(2000)
                    .HasColumnType("nvarchar(2000)");

                b.Property<int>("Status")
                    .HasColumnType("int");

                b.Property<string>("Tags")
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                b.Property<DateTimeOffset>("UpdatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<Guid>("UserId")
                    .HasColumnType("uniqueidentifier");

                b.HasKey("Id");

                b.HasIndex("JobId");

                b.HasIndex("UserId");

                b.HasIndex("UserId", "Status", "UpdatedAt");

                b.ToTable("UserJobs");
            });

            modelBuilder.Entity("JobAggregator.Infrastructure.Entities.UserJob", b =>
            {
                b.HasOne("JobAggregator.Infrastructure.Entities.Job", "Job")
                    .WithMany("UserJobs")
                    .HasForeignKey("JobId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("JobAggregator.Infrastructure.Entities.User", "User")
                    .WithMany("UserJobs")
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Job");

                b.Navigation("User");
            });

            modelBuilder.Entity("JobAggregator.Infrastructure.Entities.Job", b =>
            {
                b.Navigation("UserJobs");
            });

            modelBuilder.Entity("JobAggregator.Infrastructure.Entities.User", b =>
            {
                b.Navigation("UserJobs");
            });
        }
    }
}
