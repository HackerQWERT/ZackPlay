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
using Mapster;
using Infrastructure.Mapping;

namespace ZackPlay.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册所有应用程序服务 (生产环境)
    /// </summary>
    public static IServiceCollection AddAllServices(this IServiceCollection services, IConfiguration configuration)
    {

        // 应用层服务
        services.AddApplicationServices();
        // 基础设施服务          
        services.AddInfrastructureServices(configuration);


        return services;
    }

    #region 私有方法


    /// <summary>
    /// 配置应用层服务 - 机票预订业务
    /// </summary>
    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // 注册应用层服务
        services.AddScoped<Application.FlightBooking.IFlightBookingService, Application.FlightBooking.FlightBookingService>();

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

        // 配置Mapster
        services.AddMapster();

        // 注册映射配置
        FlightBookingMappingProfile.Configure();

        return services;
    }
    #endregion


}
