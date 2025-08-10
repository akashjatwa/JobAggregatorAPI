using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using JobAggregator.Application.DTOs;
using JobAggregator.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobAggregator.Infrastructure.Scrapers
{
    public class RemoteOkScraper : IScraper
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RemoteOkScraper> _logger;
        private const string ApiUrl = "https://remoteok.com/api";

        public RemoteOkScraper(IHttpClientFactory httpClientFactory, ILogger<RemoteOkScraper> logger)
        {
            _httpClient = httpClientFactory.CreateClient("RemoteOk");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<JobDto>> SearchAsync(JobSearchQuery query, CancellationToken ct)
        {
            _logger.LogInformation("Searching RemoteOK for: {Query}", query.Query);
            
            try
            {
                // Set user agent to avoid being blocked
                if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                {
                    _httpClient.DefaultRequestHeaders.Add("User-Agent", "JobAggregator/1.0");
                }

                // RemoteOK API doesn't support direct search queries, so we fetch all jobs and filter client-side
                var response = await _httpClient.GetAsync(ApiUrl, ct);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync(ct);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                // The API returns an array with the first element being a warning message, so we skip it
                var jobItems = JsonSerializer.Deserialize<List<RemoteOkJobItem>>(content, options);
                
                if (jobItems == null || jobItems.Count == 0)
                {
                    _logger.LogWarning("No jobs returned from RemoteOK API");
                    return Array.Empty<JobDto>();
                }
                
                // Skip the first item which is usually a message from the API
                var actualJobs = jobItems.Skip(1).ToList();
                _logger.LogInformation("Retrieved {Count} jobs from RemoteOK", actualJobs.Count);
                
                // Filter jobs based on query
                var filteredJobs = actualJobs
                    .Where(job => IsRelevantToQuery(job, query))
                    .Take(10) // Limit to 10 jobs for performance
                    .Select(MapToJobDto)
                    .ToList();
                
                _logger.LogInformation("Found {Count} relevant jobs from RemoteOK", filteredJobs.Count);
                return filteredJobs;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("RemoteOK job search was canceled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching RemoteOK jobs");
                return Array.Empty<JobDto>();
            }
        }
        
        private bool IsRelevantToQuery(RemoteOkJobItem jobItem, JobSearchQuery query)
        {
            // Simple relevance check - see if the query terms appear in the title, description, or tags
            var searchTerms = query.Query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var searchableText = $"{jobItem.Position} {jobItem.Description} {string.Join(" ", jobItem.Tags ?? Array.Empty<string>())}".ToLowerInvariant();
            
            // Check if any search term is found in the searchable text
            foreach (var term in searchTerms)
            {
                if (searchableText.Contains(term.ToLowerInvariant()))
                {
                    return true;
                }
            }
            
            // Check location filter if specified
            if (!string.IsNullOrEmpty(query.Location) && 
                !string.Equals(jobItem.Location, query.Location, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            
            // Check remote filter if specified
            if (query.Remote.HasValue && query.Remote.Value && !jobItem.Location.Contains("Remote", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            
            // Check salary filter if specified
            if (query.MinSalary.HasValue && jobItem.SalaryMax.HasValue && jobItem.SalaryMax.Value < query.MinSalary.Value)
            {
                return false;
            }
            
            return true;
        }
        
        private JobDto MapToJobDto(RemoteOkJobItem jobItem)
        {
            // Extract salary information if available
            (decimal? salaryMin, decimal? salaryMax) = ExtractSalaryRange(jobItem.Salary);
            
            return new JobDto(
                Source: "RemoteOK",
                ExternalId: $"remoteok-{jobItem.Id}",
                Title: jobItem.Position ?? "Untitled Job",
                Company: jobItem.Company ?? "Unknown",
                Location: jobItem.Location ?? "Remote", // RemoteOK jobs are typically remote
                Url: jobItem.Url ?? $"https://remoteok.com/jobs/{jobItem.Slug}",
                Description: jobItem.Description ?? string.Empty,
                PostedAt: ParseDate(jobItem.Date),
                SalaryMin: salaryMin,
                SalaryMax: salaryMax
            );
        }
        
        private DateTime ParseDate(string? dateString)
        {
            if (string.IsNullOrEmpty(dateString))
            {
                return DateTime.UtcNow;
            }
            
            // RemoteOK uses UTC timestamps
            if (long.TryParse(dateString, out long timestamp))
            {
                return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            }
            
            // Fallback to current time if parsing fails
            return DateTime.UtcNow;
        }
        
        private (decimal? min, decimal? max) ExtractSalaryRange(string? salary)
        {
            if (string.IsNullOrEmpty(salary))
            {
                return (null, null);
            }
            
            try
            {
                // RemoteOK typically formats salary as "$50k - $70k" or similar
                var match = Regex.Match(salary, @"\$?(\d+)k?\s*-\s*\$?(\d+)k?");
                if (match.Success && match.Groups.Count >= 3)
                {
                    decimal min = decimal.Parse(match.Groups[1].Value);
                    decimal max = decimal.Parse(match.Groups[2].Value);
                    
                    // Convert to annual salary if in thousands
                    if (salary.Contains('k', StringComparison.OrdinalIgnoreCase))
                    {
                        min *= 1000;
                        max *= 1000;
                    }
                    
                    return (min, max);
                }
                
                // Single value format like "$60k"
                match = Regex.Match(salary, @"\$?(\d+)k?");
                if (match.Success)
                {
                    decimal value = decimal.Parse(match.Groups[1].Value);
                    
                    // Convert to annual salary if in thousands
                    if (salary.Contains('k', StringComparison.OrdinalIgnoreCase))
                    {
                        value *= 1000;
                    }
                    
                    return (value, value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse salary: {Salary}", salary);
            }
            
            return (null, null);
        }
        
        // Class to deserialize RemoteOK API responses
        private class RemoteOkJobItem
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }
            
            [JsonPropertyName("company")]
            public string? Company { get; set; }
            
            [JsonPropertyName("position")]
            public string? Position { get; set; }
            
            [JsonPropertyName("description")]
            public string? Description { get; set; }
            
            [JsonPropertyName("date")]
            public string? Date { get; set; }
            
            [JsonPropertyName("url")]
            public string? Url { get; set; }
            
            [JsonPropertyName("apply_url")]
            public string? ApplyUrl { get; set; }
            
            [JsonPropertyName("location")]
            public string? Location { get; set; }
            
            [JsonPropertyName("tags")]
            public string[]? Tags { get; set; }
            
            [JsonPropertyName("slug")]
            public string? Slug { get; set; }
            
            [JsonPropertyName("salary")]
            public string? Salary { get; set; }
            
            [JsonPropertyName("salary_min")]
            public decimal? SalaryMin { get; set; }
            
            [JsonPropertyName("salary_max")]
            public decimal? SalaryMax { get; set; }
        }
    }
}