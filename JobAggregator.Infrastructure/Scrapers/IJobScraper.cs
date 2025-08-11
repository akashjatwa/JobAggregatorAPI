using JobAggregator.Application.DTOs;

namespace JobAggregator.Infrastructure.Scrapers;

public interface IJobScraper
{
    Task<IEnumerable<JobDto>> SearchAsync(string? query, CancellationToken cancellationToken);
}
