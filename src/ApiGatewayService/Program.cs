using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Shared;

var builder = WebApplication.CreateBuilder(args);
builder.UseServiceObservability();

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Api Gateway API",
        Version = "v1"
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api Gateway API");
});

app.UseCorrelationAndLogging();
app.UseRouting();

app.MapGet("/", () => Results.Ok(new { service = "Api Gateway", status = "running" }))
    .WithName("GetGatewayStatus");
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Api Gateway" }));
app.MapReverseProxy();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.Run();
