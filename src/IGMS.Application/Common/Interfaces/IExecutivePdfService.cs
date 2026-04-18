namespace IGMS.Application.Common.Interfaces;

public interface IExecutivePdfService
{
    Task<byte[]> GenerateExecutiveReportAsync(string tenantName);
}
