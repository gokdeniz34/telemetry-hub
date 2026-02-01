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

        // 1. Önce Loglama (Console/File)
        logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}",
            traceId);

        // 2. PROFESYONEL EKLEME: Hatayı Telemetri Sistemine (MongoDB) Kaydetme
        // Middleware Singleton olduğu için Scoped olan TelemetryQueue'yu context üzerinden alıyoruz.
        try
        {
            var queue = context.RequestServices.GetRequiredService<TelemetryQueue>();

            var errorTelemetry = TelemetryEvent.Create(
                source: "telemetry-api-internal",
                service: "exception-middleware",
                eventName: "unhandled_exception",
                deviceId: "api-server",
                level: "Critical",
                payload: new Dictionary<string, object>
                {
                    { "TraceId", traceId },
                    { "Exception", exception.GetType().Name },
                    { "Message", exception.Message },
                    { "StackTrace", exception.StackTrace ?? string.Empty },
                    { "Path", context.Request.Path },
                    { "Method", context.Request.Method }
                }
            );

            // Kuyruğa at (Bekleme yapmaz, arka planda worker halleder)
            await queue.Writer.WriteAsync(errorTelemetry);
        }
        catch (Exception telemetryEx)
        {
            // Telemetri kuyruğunda bir sorun olursa ana akışı bozmamak için sadece logla
            logger.LogCritical(telemetryEx, "Could not queue error telemetry.");
        }

        // 3. Kullanıcıya ProblemDetails Yanıtı Dönme
        var problem = new ProblemDetails
        {
            Type = "telemetryhub/internal-error",
            Title = "Unexpected error occurred",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = "An unexpected error occurred. Please contact support.",
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = traceId;
        problem.Extensions["timestamp"] = DateTime.UtcNow;

        context.Response.StatusCode = problem.Status.Value;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem);
    }
}