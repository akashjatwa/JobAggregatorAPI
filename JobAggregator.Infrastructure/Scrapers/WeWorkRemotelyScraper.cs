using JobAggregator.Application.DTOs;
using HtmlAgilityPack;

namespace JobAggregator.Infrastructure.Scrapers;

public class WeWorkRemotelyScraper
{
    public IEnumerable<JobDto> Parse(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return Enumerable.Empty<JobDto>();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var jobs = new List<JobDto>();
        var nodes = doc.DocumentNode.SelectNodes("//li[@class='job']");
        if (nodes == null)
            return jobs;

        foreach (var li in nodes)
        {
            var id = li.GetAttributeValue("data-id", string.Empty);
            if (string.IsNullOrEmpty(id))
                continue;

            var titleNode = li.SelectSingleNode(".//a[@class='title']");
            var companyNode = li.SelectSingleNode(".//span[@class='company']");
            var locationNode = li.SelectSingleNode(".//span[@class='location']");

            var title = titleNode?.InnerText?.Trim();
            var company = companyNode?.InnerText?.Trim();
            var location = locationNode?.InnerText?.Trim();
            var url = titleNode?.GetAttributeValue("href", null);

            jobs.Add(new JobDto(
                Source: "weworkremotely",
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
