# IGMS Context – نظام الحوكمة المؤسسية
## الإصدار 3.0 – أبريل 2026
### انسخ هذا الملف بعد 00_SHARED_PLATFORM.md في كل محادثة جديدة

---

## ما تم بناؤه فعلياً

### Phase 0 ✅ مكتمل
```
✅ Solution Structure (Clean Architecture)
✅ Domain: AuditableEntity, ApiResponse<T>, Result<T>, PagedResult<T>
✅ Application: Interfaces + Models + Strategy Pattern
✅ Infrastructure: TenantDbContext (Code First) + Auth Strategies + Session
✅ API: JWT + Swagger + TenantMiddleware + Auth/UaePass Controllers
✅ Web: React 18 + UAE DLS + i18n + Login (Local/AD/UaePass)
```

### Phase 1 ✅ مكتمل
```
✅ DB Schema – Entities + EF Configurations + Seed Data
✅ EF Migrations: InitialCreate + FixQueryFilters → DB: IGMS_UAE_SPORT على MANSOUR
✅ RBAC كامل – Roles × Permissions × UserRoles (37 Permission, 4 Roles)
✅ BCrypt Auth – LocalAuthStrategy تقرأ من DB وتُضمّن الصلاحيات في JWT
✅ JWT يحمل: UserId + Username + TenantKey + Roles + Permissions + Language
✅ React Layout – AppLayout (Sidebar + TopBar) + Dashboard
✅ Sidebar مدفوع بالصلاحيات – كل عنصر يظهر فقط إذا كان للمستخدم hasPermission()
✅ authStore يفكّك JWT تلقائياً → roles + permissions + fullName
```

---

## Solution Structure
```
IGMS/
├── IGMS.sln
├── README.md                       ← حالة البناء + تشغيل المشروع
├── tenants/
│   └── uae-sport.json              ← Dev tenant (server: MANSOUR, mode: Mixed)
├── scripts/
│   └── provision-tenant.ps1
└── src/
    ├── IGMS.Domain/
    ├── IGMS.Application/
    ├── IGMS.Infrastructure/
    ├── IGMS.API/                   ← http://localhost:5257
    └── IGMS.Web/                   ← http://localhost:5173
```

---

## IGMS.Domain – Entities

```
Common/
  AuditableEntity.cs    ← Id, CreatedAt/By, ModifiedAt/By, IsDeleted, DeletedAt/By
  ApiResponse<T>.cs     ← Ok / Created / Fail / NotFound / Unauthorized
  Result<T>.cs          ← Success(value) / Failure(error)
  PagedResult<T>.cs     ← Items + TotalCount + CurrentPage + PageSize
Interfaces/
  ILocalizable.cs       ← NameAr + NameEn

Entities/
  Department.cs         ← Self-referencing hierarchy, ManagerId FK, Level, Code
  UserProfile.cs        ← BCrypt PasswordHash, AdObjectId, UaePassSub, EmiratesId
  Role.cs               ← IsSystemRole, IsActive, Code (ADMIN/MANAGER/USER/VIEWER)
  Permission.cs         ← Module + Action + Code ("MODULE.ACTION")
  UserRole.cs           ← Composite PK (UserId+RoleId), ExpiresAt, IsActive (computed)
  RolePermission.cs     ← Composite PK (RoleId+PermissionId), GrantedBy/At
  AuditLog.cs           ← long Id (حجم عالٍ), OldValues/NewValues (JSON)
```

---

## IGMS.Application

```
Common/Interfaces/
  ITenantDbContext.cs       ← Abstraction عن EF (Application لا يعرف EF)
  IJwtService.cs            ← GenerateToken(userId, username, roles, permissions, tenantKey, language)
  IAuthStrategy.cs          ← ProviderName + AuthenticateAsync
  IAuthService.cs           ← (deprecated – لا يُستخدم)
  ICurrentUserService.cs    ← UserId, Username, TenantKey, Language, Roles, IsAuthenticated
  ITenantConfigLoader.cs    ← LoadAsync(tenantKey) → TenantContext?
  ISessionService.cs        ← CreateSession / GetSession / RevokeSession
  IUaePassService.cs        ← BuildAuthorizationUrl + ExchangeCodeAsync

Common/Models/
  TenantContext.cs          ← TenantKey, ConnectionString, Organization, Authentication.Mode
  AuthModels.cs             ← LoginRequest, LoginResponse (Token+SessionId+FullName+Roles)
  SessionData.cs            ← SessionId, UserId, TenantKey, Roles, AuthProvider, ExpiresAt
```

---

## IGMS.Infrastructure

```
Persistence/
  TenantDbContext.cs              ← EF Core 8 Code First, Arabic_CI_AS
  TenantDbContextFactory.cs       ← IDesignTimeDbContextFactory (للـ EF CLI فقط)
  DbSeeder.cs                     ← HasData: 37 Permissions, 4 Roles, Admin user (BCrypt)
  Migrations/
    InitialCreate                 ← Schema كامل + Seed Data
    FixQueryFilters               ← Global query filters على UserRole + RolePermission
  Configurations/
    UserProfileConfiguration.cs   ← HasQueryFilter(!IsDeleted), Unique indexes
    DepartmentConfiguration.cs    ← Self-referencing, Cascade rules
    RoleConfiguration.cs
    PermissionConfiguration.cs
    UserRoleConfiguration.cs      ← HasQueryFilter(!Role.IsDeleted && !User.IsDeleted)
    RolePermissionConfiguration.cs← HasQueryFilter(!Role.IsDeleted && !Permission.IsDeleted)
    AuditLogConfiguration.cs

Auth/
  JwtService.cs                   ← Claims: UserId+Username+TenantKey+Roles+Permissions+Language
  Strategies/
    LocalAuthStrategy.cs          ← BCrypt.Verify + Include chain (Roles→Permissions) + LastLoginAt
    AdAuthStrategy.cs             ← LDAP via System.DirectoryServices.AccountManagement
  LocalAuthService.cs             ← (deprecated Phase 0 – غير مسجّل في DI)
  UaePassService.cs               ← OAuth2 PKCE (stg-id.uaepass.ae)

Services/
  SessionService.cs               ← IDistributedCache, TTL = JWT expiry
  CurrentUserService.cs           ← Claims من IHttpContextAccessor

DependencyInjection.cs            ← RegisterSession + RegisterTenancy + RegisterDatabase +
                                     RegisterAuthStrategies + RegisterUaePass + RegisterServices
```

---

## IGMS.API

```
Middleware/
  TenantMiddleware.cs             ← X-Tenant-Key → TenantContext في HttpContext.Items
                                     Exempt: /health, /swagger, /favicon.ico

Controllers/
  AuthController.cs               ← POST /login / /login/ad / /logout
                                     GET  /methods
  UaePassController.cs            ← GET  /uaepass/redirect / /uaepass/callback

Extensions/
  SwaggerExtensions.cs            ← Swashbuckle v6 + TenantKey ApiKey header + Bearer JWT
  JwtExtensions.cs                ← JWT Bearer validation
```

---

## IGMS.Web (React)

```
src/
  index.html                      ← dir="rtl" lang="ar" من البداية
  i18n/
    ar.json / en.json / index.js  ← auth + common + nav + dashboard
  services/
    api.js                        ← Axios + X-Tenant-Key + 401 redirect
                                     authApi: login/loginAd/logout/getMethods/getUaePassRedirect
  store/
    authStore.js                  ← Zustand + decodeJwt() → roles/permissions/fullName
                                     hasPermission(code) + hasRole(name)
  components/layout/
    AppLayout.jsx                 ← TopBar + Sidebar + <Outlet /> (React Router)
    Sidebar.jsx                   ← permission-driven nav, collapsible, 10 modules
    TopBar.jsx                    ← اسم النظام + Language toggle + User avatar + Logout
  pages/
    Login.jsx                     ← UAE Pass button + Local form
    AuthCallback.jsx              ← يستقبل #token=...&sessionId=... من UAE Pass
    Dashboard.jsx                 ← KPI cards × 4 + Recent Activity table
  App.jsx                         ← ProtectedRoute + GuestRoute + AppLayout كـ outlet
```

---

## RBAC Schema

```
Modules: USERS / DEPARTMENTS / RACI / POLICIES / TASKS / KPI / RISKS / REPORTS / SETTINGS
Actions: READ / CREATE / UPDATE / DELETE / APPROVE / PUBLISH / EXPORT / ASSIGN / MANAGE

Roles:
  ADMIN   → جميع الـ 37 صلاحية
  MANAGER → الكل ما عدا USERS.MANAGE و SETTINGS.UPDATE
  USER    → كل READ + TASKS.CREATE/UPDATE/ASSIGN
  VIEWER  → كل READ فقط

Permission code format: "MODULE.ACTION"  (مثال: "RACI.APPROVE")
JWT claim key: "permission" (يتكرر بعدد الصلاحيات)
```

---

## بيئة التطوير

```
SQL Server  : 2025 Standard Developer (Server: MANSOUR, Windows Auth)
DB          : IGMS_UAE_SPORT (مُنشأ، Migrations مُطبَّقة، Seed مكتمل)
.NET SDK    : 9.0.311 (targets net8.0)
Node.js     : v24.14.1 / npm 11.11.0
API Port    : http://localhost:5257
React Port  : http://localhost:5173
Swagger     : http://localhost:5257/swagger
```

---

## تشغيل المشروع

```powershell
# Terminal 1 – API
cd C:\Users\manso\source\repos\IGSM
dotnet run --project src/IGMS.API
# → http://localhost:5257/swagger

# Terminal 2 – React
cd C:\Users\manso\source\repos\IGMS\src\IGMS.Web
npm run dev
# → http://localhost:5173

# Dev Credentials
# Swagger Header : X-Tenant-Key = uae-sport
# Login          : admin / Admin@123
```

> **ملاحظة:** عند تغيير الكود تأكد من تسجيل خروج في المتصفح ثم إعادة الدخول
> لأن الـ sessionStorage يخزّن التوكن القديم.

---

## Auth Modes (tenant config)

```
"Local"   → Local form فقط (BCrypt + DB)
"AD"      → AD form فقط (LDAP)
"UaePass" → UAE Pass button فقط
"Mixed"   → كل الخيارات (الحالي: uae-sport.json)
```

---

## UAE Pass Integration

```
Sandbox URL : https://stg-id.uaepass.ae
Production  : https://id.uaepass.ae (غيّر BaseUrl فقط)
Flow        : Authorization Code

ملاحظة: UAE Pass لا يقبل localhost كـ Redirect URI
الحل للتطوير: ngrok
```

---

## ما تبقى (Phase 2+)

```
Phase 2 – RACI Module        ← أعلى قيمة للعميل – يستحق البدء هنا
Phase 3 – Policies + Approval Lifecycle
Phase 4 – Tasks + Flutter MVP
Phase 5 – Dashboard KPIs (RAG: Red/Amber/Green)
Phase 6 – Audit Log Viewer + Reports
Phase 7 – Go-Live + Tenant 2
HPMS     ← يبدأ بعد IGMS Phase 2
```

---

## NuGet Packages

```
IGMS.Infrastructure:
  Microsoft.EntityFrameworkCore.SqlServer          8.0.x
  Microsoft.EntityFrameworkCore.Design             8.0.x
  Microsoft.AspNetCore.Authentication.JwtBearer    8.0.x
  Microsoft.Extensions.Caching.StackExchangeRedis  10.0.x
  System.DirectoryServices.AccountManagement       10.0.x
  BCrypt.Net-Next                                  4.1.0

IGMS.API:
  Swashbuckle.AspNetCore    6.x   ← لا تُحدّث لـ v10 (breaking changes)
  Microsoft.EntityFrameworkCore.Design  8.0.x
```

---

*IGMS Context File – الإصدار 3.0 – أبريل 2026*
*Phase 0 + Phase 1 مكتملان: Auth + RBAC + EF Migrations + React Layout*
