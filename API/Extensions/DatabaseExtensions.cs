using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Infrastructure.Data;
using Domain.User.Entities;
using Domain.User.Repositories;

namespace ZackPlay.Extensions;

public static class DatabaseExtensions
{
    /// <summary>
    /// 初始化数据库
    /// </summary>
    public static async Task<IHost> InitializeDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FlightBookingDbContext>();

        // 应用任何挂起的迁移
        await context.Database.MigrateAsync();

        await SeedDefaultAdminAsync(scope.ServiceProvider);

        return host;
    }

    private static async Task SeedDefaultAdminAsync(IServiceProvider serviceProvider)
    {
        var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher<User>>();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");

        const string defaultUsername = "admin";
        const string defaultEmail = "admin@zackplay.local";
        const string defaultDisplayName = "系统管理员";
        const string defaultPassword = "Admin@123456";

        var existing = await userRepository.GetByUsernameAsync(defaultUsername);
        if (existing is not null)
        {
            return;
        }

        var admin = new User(defaultUsername, defaultEmail, defaultDisplayName, User.Roles.Admin);
        var hashedPassword = passwordHasher.HashPassword(admin, defaultPassword);
        admin.SetPasswordHash(hashedPassword);

        await userRepository.AddAsync(admin);

        logger.LogInformation("已创建默认管理员账号 {Username}，请尽快修改默认密码", defaultUsername);
    }

}
