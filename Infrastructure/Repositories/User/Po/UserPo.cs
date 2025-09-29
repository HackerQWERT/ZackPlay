using UserEntity = Domain.User.Entities.User;

namespace Infrastructure.Repositories.User.Po;

/// <summary>
/// 用户持久化对象
/// </summary>
public class UserPo
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string NormalizedUsername { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string NormalizedEmail { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Role { get; set; } = UserEntity.Roles.User;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public UserEntity ToDomain()
    {
        return UserEntity.Rehydrate(
            Id,
            Username,
            Email,
            DisplayName,
            PasswordHash,
            Role,
            IsActive,
            CreatedAt,
            UpdatedAt,
            LastLoginAt);
    }

    public static UserPo FromDomain(UserEntity user)
    {
        return new UserPo
        {
            Id = user.Id,
            Username = user.Username,
            NormalizedUsername = Normalize(user.Username),
            Email = user.Email,
            NormalizedEmail = Normalize(user.Email),
            DisplayName = user.DisplayName,
            PasswordHash = user.PasswordHash,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public static string Normalize(string value) => value.Trim().ToUpperInvariant();
}