namespace IGMS.Application.Common.Models;

// ── Department Ranking ────────────────────────────────────────────────────────

public class DepartmentRankDto
{
    public int    DepartmentId   { get; set; }
    public string DepartmentNameAr { get; set; } = string.Empty;

    /// <summary>الترتيب الحقيقي — مرئي للأدمن فقط</summary>
    public int TrueRank         { get; set; }

    /// <summary>
    /// الترتيب المعروض — الأول يرى نفسه ثانياً (Phantom Target).
    /// مطابق لـ TrueRank للأدمن.
    /// </summary>
    public int DisplayRank      { get; set; }

    public int TotalDepartments { get; set; }

    public decimal OverallScore { get; set; }   // 0–100

    // تفاصيل المحاور
    public decimal TasksScore      { get; set; }   // نسبة إنجاز المهام
    public decimal KpisScore       { get; set; }   // نسبة تحقق المؤشرات
    public decimal RisksScore      { get; set; }   // نسبة معالجة المخاطر
    public decimal PoliciesScore   { get; set; }   // نسبة الإقرار بالسياسات
    public decimal IncidentsScore  { get; set; }   // نسبة حل الحوادث

    public int MemberCount { get; set; }

    /// <summary>هل هذا القسم هو قسم المستخدم الحالي؟</summary>
    public bool IsCurrentUserDept { get; set; }
}

public class DepartmentRankingResponse
{
    /// <summary>
    /// للأدمن: جميع الأقسام مرتبة حسب الترتيب الحقيقي.
    /// لغيره: قسمه فقط.
    /// </summary>
    public List<DepartmentRankDto> Rankings    { get; set; } = [];
    public bool                    IsAdminView { get; set; }
}

// ── Employee Ranking ──────────────────────────────────────────────────────────

public class EmployeeRankDto
{
    public int    UserId         { get; set; }
    public string FullNameAr     { get; set; } = string.Empty;
    public string DepartmentNameAr { get; set; } = string.Empty;
    public int    DepartmentId   { get; set; }

    public int  TrueRank        { get; set; }
    public int  DisplayRank     { get; set; }   // Phantom Target للمركز الأول
    public int  TotalInScope    { get; set; }   // إجمالي من في نطاق العرض

    public decimal OverallScore { get; set; }   // 0–100

    // تفاصيل المحاور
    public decimal TasksScore        { get; set; }   // إنجاز المهام
    public decimal PoliciesScore     { get; set; }   // إقرار السياسات
    public decimal MeetingActionsScore { get; set; } // بنود الاجتماعات
    public decimal IncidentsScore    { get; set; }   // حل الحوادث

    public bool IsCurrentUser { get; set; }
}

public class EmployeeRankingResponse
{
    /// <summary>
    /// للأدمن: جميع الموظفين.
    /// للمدير: موظفو قسمه فقط.
    /// للموظف: نفسه فقط.
    /// </summary>
    public List<EmployeeRankDto> Rankings    { get; set; } = [];
    public bool                  IsAdminView { get; set; }
    public string                ScopeLabel  { get; set; } = string.Empty;
}

// ── My Score (تفاصيل درجة المستخدم الحالي) ───────────────────────────────────

public class MyScoreDto
{
    // القسم
    public DepartmentRankDto? DepartmentRank { get; set; }

    // الموظف
    public EmployeeRankDto? EmployeeRank { get; set; }

    // تفاصيل تفصيلية للموظف
    public int TasksTotal       { get; set; }
    public int TasksDone        { get; set; }
    public int PoliciesTotal    { get; set; }
    public int PoliciesAcked    { get; set; }
    public int MeetingActionsTotal    { get; set; }
    public int MeetingActionsDone     { get; set; }
    public int IncidentsTotal   { get; set; }
    public int IncidentsResolved { get; set; }
}
