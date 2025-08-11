using Microsoft.AspNetCore.Mvc;

using JobAggregator.Application.Interfaces;
using JobAggregator.Application.DTOs;

namespace JobAggregator.Api.Controllers
{
using JobAggregator.Application.DTOs;
using JobAggregator.Application.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace JobAggregator.Api.Controllers
{
#nullable enable
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _service;

        public JobsController(IJobService service)
        {
            _service = service;
        }

        [HttpGet("search")]
        public async Task<IEnumerable<JobDto>> Search([FromQuery] string? q, CancellationToken cancellationToken)
            => await _service.SearchAsync(q, cancellationToken);
    }
}
        private readonly IJobSearchService _jobSearchService;
        private readonly ILogger<JobsController> _logger;

        public JobsController(IJobSearchService jobSearchService, ILogger<JobsController> logger)
        {
            _jobSearchService = jobSearchService;
            _logger = logger;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get API information",
            Description = "Returns basic information about the Job Aggregator API",
            OperationId = "Jobs_GetInfo",
            Tags = new[] { "Jobs" }
        )]
        [SwaggerResponse(200, "API information", typeof(object))]
        public IActionResult Get() => Ok(new { Message = "Job aggregator API" });

        [HttpGet("search")]
        [SwaggerOperation(
            Summary = "Search for jobs",
            Description = "Searches for jobs across multiple sources based on the provided criteria",
            OperationId = "Jobs_Search",
            Tags = new[] { "Jobs" }
        )]
        [SwaggerResponse(200, "List of jobs matching the search criteria", typeof(IReadOnlyList<JobDto>))]
        [SwaggerResponse(400, "Invalid input parameters")]
        public async Task<IActionResult> Search(
            [FromQuery, Required] string query,
            [FromQuery] string? location = null,
            [FromQuery] bool? remote = null,
            [FromQuery] decimal? minSalary = null,
            [FromQuery] int page = 1,
            [FromQuery, Range(1, 50)] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            // Validate and normalize parameters
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

            _logger.LogInformation("Searching for jobs with query: {Query}, location: {Location}, remote: {Remote}, minSalary: {MinSalary}, page: {Page}, pageSize: {PageSize}", 
                query, location, remote, minSalary, page, pageSize);

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
            _logger.LogInformation("Found {Count} jobs matching the search criteria", results.Count);
            return Ok(results);
        }
    }
}
#nullable disable
