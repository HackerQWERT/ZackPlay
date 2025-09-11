# 机票订阅系统

这是一个模拟机票订阅的ASP.NET Core Web API项目，实现了机票发布、客户订阅和通知功能。

## 技术栈

- **后端框架**: ASP.NET Core 9.0
- **数据库**: SQL Server + Entity Framework Core
- **缓存**: Redis
- **消息队列**: RabbitMQ
- **日志**: Elasticsearch + Serilog
- **架构模式**: 依赖注入(DI) + Repository模式

## 核心功能

### 1. 机票管理
- 创建、查询、更新、删除航班信息
- 航班座位库存管理
- 航班可用性检查
- 乐观锁(RowVersion)防止并发冲突

### 2. 客户订阅
- 客户可以订阅特定航线的机票信息
- 支持价格限制和日期偏好设置
- 支持邮件和短信通知偏好

### 3. 通知系统
- 新航班发布时自动通知匹配的订阅者
- 使用RabbitMQ异步处理通知
- 支持邮件和短信多渠道通知

### 4. 缓存策略
- 使用Redis缓存热点数据
- 航班信息缓存5-10分钟
- 自动缓存失效和更新

### 5. 日志系统
- 使用Serilog记录应用日志
- 日志存储到Elasticsearch
- 支持结构化日志查询

## 数据模型

### Flight (航班)
- 航班号、起降地、时间、价格
- 座位数量和可用座位
- 乐观锁版本控制

### Customer (客户)
- 基本信息、联系方式
- 护照号码

### FlightSubscription (航班订阅)
- 订阅航线和偏好设置
- 通知偏好配置
- 订阅状态管理

### Booking (预订)
- 预订信息和状态
- 乘客数量和总价

## API 端点

### 航班管理
```
GET    /api/flights                    # 查询航班
GET    /api/flights/{id}               # 获取特定航班
POST   /api/flights                    # 创建航班
PUT    /api/flights/{id}               # 更新航班
DELETE /api/flights/{id}               # 删除航班
GET    /api/flights/{id}/availability  # 检查可用性
```

### 订阅管理
```
POST   /api/subscriptions                      # 创建订阅
GET    /api/subscriptions/customer/{id}        # 获取客户订阅
GET    /api/subscriptions/active               # 获取活跃订阅
PUT    /api/subscriptions/{id}                 # 更新订阅
DELETE /api/subscriptions/{id}                 # 删除订阅
```

## 环境要求

### 开发环境
- .NET 9.0 SDK
- SQL Server (或 LocalDB)
- Redis Server
- RabbitMQ Server
- Elasticsearch (可选，用于日志)

### 配置文件
在 `appsettings.json` 中配置连接字符串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FlightBookingDb;Trusted_Connection=true;MultipleActiveResultSets=true",
    "Redis": "localhost:6379",
    "RabbitMQ:HostName": "localhost",
    "RabbitMQ:Port": "5672",
    "RabbitMQ:UserName": "guest",
    "RabbitMQ:Password": "guest"
  },
  "Elasticsearch": {
    "Uri": "http://localhost:9200",
    "IndexFormat": "flightbooking-logs-{0:yyyy.MM.dd}"
  }
}
```

## 快速开始

1. **安装依赖服务**
   ```bash
   # 安装 Redis (Windows)
   # 下载并安装 Redis for Windows

   # 安装 RabbitMQ (Windows)
   # 下载并安装 RabbitMQ Server

   # 安装 Elasticsearch (可选)
   # 下载并安装 Elasticsearch
   ```

2. **还原NuGet包**
   ```bash
   dotnet restore
   ```

3. **运行应用**
   ```bash
   dotnet run
   ```

4. **访问API文档**
   ```
   https://localhost:7xxx/openapi/v1.json
   ```

## 示例数据

应用启动时会自动创建示例数据：
- 3个示例客户
- 4个示例航班
- 3个示例订阅
- 2个示例预订

## 架构特点

### 乐观锁实现
使用 `[Timestamp]` 属性和 `RowVersion` 字段实现乐观锁，防止并发更新冲突。

### 依赖注入
- `IFlightService` - 航班业务逻辑
- `ISubscriptionService` - 订阅业务逻辑  
- `ICacheService` - Redis缓存服务
- `IMessageQueueService` - RabbitMQ消息队列服务

### 异步通知
新航班创建时，会自动查找匹配的订阅并通过消息队列发送通知。

### 缓存策略
- 航班查询结果缓存5分钟
- 单个航班信息缓存10分钟
- 数据更新时自动失效相关缓存

## 扩展性

- 支持多种消息队列实现 (RabbitMQ/Kafka)
- 支持多种缓存实现 (Redis/Memory)
- 支持多种通知渠道 (Email/SMS/Push)
- 支持微服务架构拆分

## 监控和运维

- 使用Serilog记录结构化日志
- 日志自动发送到Elasticsearch
- 支持应用性能监控和错误追踪
