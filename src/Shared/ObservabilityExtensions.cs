using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.IO;
using System.Linq;

namespace Shared;

public static class ObservabilityExtensions
{
    private static string ResolveLogsDirectory(IHostEnvironment environment)
    {
        foreach (var candidateRoot in new[]
                 {
                     environment.ContentRootPath,
                     AppContext.BaseDirectory,
                     Directory.GetCurrentDirectory()
                 }.Where(path => !string.IsNullOrWhiteSpace(path))
                 .Select(path => Path.GetFullPath(path!))
                 .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var serviceRoot = FindServiceRoot(candidateRoot);
            if (!string.IsNullOrWhiteSpace(serviceRoot))
            {
                var logsPath = Path.GetFullPath(Path.Combine(serviceRoot, "logs"));
                Directory.CreateDirectory(logsPath);
                return logsPath;
            }
        }

        var fallbackPath = Path.GetFullPath(Path.Combine(environment.ContentRootPath ?? AppContext.BaseDirectory, "logs"));
        Directory.CreateDirectory(fallbackPath);
        return fallbackPath;
    }

    private static string? FindServiceRoot(string startPath)
    {
        var currentDirectory = new DirectoryInfo(startPath);
        while (currentDirectory != null)
        {
            var hasProjectFile = currentDirectory.GetFiles("*.csproj").Length > 0;
            var hasAppSettings = File.Exists(Path.Combine(currentDirectory.FullName, "appsettings.json"));
            if (hasProjectFile || hasAppSettings)
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }

    public static IHostApplicationBuilder UseServiceObservability(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSerilog((services, loggerConfiguration) =>
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            var environment = services.GetRequiredService<IHostEnvironment>();

            var logsPath = ResolveLogsDirectory(environment);
            var serviceName = environment.ApplicationName?.Replace(" ", "-") ?? "service";
            var logFilePath = Path.Combine(logsPath, $"{serviceName}.log");

            loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .Filter.ByExcluding(logEvent =>
                {
                    if (logEvent.Properties.TryGetValue("RequestPath", out var requestPathValue)
                        && requestPathValue is ScalarValue scalarValue
                        && scalarValue.Value is string requestPath
                        && requestPath.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    return false;
                })
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", serviceName)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {ServiceName} {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {ServiceName} {Message:lj} {Properties:j}{NewLine}{Exception}");

            var seqUrl = configuration["Seq:ServerUrl"];
            if (!string.IsNullOrWhiteSpace(seqUrl))
            {
                loggerConfiguration.WriteTo.Seq(seqUrl);
            }
        });

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHealthChecks();
        return builder;
    }

    public static IApplicationBuilder UseCorrelationAndLogging(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                ?? context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                ?? Guid.NewGuid().ToString("N");

            context.Items["CorrelationId"] = correlationId;
            context.Request.Headers["X-Correlation-ID"] = correlationId;
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await next();
            }
        });

        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, elapsed, exception) =>
            {
                if (httpContext.Request.Path.StartsWithSegments("/health"))
                {
                    return LogEventLevel.Debug;
                }

                return exception != null || httpContext.Response.StatusCode >= 500
                    ? LogEventLevel.Error
                    : LogEventLevel.Information;
            };

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("CorrelationId", httpContext.Items["CorrelationId"]?.ToString());
            };
        });

        return app;
    }
}
