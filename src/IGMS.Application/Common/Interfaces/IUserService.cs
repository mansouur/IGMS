using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

public interface IUserService
{
    Task<Result<PagedResult<UserListDto>>>  GetPagedAsync(UserQuery query);
    Task<Result<UserDetailDto>>             GetByIdAsync(int id);
    Task<Result<UserDetailDto>>             CreateAsync(CreateUserRequest request, string createdBy);
    Task<Result<UserDetailDto>>             UpdateAsync(UpdateUserRequest request, string modifiedBy);
    Task<Result<bool>>                      DeleteAsync(int id, string deletedBy);
    Task<Result<bool>>                      SetActiveAsync(int id, bool isActive, string modifiedBy);

    /// <summary>Returns Id + FullNameAr for all active users – used by UserIdPicker.</summary>
    Task<Result<List<UserListDto>>>         GetLookupAsync();

    Task<Result<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<byte[]>       ExportAsync(UserQuery query);
}
