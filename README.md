# منظومة المنتجات النهائية – Arab Gov Digital Suite
## القرار النهائي: منتجان فقط

---

## المنتجات المعتمدة

```
المنتج 1: IGMS  – نظام الحوكمة المؤسسية      ← Phase 0 مكتمل ✅
المنتج 2: HPMS  – نظام إدارة أداء الموظفين    ← يكمّل IGMS ويرفع قيمة البيع
```

## المنتجات المستبعدة ولماذا

```
❌ MMS  (إدارة الاجتماعات) ← سوق مشبع بـ Convene وDiligent وBoardPAC
❌ SQMS (جودة الخدمات)     ← موجود حكومياً: nCRM + Customer Pulse من TDRA
```

---

## كيف تستخدم الملفات

### لمحادثة IGMS الجديدة:
```
انسخ: 00_SHARED_PLATFORM.md + 01_IGMS_CONTEXT.md
```

### لمحادثة HPMS الجديدة:
```
انسخ: 00_SHARED_PLATFORM.md + 02_HPMS_CONTEXT.md
```

---

## الاستراتيجية التجارية النهائية

```
البيع المنفرد:
  IGMS  → 63–95K AED   (One-Time)
  HPMS  → 55–80K AED   (One-Time)

Bundle (الاثنان معاً):
  IGMS + HPMS → 100–150K AED (خصم 15–20%)

التوسع الجغرافي:
  🇦🇪 UAE (#1) → ابدأ هنا – أنت تعرف السوق
  🇸🇦 KSA (#2) → تعديل ~15% (SAMA + NCA ECC)
  🇶🇦 QAT (#3) → تعديل ~10% (NCSA)
  🇸🇾 SYR (#4) → Annual SaaS – سوق مختلف
```

---

## حالة البناء

### IGMS – Phase 0 ✅ مكتمل
```
✅ Solution Structure (Clean Architecture)
✅ Domain: AuditableEntity, ApiResponse<T>, Result<T>, PagedResult<T>
✅ Application: Interfaces + Models + Strategy Pattern
✅ Infrastructure: TenantDbContext (Code First) + Auth Strategies + Session
✅ API: JWT + Swagger + TenantMiddleware + Auth/UaePass Controllers
✅ Web: React 18 + UAE DLS + i18n + Login (Local/AD/UaePass)
```

### IGMS – Phase 1 ✅ مكتمل
```
✅ DB Schema: 7 Entities + EF Configurations + Seed Data
✅ EF Migrations: InitialCreate + FixQueryFilters → DB: IGMS_UAE_SPORT على MANSOUR
✅ RBAC: 37 Permissions × 4 Roles × UserRoles (مع ExpiresAt)
✅ BCrypt Auth: LocalAuthStrategy تقرأ من DB وتُدرج الصلاحيات في JWT
✅ JWT Claims: UserId + Username + TenantKey + Roles + Permissions + Language
✅ React Layout: AppLayout (Sidebar + TopBar) + Dashboard
✅ Sidebar: مدفوع بالصلاحيات (hasPermission) – 10 modules
✅ authStore: يفكّك JWT تلقائياً → roles + permissions + fullNameAr/En

Auth Modes: Local | AD | UaePass | Mixed
Session: IDistributedCache (Memory → Redis بسطر واحد)
```

### ترتيب البناء القادم
```
1. IGMS Phase 2  ← RACI Module (القيمة الأوضح للعميل)
2. IGMS Phase 3  ← Policies + Approval Lifecycle
3. IGMS Phase 4  ← Tasks + Flutter MVP
4. IGMS Phase 5  ← Dashboard KPIs (RAG: Red/Amber/Green)
5. IGMS Phase 6  ← Audit Log Viewer + Reports
6. IGMS Phase 7  ← Go-Live + Tenant 2
7. HPMS Phase 0  ← يبدأ بعد IGMS Phase 2
```

---

## تشغيل المشروع (IGMS)

```powershell
# Terminal 1 – API
cd src/IGMS.API
dotnet run
# → http://localhost:5257
# → http://localhost:5257/swagger

# Terminal 2 – React
cd src/IGMS.Web
npm run dev
# → http://localhost:5173

# Dev Credentials
# Swagger Header: X-Tenant-Key = uae-sport
# Login: admin / Admin@123
```

---

## بيئة التطوير

```
SQL Server  : 2025 Standard Developer (Server: MANSOUR, Windows Auth)
.NET SDK    : 9.0.311 (targets net8.0)
Node.js     : v24.14.1 / npm 11.11.0
OS          : Windows 11 / PowerShell
IDE         : VS Code + C# Dev Kit + ms-mssql + Tailwind + ESLint
```

---
*القرار النهائي – أبريل 2026 | Phase 0 + Phase 1 مكتملان*
