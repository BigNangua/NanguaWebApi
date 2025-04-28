using System.Text;

namespace WebApi.Common
{
    /// <summary>
    /// 请求日志中间件
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 获取请求信息
            var request = context.Request;
            var method = request.Method;
            var path = request.Path;
            var queryString = request.QueryString.ToString();

            // 记录请求方法、路径和查询参数
            _logger.LogInformation($"Request: {method} {path}{queryString}");

            // 如果是 POST 或 PUT 请求，并且请求包含请求体，则记录请求体
            if (method == HttpMethods.Post || method == HttpMethods.Put)
            {
                request.EnableBuffering(); // 启用请求体缓冲
                using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    var body = await reader.ReadToEndAsync();
                    _logger.LogInformation($"Request Body: {body}");
                    request.Body.Position = 0; // 重新设置流的位置，以便其他中间件读取
                }
            }

            // 调用下一个中间件
            await _next(context);
        }
    }
}
