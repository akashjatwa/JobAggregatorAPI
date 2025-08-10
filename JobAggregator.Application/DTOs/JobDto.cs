namespace JobAggregator.Application.DTOs
{
    public record JobDto(
        string Source,
        string ExternalId,
        string Title,
        string Company,
        string Location,
        string Url,
        string Description,
        DateTime PostedAt,
        decimal? SalaryMin,
        decimal? SalaryMax);
}
