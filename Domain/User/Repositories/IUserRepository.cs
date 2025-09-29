using UserEntity = Domain.User.Entities.User;

namespace Domain.User.Repositories;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(Guid id);
    Task<UserEntity?> GetByUsernameAsync(string username);
    Task<UserEntity?> GetByEmailAsync(string email);
    Task AddAsync(UserEntity user);
    Task UpdateAsync(UserEntity user);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<bool> ExistsByEmailAsync(string email);
}