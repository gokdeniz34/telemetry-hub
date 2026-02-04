using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Serilog;
using Serilog.Sinks.Elasticsearch;
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

// --- 1. SERILOG & ELK YAPILANDIRMASI ---
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TelemetryHub.Api")
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "telemetry-hub-logs-{0:yyyy.MM.dd}",
        NumberOfReplicas = 1,
        NumberOfShards = 2
    })
    .CreateLogger();

builder.Host.UseSerilog();

// --- 2. MONGODB SERIALIZATION ---
var objectSerializer = new ObjectSerializer(type =>
    ObjectSerializer.DefaultAllowedTypes(type) ||
    (type.FullName != null && type.FullName.StartsWith("System.Text.Json")));
BsonSerializer.RegisterSerializer(objectSerializer);

// --- 3. VERİTABANI SERVİSLERİ ---
// MongoDB
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});
builder.Services.AddScoped<MongoContext>();

// MySQL / EF Core
builder.Services.AddDbContext<TelemetryHubDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("TelemetryHubDb"),
        new MySqlServerVersion(new Version(8, 0, 32))
    ), ServiceLifetime.Scoped);

// --- 4. APPLICATION SERVİSLERİ ---
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddScoped<IngestTelemetryHandler>();
builder.Services.AddScoped<TelemetryQueryHandler>();

// --- 5. BACKGROUND JOBS & QUEUE ---
builder.Services.AddSingleton<TelemetryQueue>();
builder.Services.AddHostedService<TelemetryBackgroundWorker>();
builder.Services.AddHostedService<TelemetryAggregationJob>();
builder.Services.AddHostedService<TelemetrySimulator>();

// --- 6. REPOSITORIES ---
builder.Services.AddScoped<ITelemetryRepository, MongoTelemetryRepository>();
builder.Services.AddScoped<IAuditLogRepository, MySqlAuditLogRepository>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- 7. MIDDLEWARE PIPELINE ---
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "TelemetryHub API";
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("TelemetryHub alive"));

try
{
    Log.Information("Starting TelemetryHub API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
