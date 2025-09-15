using System.Reflection;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace ZackPlay.Extensions;

public static class ScalarExtensions
{
    // 为 OpenAPI 注入 Bearer 安全定义，并加载 XML 注释转换器（operation 级）
    public static IServiceCollection AddOpenApiWithAuthAndXmlComments(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            // 文档级：注入 Bearer 安全方案
            options.AddDocumentTransformer(new BearerSecurityDocumentTransformer());

            // Operation 级：从 XML 注释中补充 Summary/Description（基础示例）
            options.AddOperationTransformer(new XmlCommentsOperationTransformer());
        });

        return services;
    }

    // 配置 Scalar UI：预选 Bearer，并提供 token 输入入口
    public static IEndpointConventionBuilder MapScalarWithAuth(this IEndpointRouteBuilder app)
    {
        return app.MapScalarApiReference((scalar, _ctx) =>
        {
            scalar
                .WithTitle("Flight Booking API")
                .WithTheme(ScalarTheme.BluePlanet)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .AddPreferredSecuritySchemes("BearerAuth")
                .AddHttpAuthentication("BearerAuth", auth =>
                {
                    // 默认留空，用户可在 UI 里粘贴 Token
                    auth.Token = string.Empty;
                })
                .WithPersistentAuthentication();
        });
    }
}

// 简单的 XML 注释读取器：仅将 <summary>/<remarks> 映射到 Operation.Summary/Description
internal sealed class XmlCommentsOperationTransformer : IOpenApiOperationTransformer
{
    private readonly Lazy<Dictionary<string, (string? summary, string? remarks)>> _xmlIndex;

    public XmlCommentsOperationTransformer()
    {
        _xmlIndex = new Lazy<Dictionary<string, (string?, string?)>>(LoadXmlComments, isThreadSafe: true);
    }

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        try
        {
            // 生成类似于 Swashbuckle 的成员名：M:Namespace.Controller.Action(System.String)
            var actionMethod = (context.Description.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo
                ?? context.Description.ActionDescriptor?.EndpointMetadata
                    .OfType<MethodInfo>()
                    .FirstOrDefault();

            if (actionMethod is null)
            {
                return Task.CompletedTask;
            }

            var memberName = GetMemberName(actionMethod);
            if (_xmlIndex.Value.TryGetValue(memberName, out var doc))
            {
                // 不修改 Summary，避免影响左侧导航/标题；仅把 XML 注释追加到 Description
                var parts = new List<string>(2);
                if (!string.IsNullOrWhiteSpace(doc.summary)) parts.Add(doc.summary!);
                if (!string.IsNullOrWhiteSpace(doc.remarks)) parts.Add(doc.remarks!);

                var xmlBlock = string.Join("\n\n", parts);
                if (!string.IsNullOrWhiteSpace(xmlBlock))
                {
                    if (string.IsNullOrWhiteSpace(operation.Description))
                    {
                        operation.Description = xmlBlock;
                    }
                    else if (!operation.Description.Contains(xmlBlock, StringComparison.OrdinalIgnoreCase))
                    {
                        operation.Description = operation.Description.TrimEnd() + "\n\n" + xmlBlock;
                    }
                }
            }
        }
        catch
        {
            // 忽略 XML 解析失败，避免影响运行
        }

        return Task.CompletedTask;
    }

    private static string GetMemberName(MethodInfo method)
    {
        var parameters = method.GetParameters();
        var paramList = parameters.Length == 0
            ? string.Empty
            : "(" + string.Join(",", parameters.Select(p => p.ParameterType.FullName)) + ")";
        return $"M:{method.DeclaringType!.FullName}.{method.Name}{paramList}";
    }

    private static Dictionary<string, (string? summary, string? remarks)> LoadXmlComments()
    {
        var result = new Dictionary<string, (string?, string?)>();
        try
        {
            var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var xmlPath = Path.ChangeExtension(asm.Location, ".xml");
            if (!File.Exists(xmlPath))
            {
                return result;
            }

            var doc = new System.Xml.XmlDocument();
            doc.Load(xmlPath);
            var members = doc.SelectNodes("/doc/members/member");
            if (members is null) return result;

            foreach (System.Xml.XmlNode member in members)
            {
                var nameAttr = member.Attributes?["name"]?.Value;
                if (string.IsNullOrWhiteSpace(nameAttr)) continue;

                var summary = member.SelectSingleNode("summary")?.InnerText?.Trim();
                var remarks = member.SelectSingleNode("remarks")?.InnerText?.Trim();
                result[nameAttr] = (summary, remarks);
            }
        }
        catch
        {
            // ignore
        }
        return result;
    }
}

internal sealed class BearerSecurityDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        const string schemeName = "BearerAuth";
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

        if (!document.Components.SecuritySchemes.ContainsKey(schemeName))
        {
            document.Components.SecuritySchemes[schemeName] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "输入 JWT Bearer Token，例如: eyJhbGciOi..."
            };
        }

        // 全局安全要求（如不需要可移除）
        document.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();
        var schemeRef = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = schemeName
            }
        };

        if (!document.SecurityRequirements.Any(r => r.ContainsKey(schemeRef)))
        {
            document.SecurityRequirements.Add(new OpenApiSecurityRequirement
            {
                [schemeRef] = new List<string>()
            });
        }

        return Task.CompletedTask;
    }
}
