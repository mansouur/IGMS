using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

public interface IVendorService
{
    Task<PagedResult<VendorListDto>>  GetPagedAsync(VendorQuery query);
    Task<VendorDetailDto?>            GetByIdAsync(int id);
    Task<VendorDetailDto>             CreateAsync(SaveVendorRequest req);
    Task<VendorDetailDto>             UpdateAsync(int id, SaveVendorRequest req);
    Task                              DeleteAsync(int id);
    Task<VendorDetailDto>             AssessRiskAsync(int id, AssessVendorRiskRequest req);
}
