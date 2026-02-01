using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace TelemetryHub.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var traceId = context.TraceIdentifier;

        _logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}",
            traceId);

        var problem = new ProblemDetails
        {
            Type = "telemetryhub/internal-error",
            Title = "Unexpected error occurred",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = "An unexpected error occurred. Please contact support.",
            Instance = context.Request.Path
        };

        // ⭐ Hibrit yaklaşım burada
        problem.Extensions["traceId"] = traceId;
        problem.Extensions["timestamp"] = DateTime.UtcNow;

        context.Response.StatusCode = problem.Status.Value;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem);
    }
}
