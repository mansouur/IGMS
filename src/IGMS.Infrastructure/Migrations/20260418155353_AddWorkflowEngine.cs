using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IGMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegulatoryFrameworks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegulatoryFrameworks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegulatoryControls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegulatoryFrameworkId = table.Column<int>(type: "int", nullable: false),
                    ControlCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DomainAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DomainEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegulatoryControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegulatoryControls_RegulatoryFrameworks_RegulatoryFrameworkId",
                        column: x => x.RegulatoryFrameworkId,
                        principalTable: "RegulatoryFrameworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowDefinitionId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrentStageOrder = table.Column<int>(type: "int", nullable: true),
                    SubmittedById = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_UserProfiles_SubmittedById",
                        column: x => x.SubmittedById,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowDefinitionId = table.Column<int>(type: "int", nullable: false),
                    StageOrder = table.Column<int>(type: "int", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredRoleId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowStages_Roles_RequiredRoleId",
                        column: x => x.RequiredRoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkflowStages_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ControlMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegulatoryControlId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    ComplianceStatus = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlMappings_RegulatoryControls_RegulatoryControlId",
                        column: x => x.RegulatoryControlId,
                        principalTable: "RegulatoryControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstanceActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: false),
                    StageOrder = table.Column<int>(type: "int", nullable: false),
                    ActorId = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstanceActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstanceActions_UserProfiles_ActorId",
                        column: x => x.ActorId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowInstanceActions_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DescriptionAr", "DescriptionEn", "IsDeleted", "ModifiedAt", "ModifiedBy", "Module" },
                values: new object[,]
                {
                    { 43, "READ", "WORKFLOWS.READ", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "عرض تعريفات سير العمل", "View Workflow Definitions", false, null, null, "WORKFLOWS" },
                    { 44, "MANAGE", "WORKFLOWS.MANAGE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إدارة سير العمل", "Manage Workflow Definitions", false, null, null, "WORKFLOWS" },
                    { 45, "APPROVE", "WORKFLOWS.APPROVE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "اعتماد في سير العمل", "Approve Workflow Instances", false, null, null, "WORKFLOWS" },
                    { 46, "READ", "COMPLIANCE.READ", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "عرض مكتبة الامتثال", "View Compliance Library", false, null, null, "COMPLIANCE" },
                    { 47, "MANAGE", "COMPLIANCE.MANAGE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إدارة خرائط الامتثال", "Manage Compliance Mappings", false, null, null, "COMPLIANCE" }
                });

            migrationBuilder.InsertData(
                table: "RegulatoryFrameworks",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DescriptionAr", "DescriptionEn", "IsActive", "IsDeleted", "ModifiedAt", "ModifiedBy", "NameAr", "NameEn", "Version" },
                values: new object[,]
                {
                    { 1, "ISO27001", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "معيار دولي لإدارة أمن المعلومات", "International standard for information security management", true, false, null, null, "ISO/IEC 27001:2022", "ISO/IEC 27001:2022", "2022" },
                    { 2, "UAENESA", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "معيار أمن المعلومات الصادر عن الهيئة الوطنية لأمن المعلومات", "UAE National Electronic Security Authority Information Assurance Standards", true, false, null, null, "إطار هيئة الأمن الوطني الإماراتية", "UAE NESA IAS", "2.0" }
                });

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$E1Z716CQAXeUigYowprYRuvV6EP9C3MEpqaw3t9NiCaIz8rgDF.ky");

            migrationBuilder.InsertData(
                table: "RegulatoryControls",
                columns: new[] { "Id", "ControlCode", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DescriptionAr", "DescriptionEn", "DomainAr", "DomainEn", "IsDeleted", "ModifiedAt", "ModifiedBy", "RegulatoryFrameworkId", "TitleAr", "TitleEn" },
                values: new object[,]
                {
                    { 1, "A.5.1", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "سياسات أمن المعلومات", "Policies for information security" },
                    { 2, "A.5.2", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "أدوار ومسؤوليات أمن المعلومات", "Information security roles and responsibilities" },
                    { 3, "A.5.3", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "فصل الواجبات", "Segregation of duties" },
                    { 4, "A.5.4", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "مسؤوليات الإدارة", "Management responsibilities" },
                    { 5, "A.5.5", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "التواصل مع السلطات", "Contact with authorities" },
                    { 6, "A.5.6", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "التواصل مع المجموعات المتخصصة", "Contact with special interest groups" },
                    { 7, "A.5.7", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "استخبارات التهديدات", "Threat intelligence" },
                    { 8, "A.5.8", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "أمن المعلومات في إدارة المشاريع", "Information security in project management" },
                    { 9, "A.5.9", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "جرد المعلومات والأصول المرتبطة بها", "Inventory of information and other associated assets" },
                    { 10, "A.5.10", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "الاستخدام المقبول للمعلومات والأصول", "Acceptable use of information and other associated assets" },
                    { 11, "A.5.11", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "إعادة الأصول", "Return of assets" },
                    { 12, "A.5.12", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "تصنيف المعلومات", "Classification of information" },
                    { 13, "A.5.13", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "وضع علامات على المعلومات", "Labelling of information" },
                    { 14, "A.5.14", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "نقل المعلومات", "Information transfer" },
                    { 15, "A.5.15", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "التحكم في الوصول", "Access control" },
                    { 16, "A.5.16", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "إدارة الهوية", "Identity management" },
                    { 17, "A.5.17", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "معلومات المصادقة", "Authentication information" },
                    { 18, "A.5.18", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "حقوق الوصول", "Access rights" },
                    { 19, "A.5.19", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "أمن المعلومات في علاقات الموردين", "Information security in supplier relationships" },
                    { 20, "A.5.20", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "معالجة أمن المعلومات ضمن اتفاقيات الموردين", "Addressing information security within supplier agreements" },
                    { 21, "A.5.21", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "إدارة أمن المعلومات في سلسلة توريد تكنولوجيا المعلومات", "Managing information security in the ICT supply chain" },
                    { 22, "A.5.22", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "مراقبة الخدمات الخارجية ومراجعتها وإدارة التغييرات", "Monitoring, review and change management of supplier services" },
                    { 23, "A.5.23", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "أمن المعلومات لاستخدام الخدمات السحابية", "Information security for use of cloud services" },
                    { 24, "A.5.24", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "تخطيط إدارة حوادث أمن المعلومات والتحضير لها", "Information security incident management planning and preparation" },
                    { 25, "A.5.25", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "تقييم أحداث أمن المعلومات والقرار بشأنها", "Assessment and decision on information security events" },
                    { 26, "A.5.26", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "الاستجابة لحوادث أمن المعلومات", "Response to information security incidents" },
                    { 27, "A.5.27", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "التعلم من حوادث أمن المعلومات", "Learning from information security incidents" },
                    { 28, "A.5.28", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "جمع الأدلة", "Collection of evidence" },
                    { 29, "A.5.29", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "أمن المعلومات أثناء الاضطرابات", "Information security during disruption" },
                    { 30, "A.5.30", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "استعداد تكنولوجيا المعلومات للاستمرارية", "ICT readiness for business continuity" },
                    { 31, "A.5.31", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "المتطلبات القانونية والتنظيمية والتعاقدية", "Legal, statutory, regulatory and contractual requirements" },
                    { 32, "A.5.32", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "حقوق الملكية الفكرية", "Intellectual property rights" },
                    { 33, "A.5.33", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "حماية السجلات", "Protection of records" },
                    { 34, "A.5.34", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "الخصوصية وحماية المعلومات الشخصية", "Privacy and protection of personal identifiable information" },
                    { 35, "A.5.35", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "مراجعة مستقلة لأمن المعلومات", "Independent review of information security" },
                    { 36, "A.5.36", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "الامتثال للسياسات والمعايير والقواعد", "Compliance with policies, rules and standards for information security" },
                    { 37, "A.5.37", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تنظيمية", "Organizational Controls", false, null, null, 1, "إجراءات التشغيل الموثقة", "Documented operating procedures" },
                    { 38, "A.6.1", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط بشرية", "People Controls", false, null, null, 1, "الفحص", "Screening" },
                    { 39, "A.6.2", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط بشرية", "People Controls", false, null, null, 1, "شروط وأحكام التوظيف", "Terms and conditions of employment" },
                    { 40, "A.6.3", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط بشرية", "People Controls", false, null, null, 1, "التوعية والتثقيف والتدريب على أمن المعلومات", "Information security awareness, education and training" },
                    { 41, "A.6.4", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط بشرية", "People Controls", false, null, null, 1, "عملية التأديب", "Disciplinary process" },
                    { 42, "A.6.5", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط بشرية", "People Controls", false, null, null, 1, "المسؤوليات بعد إنهاء التوظيف أو تغييره", "Responsibilities after termination or change of employment" },
                    { 43, "A.6.6", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط بشرية", "People Controls", false, null, null, 1, "اتفاقيات السرية أو عدم الإفصاح", "Confidentiality or non-disclosure agreements" },
                    { 44, "A.6.7", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط بشرية", "People Controls", false, null, null, 1, "العمل عن بُعد", "Remote working" },
                    { 45, "A.6.8", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط بشرية", "People Controls", false, null, null, 1, "الإبلاغ عن أحداث أمن المعلومات", "Information security event reporting" },
                    { 46, "A.7.1", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط مادية", "Physical Controls", false, null, null, 1, "محيطات الأمن المادي", "Physical security perimeters" },
                    { 47, "A.7.2", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط مادية", "Physical Controls", false, null, null, 1, "الدخول المادي", "Physical entry" },
                    { 48, "A.7.3", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط مادية", "Physical Controls", false, null, null, 1, "تأمين المكاتب والغرف والمرافق", "Securing offices, rooms and facilities" },
                    { 49, "A.7.4", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط مادية", "Physical Controls", false, null, null, 1, "مراقبة الأمن المادي", "Physical security monitoring" },
                    { 50, "A.7.5", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط مادية", "Physical Controls", false, null, null, 1, "الحماية من التهديدات المادية والبيئية", "Protecting against physical and environmental threats" },
                    { 51, "A.7.6", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط مادية", "Physical Controls", false, null, null, 1, "العمل في المناطق الآمنة", "Working in secure areas" },
                    { 52, "A.7.7", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط مادية", "Physical Controls", false, null, null, 1, "سياسة المكتب النظيف وشاشة نظيفة", "Clear desk and clear screen" },
                    { 53, "A.7.8", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط مادية", "Physical Controls", false, null, null, 1, "تحديد موقع المعدات وحمايتها", "Equipment siting and protection" },
                    { 54, "A.7.9", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط مادية", "Physical Controls", false, null, null, 1, "أمن الأصول خارج المبنى", "Security of assets off-premises" },
                    { 55, "A.7.10", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط مادية", "Physical Controls", false, null, null, 1, "وسائط التخزين", "Storage media" },
                    { 56, "A.7.11", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط مادية", "Physical Controls", false, null, null, 1, "المرافق الداعمة", "Supporting utilities" },
                    { 57, "A.7.12", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط مادية", "Physical Controls", false, null, null, 1, "أمن التوصيل", "Cabling security" },
                    { 58, "A.7.13", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط مادية", "Physical Controls", false, null, null, 1, "صيانة المعدات", "Equipment maintenance" },
                    { 59, "A.7.14", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط مادية", "Physical Controls", false, null, null, 1, "التخلص الآمن أو إعادة استخدام المعدات", "Secure disposal or re-use of equipment" },
                    { 60, "A.8.1", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "أجهزة نقاط النهاية للمستخدمين", "User endpoint devices" },
                    { 61, "A.8.2", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "امتيازات الوصول المميز", "Privileged access rights" },
                    { 62, "A.8.3", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "قيود الوصول للمعلومات", "Information access restriction" },
                    { 63, "A.8.4", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "الوصول إلى الكود المصدري", "Access to source code" },
                    { 64, "A.8.5", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "المصادقة الآمنة", "Secure authentication" },
                    { 65, "A.8.6", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "إدارة القدرة", "Capacity management" },
                    { 66, "A.8.7", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "الحماية من البرمجيات الضارة", "Protection against malware" },
                    { 67, "A.8.8", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "إدارة الثغرات التقنية", "Management of technical vulnerabilities" },
                    { 68, "A.8.9", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "إدارة التهيئة", "Configuration management" },
                    { 69, "A.8.10", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "حذف المعلومات", "Information deletion" },
                    { 70, "A.8.11", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "إخفاء البيانات", "Data masking" },
                    { 71, "A.8.12", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "منع تسرب البيانات", "Data leakage prevention" },
                    { 72, "A.8.13", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "النسخ الاحتياطي للمعلومات", "Information backup" },
                    { 73, "A.8.14", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "التكرار لمرافق معالجة المعلومات", "Redundancy of information processing facilities" },
                    { 74, "A.8.15", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "التسجيل", "Logging" },
                    { 75, "A.8.16", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "أنشطة المراقبة", "Monitoring activities" },
                    { 76, "A.8.17", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "مزامنة الساعة", "Clock synchronization" },
                    { 77, "A.8.18", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "استخدام برامج المرافق المميزة", "Use of privileged utility programs" },
                    { 78, "A.8.19", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "تثبيت البرامج على الأنظمة التشغيلية", "Installation of software on operational systems" },
                    { 79, "A.8.20", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "أمن الشبكات", "Networks security" },
                    { 80, "A.8.21", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "أمن خدمات الشبكة", "Security of network services" },
                    { 81, "A.8.22", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "فصل الشبكات", "Segregation of networks" },
                    { 82, "A.8.23", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "تصفية الويب", "Web filtering" },
                    { 83, "A.8.24", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "استخدام التشفير", "Use of cryptography" },
                    { 84, "A.8.25", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "دورة حياة تطوير آمنة", "Secure development life cycle" },
                    { 85, "A.8.26", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "متطلبات أمان التطبيقات", "Application security requirements" },
                    { 86, "A.8.27", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "هندسة النظام الآمنة ومبادئ البناء", "Secure system architecture and engineering principles" },
                    { 87, "A.8.28", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "ترميز آمن", "Secure coding" },
                    { 88, "A.8.29", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "اختبار الأمان في التطوير والقبول", "Security testing in development and acceptance" },
                    { 89, "A.8.30", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "التطوير الخارجي", "Outsourced development" },
                    { 90, "A.8.31", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "فصل بيئات التطوير والاختبار والإنتاج", "Separation of development, test and production environments" },
                    { 91, "A.8.32", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "إدارة التغيير", "Change management" },
                    { 92, "A.8.33", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "معلومات الاختبار", "Test information" },
                    { 93, "A.8.34", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط تقنية", "Technological Controls", false, null, null, 1, "حماية أنظمة المعلومات أثناء اختبار التدقيق", "Protection of information systems during audit testing" },
                    { 94, "NESA-1.1", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "حوكمة أمن المعلومات", "Information Security Governance", false, null, null, 2, "سياسة أمن المعلومات", "Information Security Policy" },
                    { 95, "NESA-1.2", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "حوكمة أمن المعلومات", "Information Security Governance", false, null, null, 2, "إطار حوكمة أمن المعلومات", "Information Security Governance Framework" },
                    { 96, "NESA-1.3", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "حوكمة أمن المعلومات", "Information Security Governance", false, null, null, 2, "أدوار ومسؤوليات أمن المعلومات", "IS Roles and Responsibilities" },
                    { 97, "NESA-1.4", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "حوكمة أمن المعلومات", "Information Security Governance", false, null, null, 2, "التوعية بأمن المعلومات", "Information Security Awareness" },
                    { 98, "NESA-1.5", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "حوكمة أمن المعلومات", "Information Security Governance", false, null, null, 2, "قياس أداء أمن المعلومات", "IS Performance Measurement" },
                    { 99, "NESA-2.1", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "إدارة مخاطر المعلومات", "Information Risk Management", false, null, null, 2, "منهجية إدارة المخاطر", "Risk Management Methodology" },
                    { 100, "NESA-2.2", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "إدارة مخاطر المعلومات", "Information Risk Management", false, null, null, 2, "تحديد المخاطر وتقييمها", "Risk Identification and Assessment" },
                    { 101, "NESA-2.3", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "إدارة مخاطر المعلومات", "Information Risk Management", false, null, null, 2, "معالجة المخاطر", "Risk Treatment" },
                    { 102, "NESA-2.4", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "إدارة مخاطر المعلومات", "Information Risk Management", false, null, null, 2, "مراقبة المخاطر ومراجعتها", "Risk Monitoring and Review" },
                    { 103, "NESA-3.1", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط أمن المعلومات", "Information Security Controls", false, null, null, 2, "التحكم في الوصول المادي", "Physical Access Control" },
                    { 104, "NESA-3.2", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط أمن المعلومات", "Information Security Controls", false, null, null, 2, "التحكم في الوصول المنطقي", "Logical Access Control" },
                    { 105, "NESA-3.3", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط أمن المعلومات", "Information Security Controls", false, null, null, 2, "إدارة الهوية والوصول", "Identity and Access Management" },
                    { 106, "NESA-3.4", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط أمن المعلومات", "Information Security Controls", false, null, null, 2, "تشفير البيانات", "Data Encryption" },
                    { 107, "NESA-3.5", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط أمن المعلومات", "Information Security Controls", false, null, null, 2, "أمن الشبكة والبنية التحتية", "Network and Infrastructure Security" },
                    { 108, "NESA-3.6", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط أمن المعلومات", "Information Security Controls", false, null, null, 2, "إدارة الثغرات", "Vulnerability Management" },
                    { 109, "NESA-3.7", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط أمن المعلومات", "Information Security Controls", false, null, null, 2, "الاستجابة للحوادث", "Incident Response" },
                    { 110, "NESA-3.8", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط أمن المعلومات", "Information Security Controls", false, null, null, 2, "النسخ الاحتياطي واستعادة البيانات", "Data Backup and Recovery" },
                    { 111, "NESA-3.9", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط أمن المعلومات", "Information Security Controls", false, null, null, 2, "أمن التطبيقات", "Application Security" },
                    { 112, "NESA-3.10", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "ضوابط أمن المعلومات", "Information Security Controls", false, null, null, 2, "إدارة أمن الأطراف الثالثة", "Third Party Security Management" },
                    { 113, "NESA-4.1", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "استمرارية الأعمال والتعافي من الكوارث", "Business Continuity & DR", false, null, null, 2, "خطة استمرارية الأعمال", "Business Continuity Plan" },
                    { 114, "NESA-4.2", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "استمرارية الأعمال والتعافي من الكوارث", "Business Continuity & DR", false, null, null, 2, "خطة التعافي من الكوارث", "Disaster Recovery Plan" },
                    { 115, "NESA-4.3", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "استمرارية الأعمال والتعافي من الكوارث", "Business Continuity & DR", false, null, null, 2, "اختبار الاستمرارية", "Continuity Testing" },
                    { 116, "NESA-5.1", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "الامتثال والتدقيق", "Compliance & Audit", false, null, null, 2, "الامتثال للمتطلبات القانونية", "Legal and Regulatory Compliance" },
                    { 117, "NESA-5.2", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "الامتثال والتدقيق", "Compliance & Audit", false, null, null, 2, "مراجعة أمن المعلومات", "Information Security Review" },
                    { 118, "NESA-5.3", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, "الامتثال والتدقيق", "Compliance & Audit", false, null, null, 2, "التدقيق الداخلي", "Internal Audit" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "GrantedAt", "GrantedBy" },
                values: new object[,]
                {
                    { 43, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 44, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 45, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 46, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 47, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 43, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 44, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 45, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 46, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 47, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 43, 3, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 46, 3, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 43, 4, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 46, 4, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ControlMappings_RegulatoryControlId",
                table: "ControlMappings",
                column: "RegulatoryControlId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryControls_RegulatoryFrameworkId",
                table: "RegulatoryControls",
                column: "RegulatoryFrameworkId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceActions_ActorId",
                table: "WorkflowInstanceActions",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceActions_WorkflowInstanceId",
                table: "WorkflowInstanceActions",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_SubmittedById",
                table: "WorkflowInstances",
                column: "SubmittedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_WorkflowDefinitionId",
                table: "WorkflowInstances",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStages_RequiredRoleId",
                table: "WorkflowStages",
                column: "RequiredRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStages_WorkflowDefinitionId",
                table: "WorkflowStages",
                column: "WorkflowDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ControlMappings");

            migrationBuilder.DropTable(
                name: "WorkflowInstanceActions");

            migrationBuilder.DropTable(
                name: "WorkflowStages");

            migrationBuilder.DropTable(
                name: "RegulatoryControls");

            migrationBuilder.DropTable(
                name: "WorkflowInstances");

            migrationBuilder.DropTable(
                name: "RegulatoryFrameworks");

            migrationBuilder.DropTable(
                name: "WorkflowDefinitions");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 43, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 44, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 45, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 46, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 47, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 43, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 44, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 45, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 46, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 47, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 43, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 46, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 43, 4 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 46, 4 });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$hl2SuX2lk/UCw9yeIca.4OX6Hsmls801uY2uMAb86dBnRcOv4Hp.S");
        }
    }
}
