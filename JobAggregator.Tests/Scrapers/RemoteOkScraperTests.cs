using System.IO;
using JobAggregator.Infrastructure.Scrapers;
using Xunit;

namespace JobAggregator.Tests.Scrapers;

public class RemoteOkScraperTests
{
    [Fact]
    public void Parse_ReturnsJobsFromJson()
    {
        var json = File.ReadAllText(Path.Combine("Fixtures", "remoteok_sample.json"));
        var scraper = new RemoteOkScraper();

        var jobs = scraper.Parse(json).ToList();

        Assert.Single(jobs);
        var job = jobs[0];
        Assert.Equal("remoteok", job.Source);
        Assert.Equal("12345", job.ExternalId);
        Assert.Equal("Software Engineer", job.Title);
        Assert.Equal("Acme Corp", job.Company);
        Assert.Equal("Remote", job.Location);
        Assert.Equal("https://example.com/job1", job.Url);
    }

    [Fact]
    public void Parse_WithEmptyJson_ReturnsEmpty()
    {
        var scraper = new RemoteOkScraper();
        var jobs = scraper.Parse(string.Empty);
        Assert.Empty(jobs);
    }
}
