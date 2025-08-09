using Microsoft.AspNetCore.Mvc;

namespace JobAggregator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { Message = "Job aggregator API" });
    }
}
