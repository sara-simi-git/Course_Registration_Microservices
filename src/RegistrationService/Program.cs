using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RegistrationService.Data;
using RegistrationService.Events;
using RegistrationService.Interfaces;
using RegistrationService.Repositories;
using RegistrationService.Services;
using Shared;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
builder.UseServiceObservability();

// Dedicated database per service
builder.Services.AddDbContext<LearnBudgetContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("RegistrationConnection")));

// Register services and repositories
builder.Services.AddScoped<RegistrationService.Interfaces.ICourseRegistrationRepository, RegistrationService.Repositories.CourseRegistrationRepository>();
builder.Services.AddScoped<RegistrationService.Interfaces.IRegistrationService, RegistrationService.Services.CourseRegistrationService>();

// RabbitMQ event bus
builder.Services.AddSingleton<IEventBus, RabbitMqEventBus>();

// Redis connection for idempotency
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var conn = cfg["Redis:Connection"] ?? "localhost:6379";
    return StackExchange.Redis.ConnectionMultiplexer.Connect(conn);
});

// Hosted consumer to receive inventory results
builder.Services.AddHostedService<RegistrationService.Consumers.InventoryResultConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LearnBudgetContext>(); 
        context.Database.Migrate(); // השורה הזו מריצה את המיגרציות אוטומטית!
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error applying migrations: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCorrelationAndLogging();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Registration Service" }));
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();
