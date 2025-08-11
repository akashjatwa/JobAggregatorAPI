using JobAggregator.Application.DTOs;
using HtmlAgilityPack;

namespace JobAggregator.Infrastructure.Scrapers;

public class HtmlBoardScraper : IJobScraper
{
    public Task<IEnumerable<JobDto>> SearchAsync(string? query, CancellationToken cancellationToken)
        => Task.FromResult<IEnumerable<JobDto>>(Enumerable.Empty<JobDto>());

    public IEnumerable<JobDto> Parse(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return Enumerable.Empty<JobDto>();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var jobs = new List<JobDto>();
        var nodes = doc.DocumentNode.SelectNodes("//div[@class='job']");
        if (nodes == null)
            return jobs;

        foreach (var div in nodes)
        {
            var id = div.GetAttributeValue("data-id", string.Empty);
            if (string.IsNullOrEmpty(id))
                continue;

            var titleNode = div.SelectSingleNode(".//a[@class='title']");
            var companyNode = div.SelectSingleNode(".//span[@class='company']");
            var locationNode = div.SelectSingleNode(".//span[@class='location']");

            var title = titleNode?.InnerText?.Trim();
            var company = companyNode?.InnerText?.Trim();
            var location = locationNode?.InnerText?.Trim();
            var url = titleNode?.GetAttributeValue("href", null);

            jobs.Add(new JobDto(
                Source: "htmlboard",
                ExternalId: id,
                Title: title,
                Company: company,
                Location: location,
                Url: url,
                Description: null,
                PostedAt: null,
                SalaryMin: null,
                SalaryMax: null,
                RawJson: null));
        }

        return jobs;
    }
}

