using System.IO;
using JobAggregator.Infrastructure.Scrapers;
using Xunit;

namespace JobAggregator.Tests.Scrapers;

public class HtmlBoardScraperTests
{
    [Fact]
    public void Parse_ReturnsJobsFromHtml()
    {
        var html = File.ReadAllText(Path.Combine("Fixtures", "htmlboard_sample.html"));
        var scraper = new HtmlBoardScraper();

        var jobs = scraper.Parse(html).ToList();

        Assert.Equal(2, jobs.Count);
        var first = jobs[0];
        Assert.Equal("htmlboard", first.Source);
        Assert.Equal("1", first.ExternalId);
        Assert.Equal("Full Stack Engineer", first.Title);
        Assert.Equal("Gamma LLC", first.Company);
        Assert.Equal("Remote", first.Location);
        Assert.Equal("https://example.com/job3", first.Url);

        var second = jobs[1];
        Assert.Equal("2", second.ExternalId);
        Assert.Null(second.Location); // missing selector handled
    }

    [Fact]
    public void Parse_WithEmptyHtml_ReturnsEmpty()
    {
        var scraper = new HtmlBoardScraper();
        var jobs = scraper.Parse(string.Empty);
        Assert.Empty(jobs);
    }
}
