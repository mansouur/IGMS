using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace IGMS.Infrastructure.Services;

/// <summary>
/// Builds HTML email templates and dispatches them via IEmailService.
/// Never throws – all exceptions are caught internally.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IEmailService _email;
    private readonly ILogger<NotificationService> _log;

    public NotificationService(IEmailService email, ILogger<NotificationService> log)
    {
        _email = email;
        _log   = log;
    }

    // ── Policy Published ─────────────────────────────────────────────────────

    public async Task PolicyPublishedAsync(
        string policyTitle,
        string ownerEmail,    string ownerName,
        string approverEmail, string approverName,
        string organizationName)
    {
        try
        {
            var subject = $"تم نشر السياسة: {policyTitle}";

            // Notify owner
            if (!string.IsNullOrWhiteSpace(ownerEmail))
            {
                var body = BuildPolicyPublishedOwnerEmail(policyTitle, ownerName, approverName, organizationName);
                await _email.SendAsync(ownerEmail, ownerName, subject, body);
            }

            // Notify approver (if different from owner)
            if (!string.IsNullOrWhiteSpace(approverEmail) && approverEmail != ownerEmail)
            {
                var body = BuildPolicyPublishedApproverEmail(policyTitle, approverName, organizationName);
                await _email.SendAsync(approverEmail, approverName, subject, body);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "NotificationService.PolicyPublishedAsync failed for policy {Title}", policyTitle);
        }
    }

    // ── Task Assigned ────────────────────────────────────────────────────────

    public async Task TaskAssignedAsync(
        string taskTitle,
        string assigneeEmail, string assigneeName,
        string? dueDate,
        string organizationName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(assigneeEmail)) return;

            var subject = $"تم تكليفك بمهمة جديدة: {taskTitle}";
            var body    = BuildTaskAssignedEmail(taskTitle, assigneeName, dueDate, organizationName);
            await _email.SendAsync(assigneeEmail, assigneeName, subject, body);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "NotificationService.TaskAssignedAsync failed for task {Title}", taskTitle);
        }
    }

    // ── Policy Expiring ──────────────────────────────────────────────────────

    public async Task PolicyExpiringAsync(
        string policyTitle,
        string ownerEmail, string ownerName,
        int daysRemaining,
        string organizationName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ownerEmail)) return;
            var subject = $"تنبيه: سياسة تنتهي خلال {daysRemaining} يوماً – {policyTitle}";
            var body    = BuildPolicyExpiringEmail(policyTitle, ownerName, daysRemaining, organizationName);
            await _email.SendAsync(ownerEmail, ownerName, subject, body);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "NotificationService.PolicyExpiringAsync failed for policy {Title}", policyTitle);
        }
    }

    // ── Task Overdue ─────────────────────────────────────────────────────────

    public async Task TaskOverdueAsync(
        string taskTitle,
        string assigneeEmail, string assigneeName,
        int daysOverdue,
        string organizationName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(assigneeEmail)) return;
            var subject = $"⚠️ مهمة متأخرة منذ {daysOverdue} أيام: {taskTitle}";
            var body    = BuildTaskOverdueEmail(taskTitle, assigneeName, daysOverdue, organizationName);
            await _email.SendAsync(assigneeEmail, assigneeName, subject, body);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "NotificationService.TaskOverdueAsync failed for task {Title}", taskTitle);
        }
    }

    // ── Task Escalated ───────────────────────────────────────────────────────

    public async Task TaskEscalatedAsync(
        string taskTitle,
        string assigneeNameAr,
        string managerEmail, string managerName,
        int daysOverdue,
        string organizationName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(managerEmail)) return;
            var subject = $"🔴 تصعيد: مهمة متأخرة جداً – {taskTitle}";
            var body    = BuildTaskEscalatedEmail(taskTitle, assigneeNameAr, managerName, daysOverdue, organizationName);
            await _email.SendAsync(managerEmail, managerName, subject, body);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "NotificationService.TaskEscalatedAsync failed for task {Title}", taskTitle);
        }
    }

    // ── Email Templates ──────────────────────────────────────────────────────

    private static string BuildPolicyPublishedOwnerEmail(
        string policyTitle, string ownerName, string approverName, string org)
    {
        return Wrap(org, $"""
            <p style="margin:0 0 16px">مرحباً {Encode(ownerName)}،</p>
            <p style="margin:0 0 16px">
                تم نشر السياسة التي تتولى الإشراف عليها رسمياً واعتمادها من قِبل <strong>{Encode(approverName)}</strong>.
            </p>
            <div style="background:#f0fdf4;border-right:4px solid #16a34a;padding:12px 16px;border-radius:6px;margin:0 0 20px">
                <p style="margin:0;font-size:15px;font-weight:600;color:#15803d">{Encode(policyTitle)}</p>
            </div>
            <p style="margin:0 0 16px;color:#374151">
                يُرجى التأكد من إبلاغ الجهات المعنية بمضمون هذه السياسة وضمان الالتزام بها.
            </p>
            """);
    }

    private static string BuildPolicyPublishedApproverEmail(
        string policyTitle, string approverName, string org)
    {
        return Wrap(org, $"""
            <p style="margin:0 0 16px">مرحباً {Encode(approverName)}،</p>
            <p style="margin:0 0 16px">
                شكراً لاعتمادكم السياسة التالية. تم تفعيلها في النظام بتاريخ اليوم:
            </p>
            <div style="background:#f0fdf4;border-right:4px solid #16a34a;padding:12px 16px;border-radius:6px;margin:0 0 20px">
                <p style="margin:0;font-size:15px;font-weight:600;color:#15803d">{Encode(policyTitle)}</p>
            </div>
            <p style="margin:0;color:#6b7280;font-size:13px">هذا الإشعار مُرسَل تلقائياً من نظام IGMS.</p>
            """);
    }

    private static string BuildTaskAssignedEmail(
        string taskTitle, string assigneeName, string? dueDate, string org)
    {
        var dueLine = string.IsNullOrWhiteSpace(dueDate)
            ? string.Empty
            : $"<p style=\"margin:0 0 8px\"><strong>الموعد النهائي:</strong> {Encode(dueDate)}</p>";

        return Wrap(org, $"""
            <p style="margin:0 0 16px">مرحباً {Encode(assigneeName)}،</p>
            <p style="margin:0 0 16px">تم تكليفك بمهمة جديدة في نظام الحوكمة المؤسسية:</p>
            <div style="background:#eff6ff;border-right:4px solid #2563eb;padding:12px 16px;border-radius:6px;margin:0 0 20px">
                <p style="margin:0 0 8px;font-size:15px;font-weight:600;color:#1d4ed8">{Encode(taskTitle)}</p>
                {dueLine}
            </div>
            <p style="margin:0 0 16px;color:#374151">
                يُرجى تسجيل الدخول إلى النظام لمراجعة تفاصيل المهمة والبدء في التنفيذ.
            </p>
            <p style="margin:0;color:#6b7280;font-size:13px">هذا الإشعار مُرسَل تلقائياً من نظام IGMS.</p>
            """);
    }

    private static string BuildPolicyExpiringEmail(
        string policyTitle, string ownerName, int daysRemaining, string org)
    {
        var urgency = daysRemaining <= 7
            ? "🔴 عاجل"
            : daysRemaining <= 14 ? "🟠 تنبيه" : "🟡 تذكير";

        return Wrap(org, $"""
            <p style="margin:0 0 16px">مرحباً {Encode(ownerName)}،</p>
            <p style="margin:0 0 16px">
                نُذكّركم بأن السياسة التالية ستنتهي صلاحيتها خلال
                <strong style="color:#dc2626">{daysRemaining} يوماً</strong>:
            </p>
            <div style="background:#fef9c3;border-right:4px solid #ca8a04;padding:12px 16px;border-radius:6px;margin:0 0 20px">
                <p style="margin:0 0 4px;font-size:11px;color:#92400e;font-weight:600">{urgency}</p>
                <p style="margin:0;font-size:15px;font-weight:600;color:#78350f">{Encode(policyTitle)}</p>
            </div>
            <p style="margin:0 0 16px;color:#374151">
                يُرجى مراجعة السياسة واتخاذ الإجراء المناسب: تجديدها أو أرشفتها قبل انتهاء الموعد.
            </p>
            <p style="margin:0;color:#6b7280;font-size:13px">هذا الإشعار مُرسَل تلقائياً من نظام IGMS.</p>
            """);
    }

    private static string BuildTaskOverdueEmail(
        string taskTitle, string assigneeName, int daysOverdue, string org)
    {
        return Wrap(org, $"""
            <p style="margin:0 0 16px">مرحباً {Encode(assigneeName)}،</p>
            <p style="margin:0 0 16px">
                نُنبّهك إلى أن المهمة التالية المكلّف بها قد تجاوزت موعدها النهائي بـ
                <strong style="color:#dc2626">{daysOverdue} أيام</strong>:
            </p>
            <div style="background:#fef2f2;border-right:4px solid #dc2626;padding:12px 16px;border-radius:6px;margin:0 0 20px">
                <p style="margin:0 0 4px;font-size:11px;color:#991b1b;font-weight:600">⚠️ متأخرة</p>
                <p style="margin:0;font-size:15px;font-weight:600;color:#7f1d1d">{Encode(taskTitle)}</p>
            </div>
            <p style="margin:0 0 16px;color:#374151">
                يُرجى تسجيل الدخول إلى النظام وتحديث حالة المهمة أو التواصل مع مديرك المباشر.
            </p>
            <p style="margin:0;color:#6b7280;font-size:13px">هذا الإشعار مُرسَل تلقائياً من نظام IGMS.</p>
            """);
    }

    private static string BuildTaskEscalatedEmail(
        string taskTitle, string assigneeNameAr, string managerName, int daysOverdue, string org)
    {
        return Wrap(org, $"""
            <p style="margin:0 0 16px">مرحباً {Encode(managerName)}،</p>
            <p style="margin:0 0 16px">
                نُحيطكم علماً بأن المهمة التالية المكلّف بها <strong>{Encode(assigneeNameAr)}</strong>
                قد تجاوزت موعدها النهائي بـ <strong style="color:#dc2626">{daysOverdue} أيام</strong>
                وتتطلب تدخلكم:
            </p>
            <div style="background:#fef2f2;border-right:4px solid #dc2626;padding:12px 16px;border-radius:6px;margin:0 0 20px">
                <p style="margin:0 0 4px;font-size:11px;color:#991b1b;font-weight:600">🔴 تصعيد عاجل</p>
                <p style="margin:0;font-size:15px;font-weight:600;color:#7f1d1d">{Encode(taskTitle)}</p>
                <p style="margin:6px 0 0;font-size:12px;color:#991b1b">المكلّف: {Encode(assigneeNameAr)}</p>
            </div>
            <p style="margin:0 0 16px;color:#374151">
                يُرجى مراجعة الوضع ومتابعة المكلف أو إعادة تعيين المهمة لضمان الإنجاز في أقرب وقت.
            </p>
            <p style="margin:0;color:#6b7280;font-size:13px">هذا الإشعار مُرسَل تلقائياً من نظام IGMS.</p>
            """);
    }

    // ── Layout wrapper ───────────────────────────────────────────────────────

    private static string Wrap(string orgName, string content) => $"""
        <!DOCTYPE html>
        <html lang="ar" dir="rtl">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width"/></head>
        <body style="margin:0;padding:0;background:#f9fafb;font-family:'Segoe UI',Arial,sans-serif;direction:rtl">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#f9fafb;padding:32px 16px">
            <tr><td align="center">
              <table width="580" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e5e7eb">
                <!-- Header -->
                <tr>
                  <td style="background:#15803d;padding:20px 28px">
                    <p style="margin:0;color:#ffffff;font-size:18px;font-weight:700">{Encode(orgName)}</p>
                    <p style="margin:4px 0 0;color:#bbf7d0;font-size:12px">نظام إدارة الحوكمة المؤسسية – IGMS</p>
                  </td>
                </tr>
                <!-- Body -->
                <tr>
                  <td style="padding:28px;color:#1f2937;font-size:14px;line-height:1.7">
                    {content}
                  </td>
                </tr>
                <!-- Footer -->
                <tr>
                  <td style="background:#f3f4f6;padding:14px 28px;border-top:1px solid #e5e7eb">
                    <p style="margin:0;color:#9ca3af;font-size:11px">
                      هذا بريد إلكتروني تلقائي، يُرجى عدم الرد عليه مباشرةً.
                    </p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;

    // ── Compliance Report ─────────────────────────────────────────────────────

    public async Task ComplianceReportAsync(
        string recipientEmail, string recipientName,
        ComplianceReportData report,
        string organizationName)
    {
        try
        {
            var subject = $"تقرير الامتثال الدوري – {organizationName} – {report.ReportDate:yyyy-MM}";
            var body    = BuildComplianceReportEmail(recipientName, report, organizationName);
            await _email.SendAsync(recipientEmail, recipientName, subject, body);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "ComplianceReportAsync failed for {Email}", recipientEmail);
        }
    }

    private static string BuildComplianceReportEmail(
        string recipientName, ComplianceReportData d, string orgName)
    {
        var scoreColor = d.AvgCompliance >= 70 ? "#065f46" : d.AvgCompliance >= 40 ? "#92400e" : "#7f1d1d";
        var scoreLabel = d.AvgCompliance >= 70 ? "جيد" : d.AvgCompliance >= 40 ? "متوسط" : "يحتاج تحسين";

        var rows = new System.Text.StringBuilder();
        foreach (var fw in d.Frameworks)
        {
            var barWidth = (int)Math.Min(fw.OverallPct, 100);
            var barColor = fw.OverallPct >= 70 ? "#059669" : fw.OverallPct >= 40 ? "#d97706" : "#dc2626";
            var groupBadge = fw.Group == "إماراتي"
                ? "<span style='font-size:10px;background:#e0f2fe;color:#0369a1;padding:1px 6px;border-radius:9px;margin-right:4px'>إماراتي</span>"
                : "";
            rows.Append($"""
              <tr style="border-bottom:1px solid #f0f0f0">
                <td style="padding:10px 12px;font-weight:600">{Encode(fw.Label)}{groupBadge}</td>
                <td style="padding:10px 12px;text-align:center">{fw.PolCovered}/{fw.PolTotal} <span style="color:#888;font-size:11px">({fw.PolPct}%)</span></td>
                <td style="padding:10px 12px;text-align:center">{fw.RskCovered}/{fw.RskTotal} <span style="color:#888;font-size:11px">({fw.RskPct}%)</span></td>
                <td style="padding:10px 12px">
                  <div style="background:#e5e7eb;border-radius:4px;height:8px;width:100%">
                    <div style="background:{barColor};height:8px;border-radius:4px;width:{barWidth}%"></div>
                  </div>
                  <div style="font-size:11px;font-weight:700;color:{barColor};margin-top:2px">{fw.OverallPct}%</div>
                </td>
              </tr>
            """);
        }

        return $"""
        <!DOCTYPE html>
        <html dir="rtl" lang="ar">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;padding:0;background:#f3f4f6;font-family:Arial,sans-serif;direction:rtl">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#f3f4f6;padding:32px 0">
            <tr><td align="center">
              <table width="620" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08)">

                <!-- Header -->
                <tr><td style="background:#1e3a5f;padding:28px 32px">
                  <div style="font-size:22px;font-weight:800;color:#ffffff">{Encode(orgName)}</div>
                  <div style="font-size:14px;color:#93c5fd;margin-top:4px">تقرير الامتثال الدوري – {d.ReportDate:MMMM yyyy}</div>
                </td></tr>

                <!-- Greeting -->
                <tr><td style="padding:24px 32px 0">
                  <p style="margin:0;font-size:15px;color:#1f2937">السيد/ة {Encode(recipientName)}،</p>
                  <p style="margin:8px 0 0;font-size:14px;color:#4b5563;line-height:1.7">
                    نُرسل إليكم هذا التقرير الدوري لوضع تغطية الامتثال لأطر الحوكمة المعتمدة
                    بتاريخ <strong>{d.ReportDate:yyyy-MM-dd}</strong>.
                  </p>
                </td></tr>

                <!-- Score card -->
                <tr><td style="padding:20px 32px">
                  <table width="100%" cellpadding="0" cellspacing="0">
                    <tr>
                      <td style="background:{scoreColor};border-radius:10px;padding:20px;text-align:center;width:32%">
                        <div style="font-size:36px;font-weight:800;color:#fff">{d.AvgCompliance}%</div>
                        <div style="font-size:12px;color:rgba(255,255,255,0.85);margin-top:4px">متوسط الامتثال – {scoreLabel}</div>
                      </td>
                      <td style="width:4%"></td>
                      <td style="vertical-align:top">
                        <table width="100%" cellpadding="0" cellspacing="0">
                          <tr>
                            <td style="padding:8px 12px;background:#f9fafb;border-radius:8px;margin-bottom:8px">
                              <div style="font-size:12px;color:#6b7280">إجمالي السياسات</div>
                              <div style="font-size:20px;font-weight:700;color:#1f2937">{d.PolTotal}</div>
                            </td>
                            <td style="width:8px"></td>
                            <td style="padding:8px 12px;background:#f9fafb;border-radius:8px">
                              <div style="font-size:12px;color:#6b7280">إجمالي المخاطر</div>
                              <div style="font-size:20px;font-weight:700;color:#1f2937">{d.RskTotal}</div>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                </td></tr>

                <!-- Frameworks table -->
                <tr><td style="padding:0 32px 24px">
                  <div style="font-size:14px;font-weight:700;color:#1e3a5f;margin-bottom:10px">تغطية كل إطار</div>
                  <table width="100%" cellpadding="0" cellspacing="0" style="border:1px solid #e5e7eb;border-radius:8px;overflow:hidden;font-size:13px">
                    <thead>
                      <tr style="background:#f8fafc">
                        <th style="padding:10px 12px;text-align:right;font-size:12px;color:#6b7280;font-weight:600">الإطار</th>
                        <th style="padding:10px 12px;text-align:center;font-size:12px;color:#6b7280;font-weight:600">السياسات</th>
                        <th style="padding:10px 12px;text-align:center;font-size:12px;color:#6b7280;font-weight:600">المخاطر</th>
                        <th style="padding:10px 12px;text-align:right;font-size:12px;color:#6b7280;font-weight:600">التغطية الكلية</th>
                      </tr>
                    </thead>
                    <tbody>{rows}</tbody>
                  </table>
                </td></tr>

                <!-- Footer -->
                <tr><td style="background:#f8fafc;border-top:1px solid #e5e7eb;padding:16px 32px;text-align:center">
                  <p style="margin:0;font-size:11px;color:#9ca3af">
                    هذا التقرير مُولَّد تلقائياً من نظام حوكمة المؤسسة IGMS
                    · {d.ReportDate:yyyy-MM-dd}
                  </p>
                </td></tr>

              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
    }

    // ── OTP Email (Two-Factor Authentication) ────────────────────────────────

    public async Task SendOtpEmailAsync(
        string recipientEmail, string recipientNameAr, string otp, string organizationName)
    {
        try
        {
            var subject = $"رمز التحقق – {organizationName}";
            var body    = Wrap(organizationName, $"""
                <p style="margin:0 0 16px">مرحباً {Encode(recipientNameAr)}،</p>
                <p style="margin:0 0 20px;color:#374151">
                    تم طلب رمز التحقق لتسجيل الدخول إلى نظام IGMS.
                    أدخل الرمز التالي لإتمام عملية الدخول:
                </p>
                <div style="text-align:center;margin:0 0 24px">
                  <div style="display:inline-block;background:#f0fdf4;border:2px solid #16a34a;border-radius:12px;padding:20px 40px">
                    <span style="font-size:36px;font-weight:900;letter-spacing:10px;color:#15803d;font-family:monospace">{otp}</span>
                  </div>
                </div>
                <p style="margin:0 0 12px;color:#6b7280;font-size:13px">
                  ⏱ هذا الرمز صالح لمدة <strong>5 دقائق</strong> فقط.
                </p>
                <p style="margin:0;color:#6b7280;font-size:13px">
                  إذا لم تطلب تسجيل الدخول، تجاهل هذا البريد وقم بتغيير كلمة مرورك فوراً.
                </p>
                """);
            await _email.SendAsync(recipientEmail, recipientNameAr, subject, body);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "SendOtpEmailAsync failed for {Email}", recipientEmail);
        }
    }

    private static string Encode(string? text) =>
        System.Net.WebUtility.HtmlEncode(text ?? string.Empty);
}
