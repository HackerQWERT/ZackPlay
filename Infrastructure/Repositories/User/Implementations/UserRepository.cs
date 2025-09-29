using Microsoft.EntityFrameworkCore;
using Domain.User.Repositories;
using Infrastructure.Data;
using Infrastructure.Repositories.User.Po;

using UserEntity = Domain.User.Entities.User;

namespace Infrastructure.Repositories.User.Implementations;

/// <summary>
/// 用户仓储实现
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly FlightBookingDbContext _context;

    public UserRepository(FlightBookingDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntity?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;

        var normalized = UserPo.Normalize(username);
        var userPo = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.NormalizedUsername == normalized);
        return userPo?.ToDomain();
    }

    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;

        var normalized = UserPo.Normalize(email);
        var userPo = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized);
        return userPo?.ToDomain();
    }

    public async Task<UserEntity?> GetByIdAsync(Guid id)
    {
        var userPo = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
        return userPo?.ToDomain();
    }

    public async Task AddAsync(UserEntity user)
    {
        var entity = UserPo.FromDomain(user);
        await _context.Users.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserEntity user)
    {
        var existing = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (existing is null)
        {
            throw new InvalidOperationException($"用户 {user.Id} 不存在");
        }

        existing.Username = user.Username;
        existing.NormalizedUsername = UserPo.Normalize(user.Username);
        existing.Email = user.Email;
        existing.NormalizedEmail = UserPo.Normalize(user.Email);
        existing.DisplayName = user.DisplayName;
        existing.PasswordHash = user.PasswordHash;
        existing.Role = user.Role;
        existing.IsActive = user.IsActive;
        existing.UpdatedAt = user.UpdatedAt;
        existing.LastLoginAt = user.LastLoginAt;

        await _context.SaveChangesAsync();
    }

    public Task<bool> ExistsByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Task.FromResult(false);
        }

        var normalized = UserPo.Normalize(username);
        return _context.Users.AsNoTracking().AnyAsync(u => u.NormalizedUsername == normalized);
    }

    public Task<bool> ExistsByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Task.FromResult(false);
        }

        var normalized = UserPo.Normalize(email);
        return _context.Users.AsNoTracking().AnyAsync(u => u.NormalizedEmail == normalized);
    }
}