using JobAggregator.Application.DTOs;

namespace JobAggregator.Application.Interfaces
{
    public interface IScraper
    {
        Task<IReadOnlyList<JobDto>> SearchAsync(JobSearchQuery query, CancellationToken ct);
    }
}