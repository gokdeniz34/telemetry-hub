using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using TelemetryHub.Api;
using TelemetryHub.Api.Application.Telemetry.Handlers;
using TelemetryHub.Api.BackgroundJobs;
using TelemetryHub.Api.Domain.Audit.Repositories;
using TelemetryHub.Api.Domain.Telemetry.Repositories;
using TelemetryHub.Api.Filters;
using TelemetryHub.Api.Infrastructure.Audit.Repositories;
using TelemetryHub.Api.Infrastructure.Common;
using TelemetryHub.Api.Infrastructure.Mongo;
using TelemetryHub.Api.Infrastructure.Mongo.Repositories;
using TelemetryHub.Api.Infrastructure.MySql;
using TelemetryHub.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

var objectSerializer = new ObjectSerializer(type =>
    ObjectSerializer.DefaultAllowedTypes(type) ||
    (type.FullName != null && type.FullName.StartsWith("System.Text.Json")));
BsonSerializer.RegisterSerializer(objectSerializer);

//
// MongoDB
//
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("Mongo"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped<MongoContext>();

//
// MySQL / EF Core
//
builder.Services.AddDbContext<TelemetryHubDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("TelemetryHubDb"),
        new MySqlServerVersion(new Version(8, 0, 32))
    ), ServiceLifetime.Scoped);


//
// Filters
//
builder.Services.AddScoped<AuditActionFilter>();

//
// Handlers
//
builder.Services.AddScoped<TelemetryIngestHandler>();
builder.Services.AddScoped<TelemetryQueryHandler>();


builder.Services.AddSingleton<TelemetryQueue>(); // Önemli: Singleton olmalı
builder.Services.AddHostedService<TelemetryBackgroundWorker>();
builder.Services.AddHostedService<TelemetryAggregationJob>();
builder.Services.AddScoped<ITelemetryRepository, MongoTelemetryRepository>();
builder.Services.AddScoped<IAuditLogRepository, MySqlAuditLogRepository>();

//
// Controllers
//
builder.Services.AddControllers(options =>
{
    // options.Filters.Add<AuditActionFilter>();
    options.Filters.Add<ValidationFilter>();
});

//
// Swagger
//
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//
// Middleware
//
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "TelemetryHub API";
        c.RoutePrefix = "swagger"; // explicit
    });
}

app.UseHttpsRedirection();
app.MapControllers();

//
// Health check
//
app.MapGet("/health", () => Results.Ok("TelemetryHub alive"));

app.Run();
