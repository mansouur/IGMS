using IGMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IGMS.Infrastructure.Persistence;

/// <summary>
/// Seeds development-only test data into a specific tenant DB.
/// Called from Program.cs ONLY when ASPNETCORE_ENVIRONMENT=Development.
///
/// Builds a realistic organizational structure for the demo tenant:
/// وزارة الرياضة – دولة الإمارات العربية المتحدة
/// (UAE Ministry of Sports)
///
/// Users seeded here represent ministry staff who would use the IGMS system
/// (IT, governance, audit, strategy) — not every ministry employee.
///
/// NEVER runs in production.
/// </summary>
public static class DevDataSeeder
{
    // User@123  (BCrypt, cost 11)
    private const string DevPass = "$2a$11$5kY6gHHVFHHK/7JdnJvxAO12RWhjaWTwQffqLnRPycOXVxBfEtV2u";

    public static async Task SeedAsync(IServiceProvider services, string tenantKey, ILogger logger)
    {
        var loader = services.GetRequiredService<ITenantConfigLoader>();
        var tenantCtx = await loader.LoadAsync(tenantKey);

        if (tenantCtx is null)
        {
            logger.LogWarning("DevDataSeeder: tenant '{Key}' not found, skipping.", tenantKey);
            return;
        }

        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer(tenantCtx.ConnectionString,
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3))
            .Options;

        await using var db = new TenantDbContext(options, tenantCtx);

        if (!await db.UserProfiles.AnyAsync(u => u.Id == 2))
        {
            logger.LogInformation("DevDataSeeder: seeding base data for '{Key}'...", tenantKey);
            await SeedDepartmentsAsync(db);
            await SeedUsersAsync(db);
            await LinkAsync(db);
        }

        await SeedGovernanceAsync(db);

        logger.LogInformation("DevDataSeeder: done.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // الهيكل التنظيمي – وزارة الرياضة / UAE Ministry of Sports
    //
    // 3 مستويات: الوزارة ← الإدارات ← الأقسام
    // الكود: MOS (Ministry of Sports)
    // ─────────────────────────────────────────────────────────────────────────

    private static async Task SeedDepartmentsAsync(TenantDbContext db)
    {
        if (await db.Departments.AnyAsync()) return;

        await db.Database.ExecuteSqlRawAsync("""
            SET IDENTITY_INSERT [Departments] ON;

            INSERT INTO [Departments]
                (Id, NameAr, NameEn, Code, DescriptionAr, DescriptionEn,
                 Level, IsActive, IsDeleted, ParentId, ManagerId, CreatedAt, CreatedBy)
            VALUES

            -- ══════════════════════════════════════════════════════
            --  Level 1 – الوزارة
            -- ══════════════════════════════════════════════════════
            (1,
             N'وزارة الرياضة',
             'Ministry of Sports',
             'MOS',
             N'الوزارة الاتحادية المعنية بتنظيم القطاع الرياضي وتطويره في دولة الإمارات العربية المتحدة',
             'The federal ministry responsible for organising and developing the sports sector in the UAE.',
             1, 1, 0, NULL, NULL, '2026-01-01', 'DevSeed'),

            -- ══════════════════════════════════════════════════════
            --  Level 2 – الإدارات الرئيسية (تابعة للوزارة)
            -- ══════════════════════════════════════════════════════
            (2,
             N'ديوان الوزير',
             'Minister''s Cabinet',
             'MOS-DW',
             N'يدعم الوزير في الإشراف الاستراتيجي والتنسيق المؤسسي والعلاقات الخارجية',
             'Supports the Minister in strategic oversight, institutional coordination and external relations.',
             2, 1, 0, 1, NULL, '2026-01-01', 'DevSeed'),

            (3,
             N'إدارة الشؤون الرياضية',
             'Sports Affairs Directorate',
             'MOS-SA',
             N'تُشرف على البرامج الرياضية والاتحادات والأندية وتطوير الرياضة الوطنية',
             'Oversees sports programmes, federations, clubs and national sports development.',
             2, 1, 0, 1, NULL, '2026-01-01', 'DevSeed'),

            (4,
             N'إدارة المنشآت والبنية التحتية الرياضية',
             'Sports Facilities & Infrastructure Directorate',
             'MOS-FI',
             N'تُدير وتُطور الملاعب والمرافق الرياضية على المستوى الاتحادي',
             'Manages and develops stadiums and sports facilities at the federal level.',
             2, 1, 0, 1, NULL, '2026-01-01', 'DevSeed'),

            (5,
             N'إدارة تطوير المواهب والكوادر الرياضية',
             'Talent Development & Sports Cadre Directorate',
             'MOS-TD',
             N'تعتني باكتشاف المواهب الرياضية وتأهيل الكوادر والمدربين الوطنيين',
             'Identifies sporting talent and qualifies national coaches and sports cadres.',
             2, 1, 0, 1, NULL, '2026-01-01', 'DevSeed'),

            (6,
             N'إدارة الشراكات والفعاليات الدولية',
             'International Partnerships & Events Directorate',
             'MOS-PE',
             N'تُدير العلاقات الدولية واستضافة الفعاليات الرياضية الكبرى',
             'Manages international relations and the hosting of major sporting events.',
             2, 1, 0, 1, NULL, '2026-01-01', 'DevSeed'),

            (7,
             N'إدارة الشؤون المالية والإدارية',
             'Finance & Administrative Affairs Directorate',
             'MOS-FA',
             N'تُشرف على الميزانية والمحاسبة والموارد البشرية والمشتريات',
             'Oversees budgeting, accounting, human resources and procurement.',
             2, 1, 0, 1, NULL, '2026-01-01', 'DevSeed'),

            (8,
             N'إدارة تقنية المعلومات والتحول الرقمي',
             'Information Technology & Digital Transformation Directorate',
             'MOS-IT',
             N'تقود التحول الرقمي للوزارة وتُدير البنية التحتية التقنية وأمن المعلومات',
             'Leads the ministry''s digital transformation and manages technical infrastructure and information security.',
             2, 1, 0, 1, NULL, '2026-01-01', 'DevSeed'),

            (9,
             N'إدارة الشؤون القانونية والامتثال',
             'Legal Affairs & Compliance Directorate',
             'MOS-LC',
             N'تُقدم المشورة القانونية وتُشرف على الامتثال للتشريعات والسياسات والحوكمة',
             'Provides legal counsel and oversees compliance with legislation, policies and governance.',
             2, 1, 0, 1, NULL, '2026-01-01', 'DevSeed'),

            -- ══════════════════════════════════════════════════════
            --  Level 3 – الأقسام
            -- ══════════════════════════════════════════════════════

            -- تحت: إدارة الشؤون الرياضية (3)
            (10,
             N'قسم الرياضة الأولمبية والمنتخبات الوطنية',
             'Olympic Sports & National Teams Division',
             'MOS-SA-OL',
             N'يُشرف على الاتحادات الرياضية الأولمبية وإعداد المنتخبات الوطنية للمحافل الدولية',
             'Oversees Olympic sports federations and national team preparation for international competitions.',
             3, 1, 0, 3, NULL, '2026-01-01', 'DevSeed'),

            (11,
             N'قسم رياضة الجماهير والمجتمع',
             'Community & Mass Sports Division',
             'MOS-SA-CM',
             N'يُعزز ممارسة الرياضة بين أفراد المجتمع عبر البرامج والمبادرات الجماهيرية',
             'Promotes sports participation among community members through mass programmes and initiatives.',
             3, 1, 0, 3, NULL, '2026-01-01', 'DevSeed'),

            (12,
             N'قسم رياضة ذوي الهمم',
             'Para Sports Division',
             'MOS-SA-PS',
             N'يرعى الرياضيين ذوي الهمم ويُطور البرامج والبطولات الخاصة بهم',
             'Supports athletes with disabilities and develops their dedicated programmes and championships.',
             3, 1, 0, 3, NULL, '2026-01-01', 'DevSeed'),

            -- تحت: إدارة المنشآت والبنية التحتية (4)
            (13,
             N'قسم الملاعب والمرافق الرياضية',
             'Stadiums & Sports Venues Division',
             'MOS-FI-SV',
             N'يُدير عمليات الملاعب والصالات الرياضية والمرافق الاتحادية',
             'Manages operations of federal stadiums, sports halls and facilities.',
             3, 1, 0, 4, NULL, '2026-01-01', 'DevSeed'),

            (14,
             N'قسم الصيانة والتشغيل',
             'Maintenance & Operations Division',
             'MOS-FI-MO',
             N'يُشرف على صيانة وتشغيل المنشآت الرياضية وضمان جاهزيتها الدائمة',
             'Oversees maintenance and operation of sports facilities to ensure permanent readiness.',
             3, 1, 0, 4, NULL, '2026-01-01', 'DevSeed'),

            -- تحت: إدارة تقنية المعلومات (8)
            (15,
             N'قسم البنية التحتية التقنية',
             'Technical Infrastructure Division',
             'MOS-IT-INF',
             N'يُدير الشبكات والخوادم ومراكز البيانات والبنية التحتية التقنية للوزارة',
             'Manages ministry networks, servers, data centres and technical infrastructure.',
             3, 1, 0, 8, NULL, '2026-01-01', 'DevSeed'),

            (16,
             N'قسم التطبيقات والأنظمة',
             'Applications & Systems Division',
             'MOS-IT-APP',
             N'يُطور ويصون أنظمة المعلومات والتطبيقات المؤسسية للوزارة',
             'Develops and maintains the ministry''s information systems and enterprise applications.',
             3, 1, 0, 8, NULL, '2026-01-01', 'DevSeed'),

            (17,
             N'قسم أمن المعلومات',
             'Information Security Division',
             'MOS-IT-SEC',
             N'يحمي بيانات وأنظمة الوزارة ويضمن الامتثال لمتطلبات الأمن السيبراني',
             'Protects ministry data and systems, ensuring compliance with cybersecurity requirements.',
             3, 1, 0, 8, NULL, '2026-01-01', 'DevSeed'),

            -- تحت: إدارة الشؤون المالية والإدارية (7)
            (18,
             N'قسم الميزانية والمحاسبة',
             'Budget & Accounting Division',
             'MOS-FA-BA',
             N'يُعدّ الميزانيات ويتابع الإنفاق ويُشرف على العمليات المحاسبية للوزارة',
             'Prepares budgets, monitors expenditure and oversees the ministry''s accounting operations.',
             3, 1, 0, 7, NULL, '2026-01-01', 'DevSeed'),

            (19,
             N'قسم الموارد البشرية',
             'Human Resources Division',
             'MOS-FA-HR',
             N'يُدير دورة حياة الموظف من التوظيف والتطوير المهني إلى التقاعد',
             'Manages the employee lifecycle from recruitment and professional development to retirement.',
             3, 1, 0, 7, NULL, '2026-01-01', 'DevSeed');

            SET IDENTITY_INSERT [Departments] OFF;
            """);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // المستخدمون التجريبيون – موظفو وزارة الرياضة مستخدمو IGMS
    // البريد الإلكتروني: @mosc.gov.ae  (Ministry of Sports)
    // ─────────────────────────────────────────────────────────────────────────

    private static async Task SeedUsersAsync(TenantDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync($"""
            SET IDENTITY_INSERT [UserProfiles] ON;

            INSERT INTO [UserProfiles]
                (Id, Username, PasswordHash, FullNameAr, FullNameEn, Email,
                 IsActive, IsDeleted, CreatedAt, CreatedBy)
            VALUES
            -- ── إدارة تقنية المعلومات والتحول الرقمي ──────────────────────────
            (2,  'it.director',    '{DevPass}',
             N'م. سعيد الكتبي',           'Saeed Al Ketbi',
             'it.director@mosc.gov.ae',    1, 0, '2026-04-01', 'DevSeed'),

            (3,  'digital.lead',   '{DevPass}',
             N'م. هند الشامسي',           'Hind Al Shamsi',
             'digital.lead@mosc.gov.ae',   1, 0, '2026-04-01', 'DevSeed'),

            (4,  'infosec',        '{DevPass}',
             N'م. خالد المطيري',          'Khalid Al Mutairi',
             'infosec@mosc.gov.ae',        1, 0, '2026-04-01', 'DevSeed'),

            (5,  'it.systems',     '{DevPass}',
             N'أ. فاطمة العامري',         'Fatima Al Ameri',
             'it.systems@mosc.gov.ae',     1, 0, '2026-04-01', 'DevSeed'),

            (6,  'it.infra',       '{DevPass}',
             N'أ. محمد الهاجري',          'Mohammed Al Hajri',
             'it.infra@mosc.gov.ae',       1, 0, '2026-04-01', 'DevSeed'),

            -- ── ديوان الوزير / الحوكمة والاستراتيجية ──────────────────────────
            (7,  'strategy.head',  '{DevPass}',
             N'م. نورة الظاهري',          'Noura Al Dhaheri',
             'strategy@mosc.gov.ae',       1, 0, '2026-04-01', 'DevSeed'),

            (8,  'gov.analyst.1',  '{DevPass}',
             N'أ. مريم الرميثي',          'Mariam Al Rumaithi',
             'gov.analyst1@mosc.gov.ae',   1, 0, '2026-04-01', 'DevSeed'),

            (9,  'gov.analyst.2',  '{DevPass}',
             N'أ. علي النعيمي',           'Ali Al Nuaimi',
             'gov.analyst2@mosc.gov.ae',   1, 0, '2026-04-01', 'DevSeed'),

            -- ── الشؤون القانونية والامتثال ─────────────────────────────────────
            (10, 'audit.head',     '{DevPass}',
             N'م. عبدالله المنصوري',      'Abdullah Al Mansouri',
             'audit@mosc.gov.ae',          1, 0, '2026-04-01', 'DevSeed'),

            (11, 'risk.officer',   '{DevPass}',
             N'أ. سارة الكعبي',           'Sara Al Kaabi',
             'risk@mosc.gov.ae',           1, 0, '2026-04-01', 'DevSeed'),

            (12, 'compliance',     '{DevPass}',
             N'أ. حمد الحوسني',           'Hamad Al Hosni',
             'compliance@mosc.gov.ae',     1, 0, '2026-04-01', 'DevSeed'),

            -- ── إدارة الشؤون الرياضية ──────────────────────────────────────────
            (13, 'sports.head',    '{DevPass}',
             N'م. أحمد الزعابي',          'Ahmed Al Zaabi',
             'sports.head@mosc.gov.ae',    1, 0, '2026-04-01', 'DevSeed'),

            (14, 'sports.pm',      '{DevPass}',
             N'أ. لطيفة الفلاسي',         'Latifa Al Falasi',
             'sports.pm@mosc.gov.ae',      1, 0, '2026-04-01', 'DevSeed'),

            -- ── الشؤون المالية والإدارية ───────────────────────────────────────
            (15, 'finance.head',   '{DevPass}',
             N'م. يوسف الدرعي',           'Yousef Al Darei',
             'finance@mosc.gov.ae',        1, 0, '2026-04-01', 'DevSeed');

            SET IDENTITY_INSERT [UserProfiles] OFF;
            """);

        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO [UserRoles] (UserId, RoleId, AssignedBy, AssignedAt) VALUES
            (2,  2, 'DevSeed', '2026-04-01'),   -- it.director    → Manager
            (3,  2, 'DevSeed', '2026-04-01'),   -- digital.lead   → Manager
            (4,  2, 'DevSeed', '2026-04-01'),   -- infosec        → Manager
            (5,  3, 'DevSeed', '2026-04-01'),   -- it.systems     → User
            (6,  3, 'DevSeed', '2026-04-01'),   -- it.infra       → User
            (7,  2, 'DevSeed', '2026-04-01'),   -- strategy.head  → Manager
            (8,  3, 'DevSeed', '2026-04-01'),   -- gov.analyst.1  → User
            (9,  3, 'DevSeed', '2026-04-01'),   -- gov.analyst.2  → User
            (10, 2, 'DevSeed', '2026-04-01'),   -- audit.head     → Manager
            (11, 3, 'DevSeed', '2026-04-01'),   -- risk.officer   → User
            (12, 3, 'DevSeed', '2026-04-01'),   -- compliance     → User
            (13, 2, 'DevSeed', '2026-04-01'),   -- sports.head    → Manager
            (14, 3, 'DevSeed', '2026-04-01'),   -- sports.pm      → User
            (15, 2, 'DevSeed', '2026-04-01');   -- finance.head   → Manager
            """);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ربط الموظفين بأقسامهم + تعيين مديري الأقسام
    // ─────────────────────────────────────────────────────────────────────────

    private static async Task LinkAsync(TenantDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            SET QUOTED_IDENTIFIER ON;

            UPDATE [UserProfiles] SET DepartmentId = 8  WHERE Id = 2;   -- it.director   → MOS-IT
            UPDATE [UserProfiles] SET DepartmentId = 8  WHERE Id = 3;   -- digital.lead  → MOS-IT
            UPDATE [UserProfiles] SET DepartmentId = 17 WHERE Id = 4;   -- infosec       → MOS-IT-SEC
            UPDATE [UserProfiles] SET DepartmentId = 16 WHERE Id = 5;   -- it.systems    → MOS-IT-APP
            UPDATE [UserProfiles] SET DepartmentId = 15 WHERE Id = 6;   -- it.infra      → MOS-IT-INF
            UPDATE [UserProfiles] SET DepartmentId = 2  WHERE Id = 7;   -- strategy.head → MOS-DW
            UPDATE [UserProfiles] SET DepartmentId = 2  WHERE Id = 8;   -- gov.analyst.1 → MOS-DW
            UPDATE [UserProfiles] SET DepartmentId = 2  WHERE Id = 9;   -- gov.analyst.2 → MOS-DW
            UPDATE [UserProfiles] SET DepartmentId = 9  WHERE Id = 10;  -- audit.head    → MOS-LC
            UPDATE [UserProfiles] SET DepartmentId = 9  WHERE Id = 11;  -- risk.officer  → MOS-LC
            UPDATE [UserProfiles] SET DepartmentId = 9  WHERE Id = 12;  -- compliance    → MOS-LC
            UPDATE [UserProfiles] SET DepartmentId = 3  WHERE Id = 13;  -- sports.head   → MOS-SA
            UPDATE [UserProfiles] SET DepartmentId = 3  WHERE Id = 14;  -- sports.pm     → MOS-SA
            UPDATE [UserProfiles] SET DepartmentId = 7  WHERE Id = 15;  -- finance.head  → MOS-FA

            UPDATE [Departments] SET ManagerId = 2  WHERE Id = 8;   -- MOS-IT     ← it.director
            UPDATE [Departments] SET ManagerId = 4  WHERE Id = 17;  -- MOS-IT-SEC ← infosec
            UPDATE [Departments] SET ManagerId = 5  WHERE Id = 16;  -- MOS-IT-APP ← it.systems
            UPDATE [Departments] SET ManagerId = 6  WHERE Id = 15;  -- MOS-IT-INF ← it.infra
            UPDATE [Departments] SET ManagerId = 7  WHERE Id = 2;   -- MOS-DW     ← strategy.head
            UPDATE [Departments] SET ManagerId = 10 WHERE Id = 9;   -- MOS-LC     ← audit.head
            UPDATE [Departments] SET ManagerId = 13 WHERE Id = 3;   -- MOS-SA     ← sports.head
            UPDATE [Departments] SET ManagerId = 15 WHERE Id = 7;   -- MOS-FA     ← finance.head
            """);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // بيانات الحوكمة التجريبية – سياسات، مخاطر، مهام، مؤشرات
    // ─────────────────────────────────────────────────────────────────────────

    private static async Task SeedGovernanceAsync(TenantDbContext db)
    {
        // يعمل حتى لو كانت البيانات الأساسية موجودة مسبقاً
        if (await db.Policies.AnyAsync()) return;

        await db.Database.ExecuteSqlRawAsync("""
            -- ══════════════════════════════
            --  السياسات (Policies)
            -- ══════════════════════════════
            SET IDENTITY_INSERT [Policies] ON;
            INSERT INTO [Policies]
                (Id, TitleAr, TitleEn, Code, DescriptionAr, Category, Status,
                 EffectiveDate, DepartmentId, OwnerId, IsDeleted, CreatedAt, CreatedBy)
            VALUES
            (1, N'سياسة أمن المعلومات', 'Information Security Policy', 'POL-IT-001',
             N'تحدد هذه السياسة متطلبات حماية المعلومات والأنظمة في الوزارة',
             1, 1, '2026-01-01', 8, 4, 0, '2026-01-01', 'DevSeed'),

            (2, N'سياسة الحوكمة المؤسسية', 'Corporate Governance Policy', 'POL-GOV-001',
             N'تُرسي مبادئ الحوكمة الرشيدة والشفافية والمساءلة',
             0, 1, '2026-01-01', 2, 7, 0, '2026-01-01', 'DevSeed'),

            (3, N'سياسة إدارة المشتريات', 'Procurement Management Policy', 'POL-FIN-001',
             N'تضبط إجراءات الشراء والتعاقد وفق اللوائح المالية المعتمدة',
             3, 0, NULL, 7, 15, 0, '2026-03-01', 'DevSeed');
            SET IDENTITY_INSERT [Policies] OFF;

            -- ══════════════════════════════
            --  المخاطر (Risks)
            -- ══════════════════════════════
            SET IDENTITY_INSERT [Risks] ON;
            INSERT INTO [Risks]
                (Id, TitleAr, TitleEn, Code, DescriptionAr, MitigationPlanAr,
                 Category, Status, Likelihood, Impact,
                 DepartmentId, OwnerId, IsDeleted, CreatedAt, CreatedBy)
            VALUES
            (1, N'اختراق أنظمة المعلومات', 'Information Systems Breach', 'RSK-IT-001',
             N'خطر تعرض أنظمة الوزارة للاختراق الإلكتروني وسرقة البيانات',
             N'تطبيق جدران الحماية وبروتوكولات التشفير وتدريب الموظفين',
             2, 0, 3, 5, 8, 4, 0, '2026-01-15', 'DevSeed'),

            (2, N'تجاوز الميزانية في المشاريع', 'Project Budget Overrun', 'RSK-FIN-001',
             N'خطر تجاوز التكاليف الفعلية للمشاريع المخصصات المعتمدة',
             N'مراجعة دورية للتكاليف وتفعيل نظام التنبيهات المبكرة',
             1, 0, 2, 4, 7, 15, 0, '2026-01-15', 'DevSeed'),

            (3, N'ضعف الكفاءات الرياضية الوطنية', 'Weak National Sports Competencies', 'RSK-SA-001',
             N'خطر عدم تحقيق الأهداف الوطنية في الألعاب الدولية',
             N'تكثيف برامج التدريب واكتشاف المواهب المبكر',
             4, 1, 3, 3, 3, 13, 0, '2026-02-01', 'DevSeed');
            SET IDENTITY_INSERT [Risks] OFF;

            -- ══════════════════════════════
            --  المهام (Tasks)
            -- ══════════════════════════════
            SET IDENTITY_INSERT [Tasks] ON;
            INSERT INTO [Tasks]
                (Id, TitleAr, TitleEn, DescriptionAr, Status, Priority,
                 DueDate, AssignedToId, DepartmentId, IsDeleted, CreatedAt, CreatedBy)
            VALUES
            (1, N'مراجعة سياسة أمن المعلومات', 'Review Information Security Policy',
             N'مراجعة وتحديث سياسة أمن المعلومات بما يتوافق مع المعايير الدولية',
             1, 2, '2026-05-01', 4, 8, 0, '2026-04-01', 'DevSeed'),

            (2, N'إعداد تقرير المخاطر الربع سنوي', 'Prepare Quarterly Risk Report',
             N'تجميع وتحليل المخاطر التشغيلية للربع الأول من 2026',
             0, 1, '2026-04-30', 10, 9, 0, '2026-04-01', 'DevSeed'),

            (3, N'تطوير برنامج اكتشاف المواهب', 'Talent Discovery Programme Development',
             N'تصميم وإطلاق برنامج وطني لاكتشاف المواهب الرياضية في المدارس',
             0, 3, '2026-06-01', 13, 3, 0, '2026-04-01', 'DevSeed');
            SET IDENTITY_INSERT [Tasks] OFF;

            -- ══════════════════════════════
            --  مؤشرات الأداء (KPIs)
            -- ══════════════════════════════
            SET IDENTITY_INSERT [Kpis] ON;
            INSERT INTO [Kpis]
                (Id, TitleAr, TitleEn, Code, Unit, TargetValue, ActualValue,
                 Year, Quarter, Status, DepartmentId, OwnerId, IsDeleted, CreatedAt, CreatedBy)
            VALUES
            (1, N'نسبة تنفيذ السياسات المعتمدة', 'Approved Policies Implementation Rate',
             'KPI-GOV-001', '%', 90, 72, 2026, 1, 1, 2, 7, 0, '2026-01-01', 'DevSeed'),

            (2, N'عدد الرياضيين الموهوبين المكتشفين', 'Talented Athletes Discovered',
             'KPI-SA-001', N'رياضي', 50, 38, 2026, 1, 0, 3, 13, 0, '2026-01-01', 'DevSeed'),

            (3, N'نسبة إغلاق المخاطر عالية الأثر', 'High Impact Risk Closure Rate',
             'KPI-RISK-001', '%', 80, 40, 2026, NULL, 2, 9, 10, 0, '2026-01-01', 'DevSeed');
            SET IDENTITY_INSERT [Kpis] OFF;
            """);
    }
}
