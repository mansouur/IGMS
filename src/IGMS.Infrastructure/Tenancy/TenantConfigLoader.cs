using System.Text.Json;
using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace IGMS.Infrastructure.Tenancy;

/// <summary>
/// Loads tenant configuration from /tenants/{tenantKey}.json files.
/// JSON is deserialized into TenantContext and cached in memory per key.
/// </summary>
public class TenantConfigLoader : ITenantConfigLoader
{
    private readonly string _tenantsDirectory;
    private readonly ILogger<TenantConfigLoader> _logger;

    // Cache: stores (TenantContext, file last-write time) per key.
    // Re-reads from disk if the JSON file was modified after last load.
    private readonly Dictionary<string, (TenantContext Context, DateTime LoadedAt)> _cache = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TenantConfigLoader(string tenantsDirectory, ILogger<TenantConfigLoader> logger)
    {
        _tenantsDirectory = tenantsDirectory;
        _logger = logger;
    }

    public async Task<TenantContext?> LoadAsync(string tenantKey)
    {
        var filePath = Path.Combine(_tenantsDirectory, $"{tenantKey}.json");

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Tenant config not found for key: {TenantKey}", tenantKey);
            return null;
        }

        // Return cached entry only if the file hasn't been modified since last load
        var fileModified = File.GetLastWriteTimeUtc(filePath);
        if (_cache.TryGetValue(tenantKey, out var cached) && cached.LoadedAt >= fileModified)
            return cached.Context;

        try
        {
            var json   = await File.ReadAllTextAsync(filePath);
            var config = JsonSerializer.Deserialize<TenantConfigFile>(json, JsonOptions);

            if (config is null)
            {
                _logger.LogError("Failed to deserialize tenant config for key: {TenantKey}", tenantKey);
                return null;
            }

            var context = MapToTenantContext(tenantKey, config);
            _cache[tenantKey] = (context, DateTime.UtcNow);

            _logger.LogInformation("Tenant config loaded/reloaded for: {TenantKey}", tenantKey);
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tenant config for key: {TenantKey}", tenantKey);
            return null;
        }
    }

    private static TenantContext MapToTenantContext(string tenantKey, TenantConfigFile config)
    {
        var connectionString = BuildConnectionString(config.Database);

        return new TenantContext
        {
            TenantKey = tenantKey,
            ConnectionString = connectionString,
            Organization = new TenantOrganization
            {
                NameAr = config.Organization.NameAr,
                NameEn = config.Organization.NameEn,
                Country = config.Organization.Country,
                LogoPath = config.Organization.LogoPath
            },
            Localization = new TenantLocalization
            {
                DefaultLanguage = config.Localization.DefaultLanguage,
                SupportedLanguages = config.Localization.SupportedLanguages,
                Currency = config.Localization.Currency,
                TimeZone = config.Localization.TimeZone
            },
            OrgHierarchy = new TenantHierarchy
            {
                Levels = config.OrgHierarchy.Levels
            },
            Branding = new TenantBranding
            {
                PrimaryColor = config.Branding.PrimaryColor,
                SecondaryColor = config.Branding.SecondaryColor
            },
            Authentication = new TenantAuthentication
            {
                Mode = config.Authentication.Mode,
                Domain = config.Authentication.Domain
            },
            Smtp = new TenantSmtp
            {
                Host      = config.Smtp.Host,
                Port      = config.Smtp.Port,
                Username  = config.Smtp.Username,
                Password  = config.Smtp.Password,
                FromEmail = config.Smtp.FromEmail,
                FromName  = config.Smtp.FromName,
                UseSsl    = config.Smtp.UseSsl,
            }
        };
    }

    private static string BuildConnectionString(TenantDatabaseConfig db)
    {
        return db.AuthType.Equals("WindowsAuth", StringComparison.OrdinalIgnoreCase)
            ? $"Server={db.Server};Database={db.Name};Trusted_Connection=True;TrustServerCertificate=True;"
            : $"Server={db.Server};Database={db.Name};User Id={db.UserId};Password={db.Password};TrustServerCertificate=True;";
    }
}

// Internal deserialization models – mirrors uae-sport.json structure
internal class TenantConfigFile
{
    public TenantDatabaseConfig Database { get; set; } = new();
    public TenantOrgConfig Organization { get; set; } = new();
    public TenantLocalizationConfig Localization { get; set; } = new();
    public TenantHierarchyConfig OrgHierarchy { get; set; } = new();
    public TenantBrandingConfig Branding { get; set; } = new();
    public TenantAuthConfig Authentication { get; set; } = new();
    public TenantSmtpConfig Smtp { get; set; } = new();
}

internal class TenantDatabaseConfig
{
    public string Name { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public string AuthType { get; set; } = "WindowsAuth";
    public string? UserId { get; set; }
    public string? Password { get; set; }
}

internal class TenantOrgConfig
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string LogoPath { get; set; } = string.Empty;
}

internal class TenantLocalizationConfig
{
    public string DefaultLanguage { get; set; } = "ar";
    public List<string> SupportedLanguages { get; set; } = ["ar", "en"];
    public string Currency { get; set; } = "AED";
    public string TimeZone { get; set; } = "Arab Standard Time";
}

internal class TenantHierarchyConfig
{
    public Dictionary<string, List<string>> Levels { get; set; } = new();
}

internal class TenantBrandingConfig
{
    public string PrimaryColor { get; set; } = "#1B4F72";
    public string SecondaryColor { get; set; } = "#2E86C1";
}

internal class TenantAuthConfig
{
    public string Mode { get; set; } = "Local";
    public string? Domain { get; set; }
}

internal class TenantSmtpConfig
{
    public string Host      { get; set; } = string.Empty;
    public int    Port      { get; set; } = 587;
    public string Username  { get; set; } = string.Empty;
    public string Password  { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName  { get; set; } = string.Empty;
    public bool   UseSsl    { get; set; } = false;
}
