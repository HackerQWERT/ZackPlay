using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace ZackPlay.Extensions;

/// <summary>
/// Hangfire 扩展方法
/// </summary>
public static class HangfireExtensions
{
    /// <summary>
    /// 添加 Hangfire 服务
    /// </summary>
    public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 获取数据库连接字符串
        var sqlConn = configuration["SqlServer:ConnectionString"];

        // 配置 Hangfire
        var hangfireConfig = configuration.GetSection("Hangfire");
        var hangfireConnectionString = hangfireConfig["ConnectionString"] ?? sqlConn;
        var commandBatchMaxTimeout = TimeSpan.Parse(hangfireConfig["CommandBatchMaxTimeout"] ?? "00:05:00");
        var slidingInvisibilityTimeout = TimeSpan.Parse(hangfireConfig["SlidingInvisibilityTimeout"] ?? "00:05:00");
        var queuePollInterval = TimeSpan.Parse(hangfireConfig["QueuePollInterval"] ?? "00:00:00");
        var useRecommendedIsolationLevel = bool.Parse(hangfireConfig["UseRecommendedIsolationLevel"] ?? "true");
        var disableGlobalLocks = bool.Parse(hangfireConfig["DisableGlobalLocks"] ?? "true");

        services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(hangfireConnectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = commandBatchMaxTimeout,
                SlidingInvisibilityTimeout = slidingInvisibilityTimeout,
                QueuePollInterval = queuePollInterval,
                UseRecommendedIsolationLevel = useRecommendedIsolationLevel,
                DisableGlobalLocks = disableGlobalLocks
            }));

        services.AddHangfireServer();

        // 配置 Hangfire Dashboard
        var dashboardConfig = hangfireConfig.GetSection("Dashboard");
        var dashboardUsername = dashboardConfig["Username"] ?? "admin";
        var dashboardPassword = dashboardConfig["Password"] ?? "hangfire123";
        var dashboardTitle = dashboardConfig["Title"] ?? "Hangfire Dashboard";

        services.AddSingleton(new DashboardOptions
        {
            Authorization = new[]
            {
                new HangfireDashboardAuthorizationFilter(dashboardUsername, dashboardPassword)
            },
            DashboardTitle = dashboardTitle,
            DisplayStorageConnectionString = false
        });

        return services;
    }

    /// <summary>
    /// Hangfire Dashboard 授权过滤器
    /// </summary>
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string _username;
        private readonly string _password;

        public HangfireDashboardAuthorizationFilter(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // 检查是否已认证
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                return true;
            }

            // 基本认证
            string? authHeader = httpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Basic "))
            {
                var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();
                if (!string.IsNullOrEmpty(encodedUsernamePassword))
                {
                    try
                    {
                        var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                        var username = decodedUsernamePassword.Split(':', 2)[0];
                        var password = decodedUsernamePassword.Split(':', 2)[1];

                        if (username == _username && password == _password)
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        // 忽略解码错误
                    }
                }
            }

            // 如果认证失败，返回401
            httpContext.Response.Headers["WWW-Authenticate"] = "Basic";
            httpContext.Response.StatusCode = 401;
            return false;
        }
    }
}