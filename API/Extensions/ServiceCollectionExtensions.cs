using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Infrastructure.Data;
using Infrastructure.Repositories.FlightBooking.Implementations;
using Infrastructure.Services;
using Domain.Abstractions;
using Domain.FlightBooking.Repositories;
using Domain.User.Repositories;
using Mapster;
using Infrastructure.Mapping;
using Application.Mapping;
using Application.FlightBooking;
using Domain.FlightBooking.Services;
using API.Consumers;
using Application.Auth;
using Infrastructure.Repositories.User.Implementations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ZackPlay.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册所有应用程序服务 (生产环境)
    /// </summary>
    public static IServiceCollection AddAllServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 配置选项
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // 领域层服务
        services.AddDomainServices();

        // 应用层服务
        services.AddApplicationServices();

        // 基础设施服务
        services.AddInfrastructureServices(configuration);


        // API 层服务
        services.AddApiServices(configuration);

        return services;
    }

    #region 私有方法


    /// <summary>
    /// 配置应用层服务 - 机票预订业务
    /// </summary>
    private static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddJwtAuthentication(configuration);

        // 注册后台服务
        services.AddHostedService<FlightBookingConsumer>();
        // Hangfire 服务
        services.AddHangfireServices(configuration);

        return services;
    }

    /// <summary>
    /// 配置应用层服务 - 机票预订业务
    /// </summary>
    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // 注册应用层服务
        services.AddScoped<FlightBookingService>();
        services.AddScoped<FlightDataMockService>();

        // 注册认证服务
        services.AddScoped<AuthService>();

        // 注册密码哈希器
        services.AddScoped<IPasswordHasher<Domain.User.Entities.User>, PasswordHasher<Domain.User.Entities.User>>();

        // 配置应用层 Mapster 映射
        SimplifiedMappingProfile.Configure();

        return services;
    }

    /// <summary>
    /// 配置领域层服务 - 机票预订业务
    /// </summary>
    private static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // 注册领域服务
        services.AddScoped<BookingDomainService>();


        return services;
    }

    private static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 添加数据库（优先读取新节 SqlServer:ConnectionString，回退到 ConnectionStrings:DefaultConnection）
        var sqlConn = configuration["SqlServer:ConnectionString"];
        services.AddDbContext<FlightBookingDbContext>(options =>
            options.UseSqlServer(sqlConn));

        // 配置Redis（优先读取新节 Redis:ConnectionString，回退到旧 ConnectionStrings:Redis）
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var connectionString = configuration["Redis:ConnectionString"]
                                   ?? configuration.GetConnectionString("Redis")
                                   ?? "localhost:6379";
            return ConnectionMultiplexer.Connect(connectionString);
        });

        //添加缓存
        services.AddSingleton<ICacheService, RedisCacheService>();
        //分布式锁管理器
        services.AddSingleton<IDistributedLockManager, RedisDistributedLockManager>();
        //消息队列
        services.AddSingleton<IMessageQueueService, RabbitMQService>();



        // 注册仓储
        services.AddScoped<IAirportRepository, AirportRepository>();
        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<IPassengerRepository, PassengerRepository>();
        services.AddScoped<IFlightBookingRepository, FlightBookingRepository>();

        // 注册用户仓储
        services.AddScoped<IUserRepository, UserRepository>();

        // 配置Mapster
        services.AddMapster();

        // 注册映射配置
        FlightBookingMappingProfile.Configure();

        return services;
    }
    #endregion


    /// <summary>
    /// 配置JWT认证
    /// </summary>
    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "default-secret-key");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

        services.AddAuthorization();

        return services;
    }
}
