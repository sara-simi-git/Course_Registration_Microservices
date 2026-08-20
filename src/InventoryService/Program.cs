using InventoryService.Data;
using InventoryService.Interfaces;
using InventoryService.Repositories;
using InventoryService.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("InventoryConnection")
                       ?? "Host=localhost;Port=5432;Database=InventoryDb;Username=postgres;Password=postgres";

builder.UseServiceObservability();

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService.Services.InventoryService>();

// Redis connection for idempotency
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var conn = cfg["Redis:Connection"] ?? "localhost:6379";
    return StackExchange.Redis.ConnectionMultiplexer.Connect(conn);
});

// Hosted consumer for OrderPlaced events
builder.Services.AddHostedService<InventoryService.Consumers.OrderPlacedConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<InventoryDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error applying migrations: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCorrelationAndLogging();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Inventory Service" }));

app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();
