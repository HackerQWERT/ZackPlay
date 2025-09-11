using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZackPlay.Extensions;

namespace XUnitTest.Base;

public abstract class BaseTest
{
    protected ServiceProvider Provider { get; private set; }

    protected BaseTest()
    {
        var services = new ServiceCollection();

        // 读取主API项目的配置文件
        var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "ZackPlay");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                // 覆盖数据库连接为测试数据库
                {"ConnectionStrings:DefaultConnection", "Server=(localdb)\\MSSQLLocalDB;Database=FlightBookingTest;Trusted_Connection=true;MultipleActiveResultSets=true"},
                // 保持其他配置与主项目一致，但可以在这里覆盖测试特定的配置
            })
            .AddEnvironmentVariables()
            .Build();

        // 使用测试专用的服务注册
        services.AddAllServices(configuration);

        // 可以额外覆盖一些服务，例如 Mock Redis
        // services.AddSingleton<ICacheService, MockCacheService>();

        // 构建 ServiceProvider
        Provider = services.BuildServiceProvider();
    }

}

