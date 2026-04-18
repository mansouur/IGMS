using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

public interface ITaskService
{
    Task<Result<PagedResult<TaskListDto>>> GetPagedAsync(TaskQuery query);
    Task<Result<TaskDetailDto>>            GetByIdAsync(int id);
    Task<Result<TaskDetailDto>>            SaveAsync(SaveTaskRequest request, string by);
    Task<Result<bool>>                     DeleteAsync(int id, string by);
    Task<byte[]>                           ExportAsync(TaskQuery query);
    Task<List<TaskListDto>>                GetByRiskAsync(int riskId);
}
