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
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using JobAggregator.Infrastructure.Data;
using JobAggregator.Application.Interfaces;
using JobAggregator.Infrastructure.Services;
using JobAggregator.Infrastructure.Scrapers;
using Serilog;
using Serilog.Formatting.Json;
using Serilog;
using Serilog.Formatting.Json;
using JobAggregator.Application.Interfaces;
using JobAggregator.Infrastructure.Caching;
using JobAggregator.Infrastructure.Scrapers;
using JobAggregator.Infrastructure.Services;
using StackExchange.Redis;
using System;
using System.Net.Http;


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
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IUserJobService, UserJobService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IJobService, JobSearchService>();
builder.Services.AddScoped<IJobScraper, RemoteOkScraper>();
builder.Services.AddScoped<IJobScraper, WeWorkRemotelyScraper>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JobAggregator API", Version = "v1" });
});
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddRedis(builder.Configuration.GetConnectionString("RedisConnection")!);

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
