using StackExchange.Redis;
using Newtonsoft.Json;
using Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);

            if (!value.HasValue)
                return default(T);

            return JsonConvert.DeserializeObject<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting key {key} from cache");
            return default(T);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var json = JsonConvert.SerializeObject(value);
            await _database.StringSetAsync(key, json, expiration);

            _logger.LogDebug($"Set key {key} in cache with expiration {expiration}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting key {key} in cache");
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
            _logger.LogDebug($"Removed key {key} from cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing key {key} from cache");
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking if key {key} exists in cache");
            return false;
        }
    }
}
