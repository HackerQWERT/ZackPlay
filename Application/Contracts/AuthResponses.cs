namespace Application.Contracts;

/// <summary>
/// 登录响应
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; set; } = default!;

    /// <summary>
    /// 令牌类型，默认 Bearer
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// 过期秒数
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// 过期时间（UTC）
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 登录用户信息
    /// </summary>
    public AuthenticatedUser User { get; set; } = default!;
}

/// <summary>
/// 登录用户信息
/// </summary>
public class AuthenticatedUser
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// 注册响应
/// </summary>
public class RegisterResponse
{
    public Guid UserId { get; set; }
    public string Message { get; set; } = default!;
}