using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JobAggregator.Application.DTOs;
using JobAggregator.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobAggregator.Infrastructure.Scrapers
{
    public class LinkedInScraper : IScraper
    {
        private readonly ILogger<LinkedInScraper> _logger;

        public LinkedInScraper(ILogger<LinkedInScraper> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<JobDto>> SearchAsync(JobSearchQuery query, CancellationToken ct)
        {
            _logger.LogInformation("Searching LinkedIn for: {Query} in {Location}", query.Query, query.Location);
            
            // Simulate network delay (slightly longer than Indeed to demonstrate different response times)
            await Task.Delay(TimeSpan.FromSeconds(1.5), ct);
            
            // This is a mock implementation that returns sample data
            // In a real implementation, this would make HTTP requests to LinkedIn's API or scrape their website
            var results = new List<JobDto>();
            
            // Add some sample jobs
            for (int i = 1; i <= 7; i++)
            {
                results.Add(new JobDto(
                    Source: "LinkedIn",
                    ExternalId: $"linkedin-{Guid.NewGuid()}",
                    Title: $"Senior {query.Query} Engineer {i}",
                    Company: $"Enterprise {i}",
                    Location: query.Location ?? (query.Remote == true ? "Remote" : "Various Locations"),
                    Url: $"https://linkedin.com/jobs/{i}",
                    Description: $"We are looking for an experienced {query.Query} Engineer to join our team.",
                    PostedAt: DateTime.UtcNow.AddDays(-(i % 5)),
                    SalaryMin: query.MinSalary ?? 70000 + (i * 8000),
                    SalaryMax: (query.MinSalary ?? 70000) + (i * 12000)
                ));
            }
            
            return results;
        }
    }
}