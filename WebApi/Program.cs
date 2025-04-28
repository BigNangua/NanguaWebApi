using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Reflection;
using WebApi.Common;

var builder = WebApplication.CreateBuilder(args);

// 其他服务的注册
builder.Services.AddControllers();

// 配置 Serilog
var logFolder = $"logs/{DateTime.Now:yyyy-MM-dd}";
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Filter.ByExcluding(@"SourceContext = 'Microsoft.AspNetCore.Hosting.Diagnostics' and 
                          (RequestPath like '/swagger%' or 
                           RequestPath like '/_framework%' or 
                           RequestPath like '/_vs%' or 
                           RequestPath like '/browserLink%')")
    .WriteTo.File(
        path: "logs/info.log",
        restrictedToMinimumLevel: LogEventLevel.Information,
        rollingInterval: RollingInterval.Day, // 按天滚动
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.File(
        path: "logs/error.log",
        restrictedToMinimumLevel: LogEventLevel.Error,
        rollingInterval: RollingInterval.Day, // 按天滚动
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();

// 注册服务 扫描命名空间 WebApi.Services 下的所有类
var serviceTypes = typeof(Program).Assembly.GetTypes()
    .Where(t => t.IsClass &&
                !t.IsAbstract &&
                t.Namespace == "WebApi.Service");

foreach (var type in serviceTypes)
{
    var interfaceType = type.GetInterfaces().FirstOrDefault();
    if (interfaceType != null)
    {
        builder.Services.AddScoped(interfaceType, type);
    }
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Description = " ", Version = "v1" });

    // 添加 XML 文档注释的路径
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// 注册自定义中间件
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
});

app.UseAuthorization();
app.MapControllers();

Console.WriteLine("正在运行中！");
app.Run();