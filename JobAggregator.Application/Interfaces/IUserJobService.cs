using JobAggregator.Application.DTOs;

namespace JobAggregator.Application.Interfaces
{
    public interface IUserJobService
    {
        Task<UserJobDto> SaveAsync(JobDto dto);
        Task<IEnumerable<UserJobDto>> ListAsync(
            string? status,
            string? company,
            string? location,
            string? tag,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? sort,
            int page,
            int pageSize);
        Task ApplyAsync(Guid id);
        Task DeleteAsync(Guid id);
    }
}
