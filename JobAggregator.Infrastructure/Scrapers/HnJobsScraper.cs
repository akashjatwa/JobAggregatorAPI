using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using JobAggregator.Application.DTOs;
using JobAggregator.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;

namespace JobAggregator.Infrastructure.Scrapers
{
    public class HnJobsScraper : IScraper
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HnJobsScraper> _logger;
        private const string ApiUrl = "https://hacker-news.firebaseio.com/v0/jobstories.json";

        public HnJobsScraper(IHttpClientFactory httpClientFactory, ILogger<HnJobsScraper> logger)
        {
            _httpClient = httpClientFactory.CreateClient("HnJobs");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<JobDto>> SearchAsync(JobSearchQuery query, CancellationToken ct)
        {
            _logger.LogInformation("Searching Hacker News for: {Query}", query.Query);
            
            try
            {
                // Fetch job IDs from Hacker News API
                var jobIds = await _httpClient.GetFromJsonAsync<int[]>(ApiUrl, ct);
                
                if (jobIds == null || jobIds.Length == 0)
                {
                    _logger.LogWarning("No job IDs returned from Hacker News API");
                    return Array.Empty<JobDto>();
                }
                
                _logger.LogInformation("Retrieved {Count} job IDs from Hacker News", jobIds.Length);
                
                // For demo purposes, limit to first 10 jobs
                var limitedJobIds = jobIds.Length > 10 ? jobIds[..10] : jobIds;
                var jobs = new List<JobDto>();
                
                // Fetch details for each job
                foreach (var jobId in limitedJobIds)
                {
                    // Check for cancellation between requests
                    ct.ThrowIfCancellationRequested();
                    
                    try
                    {
                        var jobUrl = $"https://hacker-news.firebaseio.com/v0/item/{jobId}.json";
                        var jobDetails = await _httpClient.GetFromJsonAsync<HnJobItem>(jobUrl, ct);
                        
                        if (jobDetails != null && IsRelevantToQuery(jobDetails, query))
                        {
                            jobs.Add(MapToJobDto(jobDetails));
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Error fetching job details for ID {JobId}", jobId);
                    }
                }
                
                _logger.LogInformation("Found {Count} relevant jobs from Hacker News", jobs.Count);
                return jobs;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Hacker News job search was canceled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching Hacker News jobs");
                return Array.Empty<JobDto>();
            }
        }
        
        private bool IsRelevantToQuery(HnJobItem jobItem, JobSearchQuery query)
        {
            // Simple relevance check - see if the query terms appear in the title or text
            var searchTerms = query.Query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var titleAndText = $"{jobItem.Title} {jobItem.Text}".ToLowerInvariant();
            
            foreach (var term in searchTerms)
            {
                if (titleAndText.Contains(term.ToLowerInvariant()))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private JobDto MapToJobDto(HnJobItem jobItem)
        {
            // Extract location and remote status from title if possible
            string location = ExtractLocation(jobItem.Title) ?? "Unknown";
            bool isRemote = IsRemoteJob(jobItem.Title, jobItem.Text);
            
            // Extract salary information if available
            (decimal? salaryMin, decimal? salaryMax) = ExtractSalaryRange(jobItem.Title, jobItem.Text);
            
            return new JobDto(
                Source: "HackerNews",
                ExternalId: $"hn-{jobItem.Id}",
                Title: jobItem.Title ?? "Untitled Job",
                Company: ExtractCompany(jobItem.Title) ?? "Unknown",
                Location: isRemote ? "Remote" : location,
                Url: $"https://news.ycombinator.com/item?id={jobItem.Id}",
                Description: jobItem.Text ?? string.Empty,
                PostedAt: DateTimeOffset.FromUnixTimeSeconds(jobItem.Time).DateTime,
                SalaryMin: salaryMin,
                SalaryMax: salaryMax
            );
        }
        
        private string? ExtractCompany(string? title)
        {
            if (string.IsNullOrEmpty(title)) return null;
            
            // Common patterns in HN job titles: "Company X is hiring...", "Join Company X..."
            if (title.Contains(" is hiring", StringComparison.OrdinalIgnoreCase))
            {
                return title.Split(" is hiring", StringSplitOptions.None)[0].Trim();
            }
            
            return null;
        }
        
        private string? ExtractLocation(string? title)
        {
            if (string.IsNullOrEmpty(title)) return null;
            
            // Look for location patterns like "in San Francisco", "(New York)", etc.
            // This is a simplified implementation
            return null;
        }
        
        private bool IsRemoteJob(string? title, string? text)
        {
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(text)) return false;
            
            var combinedText = $"{title} {text}".ToLowerInvariant();
            return combinedText.Contains("remote") || combinedText.Contains("work from home");
        }
        
        private (decimal? min, decimal? max) ExtractSalaryRange(string? title, string? text)
        {
            // This would be a more complex implementation to extract salary information
            // For demo purposes, we'll return null values
            return (null, null);
        }
        
        // Class to deserialize Hacker News API responses
        private class HnJobItem
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? Text { get; set; }
            public long Time { get; set; }
            public string? Url { get; set; }
            public string? By { get; set; }
            public string? Type { get; set; }
        }
    }
}