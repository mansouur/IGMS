using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Persistence;

/// <summary>
/// Seeds regulatory frameworks (ISO 27001:2022, UAE NESA) and their controls.
/// Run once via EF Migrations HasData.
/// </summary>
internal static class RegulatorySeeder
{
    private static readonly DateTime SD = new(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder mb)
    {
        mb.Entity<RegulatoryFramework>().HasData(BuildFrameworks());
        mb.Entity<RegulatoryControl>().HasData(BuildControls());
    }

    // ─────────────────────── Frameworks ──────────────────────────────────────

    private static RegulatoryFramework[] BuildFrameworks() =>
    [
        new() { Id = 1, Code = "ISO27001", NameAr = "ISO/IEC 27001:2022", NameEn = "ISO/IEC 27001:2022",
            Version = "2022", IsActive = true,
            DescriptionAr = "معيار دولي لإدارة أمن المعلومات",
            DescriptionEn = "International standard for information security management",
            CreatedAt = SD, CreatedBy = "System" },
        new() { Id = 2, Code = "UAENESA", NameAr = "إطار هيئة الأمن الوطني الإماراتية", NameEn = "UAE NESA IAS",
            Version = "2.0", IsActive = true,
            DescriptionAr = "معيار أمن المعلومات الصادر عن الهيئة الوطنية لأمن المعلومات",
            DescriptionEn = "UAE National Electronic Security Authority Information Assurance Standards",
            CreatedAt = SD, CreatedBy = "System" },
    ];

    // ─────────────────────── Controls ────────────────────────────────────────

    private static RegulatoryControl[] BuildControls()
    {
        var list = new List<(int FwId, string Code, string DomainAr, string DomainEn, string TitleAr, string TitleEn)>();

        // ── ISO 27001:2022 ────────────────────────────────────────────────────
        // Section 5 — Organizational controls (A.5.1–A.5.37)
        var orgAr = "ضوابط تنظيمية";
        var orgEn = "Organizational Controls";
        list.AddRange([
            (1,"A.5.1",  orgAr,orgEn,"سياسات أمن المعلومات","Policies for information security"),
            (1,"A.5.2",  orgAr,orgEn,"أدوار ومسؤوليات أمن المعلومات","Information security roles and responsibilities"),
            (1,"A.5.3",  orgAr,orgEn,"فصل الواجبات","Segregation of duties"),
            (1,"A.5.4",  orgAr,orgEn,"مسؤوليات الإدارة","Management responsibilities"),
            (1,"A.5.5",  orgAr,orgEn,"التواصل مع السلطات","Contact with authorities"),
            (1,"A.5.6",  orgAr,orgEn,"التواصل مع المجموعات المتخصصة","Contact with special interest groups"),
            (1,"A.5.7",  orgAr,orgEn,"استخبارات التهديدات","Threat intelligence"),
            (1,"A.5.8",  orgAr,orgEn,"أمن المعلومات في إدارة المشاريع","Information security in project management"),
            (1,"A.5.9",  orgAr,orgEn,"جرد المعلومات والأصول المرتبطة بها","Inventory of information and other associated assets"),
            (1,"A.5.10", orgAr,orgEn,"الاستخدام المقبول للمعلومات والأصول","Acceptable use of information and other associated assets"),
            (1,"A.5.11", orgAr,orgEn,"إعادة الأصول","Return of assets"),
            (1,"A.5.12", orgAr,orgEn,"تصنيف المعلومات","Classification of information"),
            (1,"A.5.13", orgAr,orgEn,"وضع علامات على المعلومات","Labelling of information"),
            (1,"A.5.14", orgAr,orgEn,"نقل المعلومات","Information transfer"),
            (1,"A.5.15", orgAr,orgEn,"التحكم في الوصول","Access control"),
            (1,"A.5.16", orgAr,orgEn,"إدارة الهوية","Identity management"),
            (1,"A.5.17", orgAr,orgEn,"معلومات المصادقة","Authentication information"),
            (1,"A.5.18", orgAr,orgEn,"حقوق الوصول","Access rights"),
            (1,"A.5.19", orgAr,orgEn,"أمن المعلومات في علاقات الموردين","Information security in supplier relationships"),
            (1,"A.5.20", orgAr,orgEn,"معالجة أمن المعلومات ضمن اتفاقيات الموردين","Addressing information security within supplier agreements"),
            (1,"A.5.21", orgAr,orgEn,"إدارة أمن المعلومات في سلسلة توريد تكنولوجيا المعلومات","Managing information security in the ICT supply chain"),
            (1,"A.5.22", orgAr,orgEn,"مراقبة الخدمات الخارجية ومراجعتها وإدارة التغييرات","Monitoring, review and change management of supplier services"),
            (1,"A.5.23", orgAr,orgEn,"أمن المعلومات لاستخدام الخدمات السحابية","Information security for use of cloud services"),
            (1,"A.5.24", orgAr,orgEn,"تخطيط إدارة حوادث أمن المعلومات والتحضير لها","Information security incident management planning and preparation"),
            (1,"A.5.25", orgAr,orgEn,"تقييم أحداث أمن المعلومات والقرار بشأنها","Assessment and decision on information security events"),
            (1,"A.5.26", orgAr,orgEn,"الاستجابة لحوادث أمن المعلومات","Response to information security incidents"),
            (1,"A.5.27", orgAr,orgEn,"التعلم من حوادث أمن المعلومات","Learning from information security incidents"),
            (1,"A.5.28", orgAr,orgEn,"جمع الأدلة","Collection of evidence"),
            (1,"A.5.29", orgAr,orgEn,"أمن المعلومات أثناء الاضطرابات","Information security during disruption"),
            (1,"A.5.30", orgAr,orgEn,"استعداد تكنولوجيا المعلومات للاستمرارية","ICT readiness for business continuity"),
            (1,"A.5.31", orgAr,orgEn,"المتطلبات القانونية والتنظيمية والتعاقدية","Legal, statutory, regulatory and contractual requirements"),
            (1,"A.5.32", orgAr,orgEn,"حقوق الملكية الفكرية","Intellectual property rights"),
            (1,"A.5.33", orgAr,orgEn,"حماية السجلات","Protection of records"),
            (1,"A.5.34", orgAr,orgEn,"الخصوصية وحماية المعلومات الشخصية","Privacy and protection of personal identifiable information"),
            (1,"A.5.35", orgAr,orgEn,"مراجعة مستقلة لأمن المعلومات","Independent review of information security"),
            (1,"A.5.36", orgAr,orgEn,"الامتثال للسياسات والمعايير والقواعد","Compliance with policies, rules and standards for information security"),
            (1,"A.5.37", orgAr,orgEn,"إجراءات التشغيل الموثقة","Documented operating procedures"),
        ]);

        // Section 6 — People controls (A.6.1–A.6.8)
        var pplAr = "ضوابط بشرية"; var pplEn = "People Controls";
        list.AddRange([
            (1,"A.6.1", pplAr,pplEn,"الفحص","Screening"),
            (1,"A.6.2", pplAr,pplEn,"شروط وأحكام التوظيف","Terms and conditions of employment"),
            (1,"A.6.3", pplAr,pplEn,"التوعية والتثقيف والتدريب على أمن المعلومات","Information security awareness, education and training"),
            (1,"A.6.4", pplAr,pplEn,"عملية التأديب","Disciplinary process"),
            (1,"A.6.5", pplAr,pplEn,"المسؤوليات بعد إنهاء التوظيف أو تغييره","Responsibilities after termination or change of employment"),
            (1,"A.6.6", pplAr,pplEn,"اتفاقيات السرية أو عدم الإفصاح","Confidentiality or non-disclosure agreements"),
            (1,"A.6.7", pplAr,pplEn,"العمل عن بُعد","Remote working"),
            (1,"A.6.8", pplAr,pplEn,"الإبلاغ عن أحداث أمن المعلومات","Information security event reporting"),
        ]);

        // Section 7 — Physical controls (A.7.1–A.7.14)
        var physAr = "ضوابط مادية"; var physEn = "Physical Controls";
        list.AddRange([
            (1,"A.7.1",  physAr,physEn,"محيطات الأمن المادي","Physical security perimeters"),
            (1,"A.7.2",  physAr,physEn,"الدخول المادي","Physical entry"),
            (1,"A.7.3",  physAr,physEn,"تأمين المكاتب والغرف والمرافق","Securing offices, rooms and facilities"),
            (1,"A.7.4",  physAr,physEn,"مراقبة الأمن المادي","Physical security monitoring"),
            (1,"A.7.5",  physAr,physEn,"الحماية من التهديدات المادية والبيئية","Protecting against physical and environmental threats"),
            (1,"A.7.6",  physAr,physEn,"العمل في المناطق الآمنة","Working in secure areas"),
            (1,"A.7.7",  physAr,physEn,"سياسة المكتب النظيف وشاشة نظيفة","Clear desk and clear screen"),
            (1,"A.7.8",  physAr,physEn,"تحديد موقع المعدات وحمايتها","Equipment siting and protection"),
            (1,"A.7.9",  physAr,physEn,"أمن الأصول خارج المبنى","Security of assets off-premises"),
            (1,"A.7.10", physAr,physEn,"وسائط التخزين","Storage media"),
            (1,"A.7.11", physAr,physEn,"المرافق الداعمة","Supporting utilities"),
            (1,"A.7.12", physAr,physEn,"أمن التوصيل","Cabling security"),
            (1,"A.7.13", physAr,physEn,"صيانة المعدات","Equipment maintenance"),
            (1,"A.7.14", physAr,physEn,"التخلص الآمن أو إعادة استخدام المعدات","Secure disposal or re-use of equipment"),
        ]);

        // Section 8 — Technological controls (A.8.1–A.8.34)
        var techAr = "ضوابط تقنية"; var techEn = "Technological Controls";
        list.AddRange([
            (1,"A.8.1",  techAr,techEn,"أجهزة نقاط النهاية للمستخدمين","User endpoint devices"),
            (1,"A.8.2",  techAr,techEn,"امتيازات الوصول المميز","Privileged access rights"),
            (1,"A.8.3",  techAr,techEn,"قيود الوصول للمعلومات","Information access restriction"),
            (1,"A.8.4",  techAr,techEn,"الوصول إلى الكود المصدري","Access to source code"),
            (1,"A.8.5",  techAr,techEn,"المصادقة الآمنة","Secure authentication"),
            (1,"A.8.6",  techAr,techEn,"إدارة القدرة","Capacity management"),
            (1,"A.8.7",  techAr,techEn,"الحماية من البرمجيات الضارة","Protection against malware"),
            (1,"A.8.8",  techAr,techEn,"إدارة الثغرات التقنية","Management of technical vulnerabilities"),
            (1,"A.8.9",  techAr,techEn,"إدارة التهيئة","Configuration management"),
            (1,"A.8.10", techAr,techEn,"حذف المعلومات","Information deletion"),
            (1,"A.8.11", techAr,techEn,"إخفاء البيانات","Data masking"),
            (1,"A.8.12", techAr,techEn,"منع تسرب البيانات","Data leakage prevention"),
            (1,"A.8.13", techAr,techEn,"النسخ الاحتياطي للمعلومات","Information backup"),
            (1,"A.8.14", techAr,techEn,"التكرار لمرافق معالجة المعلومات","Redundancy of information processing facilities"),
            (1,"A.8.15", techAr,techEn,"التسجيل","Logging"),
            (1,"A.8.16", techAr,techEn,"أنشطة المراقبة","Monitoring activities"),
            (1,"A.8.17", techAr,techEn,"مزامنة الساعة","Clock synchronization"),
            (1,"A.8.18", techAr,techEn,"استخدام برامج المرافق المميزة","Use of privileged utility programs"),
            (1,"A.8.19", techAr,techEn,"تثبيت البرامج على الأنظمة التشغيلية","Installation of software on operational systems"),
            (1,"A.8.20", techAr,techEn,"أمن الشبكات","Networks security"),
            (1,"A.8.21", techAr,techEn,"أمن خدمات الشبكة","Security of network services"),
            (1,"A.8.22", techAr,techEn,"فصل الشبكات","Segregation of networks"),
            (1,"A.8.23", techAr,techEn,"تصفية الويب","Web filtering"),
            (1,"A.8.24", techAr,techEn,"استخدام التشفير","Use of cryptography"),
            (1,"A.8.25", techAr,techEn,"دورة حياة تطوير آمنة","Secure development life cycle"),
            (1,"A.8.26", techAr,techEn,"متطلبات أمان التطبيقات","Application security requirements"),
            (1,"A.8.27", techAr,techEn,"هندسة النظام الآمنة ومبادئ البناء","Secure system architecture and engineering principles"),
            (1,"A.8.28", techAr,techEn,"ترميز آمن","Secure coding"),
            (1,"A.8.29", techAr,techEn,"اختبار الأمان في التطوير والقبول","Security testing in development and acceptance"),
            (1,"A.8.30", techAr,techEn,"التطوير الخارجي","Outsourced development"),
            (1,"A.8.31", techAr,techEn,"فصل بيئات التطوير والاختبار والإنتاج","Separation of development, test and production environments"),
            (1,"A.8.32", techAr,techEn,"إدارة التغيير","Change management"),
            (1,"A.8.33", techAr,techEn,"معلومات الاختبار","Test information"),
            (1,"A.8.34", techAr,techEn,"حماية أنظمة المعلومات أثناء اختبار التدقيق","Protection of information systems during audit testing"),
        ]);

        // ── UAE NESA IAS 2.0 ──────────────────────────────────────────────────
        // 5 Domains with key controls
        var n1Ar = "حوكمة أمن المعلومات"; var n1En = "Information Security Governance";
        var n2Ar = "إدارة مخاطر المعلومات"; var n2En = "Information Risk Management";
        var n3Ar = "ضوابط أمن المعلومات"; var n3En = "Information Security Controls";
        var n4Ar = "استمرارية الأعمال والتعافي من الكوارث"; var n4En = "Business Continuity & DR";
        var n5Ar = "الامتثال والتدقيق"; var n5En = "Compliance & Audit";

        list.AddRange([
            (2,"NESA-1.1",n1Ar,n1En,"سياسة أمن المعلومات","Information Security Policy"),
            (2,"NESA-1.2",n1Ar,n1En,"إطار حوكمة أمن المعلومات","Information Security Governance Framework"),
            (2,"NESA-1.3",n1Ar,n1En,"أدوار ومسؤوليات أمن المعلومات","IS Roles and Responsibilities"),
            (2,"NESA-1.4",n1Ar,n1En,"التوعية بأمن المعلومات","Information Security Awareness"),
            (2,"NESA-1.5",n1Ar,n1En,"قياس أداء أمن المعلومات","IS Performance Measurement"),
            (2,"NESA-2.1",n2Ar,n2En,"منهجية إدارة المخاطر","Risk Management Methodology"),
            (2,"NESA-2.2",n2Ar,n2En,"تحديد المخاطر وتقييمها","Risk Identification and Assessment"),
            (2,"NESA-2.3",n2Ar,n2En,"معالجة المخاطر","Risk Treatment"),
            (2,"NESA-2.4",n2Ar,n2En,"مراقبة المخاطر ومراجعتها","Risk Monitoring and Review"),
            (2,"NESA-3.1",n3Ar,n3En,"التحكم في الوصول المادي","Physical Access Control"),
            (2,"NESA-3.2",n3Ar,n3En,"التحكم في الوصول المنطقي","Logical Access Control"),
            (2,"NESA-3.3",n3Ar,n3En,"إدارة الهوية والوصول","Identity and Access Management"),
            (2,"NESA-3.4",n3Ar,n3En,"تشفير البيانات","Data Encryption"),
            (2,"NESA-3.5",n3Ar,n3En,"أمن الشبكة والبنية التحتية","Network and Infrastructure Security"),
            (2,"NESA-3.6",n3Ar,n3En,"إدارة الثغرات","Vulnerability Management"),
            (2,"NESA-3.7",n3Ar,n3En,"الاستجابة للحوادث","Incident Response"),
            (2,"NESA-3.8",n3Ar,n3En,"النسخ الاحتياطي واستعادة البيانات","Data Backup and Recovery"),
            (2,"NESA-3.9",n3Ar,n3En,"أمن التطبيقات","Application Security"),
            (2,"NESA-3.10",n3Ar,n3En,"إدارة أمن الأطراف الثالثة","Third Party Security Management"),
            (2,"NESA-4.1",n4Ar,n4En,"خطة استمرارية الأعمال","Business Continuity Plan"),
            (2,"NESA-4.2",n4Ar,n4En,"خطة التعافي من الكوارث","Disaster Recovery Plan"),
            (2,"NESA-4.3",n4Ar,n4En,"اختبار الاستمرارية","Continuity Testing"),
            (2,"NESA-5.1",n5Ar,n5En,"الامتثال للمتطلبات القانونية","Legal and Regulatory Compliance"),
            (2,"NESA-5.2",n5Ar,n5En,"مراجعة أمن المعلومات","Information Security Review"),
            (2,"NESA-5.3",n5Ar,n5En,"التدقيق الداخلي","Internal Audit"),
        ]);

        return list.Select((c, i) => new RegulatoryControl
        {
            Id                    = i + 1,
            RegulatoryFrameworkId = c.FwId,
            ControlCode           = c.Code,
            DomainAr              = c.DomainAr,
            DomainEn              = c.DomainEn,
            TitleAr               = c.TitleAr,
            TitleEn               = c.TitleEn,
            CreatedAt             = SD,
            CreatedBy             = "System",
        }).ToArray();
    }
}
