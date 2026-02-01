using Microsoft.AspNetCore.Mvc.Filters;
using TelemetryHub.Api;
using TelemetryHub.Api.Domain.Audit;

public sealed class AuditActionFilter : IAsyncActionFilter
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<AuditActionFilter> _logger;

    public AuditActionFilter(
        IAuditLogRepository repository,
        ILogger<AuditActionFilter> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        // Action çalışsın
        var resultContext = await next();

        // Attribute var mı?
        var auditable = context.ActionDescriptor.EndpointMetadata
            .OfType<AuditableAttribute>()
            .FirstOrDefault();

        if (auditable is null)
            return;

        try
        {
            var httpContext = context.HttpContext;

            var audit = new AuditLog
            {
                UserId = httpContext.User.Identity?.Name ?? "anonymous",
                Action = auditable.ActionName,
                Path = httpContext.Request.Path,
                HttpMethod = httpContext.Request.Method,
                OccurredAtUtc = DateTime.UtcNow,
                TraceId = httpContext.TraceIdentifier,
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString()
            };

            await _repository.InsertAsync(audit);
        }
        catch (Exception ex)
        {
            // ⚠️ Audit fail → request fail ETMEZ
            _logger.LogError(ex, "Audit logging failed");
        }
    }
}
