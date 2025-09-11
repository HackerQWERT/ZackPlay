# 映射架构设计文档

## 架构原则

本项目采用分层映射策略，基于以下原则：

### 1. 领域层核心对象（Aggregate Root, Entity, ValueObject）→ 手写 ToDomain()/FromDomain()

**原因：**
- **保持显式、可读**：手写映射代码更清晰地表达业务意图
- **业务规则表达**：可以在映射过程中调用业务方法（如 `Deactivate()`）
- **确保领域不变量**：通过构造函数和业务方法确保领域对象的一致性
- **类型安全**：编译时检查，避免运行时映射错误

**示例：**
```csharp
// ✅ 推荐：在 Po 类中手写映射方法
public Airport ToDomain()
{
    var airport = new Airport(Code, Name, City, Country, TimeZone);
    if (!IsActive) airport.Deactivate(); // 业务规则
    return airport;
}

public static AirportPo FromDomain(Airport airport)
{
    return new AirportPo
    {
        Code = airport.Code,
        Name = airport.Name,
        // ... 其他属性
    };
}
```

### 2. 应用层 DTO ↔ 外部接口模型 (API Contract, Request/Response) → 使用 Mapster

**原因：**
- **提高效率**：减少样板代码，提高开发效率
- **简单映射**：适用于没有复杂业务逻辑的数据传输对象
- **灵活配置**：支持复杂的映射逻辑和数据转换

**示例：**
```csharp
// ✅ 推荐：使用 Mapster 进行 DTO 映射
TypeAdapterConfig<AirportDto, AirportResponse>
    .NewConfig()
    .Map(dest => dest.DisplayName, src => $"{src.Name} ({src.Code})")
    .Map(dest => dest.Location, src => $"{src.City}, {src.Country})");
```

## 项目结构

```
├── Domain/                          # 领域层
│   ├── Entities/                   
│   │   ├── Airport.cs              # 使用私有构造函数 + 公共业务构造函数
│   │   ├── Flight.cs               # 领域不变量通过构造函数保证
│   │   └── ...
│   └── ValueObjects/
│
├── Infrastructure/                  # 基础设施层
│   ├── Repositories/
│   │   └── Po/                     # 持久化对象
│   │       ├── AirportPo.cs        # ✅ 手写 ToDomain()/FromDomain()
│   │       ├── FlightPo.cs         # ✅ 手写 ToDomain()/FromDomain()
│   │       └── ...
│   └── Mapping/
│       └── FlightBookingMappingProfile.cs  # 仅配置基础设施层映射
│
├── Application/                     # 应用层
│   ├── DTOs/
│   │   ├── FlightBookingDTOs.cs    # 内部数据传输对象
│   │   ├── Requests/               # 请求模型
│   │   └── Responses/              # 响应模型
│   └── Mapping/
│       └── ApplicationMappingProfile.cs    # ✅ 使用 Mapster 配置 DTO 映射
│
└── ZackPlay/                       # 表现层
    ├── Controllers/
    └── ViewModels/                 # 可选：如果需要特定的视图模型
```

## 映射层次说明

### 第一层：领域对象 ↔ 持久化对象
- **位置**：Infrastructure.Repositories.Po
- **方法**：手写 `ToDomain()` 和 `FromDomain()`
- **特点**：保证领域不变量，执行业务规则

### 第二层：领域对象 ↔ 应用层 DTO
- **位置**：Application 层服务中
- **方法**：手写或简单映射
- **特点**：转换为适合应用层处理的数据结构

### 第三层：应用层 DTO ↔ API 契约
- **位置**：Application.Mapping
- **方法**：Mapster 配置
- **特点**：高效的数据传输，格式化和展示逻辑

## 最佳实践

### ✅ 推荐做法

1. **领域对象映射**：
   ```csharp
   // 在 Po 类中
   public Airport ToDomain() { /* 手写映射 */ }
   public static AirportPo FromDomain(Airport airport) { /* 手写映射 */ }
   ```

2. **DTO 映射**：
   ```csharp
   // 在 ApplicationMappingProfile 中
   TypeAdapterConfig<AirportDto, AirportResponse>
       .NewConfig()
       .Map(dest => dest.DisplayName, src => $"{src.Name} ({src.Code})");
   ```

3. **仓储实现**：
   ```csharp
   public async Task<Airport?> GetByCodeAsync(string code)
   {
       var po = await _context.Airports.FirstOrDefaultAsync(a => a.Code == code);
       return po?.ToDomain(); // 使用手写方法
   }
   ```

### ❌ 避免做法

1. **不要在领域对象映射中使用 Mapster**：
   ```csharp
   // ❌ 避免
   return po?.Adapt<Airport>(); // 可能破坏领域不变量
   ```

2. **不要跨层级映射**：
   ```csharp
   // ❌ 避免直接从 Po 映射到 Response
   return po.Adapt<AirportResponse>(); // 跳过了业务层处理
   ```

## 优势总结

1. **清晰的关注点分离**：每一层负责自己的映射逻辑
2. **领域完整性**：通过手写映射确保业务规则执行
3. **开发效率**：在合适的地方使用工具提高效率
4. **可维护性**：映射逻辑分布合理，易于维护和测试
5. **类型安全**：编译时检查，减少运行时错误

这种架构设计平衡了代码质量、开发效率和维护性，是企业级应用的推荐做法。
