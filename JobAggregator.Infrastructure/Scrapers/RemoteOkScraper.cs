using System.Text.Json;
using JobAggregator.Application.DTOs;

namespace JobAggregator.Infrastructure.Scrapers;

public class RemoteOkScraper
{
    public IEnumerable<JobDto> Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Enumerable.Empty<JobDto>();

        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            return Enumerable.Empty<JobDto>();

        var jobs = new List<JobDto>();
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (!element.TryGetProperty("id", out var idProp))
                continue;
            var id = idProp.GetString();
            if (string.IsNullOrEmpty(id))
                continue;

            var title = element.TryGetProperty("position", out var t) ? t.GetString() : null;
            var company = element.TryGetProperty("company", out var c) ? c.GetString() : null;
            var location = element.TryGetProperty("location", out var l) ? l.GetString() : null;
            var url = element.TryGetProperty("url", out var u) ? u.GetString() : null;

            jobs.Add(new JobDto(
                Source: "remoteok",
                ExternalId: id,
                Title: title,
                Company: company,
                Location: location,
                Url: url,
                Description: null,
                PostedAt: null,
                SalaryMin: null,
                SalaryMax: null,
                RawJson: element.GetRawText()));
        }

        return jobs;
    }
}
