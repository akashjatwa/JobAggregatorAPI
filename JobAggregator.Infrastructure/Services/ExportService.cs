using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using JobAggregator.Application.Interfaces;
using JobAggregator.Infrastructure.Data;
using JobAggregator.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobAggregator.Infrastructure.Services
{
    public class ExportService : IExportService
    {
        private readonly AppDbContext _db;

        public ExportService(AppDbContext db)
        {
            _db = db;
        }

        public async IAsyncEnumerable<string> StreamAppliedJobsCsvAsync(Guid userId, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return "Title,Company,Location,Source,Url,SavedOn,AppliedOn,Notes,Tags";

            var query = _db.UserJobs
                .AsNoTracking()
                .Include(uj => uj.Job)
                .Where(uj => uj.UserId == userId && uj.Status == UserJobStatus.Applied)
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken);

            await foreach (var userJob in query)
            {
                var job = userJob.Job!;
                var line = string.Join(",",
                    Escape(job.Title),
                    Escape(job.Company),
                    Escape(job.Location),
                    Escape(job.Source),
                    Escape(job.Url),
                    Escape(userJob.LikedAt?.ToString("o")),
                    Escape(userJob.AppliedAt?.ToString("o")),
                    Escape(userJob.Notes),
                    Escape(userJob.Tags));

                yield return line;
            }
        }

        private static string Escape(string? input)
        {
            var value = input ?? string.Empty;
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}
