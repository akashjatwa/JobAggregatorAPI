using System;
using System.Threading;
using System.Threading.Tasks;
using JobAggregator.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JobAggregator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CacheHealthController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheHealthController> _logger;

        public CacheHealthController(ICacheService cacheService, ILogger<CacheHealthController> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> CheckCacheHealth(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Testing cache service: {ServiceType}", _cacheService.GetType().Name);
                
                string testKey = "health-check-test";
                string testValue = "Cache is working!";
                
                await _cacheService.SetAsync(testKey, testValue, TimeSpan.FromSeconds(30), cancellationToken);
                _logger.LogInformation("Successfully set value in cache for key: {Key}", testKey);
                
                var retrievedValue = await _cacheService.GetAsync<string>(testKey, cancellationToken);
                
                if (retrievedValue == testValue)
                {
                    _logger.LogInformation("Successfully retrieved value from cache for key: {Key}", testKey);
                    return Ok(new { 
                        Status = "Healthy", 
                        Message = "Cache is working properly",
                        CacheImplementation = _cacheService.GetType().Name 
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve correct value from cache for key: {Key}", testKey);
                    return StatusCode(500, new { 
                        Status = "Unhealthy", 
                        Message = "Cache value mismatch",
                        CacheImplementation = _cacheService.GetType().Name 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing cache service");
                return StatusCode(500, new { 
                    Status = "Unhealthy", 
                    Message = $"Cache error: {ex.Message}",
                    CacheImplementation = _cacheService.GetType().Name 
                });
            }
        }
    }
}