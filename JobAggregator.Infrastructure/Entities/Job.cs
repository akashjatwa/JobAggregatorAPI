using System;
using System.Collections.Generic;

namespace JobAggregator.Infrastructure.Entities
{
    public class Job
    {
        public Guid Id { get; set; }
        public string Source { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Company { get; set; }
        public string? Location { get; set; }
        public string? Url { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset? PostedAt { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string? RawJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<UserJob>? UserJobs { get; set; }
    }
}
