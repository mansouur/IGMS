using IGMS.Application.Common.Models;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Services;
using IGMS.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Tests.Services;

public class IncidentServiceTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IncidentService Build(string dbName) =>
        new(DbContextFactory.Create(dbName));

    private static void Seed(string dbName, params Incident[] incidents)
    {
        using var db = DbContextFactory.Create(dbName);
        db.Incidents.AddRange(incidents);
        db.SaveChanges();
    }

    private static Incident MakeIncident(
        string titleAr = "حادثة اختبار",
        IncidentSeverity severity = IncidentSeverity.Medium,
        IncidentStatus status = IncidentStatus.Open) => new()
    {
        TitleAr    = titleAr,
        Severity   = severity,
        Status     = status,
        OccurredAt = DateTime.UtcNow,
        CreatedAt  = DateTime.UtcNow,
        CreatedBy  = "test",
    };

    // ── GetPagedAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPagedAsync_NoFilter_ReturnsAll()
    {
        var db = nameof(GetPagedAsync_NoFilter_ReturnsAll);
        Seed(db, MakeIncident("أولى"), MakeIncident("ثانية"), MakeIncident("ثالثة"));

        var svc    = Build(db);
        var result = await svc.GetPagedAsync(new IncidentQuery { Page = 1, PageSize = 10 });

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task GetPagedAsync_StatusFilter_ReturnsMatched()
    {
        var db = nameof(GetPagedAsync_StatusFilter_ReturnsMatched);
        Seed(db,
            MakeIncident("مفتوحة",   status: IncidentStatus.Open),
            MakeIncident("محلولة",   status: IncidentStatus.Resolved),
            MakeIncident("مفتوحة 2", status: IncidentStatus.Open));

        var svc    = Build(db);
        var result = await svc.GetPagedAsync(new IncidentQuery { Page = 1, PageSize = 10, Status = "Open" });

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, i => Assert.Equal("Open", i.Status));
    }

    [Fact]
    public async Task GetPagedAsync_SearchFilter_ReturnsMatched()
    {
        var db = nameof(GetPagedAsync_SearchFilter_ReturnsMatched);
        Seed(db,
            MakeIncident("اختراق أمني"),
            MakeIncident("خلل في الشبكة"),
            MakeIncident("اختراق بيانات"));

        var svc    = Build(db);
        var result = await svc.GetPagedAsync(new IncidentQuery { Page = 1, PageSize = 10, Search = "اختراق" });

        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task GetPagedAsync_Pagination_ReturnsCorrectPage()
    {
        var db = nameof(GetPagedAsync_Pagination_ReturnsCorrectPage);
        Seed(db, MakeIncident("أ"), MakeIncident("ب"), MakeIncident("ج"), MakeIncident("د"), MakeIncident("هـ"));

        var svc    = Build(db);
        var result = await svc.GetPagedAsync(new IncidentQuery { Page = 2, PageSize = 2 });

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.CurrentPage);
    }

    [Fact]
    public async Task GetPagedAsync_DeletedIncidents_NotReturned()
    {
        var db = nameof(GetPagedAsync_DeletedIncidents_NotReturned);
        var deleted = MakeIncident("محذوفة");
        deleted.IsDeleted = true;
        Seed(db, MakeIncident("موجودة"), deleted);

        var svc    = Build(db);
        var result = await svc.GetPagedAsync(new IncidentQuery { Page = 1, PageSize = 10 });

        Assert.Equal(1, result.TotalCount);
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsIncident()
    {
        var db  = nameof(CreateAsync_ValidRequest_PersistsIncident);
        var svc = Build(db);

        var req = new SaveIncidentRequest
        {
            TitleAr    = "حادثة جديدة",
            Severity   = "High",
            Status     = "Open",
            OccurredAt = DateTime.UtcNow,
        };

        var result = await svc.CreateAsync(req, reportedById: 0);

        Assert.NotNull(result);
        Assert.Equal("حادثة جديدة", result.TitleAr);
        Assert.Equal("High", result.Severity);

        // Verify it's in DB
        using var db2  = DbContextFactory.Create(db);
        Assert.Equal(1, db2.Incidents.Count());
    }

    [Fact]
    public async Task CreateAsync_InvalidSeverity_Throws()
    {
        var svc = Build(nameof(CreateAsync_InvalidSeverity_Throws));
        var req = new SaveIncidentRequest
        {
            TitleAr    = "حادثة",
            Severity   = "INVALID",
            Status     = "Open",
            OccurredAt = DateTime.UtcNow,
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CreateAsync(req, 0));
    }

    // ── ResolveAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveAsync_SetsStatusResolvedAndTimestamp()
    {
        var db = nameof(ResolveAsync_SetsStatusResolvedAndTimestamp);
        Seed(db, MakeIncident());

        var svc = Build(db);
        int id;
        using (var dbCheck = DbContextFactory.Create(db))
            id = dbCheck.Incidents.First().Id;

        var before = DateTime.UtcNow.AddSeconds(-1);
        var result = await svc.ResolveAsync(id, "تم الحل");

        Assert.Equal("Resolved", result.Status);
        Assert.NotNull(result.ResolvedAt);
        Assert.True(result.ResolvedAt > before);
    }

    [Fact]
    public async Task ResolveAsync_NotFound_Throws()
    {
        var svc = Build(nameof(ResolveAsync_NotFound_Throws));

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.ResolveAsync(999, null));
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_SetsIsDeletedFlag()
    {
        var db = nameof(DeleteAsync_SetsIsDeletedFlag);
        Seed(db, MakeIncident());

        var svc = Build(db);
        int id;
        using (var dbCheck = DbContextFactory.Create(db))
            id = dbCheck.Incidents.First().Id;

        await svc.DeleteAsync(id);

        using var dbVerify = DbContextFactory.Create(db);
        var incident = dbVerify.Incidents.IgnoreQueryFilters().First();
        Assert.True(incident.IsDeleted);
        Assert.NotNull(incident.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_Throws()
    {
        var svc = Build(nameof(DeleteAsync_NotFound_Throws));

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.DeleteAsync(999));
    }
}
