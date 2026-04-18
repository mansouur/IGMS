# السياق المشترك – منظومة المنتجات الحكومية الرقمية
## Arab Gov Digital Suite – Shared Platform Context
## يُنسخ هذا الملف في كل محادثة جديدة أولاً

---

## 1. من أنا وما هو المشروع

**المطوّر:** منصور – محلل حوكمة تقنية المعلومات، الإمارات العربية المتحدة.
خبرة في: ASP.NET Core، SQL Server، EF Core، تحليل الأنظمة، دعم الإنتاج.
عمل حالي: مستشار خارجي في وزارة رياضة إماراتية.
يرى مشاكل الحوكمة الحكومية يومياً من الداخل.

**القرار النهائي للمنظومة:**
```
✅ المنتج 1: IGMS  – نظام الحوكمة المؤسسية      (Phase 0 مكتمل ✅)
✅ المنتج 2: HPMS  – نظام إدارة أداء الموظفين   (لم يبدأ بعد)
❌ MMS  – مستبعد (سوق مشبع بـ Convene/Diligent/BoardPAC)
❌ SQMS – مستبعد (موجود حكومياً: nCRM + Customer Pulse من TDRA)
```

كل منتج مستقل يعمل وحده + Integration API للربط ببعضهما عند رغبة العميل.

---

## 2. القرارات المعمارية الثابتة — لا تتغير بين المنتجين

```
Backend      : ASP.NET Core 8 Web API – Clean Architecture
               (Domain → Application → Infrastructure → API)
Database     : SQL Server 2025 On-Prem (Dev Server: MANSOUR)
               Collation: Arabic_CI_AS (مهم للبحث العربي)
ORM          : Entity Framework Core 8 – Code First Migrations حصراً
               لا تعديل مباشر على DB – كل شيء عبر Migrations
Frontend     : React 18 + Vite
UI Kit       : @aegov/design-system-react
               (UAE Design System الرسمي من TDRA)
CSS          : TailwindCSS v3.4 (مدمج مع UAE DLS)
Mobile       : Flutter – نفس الـ API (مستقبلاً)
Auth         : JWT – يحمل TenantKey + Roles + Language في Claims
Session      : IDistributedCache → Memory (Dev) / Redis (Prod)
               التبديل: سطر واحد في appsettings
Tenancy      : Database per Tenant – عزل كامل لكل عميل
i18n         : عربي افتراضي RTL + إنجليزي ثانوي LTR
Hosting      : IIS 10 – Windows Server 2019/2022 – On-Prem
Logging      : Serilog → SQL + File
```

**Auth Strategies المدعومة:**
```
Local    → username/password في tenant DB (BCrypt)
AD       → LDAP عبر System.DirectoryServices.AccountManagement
UaePass  → OAuth2 PKCE – Sandbox: stg-id.uaepass.ae
Mixed    → كل الخيارات في نفس الوقت (يحدده tenant config)
```

---

## 3. أنماط الكود الثابتة – لا تتغير بين المنتجين

```csharp
// 1. API Response – موحد لـ React + Flutter
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = [];
    public int StatusCode { get; set; }
    public PaginationMeta? Pagination { get; set; }

    public static ApiResponse<T> Ok(T data, string? msg = null) => ...;
    public static ApiResponse<T> Fail(string error, int code = 400) => ...;
    public static ApiResponse<T> NotFound(string msg) => ...;
    public static ApiResponse<T> Unauthorized(string msg) => ...;
}

// 2. Result Pattern – Service Layer (لا exceptions تصل للعميل)
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? Error { get; private set; }
    public static Result<T> Success(T v) => ...;
    public static Result<T> Failure(string e) => ...;
}

// 3. PagedResult
public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

// 4. Base Entity – Audit + Soft Delete
public abstract class AuditableEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsDeleted { get; set; } = false; // Soft Delete دائماً
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

// 5. Bilingual Interface
public interface ILocalizable
{
    string NameAr { get; set; }
    string NameEn { get; set; }
}

// 6. Auth Strategy Pattern
public interface IAuthStrategy
{
    string ProviderName { get; } // "Local" | "AD" | "UaePass"
    Task<Result<LoginResponse>> AuthenticateAsync(LoginRequest request, TenantContext tenant);
}

// 7. Session (IDistributedCache)
public interface ISessionService
{
    Task<string> CreateSessionAsync(SessionData data);
    Task<SessionData?> GetSessionAsync(string sessionId);
    Task RevokeSessionAsync(string sessionId);
}
```

---

## 4. Multi-Tenant – Database per Tenant

```csharp
// TenantDbContext – Code First, Arabic_CI_AS
public class TenantDbContext : DbContext, ITenantDbContext
{
    private readonly TenantContext _tenantContext;

    protected override void OnConfiguring(DbContextOptionsBuilder opt)
    {
        if (!opt.IsConfigured)
            opt.UseSqlServer(_tenantContext.ConnectionString,
                sql => sql.EnableRetryOnFailure(3));
    }

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.UseCollation("Arabic_CI_AS");
        m.ApplyConfigurationsFromAssembly(GetType().Assembly); // يكتشف كل IEntityTypeConfiguration تلقائياً
    }
}
```

```json
// tenant-config.json – نموذج (uae-sport.json)
{
  "tenantKey": "uae-sport",
  "database": {
    "name": "IGMS_UAE_SPORT",
    "server": "MANSOUR",
    "authType": "WindowsAuth"
  },
  "organization": {
    "nameAr": "وزارة الرياضة",
    "nameEn": "Ministry of Sport",
    "country": "UAE",
    "logoPath": "/assets/tenants/uae-sport/logo.png"
  },
  "localization": {
    "defaultLanguage": "ar",
    "supportedLanguages": ["ar", "en"],
    "currency": "AED",
    "timeZone": "Arab Standard Time"
  },
  "orgHierarchy": {
    "levels": {
      "ar": ["وزارة", "إدارة", "قسم", "وحدة"],
      "en": ["Ministry", "Directorate", "Department", "Unit"]
    }
  },
  "branding": {
    "primaryColor": "#1B4F72",
    "secondaryColor": "#2E86C1"
  },
  "authentication": {
    "mode": "Mixed",
    "domain": null
  },
  "licensing": { "type": "OneTime", "price": "85000 AED" }
}
```

---

## 5. UAE Design System – تثبيت React

```bash
npm create vite@latest {Product}.Web -- --template react
cd {Product}.Web
npm install @aegov/design-system-react
npm install -D tailwindcss@3.4 postcss autoprefixer
npm install react-i18next i18next axios react-router-dom zustand
npx tailwindcss init -p
```

```javascript
// tailwind.config.js
export default {
  content: [
    './index.html',
    './src/**/*.{js,jsx}',
    './node_modules/@aegov/design-system-react/**/*.{js,jsx}',
  ],
  theme: {
    extend: {
      colors: {
        'tenant-primary':   'var(--tenant-primary)',
        'tenant-secondary': 'var(--tenant-secondary)',
      },
      fontFamily: { arabic: ['Cairo', 'Tajawal', 'sans-serif'] },
    },
  },
}
```

---

## 6. DependencyInjection Pattern – ثابت في كل منتج

```csharp
// Program.cs – نظيف دائماً
builder.Services.AddInfrastructure(builder.Configuration, tenantsDirectory);

// DependencyInjection.cs – يُقسّم إلى:
RegisterSession()        // IDistributedCache → Memory أو Redis
RegisterTenancy()        // ITenantConfigLoader (Singleton) + TenantContext (Scoped)
RegisterDatabase()       // TenantDbContext + ITenantDbContext
RegisterAuthStrategies() // IAuthStrategy × N + IJwtService
RegisterUaePass()        // IUaePassService via HttpClient
RegisterServices()       // ICurrentUserService
```

---

## 7. Integration بين المنتجين

```
كل منتج يوفر:
GET  /api/v1/integration/info          ← معلومات المنتج والإصدار
GET  /api/v1/integration/users         ← قائمة المستخدمين المشتركة
GET  /api/v1/integration/departments   ← الأقسام المشتركة
POST /api/v1/integration/webhook       ← استقبال أحداث من المنتج الآخر

IGMS → HPMS:
  GET /api/v1/integration/raci-matrix  ← HPMS يسحب RACI لبناء أهداف الموظف

HPMS → IGMS:
  GET /api/v1/integration/performance-summary
```

---

## 8. قواعد التطوير الثابتة

```
✅ Architecture   : API-First – كل شيء عبر REST API
✅ الكود          : English دائماً (variables, methods, classes)
✅ التعليقات      : عربي أو إنجليزي – المهم الوضوح
✅ SOLID          : Strategy Pattern للـ Auth، Interfaces للـ DI
✅ DRY            : لا تكرار – shared base classes
✅ KISS           : بساطة مقصودة – لا over-engineering
✅ UAE DLS أولاً  : استخدم مكون DLS قبل بناء مكون مخصص
✅ i18n           : كل نص في ar.json/en.json – لا نصوص في JSX
✅ Multi-Tenant   : TenantDbContext حصراً – لا تعديل مباشر على DB
✅ Tenant Config  : لا hard-coded values – كل شيء من tenant JSON
✅ JWT Claims     : TenantKey + UserId + Roles + Language
✅ Session        : IDistributedCache – يُخزّن بعد كل login
✅ API Response   : ApiResponse<T> موحد دائماً
✅ Errors         : Result<T> – لا exceptions تصل للعميل
✅ Delete         : Soft Delete دائماً (IsDeleted = true)
✅ Audit          : كل تغيير في AuditLogs تلقائياً
✅ Bilingual DB   : NameAr + NameEn على كل Entity رئيسية
✅ Collation      : Arabic_CI_AS في كل Tenant DB
✅ EF Migrations  : Code First حصراً – لا SQL يدوي
✅ Versioning     : /api/v1/ – جاهز لـ v2 مستقبلاً
✅ Flutter-Ready  : نفس الـ API – نفس JWT – نفس JSON format
✅ Swagger        : Swashbuckle v6 + TenantKey header + Bearer JWT
```

---

## 9. الأسواق والتسعير

```
🇦🇪 UAE (#1) : One-Time License – ابدأ هنا
🇸🇦 KSA (#2) : One-Time License – تعديل ~15% (SAMA + NCA ECC + PDPL)
🇶🇦 QAT (#3) : One-Time License – تعديل ~10% (NCSA)
🇸🇾 SYR (#4) : Annual SaaS – تعديل ~5%

Bundle IGMS + HPMS:
  UAE: 100,000 – 150,000 AED (خصم 15-20%)
  KSA: 130,000 – 180,000 SAR
```

---

## 10. روابط مرجعية

```
UAE Design System   : https://designsystem.gov.ae
DLS React Package   : @aegov/design-system-react (TailwindCSS v3.4)
UAE Pass Developer  : https://developer.uaepass.ae
UAE Pass Sandbox    : https://stg-id.uaepass.ae
UAE Pass Production : https://id.uaepass.ae
رؤية الإمارات 2031  : https://u.ae/en/about-the-uae/strategies-initiatives-and-awards/we-the-uae-2031-vision
```

---
*Shared Platform Context – الإصدار 2.1 – أبريل 2026*
*تحديث: إضافة Session (IDistributedCache) + Auth Strategies + UAE Pass + Code First EF*
