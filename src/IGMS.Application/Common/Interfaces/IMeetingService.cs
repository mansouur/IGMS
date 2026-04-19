using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

public interface IMeetingService
{
    Task<PagedResult<MeetingListDto>> GetPagedAsync(MeetingQuery query, int currentUserId);
    Task<MeetingDetailDto?>           GetByIdAsync(int id);
    Task<MeetingDetailDto>            CreateAsync(SaveMeetingRequest req, int organizerId);
    Task<MeetingDetailDto>            UpdateAsync(int id, SaveMeetingRequest req);
    Task                              DeleteAsync(int id);
    Task<MeetingDetailDto>            StartAsync(int id);
    Task<MeetingDetailDto>            CompleteAsync(int id, SaveMinutesRequest req);
    Task<MeetingDetailDto>            CancelAsync(int id);
    Task<MeetingActionItemDto>        CompleteActionAsync(int meetingId, int actionId);
}
