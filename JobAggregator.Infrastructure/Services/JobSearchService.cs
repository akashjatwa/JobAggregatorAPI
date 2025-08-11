using JobAggregator.Application.DTOs;
using JobAggregator.Application.Interfaces;
using JobAggregator.Infrastructure.Scrapers;
using Microsoft.Extensions.Caching.Memory;

namespace JobAggregator.Infrastructure.Services;

public class JobSearchService : IJobService
{
    private readonly IEnumerable<IJobScraper> _scrapers;
    private readonly IMemoryCache _cache;

    public JobSearchService(IEnumerable<IJobScraper> scrapers, IMemoryCache cache)
    {
        _scrapers = scrapers;
        _cache = cache;
    }

    public async Task<IEnumerable<JobDto>> SearchAsync(string? query, CancellationToken cancellationToken)
    {
        var cacheKey = $"search:{query}";
        if (_cache.TryGetValue(cacheKey, out IEnumerable<JobDto>? cached))
        {
            return cached!;
        }

        var jobs = new List<JobDto>();
        foreach (var scraper in _scrapers)
        {
            var result = await scraper.SearchAsync(query, cancellationToken);
            if (result != null)
                jobs.AddRange(result);
        }

        var deduped = jobs
            .GroupBy(j => new { j.Source, j.ExternalId })
            .Select(g => g.First())
            .ToList();

        _cache.Set(cacheKey, deduped, TimeSpan.FromMinutes(5));
        return deduped;
    }
}
