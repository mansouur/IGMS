using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Infrastructure.Auth;
using IGMS.Infrastructure.Auth.Strategies;
using IGMS.Infrastructure.Jobs;
using IGMS.Infrastructure.Persistence;
using IGMS.Infrastructure.Services;
using IGMS.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IGMS.Infrastructure;

/// <summary>
/// Registers all Infrastructure services into the DI container.
/// Called once from Program.cs – keeps Program.cs clean.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string tenantsDirectory)
    {
        services.AddHttpContextAccessor();

        RegisterSession(services, configuration);
        RegisterTenancy(services, tenantsDirectory);
        RegisterDatabase(services);
        RegisterAuthStrategies(services);
        RegisterUaePass(services);
        RegisterServices(services);
        RegisterBackgroundJobs(services, tenantsDirectory);

        return services;
    }

    private static void RegisterSession(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration["Redis:ConnectionString"];

        if (!string.IsNullOrEmpty(redisConnection))
        {
            // Production: Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "igms:";
            });
        }
        else
        {
            // Development: in-memory (no Redis needed)
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ISessionService, SessionService>();
    }

    private static void RegisterTenancy(IServiceCollection services, string tenantsDirectory)
    {
        services.AddSingleton<ITenantConfigLoader>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<TenantConfigLoader>>();
            return new TenantConfigLoader(tenantsDirectory, logger);
        });

        services.AddScoped<TenantContext>(sp =>
        {
            var http = sp.GetRequiredService<IHttpContextAccessor>();

            // At design-time (EF migrations), HttpContext is null.
            // IDesignTimeDbContextFactory handles the real connection in that case.
            // In production, TenantMiddleware always populates this before DI resolves it.
            return http.HttpContext?.Items["TenantContext"] as TenantContext
                ?? new TenantContext();
        });
    }

    private static void RegisterDatabase(IServiceCollection services)
    {
        services.AddDbContext<TenantDbContext>((sp, options) =>
        {
            var tenant = sp.GetRequiredService<TenantContext>();
            options.UseSqlServer(
                tenant.ConnectionString,
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3));
        });

        services.AddScoped<ITenantDbContext>(sp => sp.GetRequiredService<TenantDbContext>());
    }

    private static void RegisterAuthStrategies(IServiceCollection services)
    {
        // All strategies registered – controller picks the right one based on tenant config
        services.AddScoped<IAuthStrategy, LocalAuthStrategy>();
        services.AddScoped<IAuthStrategy, AdAuthStrategy>();

        services.AddSingleton<IJwtService, JwtService>();
    }

    private static void RegisterUaePass(IServiceCollection services)
    {
        services.AddHttpClient<IUaePassService, UaePassService>();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IEmailService, MailKitEmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRaciService, RaciService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPolicyService, PolicyService>();
        services.AddScoped<IRiskService, RiskService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IKpiService, KpiService>();
        services.AddScoped<IKpiRecordService, KpiRecordService>();
        services.AddScoped<IComplianceMappingService, ComplianceMappingService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IAcknowledgmentService, AcknowledgmentService>();
        services.AddScoped<IExecutivePdfService, ExecutivePdfService>();
        services.AddScoped<IControlTestService, ControlTestService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<IRegulatoryService, RegulatoryService>();
        services.AddScoped<IAssessmentService, AssessmentService>();
        services.AddScoped<IIncidentService, IncidentService>();
    }

    private static void RegisterBackgroundJobs(IServiceCollection services, string tenantsDirectory)
    {
        services.AddHostedService(sp => new PolicyExpiryNotificationJob(
            sp.GetRequiredService<ITenantConfigLoader>(),
            sp.GetRequiredService<ILoggerFactory>(),
            tenantsDirectory));

        services.AddHostedService(sp => new EscalationJob(
            sp.GetRequiredService<ITenantConfigLoader>(),
            sp.GetRequiredService<ILoggerFactory>(),
            tenantsDirectory));

        services.AddHostedService(sp => new ComplianceReportJob(
            sp.GetRequiredService<ITenantConfigLoader>(),
            sp.GetRequiredService<ILoggerFactory>(),
            tenantsDirectory));
    }
}
