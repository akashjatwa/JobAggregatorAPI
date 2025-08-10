using Microsoft.AspNetCore.Mvc;
using JobAggregator.Application.DTOs;
using JobAggregator.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JobAggregator.Api.Controllers
{
#nullable enable
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobSearchService _jobSearchService;
        private readonly ILogger<JobsController> _logger;

        public JobsController(IJobSearchService jobSearchService, ILogger<JobsController> logger)
        {
            _jobSearchService = jobSearchService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get() => Ok(new { Message = "Job aggregator API" });

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string query,
            [FromQuery] string? location = null,
            [FromQuery] bool? remote = null,
            [FromQuery] decimal? minSalary = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Searching for jobs with query: {Query}, location: {Location}", query, location);

            var searchQuery = new JobSearchQuery
            {
                Query = query,
                Location = location ?? string.Empty,
                Remote = remote ?? false,
                MinSalary = minSalary,
                Page = page,
                PageSize = pageSize
            };

            var results = await _jobSearchService.SearchAsync(searchQuery, ct);
            return Ok(results);
        }
    }
}
#nullable disable
