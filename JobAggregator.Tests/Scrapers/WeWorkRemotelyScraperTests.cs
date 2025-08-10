using System.IO;
using JobAggregator.Infrastructure.Scrapers;
using Xunit;

namespace JobAggregator.Tests.Scrapers;

public class WeWorkRemotelyScraperTests
{
    [Fact]
    public void Parse_ReturnsJobsFromHtml()
    {
        var html = File.ReadAllText(Path.Combine("Fixtures", "weworkremotely_sample.html"));
        var scraper = new WeWorkRemotelyScraper();

        var jobs = scraper.Parse(html).ToList();

        Assert.Single(jobs);
        var job = jobs[0];
        Assert.Equal("weworkremotely", job.Source);
        Assert.Equal("abcde", job.ExternalId);
        Assert.Equal("Backend Developer", job.Title);
        Assert.Equal("Beta Inc", job.Company);
        Assert.Equal("Anywhere", job.Location);
        Assert.Equal("https://example.com/job2", job.Url);
    }

    [Fact]
    public void Parse_WithEmptyHtml_ReturnsEmpty()
    {
        var scraper = new WeWorkRemotelyScraper();
        var jobs = scraper.Parse(string.Empty);
        Assert.Empty(jobs);
    }
}
