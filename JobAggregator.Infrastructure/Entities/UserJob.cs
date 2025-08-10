using System;

namespace JobAggregator.Infrastructure.Entities
{
    public class UserJob
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid JobId { get; set; }
        public UserJobStatus Status { get; set; }
        public string? Tags { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset? LikedAt { get; set; }
        public DateTimeOffset? AppliedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public User? User { get; set; }
        public Job? Job { get; set; }
    }
}
