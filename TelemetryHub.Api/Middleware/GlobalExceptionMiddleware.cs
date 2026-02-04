using System.Net;
using Microsoft.AspNetCore.Mvc;
using TelemetryHub.Api.Domain.Telemetry.Entities;
using TelemetryHub.Api.Infrastructure.Common;

namespace TelemetryHub.Api.Middleware;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        // 1. ELK İÇİN STRUCTURED LOGGING
        // Serilog sayesinde bu veriler Elasticsearch'te filtrelenebilir alanlar olur.
        logger.LogError(exception,
            "Unhandled exception. TraceId: {TraceId}, Path: {Path}, Method: {Method}, Message: {ErrorMessage}",
            traceId, context.Request.Path, context.Request.Method, exception.Message);

        // 2. İÇ TELEMETRİ (MONGODB)
        try
        {
            // Middleware Singleton olduğu için Scoped servisleri request üzerinden alıyoruz.
            var queue = context.RequestServices.GetRequiredService<TelemetryQueue>();

            var errorTelemetry = TelemetryEvent.Create(
                source: "telemetry-api-internal",
                service: "exception-middleware",
                eventName: "unhandled_exception",
                deviceId: "api-server",
                level: "Critical",
                durationMs: 0,
                payload: new Dictionary<string, object>
                {
                    { "TraceId", traceId },
                    { "Exception", exception.GetType().Name },
                    { "Message", exception.Message },
                    { "Path", context.Request.Path }
                }
            );

            await queue.Writer.WriteAsync(errorTelemetry);
        }
        catch (Exception telemetryEx)
        {
            // Eğer telemetri kuyruğu da çökerse, ELK'ya kritik bir log daha düşüyoruz.
            logger.LogCritical(telemetryEx, "Critical: Could not queue error telemetry to MongoDB.");
        }

        // 3. KULLANICIYA DÖNÜLECEK HATA FORMATI (RFC 7807)
        var problem = new ProblemDetails
        {
            Type = "telemetryhub/internal-error",
            Title = "An unexpected error occurred",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = "Something went wrong on our end. Use the traceId for support.",
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = traceId;
        problem.Extensions["timestamp"] = DateTime.UtcNow;

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem);
    }
}