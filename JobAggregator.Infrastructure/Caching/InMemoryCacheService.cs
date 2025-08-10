using JobAggregator.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobAggregator.Infrastructure.Caching
{
    public class InMemoryCacheService : ICacheService
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new();
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<InMemoryCacheService> _logger;

        public InMemoryCacheService(ILogger<InMemoryCacheService> logger)
        {
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken ct)
        {
            _logger.LogInformation("Getting value from in-memory cache for key: {Key}", key);
            
            if (_cache.TryGetValue(key, out var item) && !IsExpired(item))
            {
                try
                {
                    var value = JsonSerializer.Deserialize<T>(item.Value, _jsonOptions);
                    return Task.FromResult(value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing cached value for key: {Key}", key);
                }
            }

            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct)
        {
            _logger.LogInformation("Setting value in in-memory cache for key: {Key} with TTL: {TTL}", key, ttl);
            
            try
            {
                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                var cacheItem = new CacheItem
                {
                    Value = serializedValue,
                    ExpiresAt = DateTime.UtcNow.Add(ttl)
                };

                _cache[key] = cacheItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serializing value for key: {Key}", key);
            }

            return Task.CompletedTask;
        }

        private bool IsExpired(CacheItem item)
        {
            return item.ExpiresAt < DateTime.UtcNow;
        }

        private class CacheItem
        {
            public required string Value { get; init; }
            public DateTime ExpiresAt { get; init; }
        }
    }
}