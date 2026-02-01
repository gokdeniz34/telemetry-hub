using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TelemetryHub.Api;
using TelemetryHub.Api.Infrastructure.Mongo.Repositories;
using TelemetryHub.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<TelemetryRepository>();
builder.Services.AddScoped<AuditActionFilter>();

// Controllers
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditActionFilter>();
});

// Swagger UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("Mongo"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp
        .GetRequiredService<IOptions<MongoSettings>>()
        .Value;

    return new MongoClient(settings.ConnectionString);
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "TelemetryHub API";
        c.RoutePrefix = "swagger"; // default ama net olsun
    });
}

app.UseHttpsRedirection();

app.MapControllers();

// test endpoint (istersen sonra silersin)
app.MapGet("/health", () => Results.Ok("TelemetryHub alive"));

app.Run();