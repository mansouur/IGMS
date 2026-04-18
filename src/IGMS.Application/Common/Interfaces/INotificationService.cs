using IGMS.Application.Common.Models;

namespace IGMS.Application.Common.Interfaces;

/// <summary>
/// Domain-level notification service.
/// Each method corresponds to a business event that triggers email(s).
/// Implementations must never throw – swallow all delivery exceptions internally.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// إشعار عند نشر سياسة:
    /// → المسؤول عن السياسة (Owner)
    /// → المعتمد (Approver)
    /// </summary>
    Task PolicyPublishedAsync(
        string policyTitle,
        string ownerEmail,  string ownerName,
        string approverEmail, string approverName,
        string organizationName);

    /// <summary>
    /// إشعار عند تكليف مهمة للمستخدم.
    /// </summary>
    Task TaskAssignedAsync(
        string taskTitle,
        string assigneeEmail, string assigneeName,
        string? dueDate,
        string organizationName);

    /// <summary>
    /// إشعار لمالك السياسة قبل انتهاء صلاحيتها (مجدول تلقائياً).
    /// </summary>
    Task PolicyExpiringAsync(
        string policyTitle,
        string ownerEmail, string ownerName,
        int daysRemaining,
        string organizationName);

    /// <summary>
    /// تذكير للمكلف بمهمة متأخرة عن موعدها.
    /// </summary>
    Task TaskOverdueAsync(
        string taskTitle,
        string assigneeEmail, string assigneeName,
        int daysOverdue,
        string organizationName);

    /// <summary>
    /// تصعيد مهمة متأخرة جداً إلى مدير القسم.
    /// </summary>
    Task TaskEscalatedAsync(
        string taskTitle,
        string assigneeNameAr,
        string managerEmail, string managerName,
        int daysOverdue,
        string organizationName);

    /// <summary>
    /// تقرير الامتثال الدوري (شهري/ربعي) لمسؤول الامتثال.
    /// </summary>
    Task ComplianceReportAsync(
        string recipientEmail, string recipientName,
        ComplianceReportData report,
        string organizationName);

    /// <summary>
    /// رمز التحقق المؤقت (OTP) للمصادقة الثنائية عبر البريد الإلكتروني.
    /// </summary>
    Task SendOtpEmailAsync(
        string recipientEmail,
        string recipientNameAr,
        string otp,
        string organizationName);
}
