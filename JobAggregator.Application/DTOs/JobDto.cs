namespace JobAggregator.Application.DTOs
{
    public record JobDto(
        string Source,
        string ExternalId,
        string? Title,
        string? Company,
        string? Location,
        string? Url,
        string? Description,
        DateTimeOffset? PostedAt,
        decimal? SalaryMin,
        decimal? SalaryMax,
        string? RawJson);
}
