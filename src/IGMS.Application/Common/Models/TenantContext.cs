namespace IGMS.Application.Common.Models;

/// <summary>
/// Represents a resolved tenant for the current HTTP request.
/// Loaded once per request by TenantMiddleware and stored in HttpContext.Items.
/// </summary>
public class TenantContext
{
    public string TenantKey { get; init; } = string.Empty;
    public string ConnectionString { get; init; } = string.Empty;
    public TenantOrganization Organization { get; init; } = new();
    public TenantLocalization Localization { get; init; } = new();
    public TenantHierarchy OrgHierarchy { get; init; } = new();
    public TenantBranding Branding { get; init; } = new();
    public TenantAuthentication Authentication { get; init; } = new();
    public TenantSmtp Smtp { get; init; } = new();
    public TenantEscalationConfig Escalation { get; init; } = new();
    public TenantComplianceReporting ComplianceReporting { get; init; } = new();
}

public class TenantOrganization
{
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string LogoPath { get; init; } = string.Empty;
}

public class TenantLocalization
{
    public string DefaultLanguage { get; init; } = "ar";
    public List<string> SupportedLanguages { get; init; } = ["ar", "en"];
    public string Currency { get; init; } = "AED";
    public string TimeZone { get; init; } = "Arab Standard Time";
}

public class TenantHierarchy
{
    public Dictionary<string, List<string>> Levels { get; init; } = new();
}

public class TenantBranding
{
    public string PrimaryColor { get; init; } = "#1B4F72";
    public string SecondaryColor { get; init; } = "#2E86C1";
}

public class TenantAuthentication
{
    public string Mode { get; init; } = "Local"; // "AD" or "Local"
    public string? Domain { get; init; }
}

public class TenantSmtp
{
    public string Host        { get; init; } = string.Empty;
    public int    Port        { get; init; } = 587;
    public string Username    { get; init; } = string.Empty;
    public string Password    { get; init; } = string.Empty;
    public string FromEmail   { get; init; } = string.Empty;
    public string FromName    { get; init; } = string.Empty;
    public bool   UseSsl      { get; init; } = false;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(FromEmail);
}

/// <summary>
/// إعدادات تصعيد المهام المتأخرة لكل مستأجر.
/// </summary>
public class TenantEscalationConfig
{
    /// <summary>تفعيل التصعيد التلقائي.</summary>
    public bool Enabled { get; init; } = true;
    /// <summary>عدد أيام التأخير قبل إرسال التذكير للمكلَّف (L1).</summary>
    public int ReminderAfterDays { get; init; } = 1;
    /// <summary>عدد أيام التأخير قبل التصعيد لمدير القسم (L2).</summary>
    public int EscalateAfterDays { get; init; } = 4;
}

/// <summary>
/// إعدادات تقرير الامتثال الدوري لكل مستأجر.
/// يُرسل في اليوم المحدد من كل شهر إلى بريد مسؤول الامتثال.
/// </summary>
public class TenantComplianceReporting
{
    /// <summary>تفعيل الإرسال التلقائي.</summary>
    public bool   Enabled        { get; init; } = false;
    /// <summary>اليوم من الشهر لإرسال التقرير (1–28).</summary>
    public int    DayOfMonth     { get; init; } = 1;
    /// <summary>الفترة: "Monthly" أو "Quarterly" (Q1,Q2,Q3,Q4).</summary>
    public string Schedule       { get; init; } = "Monthly";
    public string RecipientEmail { get; init; } = string.Empty;
    public string RecipientName  { get; init; } = string.Empty;
}
