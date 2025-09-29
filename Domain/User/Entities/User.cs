using Domain.Abstractions;

namespace Domain.User.Entities;

/// <summary>
/// 系统用户聚合根
/// </summary>
public class User : IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public Guid Id { get; private set; }
    public string Username { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Role { get; private set; } = Roles.User;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private User()
    {
    }

    public User(string username, string email, string displayName, string role = Roles.User)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("用户名不能为空", nameof(username));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("邮箱不能为空", nameof(email));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("显示名称不能为空", nameof(displayName));

        Id = Guid.NewGuid();
        Username = username.Trim();
        Email = email.Trim();
        DisplayName = displayName.Trim();
        Role = string.IsNullOrWhiteSpace(role) ? Roles.User : role.Trim();
        PasswordHash = string.Empty;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("密码哈希不能为空", nameof(passwordHash));
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string displayName, string email)
    {
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("显示名称不能为空", nameof(displayName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("邮箱不能为空", nameof(email));

        DisplayName = displayName.Trim();
        Email = email.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = LastLoginAt;
    }

    public void ChangeRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role)) throw new ArgumentException("角色不能为空", nameof(role));
        Role = role.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public static User Rehydrate(
        Guid id,
        string username,
        string email,
        string displayName,
        string passwordHash,
        string role,
        bool isActive,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? lastLoginAt)
    {
        return new User
        {
            Id = id,
            Username = username,
            Email = email,
            DisplayName = displayName,
            PasswordHash = passwordHash,
            Role = string.IsNullOrWhiteSpace(role) ? Roles.User : role,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            LastLoginAt = lastLoginAt
        };
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}
