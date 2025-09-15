using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using ZackPlay.Extensions;

namespace XUnitTest.Base;

public abstract class BaseTest
{
    protected ServiceProvider Provider { get; private set; }


    protected BaseTest()
    {

        var services = new ServiceCollection();

        // 加载配置文件
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        // 使用测试专用的服务注册
        services.AddAllServices(configuration);

        // 可以额外覆盖一些服务，例如 Mock Redis
        // services.AddSingleton<ICacheService, MockCacheService>();

        // 构建 ServiceProvider
        Provider = services.BuildServiceProvider();
    }

}


