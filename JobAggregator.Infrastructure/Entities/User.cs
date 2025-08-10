using System;
using System.Collections.Generic;

namespace JobAggregator.Infrastructure.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public ICollection<UserJob>? UserJobs { get; set; }
    }
}
