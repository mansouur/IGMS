using IGMS.Application.Common.Models;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Services;
using IGMS.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Tests.Services;

public class AssessmentServiceTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static AssessmentService Build(string dbName) =>
        new(DbContextFactory.Create(dbName));

    private static void Seed(string dbName, params Assessment[] assessments)
    {
        using var db = DbContextFactory.Create(dbName);
        db.Assessments.AddRange(assessments);
        db.SaveChanges();
    }

    private static Assessment MakeAssessment(
        string titleAr = "استبيان اختبار",
        AssessmentStatus status = AssessmentStatus.Draft) => new()
    {
        TitleAr   = titleAr,
        TitleEn   = "Test Assessment",
        Status    = status,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "test",
    };

    // ── GetPagedAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPagedAsync_NoFilter_ReturnsAll()
    {
        var db = nameof(GetPagedAsync_NoFilter_ReturnsAll);
        Seed(db, MakeAssessment("أول"), MakeAssessment("ثاني"), MakeAssessment("ثالث"));

        var svc    = Build(db);
        var result = await svc.GetPagedAsync(new AssessmentQuery { Page = 1, PageSize = 10 }, currentUserId: 0);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task GetPagedAsync_StatusFilter_ReturnsOnlyDraft()
    {
        var db = nameof(GetPagedAsync_StatusFilter_ReturnsOnlyDraft);
        Seed(db,
            MakeAssessment("مسودة 1",  AssessmentStatus.Draft),
            MakeAssessment("منشور",    AssessmentStatus.Published),
            MakeAssessment("مسودة 2",  AssessmentStatus.Draft));

        var svc    = Build(db);
        var result = await svc.GetPagedAsync(
            new AssessmentQuery { Page = 1, PageSize = 10, Status = "Draft" }, currentUserId: 0);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, a => Assert.Equal("Draft", a.Status));
    }

    [Fact]
    public async Task GetPagedAsync_SearchFilter_ReturnsMatched()
    {
        var db = nameof(GetPagedAsync_SearchFilter_ReturnsMatched);
        Seed(db,
            MakeAssessment("تقييم مخاطر"),
            MakeAssessment("استبيان رضا"),
            MakeAssessment("تقييم أمني"));

        var svc    = Build(db);
        var result = await svc.GetPagedAsync(
            new AssessmentQuery { Page = 1, PageSize = 10, Search = "تقييم" }, currentUserId: 0);

        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task GetPagedAsync_DeletedAssessments_NotReturned()
    {
        var db = nameof(GetPagedAsync_DeletedAssessments_NotReturned);
        var deleted = MakeAssessment("محذوف");
        deleted.IsDeleted = true;
        Seed(db, MakeAssessment("نشط"), deleted);

        var svc    = Build(db);
        var result = await svc.GetPagedAsync(new AssessmentQuery { Page = 1, PageSize = 10 }, currentUserId: 0);

        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetPagedAsync_Pagination_ReturnsCorrectPage()
    {
        var db = nameof(GetPagedAsync_Pagination_ReturnsCorrectPage);
        Seed(db,
            MakeAssessment("أ"), MakeAssessment("ب"), MakeAssessment("ج"),
            MakeAssessment("د"), MakeAssessment("هـ"));

        var svc    = Build(db);
        var result = await svc.GetPagedAsync(
            new AssessmentQuery { Page = 2, PageSize = 2 }, currentUserId: 0);

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.CurrentPage);
    }

    // ── PublishAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task PublishAsync_Draft_ChangesStatusToPublished()
    {
        var db = nameof(PublishAsync_Draft_ChangesStatusToPublished);
        Seed(db, MakeAssessment(status: AssessmentStatus.Draft));

        var svc = Build(db);
        int id;
        using (var dbCheck = DbContextFactory.Create(db))
            id = dbCheck.Assessments.First().Id;

        await svc.PublishAsync(id);

        using var dbVerify = DbContextFactory.Create(db);
        var assessment = dbVerify.Assessments.First();
        Assert.Equal(AssessmentStatus.Published, assessment.Status);
    }

    [Fact]
    public async Task PublishAsync_AlreadyPublished_Throws()
    {
        var db = nameof(PublishAsync_AlreadyPublished_Throws);
        Seed(db, MakeAssessment(status: AssessmentStatus.Published));

        var svc = Build(db);
        int id;
        using (var dbCheck = DbContextFactory.Create(db))
            id = dbCheck.Assessments.First().Id;

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.PublishAsync(id));
    }

    [Fact]
    public async Task PublishAsync_NotFound_Throws()
    {
        var svc = Build(nameof(PublishAsync_NotFound_Throws));

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.PublishAsync(999));
    }

    // ── CloseAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CloseAsync_SetsStatusToClosed()
    {
        var db = nameof(CloseAsync_SetsStatusToClosed);
        Seed(db, MakeAssessment(status: AssessmentStatus.Published));

        var svc = Build(db);
        int id;
        using (var dbCheck = DbContextFactory.Create(db))
            id = dbCheck.Assessments.First().Id;

        await svc.CloseAsync(id);

        using var dbVerify = DbContextFactory.Create(db);
        Assert.Equal(AssessmentStatus.Closed, dbVerify.Assessments.First().Status);
    }

    [Fact]
    public async Task CloseAsync_NotFound_Throws()
    {
        var svc = Build(nameof(CloseAsync_NotFound_Throws));

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CloseAsync(999));
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_SetsIsDeletedFlag()
    {
        var db = nameof(DeleteAsync_SetsIsDeletedFlag);
        Seed(db, MakeAssessment());

        var svc = Build(db);
        int id;
        using (var dbCheck = DbContextFactory.Create(db))
            id = dbCheck.Assessments.First().Id;

        await svc.DeleteAsync(id);

        using var dbVerify = DbContextFactory.Create(db);
        var assessment = dbVerify.Assessments.IgnoreQueryFilters().First();
        Assert.True(assessment.IsDeleted);
        Assert.NotNull(assessment.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_Throws()
    {
        var svc = Build(nameof(DeleteAsync_NotFound_Throws));

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.DeleteAsync(999));
    }
}
