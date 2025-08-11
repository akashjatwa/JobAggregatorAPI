using JobAggregator.Application.DTOs;

namespace JobAggregator.Application.Interfaces
{
    public interface IJobService
    {
        Task<IEnumerable<JobDto>> SearchAsync(string? query, CancellationToken cancellationToken);
    }
}
