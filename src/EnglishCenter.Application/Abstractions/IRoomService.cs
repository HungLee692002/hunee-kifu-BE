using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;

namespace EnglishCenter.Application.Abstractions;

public interface IRoomService
{
    Task<PagedResult<RoomDto>> GetPagedAsync(PagedQuery query, CancellationToken cancellationToken = default);
    Task<RoomDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoomDto> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default);
    Task<RoomDto> UpdateAsync(Guid id, UpdateRoomRequest request, CancellationToken cancellationToken = default);
    Task<RoomDto> PatchAsync(Guid id, PatchRoomRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
