namespace Domain.Abstractions;

public interface IDistributedLock : IAsyncDisposable
{
    string Resource { get; }
    bool Acquired { get; }
}

public interface IDistributedLockManager
{
    Task<IDistributedLock> AcquireAsync(
        string resource,
        TimeSpan ttl,
        TimeSpan? wait = null,
        TimeSpan? retryDelay = null,
        CancellationToken ct = default);
}
