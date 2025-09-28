using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace ZackPlay.Extensions;

public static class LoggingExtensions
{
    /// <summary>
    /// 配置Serilog日志
    /// </summary>
    public static IHostBuilder ConfigureLogging(this IHostBuilder host, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()

            .CreateLogger();

        return host.UseSerilog();
    }
}
