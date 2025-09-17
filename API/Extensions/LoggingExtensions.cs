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
            .WriteTo.Console()
            // .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["Elasticsearch:Uri"] ?? "http://localhost:9200"))
            // {
            //     IndexFormat = configuration["Elasticsearch:IndexFormat"] ?? "flightbooking-logs-{0:yyyy.MM.dd}",
            //     AutoRegisterTemplate = true,
            //     NumberOfShards = 2,
            //     NumberOfReplicas = 1
            // })
            .CreateLogger();

        return host.UseSerilog();
    }
}
