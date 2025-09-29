using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Contracts;
using Domain.User.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using UserEntity = Domain.User.Entities.User;

namespace Application.Auth;

/// <summary>
/// 认证服务
/// </summary>
public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<UserEntity> _passwordHasher;
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher<UserEntity> passwordHasher,
        IOptions<JwtOptions> jwtOptions,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtOptions = jwtOptions;
        _logger = logger;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail))
        {
            throw new ArgumentException("用户名或邮箱不能为空", nameof(request.UsernameOrEmail));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("密码不能为空", nameof(request.Password));
        }

        var identifier = request.UsernameOrEmail.Trim();
        var user = await FindUserAsync(identifier);

        if (user is null)
        {
            _logger.LogWarning("登录失败：找不到用户 {Identifier}", identifier);
            throw new UnauthorizedAccessException("用户名或密码错误");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("登录失败：用户 {Identifier} 已被禁用", identifier);
            throw new UnauthorizedAccessException("账号已被禁用");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("登录失败：用户 {Identifier} 密码错误", identifier);
            throw new UnauthorizedAccessException("用户名或密码错误");
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            var newHash = _passwordHasher.HashPassword(user, request.Password);
            user.SetPasswordHash(newHash);
        }

        user.UpdateLastLogin();
        await _userRepository.UpdateAsync(user);

        var (token, expiresAtUtc) = GenerateJwt(user);
        var response = BuildLoginResponse(user, token, expiresAtUtc);

        _logger.LogInformation("用户 {Identifier} 登录成功", identifier);

        return response;
    }

    private async Task<UserEntity?> FindUserAsync(string identifier)
    {
        var user = await _userRepository.GetByUsernameAsync(identifier);
        if (user is not null)
        {
            return user;
        }

        if (identifier.Contains('@', StringComparison.Ordinal))
        {
            user = await _userRepository.GetByEmailAsync(identifier);
        }

        return user;
    }

    private (string Token, DateTime ExpiresAtUtc) GenerateJwt(UserEntity user)
    {
        var options = _jwtOptions.Value;
        if (string.IsNullOrWhiteSpace(options.Key))
        {
            throw new InvalidOperationException("JWT Key 未配置");
        }

        var keyBytes = Encoding.UTF8.GetBytes(options.Key);
        var signingKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(options.AccessTokenExpirationMinutes <= 0 ? 60 : options.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Role, user.Role)
        };

        if (user.LastLoginAt.HasValue)
        {
            claims.Add(new Claim("last_login_at", user.LastLoginAt.Value.ToString("O")));
        }

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        var encodedToken = _tokenHandler.WriteToken(token);
        return (encodedToken, expiresAt);
    }

    private static LoginResponse BuildLoginResponse(UserEntity user, string token, DateTime expiresAtUtc)
    {
        return new LoginResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = (int)Math.Max(0, (expiresAtUtc - DateTime.UtcNow).TotalSeconds),
            ExpiresAt = expiresAtUtc,
            User = new AuthenticatedUser
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Role = user.Role,
                LastLoginAt = user.LastLoginAt
            }
        };
    }
}