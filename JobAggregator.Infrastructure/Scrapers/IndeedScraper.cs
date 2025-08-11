using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JobAggregator.Application.DTOs;
using JobAggregator.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobAggregator.Infrastructure.Scrapers
{
    public class IndeedScraper : IScraper
    {
        private readonly ILogger<IndeedScraper> _logger;

        public IndeedScraper(ILogger<IndeedScraper> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<JobDto>> SearchAsync(JobSearchQuery query, CancellationToken ct)
        {
            _logger.LogInformation("Searching Indeed for: {Query} in {Location}", query.Query, query.Location);
            
            // Simulate network delay
            await Task.Delay(TimeSpan.FromSeconds(1), ct);
            
            // This is a mock implementation that returns sample data
            // In a real implementation, this would make HTTP requests to Indeed's API or scrape their website
            var results = new List<JobDto>();
            
            // Add some sample jobs
            for (int i = 1; i <= 5; i++)
            {
                results.Add(new JobDto(
                    Source: "Indeed",
                    ExternalId: $"indeed-{Guid.NewGuid()}",
                    Title: $"{query.Query} Developer {i}",
                    Company: $"Company {i}",
                    Location: query.Location ?? "Remote",
                    Url: $"https://indeed.com/job/{i}",
                    Description: $"This is a job description for a {query.Query} Developer position.",
                    PostedAt: DateTime.UtcNow.AddDays(-i),
                    SalaryMin: query.MinSalary ?? 50000 + (i * 10000),
                    SalaryMax: (query.MinSalary ?? 50000) + (i * 15000)
                ));
            }
            
            return results;
        }
    }
}