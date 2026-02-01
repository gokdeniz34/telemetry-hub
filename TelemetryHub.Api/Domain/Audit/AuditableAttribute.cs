using System;

namespace TelemetryHub.Api.Domain.Audit;

[AttributeUsage(AttributeTargets.Method)]
public sealed class AuditableAttribute : Attribute
{
    public string ActionName { get; }

    public AuditableAttribute(string actionName)
    {
        ActionName = actionName;
    }
}
