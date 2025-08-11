using Microsoft.AspNetCore.Mvc;

using JobAggregator.Application.Interfaces;
using JobAggregator.Application.DTOs;

namespace JobAggregator.Api.Controllers
{
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
