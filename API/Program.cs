using ZackPlay.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 配置日志
builder.Host.ConfigureLogging(builder.Configuration);

// ASP.NET Core 基础服务
builder.Services.AddControllers();

// 添加 OpenAPI + Bearer 安全方案 + XML 注释
builder.Services.AddOpenApiWithAuthAndXmlComments();

// 注册所有服务 (Infrastructure + Application + Api)
builder.Services.AddAllServices(builder.Configuration);

var app = builder.Build();

// 初始化数据库
await app.InitializeDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarWithAuth();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();


