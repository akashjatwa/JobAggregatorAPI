using System.Net.Http.Json;
using JobAggregator.Application.DTOs;
using JobAggregator.Infrastructure.Data;
using JobAggregator.Infrastructure.Scrapers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JobAggregator.Tests.Controllers;

public class ApiHappyPathTests
{
    private class FakeScraper : IJobScraper
    {
        public Task<IEnumerable<JobDto>> SearchAsync(string? query, CancellationToken cancellationToken)
        {
            var jobs = new[] { new JobDto("test", "1", "Test Job", "ACME", "Remote", null, null, null, null, null) };
            return Task.FromResult<IEnumerable<JobDto>>(jobs);
        }
    }

    [Fact]
    public async Task SearchEndpoint_ReturnsResults()
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("SearchDb"));
                services.AddSingleton<IJobScraper, FakeScraper>();
            });
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/jobs/search?q=test");
        response.EnsureSuccessStatusCode();
        var jobs = await response.Content.ReadFromJsonAsync<List<JobDto>>();
        Assert.Single(jobs);
    }

    [Fact]
    public async Task UserJobs_SaveAndList()
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("UserJobsDb"));
                services.AddSingleton<IJobScraper, FakeScraper>();
            });
        });

        var client = factory.CreateClient();
        var dto = new JobDto("test", "1", "Test Job", "ACME", "Remote", "http://example.com", null, null, null, null);
        var save = await client.PostAsJsonAsync("/user/jobs", dto);
        save.EnsureSuccessStatusCode();

        var listResponse = await client.GetAsync("/user/jobs");
        listResponse.EnsureSuccessStatusCode();
        var jobs = await listResponse.Content.ReadFromJsonAsync<List<UserJobDto>>();
        Assert.Single(jobs);
    }
}
