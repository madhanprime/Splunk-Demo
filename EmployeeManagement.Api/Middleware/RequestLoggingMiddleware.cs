using System.Diagnostics;
using Serilog;

namespace EmployeeManagement.Api.Middleware
{
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
            var stopwatch = Stopwatch.StartNew();
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            try
            {
                await _next(context);
                stopwatch.Stop();

                var statusCode = context.Response.StatusCode;
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                // Log structured data formatted for Splunk indexing
                if (statusCode >= 500)
                {
                    Log.ForContext("Application", "EmployeeApp")
                       .ForContext("Endpoint", requestPath)
                       .ForContext("Method", requestMethod)
                       .ForContext("StatusCode", statusCode)
                       .ForContext("ResponseTimeMs", elapsedMs)
                       .ForContext("ClientIp", clientIp)
                       .ForContext("UserAgent", userAgent)
                       .Error("HTTP {Method} {Endpoint} responded {StatusCode} in {ResponseTimeMs}ms", requestMethod, requestPath, statusCode, elapsedMs);
                }
                else if (statusCode >= 400)
                {
                    Log.ForContext("Application", "EmployeeApp")
                       .ForContext("Endpoint", requestPath)
                       .ForContext("Method", requestMethod)
                       .ForContext("StatusCode", statusCode)
                       .ForContext("ResponseTimeMs", elapsedMs)
                       .ForContext("ClientIp", clientIp)
                       .ForContext("UserAgent", userAgent)
                       .Warning("HTTP {Method} {Endpoint} responded {StatusCode} in {ResponseTimeMs}ms", requestMethod, requestPath, statusCode, elapsedMs);
                }
                else
                {
                    Log.ForContext("Application", "EmployeeApp")
                       .ForContext("Endpoint", requestPath)
                       .ForContext("Method", requestMethod)
                       .ForContext("StatusCode", statusCode)
                       .ForContext("ResponseTimeMs", elapsedMs)
                       .ForContext("ClientIp", clientIp)
                       .ForContext("UserAgent", userAgent)
                       .Information("HTTP {Method} {Endpoint} responded {StatusCode} in {ResponseTimeMs}ms", requestMethod, requestPath, statusCode, elapsedMs);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                Log.ForContext("Application", "EmployeeApp")
                   .ForContext("Endpoint", requestPath)
                   .ForContext("Method", requestMethod)
                   .ForContext("StatusCode", 500)
                   .ForContext("ResponseTimeMs", elapsedMs)
                   .ForContext("ClientIp", clientIp)
                   .ForContext("UserAgent", userAgent)
                   .ForContext("ExceptionType", ex.GetType().Name)
                   .Error(ex, "Unhandled Exception on HTTP {Method} {Endpoint} after {ResponseTimeMs}ms: {Message}", requestMethod, requestPath, elapsedMs, ex.Message);

                throw; // Rethrow for global error handling
            }
        }
    }
}
