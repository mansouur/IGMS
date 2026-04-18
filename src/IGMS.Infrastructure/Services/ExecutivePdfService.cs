using IGMS.Application.Common.Interfaces;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DomainTaskStatus = IGMS.Domain.Entities.TaskStatus;

namespace IGMS.Infrastructure.Services;

public class ExecutivePdfService : IExecutivePdfService
{
    private readonly TenantDbContext _db;

    public ExecutivePdfService(TenantDbContext db)
    {
        _db = db;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateExecutiveReportAsync(string tenantName)
    {
        var data = await CollectDataAsync();
        var pdf  = BuildDocument(data, tenantName);
        return pdf.GeneratePdf();
    }

    // ── Data collection ───────────────────────────────────────────────────────
    private async Task<ReportData> CollectDataAsync()
    {
        var now  = DateTime.UtcNow;
        var in30 = now.AddDays(30);

        // Policies
        var polTotal    = await _db.Policies.CountAsync(p => !p.IsDeleted);
        var polActive   = await _db.Policies.CountAsync(p => !p.IsDeleted && p.Status == PolicyStatus.Active);
        var polDraft    = await _db.Policies.CountAsync(p => !p.IsDeleted && p.Status == PolicyStatus.Draft);
        var polExpiring = await _db.Policies
            .Where(p => !p.IsDeleted && p.Status == PolicyStatus.Active
                     && p.ExpiryDate.HasValue && p.ExpiryDate.Value <= in30 && p.ExpiryDate.Value >= now)
            .Select(p => new ItemRow { Code = p.Code, Title = p.TitleAr, Date = p.ExpiryDate })
            .Take(5).ToListAsync();

        // Risks
        var rskTotal     = await _db.Risks.CountAsync(r => !r.IsDeleted);
        var rskOpen      = await _db.Risks.CountAsync(r => !r.IsDeleted && r.Status == RiskStatus.Open);
        var rskMitigated = await _db.Risks.CountAsync(r => !r.IsDeleted && r.Status == RiskStatus.Mitigated);
        var rskClosed    = await _db.Risks.CountAsync(r => !r.IsDeleted && r.Status == RiskStatus.Closed);

        var topRisks = await _db.Risks
            .Include(r => r.Department)
            .Where(r => !r.IsDeleted && r.Status == RiskStatus.Open)
            .OrderByDescending(r => r.Likelihood * r.Impact)
            .Take(5)
            .Select(r => new RiskRow
            {
                Code  = r.Code,
                Title = r.TitleAr,
                Score = r.Likelihood * r.Impact,
                Dept  = r.Department != null ? r.Department.NameAr : "—",
            })
            .ToListAsync();

        // Tasks
        var tskTotal   = await _db.Tasks.CountAsync(t => !t.IsDeleted);
        var tskDone    = await _db.Tasks.CountAsync(t => !t.IsDeleted && t.Status == DomainTaskStatus.Done);
        var tskOverdue = await _db.Tasks.CountAsync(t => !t.IsDeleted
            && t.DueDate < now
            && t.Status != DomainTaskStatus.Done
            && t.Status != DomainTaskStatus.Cancelled);

        // KPIs
        var kpiTotal   = await _db.Kpis.CountAsync(k => !k.IsDeleted);
        var kpiOnTrack = await _db.Kpis.CountAsync(k => !k.IsDeleted && k.Status == KpiStatus.OnTrack);
        var kpiBehind  = await _db.Kpis.CountAsync(k => !k.IsDeleted && k.Status == KpiStatus.Behind);

        var behindKpis = await _db.Kpis
            .Include(k => k.Department)
            .Where(k => !k.IsDeleted && k.Status == KpiStatus.Behind)
            .OrderBy(k => k.ActualValue)
            .Take(5)
            .Select(k => new KpiRow
            {
                Code       = k.Code,
                Title      = k.TitleAr,
                Target     = k.TargetValue,
                Actual     = k.ActualValue,
                Achievement= k.TargetValue == 0 ? 0 : (int)Math.Round((double)k.ActualValue / (double)k.TargetValue * 100),
                Dept       = k.Department != null ? k.Department.NameAr : "—",
            })
            .ToListAsync();

        // Governance score
        double polH = polTotal > 0 ? (double)polActive   / polTotal * 100 : 100;
        double rskH = rskTotal > 0 ? (double)(rskMitigated + rskClosed) / rskTotal * 100 : 100;
        double tskB = tskTotal > 0 ? (double)tskDone     / tskTotal * 100 : 100;
        double tskP = tskTotal > 0 ? (double)tskOverdue  / tskTotal * 50  : 0;
        double tskH = Math.Max(0, tskB - tskP);
        double kpiH = kpiTotal > 0 ? (double)kpiOnTrack  / kpiTotal * 100 : 100;
        int    score = (int)Math.Round((polH + rskH + tskH + kpiH) / 4);

        return new ReportData
        {
            GeneratedAt  = now,
            Score        = score,
            PolicyPillar = (int)Math.Round(polH),
            RiskPillar   = (int)Math.Round(rskH),
            TaskPillar   = (int)Math.Round(tskH),
            KpiPillar    = (int)Math.Round(kpiH),
            PolTotal = polTotal, PolActive = polActive, PolDraft = polDraft,
            RskTotal = rskTotal, RskOpen = rskOpen, RskMitigated = rskMitigated, RskClosed = rskClosed,
            TskTotal = tskTotal, TskDone = tskDone, TskOverdue = tskOverdue,
            KpiTotal = kpiTotal, KpiOnTrack = kpiOnTrack, KpiBehind = kpiBehind,
            TopRisks     = topRisks,
            BehindKpis   = behindKpis,
            ExpiringPols = polExpiring,
        };
    }

    // ── Document builder ──────────────────────────────────────────────────────
    private static IDocument BuildDocument(ReportData d, string tenantName)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                page.Header().Element(ComposeHeader(tenantName, d));
                page.Content().Element(ComposeContent(d));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("التقرير التنفيذي لنظام حوكمة المؤسسة – صفحة ");
                    x.CurrentPageNumber();
                    x.Span(" من ");
                    x.TotalPages();
                });
            });
        });
    }

    // ── Header ────────────────────────────────────────────────────────────────
    private static Action<IContainer> ComposeHeader(string tenantName, ReportData d) => c =>
    {
        c.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().AlignRight().Column(right =>
                {
                    right.Item().Text(tenantName)
                        .FontSize(16).Bold().FontColor("#1e3a5f");
                    right.Item().Text("التقرير التنفيذي – حوكمة المؤسسة")
                        .FontSize(11).FontColor("#5a7a9a");
                    right.Item().Text($"تاريخ الإصدار: {d.GeneratedAt:yyyy-MM-dd}")
                        .FontSize(9).FontColor("#888");
                });
            });
            col.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor("#1e3a5f");
        });
    };

    // ── Content ───────────────────────────────────────────────────────────────
    private static Action<IContainer> ComposeContent(ReportData d) => c =>
    {
        c.Column(col =>
        {
            col.Spacing(14);

            // ── Governance Score ─────────────────────────────────────────────
            col.Item().Background(ScoreColor(d.Score)).Padding(16).Row(row =>
            {
                row.RelativeItem().AlignCenter().Column(cnt =>
                {
                    cnt.Item().AlignCenter().Text("درجة الحوكمة الإجمالية").FontSize(12).Bold().FontColor(Colors.White);
                    cnt.Item().AlignCenter().Text($"{d.Score}%").FontSize(36).Bold().FontColor(Colors.White);
                    cnt.Item().AlignCenter().Text(ScoreLabel(d.Score)).FontSize(10).FontColor(Colors.White);
                });
            });

            // ── Four Pillars ─────────────────────────────────────────────────
            col.Item().Row(row =>
            {
                row.Spacing(8);
                PillarCell(row.RelativeItem(), "السياسات",   d.PolicyPillar, "#1d4ed8");
                PillarCell(row.RelativeItem(), "المخاطر",    d.RiskPillar,   "#b45309");
                PillarCell(row.RelativeItem(), "المهام",     d.TaskPillar,   "#047857");
                PillarCell(row.RelativeItem(), "المؤشرات",   d.KpiPillar,    "#6d28d9");
            });

            // ── Statistics Row ───────────────────────────────────────────────
            col.Item().Table(tbl =>
            {
                tbl.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(); cols.RelativeColumn();
                    cols.RelativeColumn(); cols.RelativeColumn();
                });

                // header
                tbl.Header(h =>
                {
                    foreach (var lbl in new[] { "السياسات", "المخاطر", "المهام", "المؤشرات" })
                    {
                        h.Cell().Background("#1e3a5f").Padding(6)
                            .AlignCenter().Text(lbl).Bold().FontColor(Colors.White).FontSize(9);
                    }
                });

                // Policies col
                tbl.Cell().Padding(4).Column(cc =>
                {
                    StatLine(cc, "الإجمالي",   d.PolTotal.ToString());
                    StatLine(cc, "نشطة",       d.PolActive.ToString(), "#059669");
                    StatLine(cc, "مسودة",      d.PolDraft.ToString(), "#d97706");
                });
                // Risks col
                tbl.Cell().Padding(4).Column(cc =>
                {
                    StatLine(cc, "الإجمالي",   d.RskTotal.ToString());
                    StatLine(cc, "مفتوحة",     d.RskOpen.ToString(),  "#dc2626");
                    StatLine(cc, "مخففة",      d.RskMitigated.ToString(), "#d97706");
                    StatLine(cc, "مغلقة",      d.RskClosed.ToString(), "#059669");
                });
                // Tasks col
                tbl.Cell().Padding(4).Column(cc =>
                {
                    StatLine(cc, "الإجمالي",   d.TskTotal.ToString());
                    StatLine(cc, "منجزة",      d.TskDone.ToString(),    "#059669");
                    StatLine(cc, "متأخرة",     d.TskOverdue.ToString(), "#dc2626");
                });
                // KPIs col
                tbl.Cell().Padding(4).Column(cc =>
                {
                    StatLine(cc, "الإجمالي",   d.KpiTotal.ToString());
                    StatLine(cc, "على المسار", d.KpiOnTrack.ToString(), "#059669");
                    StatLine(cc, "متأخرة",     d.KpiBehind.ToString(),  "#dc2626");
                });
            });

            // ── Top Risks ────────────────────────────────────────────────────
            if (d.TopRisks.Count > 0)
            {
                col.Item().Column(sec =>
                {
                    sec.Item().Text("أعلى المخاطر الحرجة المفتوحة").Bold().FontSize(11).FontColor("#1e3a5f");
                    sec.Item().PaddingTop(4).Table(tbl =>
                    {
                        tbl.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(55);
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                            cols.ConstantColumn(45);
                        });
                        TableHeader(tbl, "الرمز", "المخاطرة", "القسم", "الدرجة");
                        var shade = false;
                        foreach (var r in d.TopRisks)
                        {
                            var bg = shade ? "#f8f9fa" : "#ffffff";
                            tbl.Cell().Background(bg).Padding(5).AlignRight().Text(r.Code).FontSize(8).FontFamily("Courier New");
                            tbl.Cell().Background(bg).Padding(5).AlignRight().Text(r.Title).FontSize(9);
                            tbl.Cell().Background(bg).Padding(5).AlignRight().Text(r.Dept).FontSize(9).FontColor("#555");
                            tbl.Cell().Background(bg).Padding(5).AlignCenter()
                                .Text(r.Score.ToString()).Bold()
                                .FontColor(r.Score >= 15 ? "#dc2626" : r.Score >= 8 ? "#d97706" : "#059669");
                            shade = !shade;
                        }
                    });
                });
            }

            // ── KPIs Behind ──────────────────────────────────────────────────
            if (d.BehindKpis.Count > 0)
            {
                col.Item().Column(sec =>
                {
                    sec.Item().Text("المؤشرات المتأخرة عن الهدف").Bold().FontSize(11).FontColor("#1e3a5f");
                    sec.Item().PaddingTop(4).Table(tbl =>
                    {
                        tbl.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(55);
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                            cols.ConstantColumn(55);
                        });
                        TableHeader(tbl, "الرمز", "المؤشر", "القسم", "التحقيق%");
                        var shade = false;
                        foreach (var k in d.BehindKpis)
                        {
                            var bg = shade ? "#f8f9fa" : "#ffffff";
                            tbl.Cell().Background(bg).Padding(5).AlignRight().Text(k.Code).FontSize(8).FontFamily("Courier New");
                            tbl.Cell().Background(bg).Padding(5).AlignRight().Text(k.Title).FontSize(9);
                            tbl.Cell().Background(bg).Padding(5).AlignRight().Text(k.Dept).FontSize(9).FontColor("#555");
                            tbl.Cell().Background(bg).Padding(5).AlignCenter()
                                .Text($"{k.Achievement}%").Bold().FontColor("#dc2626");
                            shade = !shade;
                        }
                    });
                });
            }

            // ── Expiring Policies ─────────────────────────────────────────────
            if (d.ExpiringPols.Count > 0)
            {
                col.Item().Column(sec =>
                {
                    sec.Item().Text("سياسات تنتهي خلال 30 يومًا").Bold().FontSize(11).FontColor("#1e3a5f");
                    sec.Item().PaddingTop(4).Table(tbl =>
                    {
                        tbl.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(70);
                            cols.RelativeColumn();
                            cols.ConstantColumn(80);
                        });
                        TableHeader(tbl, "الرمز", "السياسة", "تاريخ الانتهاء");
                        var shade = false;
                        foreach (var p in d.ExpiringPols)
                        {
                            var bg = shade ? "#f8f9fa" : "#ffffff";
                            tbl.Cell().Background(bg).Padding(5).AlignRight().Text(p.Code).FontSize(8).FontFamily("Courier New");
                            tbl.Cell().Background(bg).Padding(5).AlignRight().Text(p.Title).FontSize(9);
                            tbl.Cell().Background(bg).Padding(5).AlignCenter()
                                .Text(p.Date.HasValue ? p.Date.Value.ToString("yyyy-MM-dd") : "—")
                                .FontSize(9).FontColor("#d97706").Bold();
                            shade = !shade;
                        }
                    });
                });
            }

            // ── Footer note ──────────────────────────────────────────────────
            col.Item().PaddingTop(8).LineHorizontal(0.5f).LineColor("#ccc");
            col.Item().AlignCenter().Text(
                $"هذا التقرير مُولَّد تلقائياً بتاريخ {d.GeneratedAt:yyyy-MM-dd HH:mm} UTC – نظام حوكمة المؤسسة IGMS")
                .FontSize(8).FontColor("#aaa").Italic();
        });
    };

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static void PillarCell(IContainer c, string label, int score, string color)
    {
        c.Border(0.5f).BorderColor("#e5e7eb").Padding(10).Column(col =>
        {
            col.Item().AlignCenter().Text(label).FontSize(9).Bold().FontColor("#374151");
            col.Item().AlignCenter().Text($"{score}%").FontSize(18).Bold().FontColor(color);
        });
    }

    private static void StatLine(ColumnDescriptor col, string label, string value, string? color = null)
    {
        col.Item().Row(r =>
        {
            r.RelativeItem().AlignRight().Text(label).FontSize(9).FontColor("#6b7280");
            r.ConstantItem(30).AlignLeft().Text(value).FontSize(9).Bold()
                .FontColor(color ?? "#111827");
        });
    }

    private static void TableHeader(TableDescriptor tbl, params string[] headers)
    {
        tbl.Header(h =>
        {
            foreach (var hdr in headers)
                h.Cell().Background("#1e3a5f").Padding(5).AlignCenter()
                    .Text(hdr).Bold().FontColor(Colors.White).FontSize(9);
        });
    }

    private static string ScoreColor(int score) =>
        score >= 80 ? "#065f46" : score >= 60 ? "#92400e" : "#7f1d1d";

    private static string ScoreLabel(int score) =>
        score >= 80 ? "ممتاز – الحوكمة قوية" :
        score >= 60 ? "جيد – يتطلب انتباهاً" :
                      "ضعيف – تدخل فوري مطلوب";

    // ── DTOs ──────────────────────────────────────────────────────────────────
    private class ReportData
    {
        public DateTime  GeneratedAt  { get; set; }
        public int       Score        { get; set; }
        public int       PolicyPillar { get; set; }
        public int       RiskPillar   { get; set; }
        public int       TaskPillar   { get; set; }
        public int       KpiPillar    { get; set; }
        public int PolTotal, PolActive, PolDraft;
        public int RskTotal, RskOpen, RskMitigated, RskClosed;
        public int TskTotal, TskDone, TskOverdue;
        public int KpiTotal, KpiOnTrack, KpiBehind;
        public List<RiskRow>  TopRisks     { get; set; } = new();
        public List<KpiRow>   BehindKpis   { get; set; } = new();
        public List<ItemRow>  ExpiringPols { get; set; } = new();
    }

    private class RiskRow  { public string Code = ""; public string Title = ""; public int Score; public string Dept = ""; }
    private class KpiRow   { public string Code = ""; public string Title = ""; public decimal Target, Actual; public int Achievement; public string Dept = ""; }
    private class ItemRow  { public string Code = ""; public string Title = ""; public DateTime? Date; }
}
