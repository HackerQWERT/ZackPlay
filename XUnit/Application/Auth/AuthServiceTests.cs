using Application.Auth;
using Application.Contracts;
using Domain.User.Entities;
using Domain.User.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace XUnitTest.Application.Auth;

public class AuthServiceTests
{
    private readonly PasswordHasher<User> _passwordHasher = new();
    private readonly InMemoryUserRepository _repository = new();
    private readonly IOptions<JwtOptions> _options = Options.Create(new JwtOptions
    {
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        Key = "test-secret-key-should-be-long-enough-123456",
        AccessTokenExpirationMinutes = 30
    });

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var user = CreateUser("testuser", "test@example.com", "Test User", "StrongP@ssw0rd!");
        await _repository.AddAsync(user);

        var service = CreateService();
        var request = new LoginRequest { UsernameOrEmail = "testuser", Password = "StrongP@ssw0rd!" };

        var response = await service.LoginAsync(request);

        response.AccessToken.Should().NotBeNullOrWhiteSpace();
        response.User.Username.Should().Be("testuser");
        response.User.Email.Should().Be("test@example.com");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().BeGreaterThan(0);
        response.User.LastLoginAt.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIsInvalid()
    {
        var user = CreateUser("testuser", "test@example.com", "Test User", "StrongP@ssw0rd!");
        await _repository.AddAsync(user);

        var service = CreateService();
        var request = new LoginRequest { UsernameOrEmail = "testuser", Password = "WrongPassword" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserIsDisabled()
    {
        var user = CreateUser("disabled", "disabled@example.com", "Disabled User", "StrongP@ssw0rd!");
        user.Deactivate();
        await _repository.AddAsync(user);

        var service = CreateService();
        var request = new LoginRequest { UsernameOrEmail = "disabled", Password = "StrongP@ssw0rd!" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(request));
    }

    private AuthService CreateService()
    {
        return new AuthService(_repository, _passwordHasher, _options, NullLogger<AuthService>.Instance);
    }

    private User CreateUser(string username, string email, string displayName, string plainPassword)
    {
        var user = new User(username, email, displayName, User.Roles.User);
        var hash = _passwordHasher.HashPassword(user, plainPassword);
        user.SetPasswordHash(hash);
        return user;
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();
        private readonly object _gate = new();

        public Task AddAsync(User user)
        {
            lock (_gate)
            {
                RemoveInternal(user.Id);
                _users.Add(Clone(user));
            }
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByEmailAsync(string email)
        {
            lock (_gate)
            {
                return Task.FromResult(_users.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));
            }
        }

        public Task<bool> ExistsByUsernameAsync(string username)
        {
            lock (_gate)
            {
                return Task.FromResult(_users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)));
            }
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            lock (_gate)
            {
                var user = _users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
                return Task.FromResult(CloneOrDefault(user));
            }
        }

        public Task<User?> GetByIdAsync(Guid id)
        {
            lock (_gate)
            {
                var user = _users.FirstOrDefault(u => u.Id == id);
                return Task.FromResult(CloneOrDefault(user));
            }
        }

        public Task<User?> GetByUsernameAsync(string username)
        {
            lock (_gate)
            {
                var user = _users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
                return Task.FromResult(CloneOrDefault(user));
            }
        }

        public Task UpdateAsync(User user)
        {
            lock (_gate)
            {
                RemoveInternal(user.Id);
                _users.Add(Clone(user));
            }
            return Task.CompletedTask;
        }

        private void RemoveInternal(Guid id)
        {
            var index = _users.FindIndex(u => u.Id == id);
            if (index >= 0)
            {
                _users.RemoveAt(index);
            }
        }

        private static User? CloneOrDefault(User? user) => user is null ? null : Clone(user);

        private static User Clone(User user)
        {
            return User.Rehydrate(
                user.Id,
                user.Username,
                user.Email,
                user.DisplayName,
                user.PasswordHash,
                user.Role,
                user.IsActive,
                user.CreatedAt,
                user.UpdatedAt,
                user.LastLoginAt);
        }
    }
}
