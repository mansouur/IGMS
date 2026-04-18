using System.Text.Json;
using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class AssessmentService : IAssessmentService
{
    private readonly TenantDbContext _db;
    public AssessmentService(TenantDbContext db) => _db = db;

    // ── List ──────────────────────────────────────────────────────────────────

    public async Task<List<AssessmentListDto>> GetListAsync(int currentUserId)
    {
        var assessments = await _db.Assessments
            .AsNoTracking()
            .Include(a => a.Department)
            .Include(a => a.Questions.Where(q => !q.IsDeleted))
            .Include(a => a.Responses.Where(r => !r.IsDeleted))
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return assessments.Select(a => new AssessmentListDto
        {
            Id                   = a.Id,
            TitleAr              = a.TitleAr,
            TitleEn              = a.TitleEn,
            Status               = a.Status.ToString(),
            DepartmentName       = a.Department?.NameAr,
            DueDate              = a.DueDate,
            QuestionCount        = a.Questions.Count,
            ResponseCount        = a.Responses.Count,
            SubmittedCount       = a.Responses.Count(r => r.IsSubmitted),
            MyResponseSubmitted  = a.Responses.Any(r => r.RespondentId == currentUserId && r.IsSubmitted),
        }).ToList();
    }

    // ── Detail ────────────────────────────────────────────────────────────────

    public async Task<AssessmentDetailDto?> GetByIdAsync(int id)
    {
        var a = await _db.Assessments
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Questions.Where(q => !q.IsDeleted).OrderBy(q => q.QuestionOrder))
            .Include(x => x.Responses.Where(r => !r.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        return a == null ? null : MapDetail(a);
    }

    // ── Save ──────────────────────────────────────────────────────────────────

    public async Task<AssessmentDetailDto> SaveAsync(int? id, SaveAssessmentRequest req, int currentUserId)
    {
        Assessment assessment;

        if (id.HasValue)
        {
            assessment = await _db.Assessments
                .Include(a => a.Questions.Where(q => !q.IsDeleted))
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted)
                ?? throw new InvalidOperationException("الاستبيان غير موجود.");

            if (assessment.Status != AssessmentStatus.Draft)
                throw new InvalidOperationException("لا يمكن تعديل استبيان منشور أو مغلق.");

            assessment.TitleAr       = req.TitleAr;
            assessment.TitleEn       = req.TitleEn;
            assessment.DescriptionAr = req.DescriptionAr;
            assessment.DepartmentId  = req.DepartmentId;
            assessment.DueDate       = req.DueDate;
            assessment.ModifiedAt    = DateTime.UtcNow;

            // Remove old questions and rebuild
            foreach (var q in assessment.Questions.ToList())
            { q.IsDeleted = true; q.DeletedAt = DateTime.UtcNow; }
        }
        else
        {
            assessment = new Assessment
            {
                TitleAr       = req.TitleAr,
                TitleEn       = req.TitleEn,
                DescriptionAr = req.DescriptionAr,
                DepartmentId  = req.DepartmentId,
                DueDate       = req.DueDate,
                Status        = AssessmentStatus.Draft,
                CreatedById   = currentUserId,
                CreatedAt     = DateTime.UtcNow,
                CreatedBy     = "api",
            };
            _db.Assessments.Add(assessment);
        }

        // Rebuild questions
        for (int i = 0; i < req.Questions.Count; i++)
        {
            var qr = req.Questions[i];
            if (!Enum.TryParse<QuestionType>(qr.QuestionType, out var qt))
                throw new InvalidOperationException($"نوع سؤال غير صالح: {qr.QuestionType}");

            assessment.Questions.Add(new AssessmentQuestion
            {
                QuestionOrder = i + 1,
                QuestionType  = qt,
                TextAr        = qr.TextAr,
                TextEn        = qr.TextEn,
                IsRequired    = qr.IsRequired,
                Options       = qr.Options.Count > 0 ? JsonSerializer.Serialize(qr.Options) : null,
                CreatedAt     = DateTime.UtcNow,
                CreatedBy     = "api",
            });
        }

        await _db.SaveChangesAsync();
        return MapDetail(assessment);
    }

    public async Task DeleteAsync(int id)
    {
        var a = await _db.Assessments.FindAsync(id)
            ?? throw new InvalidOperationException("الاستبيان غير موجود.");
        a.IsDeleted = true; a.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task PublishAsync(int id)
    {
        var a = await _db.Assessments.FindAsync(id)
            ?? throw new InvalidOperationException("الاستبيان غير موجود.");
        if (a.Status != AssessmentStatus.Draft)
            throw new InvalidOperationException("الاستبيان منشور بالفعل.");
        a.Status = AssessmentStatus.Published;
        await _db.SaveChangesAsync();
    }

    public async Task CloseAsync(int id)
    {
        var a = await _db.Assessments.FindAsync(id)
            ?? throw new InvalidOperationException("الاستبيان غير موجود.");
        a.Status = AssessmentStatus.Closed;
        await _db.SaveChangesAsync();
    }

    // ── Response ──────────────────────────────────────────────────────────────

    public async Task<AssessmentResponseDto?> GetMyResponseAsync(int assessmentId, int userId)
    {
        var r = await _db.AssessmentResponses
            .AsNoTracking()
            .Include(x => x.Respondent)
            .Include(x => x.Department)
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x => x.AssessmentId == assessmentId && x.RespondentId == userId && !x.IsDeleted);

        return r == null ? null : MapResponse(r);
    }

    public async Task<AssessmentResponseDto> SaveResponseAsync(
        int assessmentId, int userId, int? departmentId,
        SubmitResponseRequest req, bool submit)
    {
        var assessment = await _db.Assessments.AsNoTracking()
            .Include(a => a.Questions.Where(q => !q.IsDeleted))
            .FirstOrDefaultAsync(a => a.Id == assessmentId && !a.IsDeleted)
            ?? throw new InvalidOperationException("الاستبيان غير موجود.");

        if (assessment.Status != AssessmentStatus.Published)
            throw new InvalidOperationException("الاستبيان غير متاح للرد.");

        // Get or create response
        var response = await _db.AssessmentResponses
            .Include(r => r.Answers)
            .FirstOrDefaultAsync(r => r.AssessmentId == assessmentId && r.RespondentId == userId && !r.IsDeleted);

        if (response == null)
        {
            response = new AssessmentResponse
            {
                AssessmentId  = assessmentId,
                RespondentId  = userId,
                DepartmentId  = departmentId,
                CreatedAt     = DateTime.UtcNow,
                CreatedBy     = "api",
            };
            _db.AssessmentResponses.Add(response);
        }

        // Validate required questions if submitting
        if (submit)
        {
            var requiredIds = assessment.Questions
                .Where(q => q.IsRequired)
                .Select(q => q.Id)
                .ToHashSet();

            var answeredIds = req.Answers
                .Where(a => !string.IsNullOrWhiteSpace(a.AnswerText))
                .Select(a => a.QuestionId)
                .ToHashSet();

            var missing = requiredIds.Except(answeredIds).ToList();
            if (missing.Count > 0)
                throw new InvalidOperationException("يرجى الإجابة على جميع الأسئلة الإلزامية.");
        }

        // Replace answers
        foreach (var existing in response.Answers.ToList())
        { existing.IsDeleted = true; existing.DeletedAt = DateTime.UtcNow; }

        foreach (var ans in req.Answers.Where(a => !string.IsNullOrWhiteSpace(a.AnswerText)))
        {
            response.Answers.Add(new AssessmentAnswer
            {
                AssessmentQuestionId = ans.QuestionId,
                AnswerText           = ans.AnswerText,
                CreatedAt            = DateTime.UtcNow,
                CreatedBy            = "api",
            });
        }

        if (submit)
        {
            response.IsSubmitted  = true;
            response.SubmittedAt  = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        // Reload with nav props
        var loaded = await _db.AssessmentResponses
            .AsNoTracking()
            .Include(r => r.Respondent)
            .Include(r => r.Department)
            .Include(r => r.Answers)
            .FirstAsync(r => r.Id == response.Id);

        return MapResponse(loaded);
    }

    // ── Report ────────────────────────────────────────────────────────────────

    public async Task<AssessmentReportDto> GetReportAsync(int assessmentId)
    {
        var assessment = await _db.Assessments
            .AsNoTracking()
            .Include(a => a.Questions.Where(q => !q.IsDeleted).OrderBy(q => q.QuestionOrder))
            .FirstOrDefaultAsync(a => a.Id == assessmentId && !a.IsDeleted)
            ?? throw new InvalidOperationException("الاستبيان غير موجود.");

        var responses = await _db.AssessmentResponses
            .AsNoTracking()
            .Include(r => r.Answers.Where(a => !a.IsDeleted))
            .Where(r => r.AssessmentId == assessmentId && r.IsSubmitted && !r.IsDeleted)
            .ToListAsync();

        var questionReports = assessment.Questions.Select(q =>
        {
            var answers = responses
                .SelectMany(r => r.Answers)
                .Where(a => a.AssessmentQuestionId == q.Id && !string.IsNullOrWhiteSpace(a.AnswerText))
                .Select(a => a.AnswerText!)
                .ToList();

            var qr = new QuestionReportDto
            {
                QuestionId   = q.Id,
                TextAr       = q.TextAr,
                QuestionType = q.QuestionType.ToString(),
                AnswerCount  = answers.Count,
            };

            switch (q.QuestionType)
            {
                case QuestionType.YesNo:
                    qr.Distribution = answers
                        .GroupBy(a => a)
                        .ToDictionary(g => g.Key, g => g.Count());
                    break;

                case QuestionType.Rating:
                    qr.Distribution = answers
                        .GroupBy(a => a)
                        .OrderBy(g => g.Key)
                        .ToDictionary(g => g.Key, g => g.Count());
                    qr.Average = answers.Count > 0
                        ? answers.Select(a => double.TryParse(a, out var v) ? v : 0).Average()
                        : null;
                    qr.Average = qr.Average.HasValue ? Math.Round(qr.Average.Value, 1) : null;
                    break;

                case QuestionType.MultiChoice:
                    qr.Distribution = answers
                        .GroupBy(a => a)
                        .ToDictionary(g => g.Key, g => g.Count());
                    break;

                case QuestionType.Text:
                    qr.TextSamples = answers.Take(5).ToList();
                    break;
            }

            return qr;
        }).ToList();

        var totalResponded = await _db.AssessmentResponses
            .CountAsync(r => r.AssessmentId == assessmentId && !r.IsDeleted);

        return new AssessmentReportDto
        {
            AssessmentId   = assessmentId,
            TitleAr        = assessment.TitleAr,
            TotalInvited   = totalResponded,
            TotalResponded = responses.Count,
            ResponseRate   = totalResponded == 0 ? 0 : Math.Round((double)responses.Count / totalResponded * 100, 1),
            Questions      = questionReports,
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static AssessmentDetailDto MapDetail(Assessment a) => new()
    {
        Id             = a.Id,
        TitleAr        = a.TitleAr,
        TitleEn        = a.TitleEn,
        DescriptionAr  = a.DescriptionAr,
        Status         = a.Status.ToString(),
        DepartmentId   = a.DepartmentId,
        DepartmentName = a.Department?.NameAr,
        DueDate        = a.DueDate,
        ResponseCount  = a.Responses?.Count ?? 0,
        SubmittedCount = a.Responses?.Count(r => r.IsSubmitted) ?? 0,
        Questions      = a.Questions
            .Where(q => !q.IsDeleted)
            .OrderBy(q => q.QuestionOrder)
            .Select(q => new AssessmentQuestionDto
            {
                Id            = q.Id,
                QuestionOrder = q.QuestionOrder,
                QuestionType  = q.QuestionType.ToString(),
                TextAr        = q.TextAr,
                TextEn        = q.TextEn,
                IsRequired    = q.IsRequired,
                Options       = string.IsNullOrEmpty(q.Options)
                    ? []
                    : JsonSerializer.Deserialize<List<string>>(q.Options) ?? [],
            }).ToList(),
    };

    private static AssessmentResponseDto MapResponse(AssessmentResponse r) => new()
    {
        Id             = r.Id,
        AssessmentId   = r.AssessmentId,
        RespondentName = r.Respondent?.FullNameAr ?? "",
        DepartmentName = r.Department?.NameAr,
        IsSubmitted    = r.IsSubmitted,
        SubmittedAt    = r.SubmittedAt,
        Answers        = r.Answers
            .Where(a => !a.IsDeleted)
            .Select(a => new AssessmentAnswerDto
            {
                QuestionId  = a.AssessmentQuestionId,
                AnswerText  = a.AnswerText,
            }).ToList(),
    };
}
