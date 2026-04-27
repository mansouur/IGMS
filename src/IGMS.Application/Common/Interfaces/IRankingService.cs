using IGMS.Application.Common.Models;

namespace IGMS.Application.Common.Interfaces;

public interface IRankingService
{
    /// <summary>
    /// ترتيب الأقسام.
    /// الأدمن يرى جميع الأقسام بالترتيب الحقيقي.
    /// غير الأدمن يرى قسمه فقط بالترتيب المعدَّل (Phantom Target).
    /// </summary>
    Task<DepartmentRankingResponse> GetDepartmentRankingsAsync(int currentUserId, bool isAdmin);

    /// <summary>
    /// ترتيب الموظفين.
    /// الأدمن: الجميع. المدير: قسمه. الموظف: نفسه فقط.
    /// </summary>
    Task<EmployeeRankingResponse> GetEmployeeRankingsAsync(int currentUserId, bool isAdmin, bool isDeptManager);

    /// <summary>درجة المستخدم الحالي بالتفصيل — للبطاقة الشخصية.</summary>
    Task<MyScoreDto> GetMyScoreAsync(int currentUserId);
}
