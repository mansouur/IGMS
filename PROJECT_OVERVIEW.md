# IGMS — Institutional Governance Management System
## نظام إدارة الحوكمة المؤسسية

> نظام SaaS متعدد المستأجرين (Multi-Tenant) لإدارة الحوكمة المؤسسية، مبني على ASP.NET Core 8 وReact 18.
> المستأجر الأول: **وزارة الرياضة – الإمارات العربية المتحدة**
> آخر تحديث: أبريل 2026 — نسبة الاكتمال: **78%**

---

## المشكلة التي يحلها النظام

تعتمد المؤسسات الحكومية على إجراءات حوكمة موزعة بين ملفات Excel وأنظمة منفصلة، مما يجعل من الصعب تتبع المخاطر، قياس الأداء، وضمان الامتثال. IGMS يوحّد هذه العمليات في منصة واحدة متكاملة.

---

## الوحدات الوظيفية المكتملة

| الوحدة | ما بُني فعلاً |
|--------|--------------|
| **RACI** | مصفوفة المسؤوليات — CRUD كامل + Workflow (Draft→UnderReview→Approved→Archived) |
| **المخاطر (Risks)** | CRUD + درجة الخطورة (احتمالية × أثر) + Heatmap + ربط بالمهام والـ KPIs + Lifecycle Visual |
| **السياسات (Policies)** | CRUD + دورة اعتماد + إقرارات + مرفقات (20MB) + Versioning + Renewal + Timeline |
| **المهام (Tasks)** | CRUD + ربط بالمخاطر + تصعيد تلقائي (L1/L2) + Export |
| **مؤشرات الأداء (KPIs)** | CRUD + تتبع تاريخي (Year/Quarter) + ربط بالمخاطر + Department Scorecard |
| **الامتثال (Compliance)** | ربط بـ ISO 31000, COBIT 2019, ISO 27001, UAE NESA, ADAA, TDRA, DSM |
| **التقارير (Reports)** | Executive PDF (QuestPDF) + Department Scorecard + Top Risks + Compliance Report |
| **سجل المراجعة (Audit Log)** | تتبع كامل لكل تغيير — paginated مع filters |
| **المستخدمون والأقسام** | CRUD كامل + هيكل هرمي للأقسام + Excel Export |
| **Dashboard** | ملخصات + Governance Score + Top Risks + Activity |
| **Executive Dashboard** | لوحة المدير التنفيذي — Gauge + تنبيهات + مقارنة أقسام + زر طباعة |

---

## البنية التقنية

```
IGMS/
├── src/
│   ├── IGMS.Domain/          ← 19 Entity — الكيانات والقواعد (لا تبعيات خارجية)
│   ├── IGMS.Application/     ← Interfaces + Models + Strategy Pattern
│   ├── IGMS.Infrastructure/  ← 19 Service + 3 Background Jobs + EF Core + Auth
│   ├── IGMS.API/             ← 15 Controller | 80+ Endpoint — ASP.NET Core 8
│   └── IGMS.Web/             ← 26 Page — React 18 + Vite + Tailwind CSS
└── tenants/
    └── uae-sport.json        ← إعدادات المستأجر (DB, Branding, Auth, Escalation)
```

**النمط المعماري:** Clean Architecture (Domain → Application → Infrastructure → API)

---

## التقنيات المستخدمة

### Backend
| التقنية | الاستخدام |
|--------|-----------|
| ASP.NET Core 8 | REST API — 15 Controller, 80+ Endpoint |
| Entity Framework Core 8 | ORM + Code First — 20 Migration مطبّقة |
| SQL Server 2025 | قاعدة البيانات (Arabic_CI_AS Collation) |
| JWT Bearer | المصادقة — Roles + Permissions مُضمَّنة |
| BCrypt.Net | تشفير كلمات المرور |
| IDistributedCache | الجلسات + OTP (TTL 5 دقائق) |
| MailKit + Mailtrap | إرسال البريد الإلكتروني (قوالب HTML عربية) |
| QuestPDF | توليد تقارير PDF |
| ClosedXML | تصدير Excel |
| BackgroundService | 3 وظائف مجدولة يومية/شهرية |

### Frontend
| التقنية | الاستخدام |
|--------|-----------|
| React 18 + Vite | 26 صفحة |
| Tailwind CSS | RTL-First Design + Mobile Responsive |
| React Router v6 | التنقل + Protected Routes |
| Zustand | Auth Store (JWT decode + permissions) |
| Axios | HTTP Client + Interceptors |
| react-i18next | عربي/إنجليزي كامل |

---

## نظام المصادقة

يدعم النظام أربعة أنماط تُحدَّد per-tenant:

```
Local    → بريد + كلمة مرور (BCrypt)
AD       → Active Directory (LDAP)
UAE Pass → هوية رقمية وطنية (OAuth2 PKCE)
Mixed    → جميع الخيارات (الوضع الحالي)
```

**المصادقة الثنائية (2FA):** كل مستخدم يفعّل 2FA من الإعدادات. عند تسجيل الدخول يُرسَل OTP (6 أرقام) بالبريد — صالح 5 دقائق. واجهة Login بـ 6 مربعات مع auto-advance وpaste support.

---

## نظام الصلاحيات (RBAC)

```
صيغة الصلاحية: MODULE.ACTION   (مثال: RISKS.APPROVE)

الأدوار:
  ADMIN   → 37 صلاحية كاملة
  MANAGER → الكل ما عدا USERS.MANAGE و SETTINGS.UPDATE
  USER    → كل READ + TASKS.CREATE/UPDATE/ASSIGN
  VIEWER  → كل READ فقط

الصلاحيات مُضمَّنة في JWT — لا استعلام DB عند كل طلب
عناصر الـ Sidebar تظهر/تختفي بناءً على hasPermission()
```

---

## السمات الرئيسية

**Multi-Tenant SaaS**
- كل مستأجر: قاعدة بيانات منفصلة + ملف JSON للتهيئة (ألوان، لغة، auth mode، escalation config)
- `X-Tenant-Key` header يُحدد المستأجر في كل طلب

**RTL-First Design**
- مبني أساساً للعربية — Tailwind logical properties (`start-0`, `border-e`)
- Mobile Sidebar: drawer RTL/LTR بـ `ltr:-translate-x-full rtl:translate-x-full`

**التصعيد التلقائي**
- مهام متأخرة: يوم 1–3 → تذكير للمكلف (L1)، يوم 4+ → تصعيد للمدير (L2)
- كل مستوى يُرسَل مرة واحدة فقط (AuditLog كـ deduplication tracker)

**التقارير**
- Executive PDF بزر واحد
- Excel Export على كل وحدة
- Department Scorecard مع مقارنة بصرية

---

## بيئة التطوير

```
Server     : MANSOUR (Windows Auth)
Database   : IGMS_UAE_SPORT
API        : http://localhost:5257
Swagger    : http://localhost:5257/swagger
React      : http://localhost:5173

بيانات دخول:
  Username : admin / Password : Admin@123
  Header   : X-Tenant-Key = uae-sport

SMTP (Mailtrap):
  User: ee956706fd3da5 / Pass: 944f8812605362
```

---

## حالة المشروع — ما تم

```
✅ Phase 0–1  البنية الأساسية + RBAC + Auth (Local/AD/UAE Pass) + EF Migrations
✅ Phase A    RACI Module كامل (CRUD + Workflow)
✅ Phase B    Policies + Tasks + Risks + KPIs + Compliance + Reports + Dashboard
              + Excel Export (كل module) + Executive PDF + Heatmap
              + Policy Versioning/Renewal + KPI History + Risk→Task/KPI Linking
              + 3 Background Jobs + Executive Dashboard + Department Scorecard
✅ Phase C    Mobile Responsive + NaN Guards + Escalation per-tenant
✅ Phase D    Two-Factor Authentication (2FA) كاملة
```

---

## ما تبقى للاكتمال (22%)

### الأولوية القصوى — تحسم العرض التنفيذي

| # | ما ينقص | الأثر |
|---|---------|-------|
| 1 | **Trend Charts في Dashboard** — مخطط خطي لاتجاه KPIs عبر الزمن (Recharts) | المدير يرى التحسن/التراجع دفعة واحدة |
| 2 | **Notification Center** — bell icon في الـ UI + قائمة إشعارات داخلية | المستخدم يعرف مهامه بدون أن يدخل كل صفحة |

### الأولوية العالية — يُغلق الفجوة مع المنافسين

| # | ما ينقص | الأثر |
|---|---------|-------|
| 3 | **Workflow Engine قابل للتهيئة** — دورة اعتماد متعددة المراحل per-tenant (حالياً Approve/Reject مباشر) | يُطابق MetricStream وServiceNow |
| 4 | **Control Testing** — رفع أدلة (evidence) + تقييم فعالية كل ضابط رقابي | أساس COBIT وISO 27001 |
| 5 | **مكتبة الأطر التنظيمية** — UAE NESA و ISO 27001 كـ templates جاهزة للاستيراد | يختصر أشهر من الإعداد على العميل |

### الأولوية المتوسطة — نضج المنتج

| # | ما ينقص | الأثر |
|---|---------|-------|
| 6 | **Unit / Integration Tests** — xUnit على Auth + Escalation + OTP logic | ضمان جودة عند إضافة ميزات جديدة |
| 7 | **RolesController CRUD كامل** — إنشاء/تعديل الأدوار من الـ UI (حالياً lookup فقط) | مرونة أكبر لـ Admins |
| 8 | **Dark Mode** | تجربة مستخدم |
| 9 | **Assessment / Survey** — استبيانات تقييم ذاتي للأقسام | مفيد للامتثال الدوري |

### المستقبل — تمايز استراتيجي

| # | ما ينقص | الأثر |
|---|---------|-------|
| 10 | **Tenant 2 + Go-Live** — توثيق deployment + CI/CD pipeline | إطلاق فعلي للإنتاج |
| 11 | **HPMS** — نظام إدارة الأداء البشري (يشارك Auth + Infrastructure مع IGMS) | المشروع الأخ المخطط |
| 12 | **AI Risk Scoring** — اقتراح درجة الخطورة بناءً على تاريخ المخاطر المشابهة | تمايز عن المنافسين |
| 13 | **Vendor Risk Module** — تقييم مخاطر الموردين والطرف الثالث | يغطي فجوة في السوق |

---

## موقع IGMS التنافسي

| الميزة | ServiceNow | MetricStream | RSA Archer | **IGMS** |
|--------|-----------|-------------|------------|---------|
| UAE Pass | ⬜ | ⬜ | ⬜ | ✅ **فريد** |
| Arabic RTL-First | ⬜ | ⬜ | ⬜ | ✅ **فريد** |
| RACI كوحدة مستقلة | ⬜ خفيف | ⬜ خفيف | ⬜ خفيف | ✅ |
| DB Isolation per-tenant | مشترك | مشترك | مشترك | ✅ |
| السعر السنوي | $80K–$500K | $50K–$300K | $40K–$200K | **أقل بكثير** |

---

*آخر تحديث: أبريل 2026 — بعد قراءة الكود الفعلي*
*الاكتمال: 78% — يحتاج Trend Charts + Notification Center + Control Testing + Tests*
