using NotificationService.Consumers;
using NotificationService.Data;
using NotificationService.Interfaces.IRepositories;
using NotificationService.Interfaces.IServices;
using NotificationService.Repositories;
using Shared;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.UseServiceObservability();

var redisConn = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));
builder.Services.AddSingleton<RedisContext>();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>();

builder.Services.AddHostedService<OrderFinalConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NotificationService v1");
    c.RoutePrefix = "swagger";
});
app.UseCorrelationAndLogging();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Notification Service" }));
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.Run();
