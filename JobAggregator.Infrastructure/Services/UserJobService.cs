using JobAggregator.Application.DTOs;
using JobAggregator.Application.Interfaces;
using JobAggregator.Infrastructure.Data;
using JobAggregator.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobAggregator.Infrastructure.Services
{
    public class UserJobService : IUserJobService
    {
        private readonly AppDbContext _db;
        private static readonly Guid DemoUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        // TODO: Replace demo user with JWT-authenticated user id

        public UserJobService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<UserJobDto> SaveAsync(JobDto dto)
        {
            var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Source == dto.Source && j.ExternalId == dto.ExternalId);
            if (job == null)
            {
                job = new Job
                {
                    Id = Guid.NewGuid(),
                    Source = dto.Source,
                    ExternalId = dto.ExternalId,
                    Title = dto.Title,
                    Company = dto.Company,
                    Location = dto.Location,
                    Url = dto.Url,
                    Description = dto.Description,
                    PostedAt = dto.PostedAt,
                    SalaryMin = dto.SalaryMin,
                    SalaryMax = dto.SalaryMax,
                    RawJson = dto.RawJson,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Jobs.Add(job);
            }

            var existingUserJob = await _db.UserJobs.FirstOrDefaultAsync(uj => uj.JobId == job.Id && uj.UserId == DemoUserId);
            if (existingUserJob != null)
            {
                return Map(existingUserJob, job);
            }

            var userJob = new UserJob
            {
                Id = Guid.NewGuid(),
                UserId = DemoUserId,
                JobId = job.Id,
                Status = UserJobStatus.Saved,
                LikedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _db.UserJobs.Add(userJob);
            await _db.SaveChangesAsync();

            return Map(userJob, job);
        }

        public async Task<IEnumerable<UserJobDto>> ListAsync(string? status, string? company, string? location, string? tag, DateTimeOffset? from, DateTimeOffset? to, string? sort, int page, int pageSize)
        {
            var query = _db.UserJobs.Include(uj => uj.Job).Where(uj => uj.UserId == DemoUserId);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<UserJobStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(uj => uj.Status == parsedStatus);
            }

            if (!string.IsNullOrEmpty(company))
            {
                query = query.Where(uj => uj.Job!.Company != null && uj.Job.Company.Contains(company));
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(uj => uj.Job!.Location != null && uj.Job.Location.Contains(location));
            }

            if (!string.IsNullOrEmpty(tag))
            {
                query = query.Where(uj => uj.Tags != null && uj.Tags.Contains(tag));
            }

            if (from.HasValue)
            {
                query = query.Where(uj => uj.UpdatedAt >= from);
            }

            if (to.HasValue)
            {
                query = query.Where(uj => uj.UpdatedAt <= to);
            }

            query = sort switch
            {
                "updatedAt" => query.OrderByDescending(uj => uj.UpdatedAt),
                _ => query.OrderByDescending(uj => uj.LikedAt)
            };

            var skip = (page - 1) * pageSize;
            var items = await query.Skip(skip).Take(pageSize).ToListAsync();
            return items.Select(uj => Map(uj, uj.Job!));
        }

        public async Task ApplyAsync(Guid id)
        {
            var userJob = await _db.UserJobs.FirstOrDefaultAsync(uj => uj.Id == id && uj.UserId == DemoUserId);
            if (userJob == null)
            {
                return;
            }

            userJob.Status = UserJobStatus.Applied;
            userJob.AppliedAt = DateTimeOffset.UtcNow;
            userJob.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var userJob = await _db.UserJobs.FirstOrDefaultAsync(uj => uj.Id == id && uj.UserId == DemoUserId);
            if (userJob == null)
            {
                return;
            }

            _db.UserJobs.Remove(userJob);
            await _db.SaveChangesAsync();
        }

        private static UserJobDto Map(UserJob userJob, Job job) =>
            new(userJob.Id, job.Source, job.ExternalId, job.Title, job.Company, job.Location, job.Url, userJob.Tags, userJob.Status.ToString(), userJob.LikedAt, userJob.AppliedAt, userJob.UpdatedAt);
    }
}
