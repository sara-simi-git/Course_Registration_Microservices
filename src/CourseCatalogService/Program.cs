using CourseCatalogService.Data;
using CourseCatalogService.Interfaces;
using CourseCatalogService.Repositories;
using CourseCatalogService.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared;

var builder = WebApplication.CreateBuilder(args);
builder.UseServiceObservability();

// שליפת הגדרות חיבור לצורך רישום ה-Health Checks
var connectionString = builder.Configuration.GetConnectionString("MongoConnection") 
                       ?? "mongodb://localhost:27017";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. רישום שכבת התשתית (Infrastructure)
builder.Services.AddSingleton<MongoDbContext>();

// 2. רישום שכבת הגישה לנתונים (Data Access Layer) אל מול הממשקים שלהן
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILecturerRepository, LecturerRepository>();

// 3. רישום שכבת הלוגיקה העסקית (Business Logic Layer) אל מול הממשקים שלהן
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILecturerService, LecturerService>();

// 4. הגדרת מערך ה-Health Checks של המיקרוסרביס
builder.Services.AddHealthChecks()
    .AddMongoDb(
        mongodbConnectionString: connectionString,
        name: "mongodb-check",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready" }
    );

// Redis for cache-aside
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var conn = cfg["Redis:Connection"] ?? "localhost:6379";
    return StackExchange.Redis.ConnectionMultiplexer.Connect(conn);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCorrelationAndLogging();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Service-Instance"] = Environment.MachineName;
    await next();
});
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Course Catalog Service" }));

// 5. חשיפת נתיבי בדיקת התקינות עבור Docker / Kubernetes
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false // מחזיר תמיד OK כל עוד האפליקציה מסוגלת לרוץ
});

app.Run();