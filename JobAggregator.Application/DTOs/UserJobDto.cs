namespace JobAggregator.Application.DTOs
{
    public record UserJobDto(
        Guid Id,
        string Source,
        string ExternalId,
        string? Title,
        string? Company,
        string? Location,
        string? Url,
        string? Tags,
        string Status,
        DateTimeOffset? LikedAt,
        DateTimeOffset? AppliedAt,
        DateTimeOffset UpdatedAt);
}
