using System;
using System.Threading;
using System.Threading.Tasks;
using JobAggregator.Application.DTOs;
using JobAggregator.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobAggregator.Api.Controllers;

[ApiController]
[Route("user/jobs")]
[Authorize]
public class UserJobsController : ControllerBase
{
    private readonly IUserJobService _service;
    private readonly IExportService _exportService;

    public UserJobsController(IUserJobService service, IExportService exportService)
    {
        _service = service;
        _exportService = exportService;
    }

    [HttpPost]
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

    [HttpGet("export")]
    public async Task Export([FromQuery] string status = "Applied", CancellationToken cancellationToken = default)
    {
        if (!string.Equals(status, "Applied", StringComparison.OrdinalIgnoreCase))
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsync("Only Applied status supported", cancellationToken);
            return;
        }

        Response.ContentType = "text/csv";
        Response.Headers.Add("Content-Disposition", "attachment; filename=applied_jobs.csv");

        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = Guid.Parse(userIdString ?? throw new InvalidOperationException("User ID not found"));

        await foreach (var line in _exportService.StreamAppliedJobsCsvAsync(userId, cancellationToken))
        {
            await Response.WriteAsync(line + "\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}
