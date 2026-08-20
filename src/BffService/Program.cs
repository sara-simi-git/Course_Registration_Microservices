using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;

var builder = WebApplication.CreateBuilder(args);
builder.UseServiceObservability();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("registration", client => client.BaseAddress = new Uri("http://registration.api:8080/"));
builder.Services.AddHttpClient("catalog", client => client.BaseAddress = new Uri("http://catalog.lb:80/"));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCorrelationAndLogging();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "BFF Service" }));
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.Run();
