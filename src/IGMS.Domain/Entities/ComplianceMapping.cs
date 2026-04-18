using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

/// <summary>
/// ربط كيان (سياسة أو مخاطرة) بإطار امتثال محدد.
/// EntityType: "Policy" | "Risk"
/// </summary>
public class ComplianceMapping : AuditableEntity
{
    public string             EntityType { get; set; } = string.Empty;
    public int                EntityId   { get; set; }
    public ComplianceFramework Framework  { get; set; }

    /// <summary>رقم البند / المادة داخل الإطار – مثال: "5.2" أو "APO01.04"</summary>
    public string? Clause { get; set; }

    public string? Notes  { get; set; }
}

public enum ComplianceFramework
{
    Iso31000  = 0,   // إدارة المخاطر
    Cobit2019 = 1,   // حوكمة تقنية المعلومات
    UaeNesa   = 2,   // الأمن السيبراني الإماراتي
    Iso27001  = 3,   // أمن المعلومات
    NiasUae   = 4,   // الإطار الوطني لأمن المعلومات
    Adaa      = 6,   // ديوان المحاسبة – أبوظبي (ADAA)
    Tdra      = 7,   // هيئة تنظيم الاتصالات الرقمية (TDRA)
    UaeIa     = 8,   // الاتحاد للمعلوماتية (UAE IA)
    DubaiSm   = 9,   // نموذج دبي للتميز الحكومي (DSM)
    Custom    = 5,   // إطار مخصص – يُكتب النص في Clause
}
