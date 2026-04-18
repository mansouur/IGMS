using IGMS.API.Extensions;
using IGMS.API.Middleware;
using IGMS.Infrastructure;
using IGMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ── Services ────────────────────────────────────────────────────────────────

builder.Services.AddControllers();

// Swagger with JWT support
builder.Services.AddSwaggerWithJwt();

// JWT Bearer authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Infrastructure: Tenancy, DbContext, Auth, CurrentUser
var tenantsDirectory = Path.Combine(builder.Environment.ContentRootPath, "..", "..", "tenants");
builder.Services.AddInfrastructure(builder.Configuration, tenantsDirectory);

// CORS – allow React dev server in development
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDevPolicy", policy =>
        policy.WithOrigins("http://localhost:5173") // Vite default port
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddHealthChecks();

// ── Rate Limiting ────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Auth endpoints: 10 requests per minute per IP (brute-force protection)
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit          = 10,
                Window               = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit           = 0,
            }));

    // General API: 200 requests per minute per IP
    options.AddPolicy("api", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit          = 200,
                Window               = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit           = 5,
            }));

    // Export endpoints: 5 requests per minute (heavy Excel/PDF generation)
    options.AddPolicy("export", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit          = 5,
                Window               = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit           = 0,
            }));

    // Response when limit exceeded
    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            """{"isSuccess":false,"message":"تجاوزت الحد المسموح من الطلبات. حاول مجدداً بعد دقيقة."}""", ct);
    };
});

// ── Pipeline ─────────────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();
}

app.UseCors("ReactDevPolicy");

app.UseRateLimiter();

app.UseHttpsRedirection();

// Must run before authentication – resolves tenant from X-Tenant-Key header
app.UseMiddleware<TenantMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// ── Dev seed (بيانات تجريبية – لا تشتغل في production) ──────────────────────
if (app.Environment.IsDevelopment())
{
    var devTenantKey = app.Configuration["DevSeed:TenantKey"] ?? "uae-sport";
    var seedLogger   = app.Services.GetRequiredService<ILogger<Program>>();
    await DevDataSeeder.SeedAsync(app.Services, devTenantKey, seedLogger);
}

app.Run();
