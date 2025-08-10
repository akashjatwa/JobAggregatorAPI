using JobAggregator.Application.DTOs;

namespace JobAggregator.Application.Interfaces
{
    public interface IJobSearchService
    {
        Task<IReadOnlyList<JobDto>> SearchAsync(JobSearchQuery query, CancellationToken ct);
    }
}