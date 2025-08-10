using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using JobAggregator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using JobAggregator.Application.Interfaces;
using JobAggregator.Infrastructure.Caching;
using JobAggregator.Infrastructure.Scrapers;
using JobAggregator.Infrastructure.Services;
using StackExchange.Redis;
using System;


var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with JSON console logging
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter()));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Redis or fallback to in-memory cache
try
{
    var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection")!;
    
    // Test Redis connection before registering services
    var redis = StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString);
    var db = redis.GetDatabase();
    var pingResult = db.Ping();
    Console.WriteLine($"Redis connection successful. Ping: {pingResult.TotalMilliseconds}ms");
    
    // Register Redis services
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp => redis);
builder.Services.AddScoped<JobAggregator.Application.Interfaces.ICacheService, JobAggregator.Infrastructure.Caching.RedisCacheService>();

// Register scrapers
builder.Services.AddScoped<IScraper, IndeedScraper>();
builder.Services.AddScoped<IScraper, LinkedInScraper>();

// Register JobSearchService
builder.Services.AddScoped<IJobSearchService, JobSearchService>();
    
    // Add health checks with Redis
var healthChecks = builder.Services.AddHealthChecks();

// Add self check
healthChecks.AddCheck("self", () => HealthCheckResult.Healthy());

// Add SQL Server check
healthChecks.AddSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")!, 
    name: "sql",
    tags: new[] { "db" });

// Add Redis check with failureStatus set to degraded instead of unhealthy
healthChecks.AddRedis(
    redisConnectionString, 
    name: "redis", 
    failureStatus: HealthStatus.Degraded, 
    tags: new[] { "cache" });
}
catch (Exception ex)
{
    // Log the exception but continue with in-memory cache
    Console.WriteLine($"Warning: Redis configuration failed: {ex.Message}");
    Console.WriteLine("Application will continue with in-memory caching.");
    
    // Register in-memory cache service as fallback
    builder.Services.AddScoped<JobAggregator.Application.Interfaces.ICacheService, JobAggregator.Infrastructure.Caching.InMemoryCacheService>();
    
    // Register scrapers
    builder.Services.AddScoped<IScraper, IndeedScraper>();
    builder.Services.AddScoped<IScraper, LinkedInScraper>();
    
    // Register JobSearchService
    builder.Services.AddScoped<IJobSearchService, JobSearchService>();
    
    // Add health checks without Redis
var healthChecks = builder.Services.AddHealthChecks();

// Add self check
healthChecks.AddCheck("self", () => HealthCheckResult.Healthy());

// Add SQL Server check
healthChecks.AddSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")!, 
    name: "sql",
    tags: new[] { "db" });

// Add a placeholder for Redis that reports as degraded
healthChecks.AddCheck(
    "redis-unavailable", 
    () => HealthCheckResult.Degraded("Redis is not available"), 
    tags: new[] { "cache" });
}

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JobAggregator API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = 200,
        [HealthStatus.Degraded] = 200,
        [HealthStatus.Unhealthy] = 503
    }
});

app.Run();
