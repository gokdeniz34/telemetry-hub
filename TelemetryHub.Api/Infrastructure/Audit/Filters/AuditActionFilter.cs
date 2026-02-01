using Microsoft.AspNetCore.Mvc.Filters;
using TelemetryHub.Api.Domain.Audit;
using TelemetryHub.Api.Domain.Audit.Entities;
using TelemetryHub.Api.Domain.Audit.Repositories;

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

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Action öncesi zamanı tut (Performans ölçümü için)
        var startTime = DateTime.UtcNow;

        var resultContext = await next();

        var auditable = context.ActionDescriptor.EndpointMetadata
            .OfType<AuditableAttribute>()
            .FirstOrDefault();

        if (auditable is null) return;

        try
        {
            var audit = AuditLog.Create(
                action: auditable.ActionName,
                userId: context.HttpContext.User.Identity?.Name ?? "anonymous",
                traceId: context.HttpContext.TraceIdentifier,
                ipAddress: context.HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            // Veritabanına yazarken CancellationToken'ı Filter'dan geçirmek önemlidir
            await _repository.InsertAsync(audit, context.HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            // Loglama hatası ana akışı (request) asla bozmamalı
            _logger.LogCritical(ex, "Audit sistemi devre dışı! Log yazılamadı.");
        }
    }
}
