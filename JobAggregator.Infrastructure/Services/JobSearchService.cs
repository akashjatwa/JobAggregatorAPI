using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JobAggregator.Application.DTOs;
using JobAggregator.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Polly.CircuitBreaker;

namespace JobAggregator.Infrastructure.Services
{
    public class JobSearchService : IJobSearchService
    {
        private readonly IEnumerable<IScraper> _scrapers;
        private readonly ICacheService _cacheService;
        private readonly ILogger<JobSearchService> _logger;
        private readonly SemaphoreSlim _semaphore;
        private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(15); // 15 minutes TTL
        private readonly int _maxConcurrentScrapers = 2; // Maximum concurrent scrapers

        public JobSearchService(
            IEnumerable<IScraper> scrapers,
            ICacheService cacheService,
            ILogger<JobSearchService> logger)
        {
            _scrapers = scrapers ?? throw new ArgumentNullException(nameof(scrapers));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _semaphore = new SemaphoreSlim(_maxConcurrentScrapers);
        }

        public async Task<IReadOnlyList<JobDto>> SearchAsync(JobSearchQuery query, CancellationToken ct)
        {
            // Generate cache key based on query parameters
            string cacheKey = GenerateCacheKey(query);

            // Try to get results from cache first
            var cachedResults = await _cacheService.GetAsync<IReadOnlyList<JobDto>>(cacheKey, ct);
            if (cachedResults != null)
            {
                _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
                return cachedResults;
            }

            _logger.LogInformation("Cache miss for key: {CacheKey}, fetching from scrapers", cacheKey);

            // Create tasks for each scraper with resilience policies
            var scraperTasks = _scrapers.Select(scraper => RunScraperWithResilienceAsync(scraper, query, ct)).ToList();

            // Wait for all scraper tasks to complete
            var results = await Task.WhenAll(scraperTasks);

            // Merge and deduplicate results
            var mergedResults = MergeAndDeduplicateResults(results.SelectMany(r => r).ToList());

            // Cache the results
            await _cacheService.SetAsync(cacheKey, mergedResults, _cacheTtl, ct);

            return mergedResults;
        }

        private async Task<IReadOnlyList<JobDto>> RunScraperWithResilienceAsync(IScraper scraper, JobSearchQuery query, CancellationToken ct)
        {
            try
            {
                // Acquire semaphore to limit concurrent scrapers
                await _semaphore.WaitAsync(ct);

                // Define resilience policies
                var timeoutPolicy = Policy.TimeoutAsync(8, TimeoutStrategy.Pessimistic); // 8 second timeout

                var retryPolicy = Policy
                    .Handle<Exception>(ex => !(ex is OperationCanceledException))
                    .WaitAndRetryAsync(
                        2, // Retry twice
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // Exponential backoff
                            + TimeSpan.FromMilliseconds(new Random().Next(0, 1000)), // Add jitter
                        onRetry: (exception, timeSpan, retryCount, context) =>
                        {
                            _logger.LogWarning(exception, "Retry {RetryCount} for scraper {ScraperType} after {RetryDelay}ms",
                                retryCount, scraper.GetType().Name, timeSpan.TotalMilliseconds);
                        });

                var circuitBreakerPolicy = Policy
                    .Handle<Exception>(ex => !(ex is OperationCanceledException))
                    .CircuitBreakerAsync(
                        exceptionsAllowedBeforeBreaking: 5,
                        durationOfBreak: TimeSpan.FromMinutes(1),
                        onBreak: (ex, breakDelay) =>
                        {
                            _logger.LogError(ex, "Circuit breaker opened for scraper {ScraperType} for {BreakDelay}ms",
                                scraper.GetType().Name, breakDelay.TotalMilliseconds);
                        },
                        onReset: () =>
                        {
                            _logger.LogInformation("Circuit breaker reset for scraper {ScraperType}",
                                scraper.GetType().Name);
                        });

                // Combine policies
                var resiliencePolicy = Policy.WrapAsync(retryPolicy, timeoutPolicy, circuitBreakerPolicy);

                // Execute with resilience policies
                return await resiliencePolicy.ExecuteAsync(async () =>
                {
                    _logger.LogInformation("Running scraper {ScraperType}", scraper.GetType().Name);
                    return await scraper.SearchAsync(query, ct);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running scraper {ScraperType}", scraper.GetType().Name);
                return Array.Empty<JobDto>();
            }
            finally
            {
                // Release semaphore
                _semaphore.Release();
            }
        }

        private IReadOnlyList<JobDto> MergeAndDeduplicateResults(List<JobDto> allJobs)
        {
            // Use a HashSet to track unique job identifiers (Source + ExternalId)
            var uniqueJobKeys = new HashSet<string>();
            var uniqueJobs = new List<JobDto>();

            foreach (var job in allJobs)
            {
                // Create a unique key for each job based on Source and ExternalId
                string uniqueKey = $"{job.Source}:{job.ExternalId}";

                // Only add the job if it hasn't been seen before
                if (uniqueJobKeys.Add(uniqueKey))
                {
                    uniqueJobs.Add(job);
                }
            }

            _logger.LogInformation("Merged and deduplicated {TotalJobs} jobs to {UniqueJobs} unique jobs",
                allJobs.Count, uniqueJobs.Count);

            return uniqueJobs;
        }

        private string GenerateCacheKey(JobSearchQuery query)
        {
            // Create a string with all query parameters
            string queryString = $"{query.Query}:{query.Location}:{query.Remote}:{query.MinSalary}:{query.Page}:{query.PageSize}";

            // Compute SHA256 hash of the query string
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(queryString));
                string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                
                // Return the final cache key
                return $"jobs:{hashString}";
            }
        }
    }
}