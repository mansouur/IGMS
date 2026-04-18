using IGMS.Application.Common.Interfaces;

namespace IGMS.API.Middleware;

/// <summary>
/// Resolves the tenant for every incoming request before it reaches any controller.
/// Reads "X-Tenant-Key" header → loads config from JSON file → stores in HttpContext.Items.
/// Must be registered before UseRouting() and UseAuthentication() in Program.cs.
/// </summary>
public class TenantMiddleware
{
    private const string TenantKeyHeader = "X-Tenant-Key";
    private const string TenantContextKey = "TenantContext";

    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantConfigLoader tenantConfigLoader)
    {
        // Skip tenant resolution for health checks and Swagger
        if (IsExemptPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var tenantKey = context.Request.Headers[TenantKeyHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tenantKey))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = $"Missing required header: {TenantKeyHeader}"
            });
            return;
        }

        var tenantContext = await tenantConfigLoader.LoadAsync(tenantKey);

        if (tenantContext is null)
        {
            _logger.LogWarning("Unknown tenant key received: {TenantKey}", tenantKey);
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = $"Tenant '{tenantKey}' not found."
            });
            return;
        }

        context.Items[TenantContextKey] = tenantContext;

        await _next(context);
    }

    private static bool IsExemptPath(PathString path) =>
        path.StartsWithSegments("/health") ||
        path.StartsWithSegments("/swagger") ||
        path.StartsWithSegments("/favicon.ico");
}
