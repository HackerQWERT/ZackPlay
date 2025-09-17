namespace Domain.Abstractions;

/// <summary>
/// 分布式限流器接口
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// 检查是否允许请求通过
    /// </summary>
    /// <param name="key">限流键</param>
    /// <param name="limit">限制数量</param>
    /// <param name="window">时间窗口（秒）</param>
    /// <returns>是否允许通过</returns>
    Task<bool> IsAllowedAsync(string key, int limit, int window);
    
    /// <summary>
    /// 获取当前计数
    /// </summary>
    /// <param name="key">限流键</param>
    /// <param name="window">时间窗口（秒）</param>
    /// <returns>当前计数</returns>
    Task<long> GetCurrentCountAsync(string key, int window);
}

/// <summary>
/// 限流结果
/// </summary>
public record RateLimitResult(
    bool IsAllowed,
    long CurrentCount,
    long Limit,
    TimeSpan RetryAfter);