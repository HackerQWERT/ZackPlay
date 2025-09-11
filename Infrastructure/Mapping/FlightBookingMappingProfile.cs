using Mapster;

namespace Infrastructure.Mapping;

/// <summary>
/// 飞行预订映射配置
/// 
/// 架构原则：
/// 1. 领域层核心对象（Aggregate Root, Entity, ValueObject）→ 使用手写 ToDomain()/FromDomain()
///    - 保持显式、可读，能表达业务规则（比如调用 Deactivate()）
///    - 确保领域不变量和业务逻辑正确执行
/// 
/// 2. 应用层 DTO ↔ 外部接口模型 (API contract, ViewModel) → 使用 Mapster
///    - 提高效率，减少样板代码
///    - 适用于简单的数据传输对象映射
///    - 这些映射应该放在应用层或 Web 层，而不是基础设施层
/// </summary>
public static class FlightBookingMappingProfile
{
    public static void Configure()
    {
        // 注释：领域对象映射已移除，改为使用手写的 ToDomain()/FromDomain() 方法
        // 这样可以保持显式、可读，能表达业务规则（比如调用 Deactivate()）

        // 注释：应用层 DTO ↔ ViewModel 映射应该放在应用层或 Web 层
        // 基础设施层应该专注于数据持久化和外部系统集成

        // 如果需要基础设施层的映射（比如外部 API 响应到内部模型），可以在这里添加
    }
}
