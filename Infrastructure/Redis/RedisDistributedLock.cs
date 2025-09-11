using Domain.Abstractions;
using StackExchange.Redis;

namespace Infrastructure.Services;

internal sealed class RedisDistributedLock : IDistributedLock
{
    private readonly IDatabase _db;
    private readonly string _token;

    public string Resource { get; }
    public bool Acquired { get; private set; }

    public RedisDistributedLock(IDatabase db, string resource, string token, bool acquired)
    {
        _db = db;
        Resource = resource;
        _token = token;
        Acquired = acquired;
    }

    public async ValueTask DisposeAsync()
    {
        if (!Acquired) return;

        // release lock only if token matches
        var script = @"if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end";
        await _db.ScriptEvaluateAsync(script, new RedisKey[] { Resource }, new RedisValue[] { _token });
        Acquired = false;
    }
}

public sealed class RedisDistributedLockManager : IDistributedLockManager
{
    private readonly IDatabase _db;
    private readonly Random _random = new();

    public RedisDistributedLockManager(IConnectionMultiplexer mux)
    {
        _db = mux.GetDatabase();
    }

    public async Task<IDistributedLock> AcquireAsync(string resource, TimeSpan ttl, TimeSpan? wait = null, TimeSpan? retryDelay = null, CancellationToken ct = default)
    {
        var token = Guid.NewGuid().ToString("N");
        var end = wait.HasValue ? DateTime.UtcNow + wait.Value : DateTime.MaxValue;
        var delay = retryDelay ?? TimeSpan.FromMilliseconds(100);

        while (DateTime.UtcNow <= end && !ct.IsCancellationRequested)
        {
            var ok = await _db.StringSetAsync(resource, token, ttl, when: When.NotExists);
            if (ok)
                return new RedisDistributedLock(_db, resource, token, acquired: true);

            var jitter = TimeSpan.FromMilliseconds(_random.Next(10, 30));
            await Task.Delay(delay + jitter, ct);
        }

        return new RedisDistributedLock(_db, resource, token, acquired: false);
    }
}
