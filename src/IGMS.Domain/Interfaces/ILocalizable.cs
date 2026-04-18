namespace IGMS.Domain.Interfaces;

/// <summary>
/// Marks any entity that requires bilingual support (Arabic + English).
/// Apply to all main entities: Department, Policy, KPI, Risk, etc.
/// Both fields are stored in DB – no runtime translation needed.
/// </summary>
public interface ILocalizable
{
    string NameAr { get; set; }
    string NameEn { get; set; }
}
