namespace Application.Contracts;

/// <summary>
/// 用户登录请求
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 用户名或邮箱
    /// </summary>
    public string UsernameOrEmail { get; set; } = default!;

    /// <summary>
    /// 登录密码
    /// </summary>
    public string Password { get; set; } = default!;

    /// <summary>
    /// 是否记住登录状态
    /// </summary>
    public bool RememberMe { get; set; }
}

/// <summary>
/// 用户注册请求
/// </summary>
public class RegisterRequest
{
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string Password { get; set; } = default!;
}