using JobAggregator.Application.DTOs;
using JobAggregator.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobAggregator.Api.Controllers;

[ApiController]
[Route("user/jobs")]
public class UserJobsController : ControllerBase
{
    private readonly IUserJobService _service;

    public UserJobsController(IUserJobService service)
    {
        _service = service;
    }

    [HttpPost]
    // TODO: use authenticated user from JWT instead of demo user
    public async Task<IActionResult> Save([FromBody] JobDto dto)
    {
        var result = await _service.SaveAsync(dto);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? status,
        [FromQuery] string? company,
        [FromQuery] string? location,
        [FromQuery] string? tag,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var results = await _service.ListAsync(status, company, location, tag, from, to, sort, page, pageSize);
        return Ok(results);
    }

    [HttpPatch("{id:guid}:apply")]
    public async Task<IActionResult> Apply(Guid id)
    {
        await _service.ApplyAsync(id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
