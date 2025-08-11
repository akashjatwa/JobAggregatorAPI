using JobAggregator.Application.DTOs;
using JobAggregator.Infrastructure.Services;
using JobAggregator.Infrastructure.Scrapers;
using Microsoft.Extensions.Caching.Memory;

namespace JobAggregator.Tests.Services;

public class JobSearchServiceTests
{
    private class FakeScraper : IJobScraper
    {
        private readonly IEnumerable<JobDto> _jobs;
        public int CallCount { get; private set; }

        public FakeScraper(IEnumerable<JobDto> jobs) => _jobs = jobs;

        public Task<IEnumerable<JobDto>> SearchAsync(string? query, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(_jobs);
        }
    }

    [Fact]
    public async Task SearchAsync_Deduplicates_And_Caches()
    {
        var job1 = new JobDto("source", "1", "Title1", "Co", "Loc", null, null, null, null, null);
        var job2 = new JobDto("source", "1", "Title1 Dup", "Co", "Loc", null, null, null, null, null);
        var job3 = new JobDto("source", "2", "Title2", "Co", "Loc", null, null, null, null, null);

        var scraper1 = new FakeScraper(new[] { job1, job3 });
        var scraper2 = new FakeScraper(new[] { job2 });

        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new JobSearchService(new[] { scraper1, scraper2 }, cache);

        var first = await service.SearchAsync("test", CancellationToken.None);
        Assert.Equal(2, first.Count());

        var second = await service.SearchAsync("test", CancellationToken.None);
        Assert.Equal(2, second.Count());

        Assert.Equal(1, scraper1.CallCount);
        Assert.Equal(1, scraper2.CallCount);
    }
}
