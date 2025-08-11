namespace JobAggregator.Application.DTOs
{
    public record JobSearchQuery
    {
        public string Query { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public bool? Remote { get; init; }
        public decimal? MinSalary { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}