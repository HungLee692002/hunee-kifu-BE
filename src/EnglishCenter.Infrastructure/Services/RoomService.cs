using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;
using EnglishCenter.Domain.Entities;
using EnglishCenter.Domain.Exceptions;
using EnglishCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Services;

public sealed class RoomService : IRoomService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public RoomService(AppDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<PagedResult<RoomDto>> GetPagedAsync(PagedQuery query, CancellationToken cancellationToken = default)
    {
        var paged = await _db.Rooms.AsQueryable().ToPagedAsync(query, cancellationToken);
        return MapPaged(paged);
    }

    public async Task<RoomDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy phòng học.");
        return entity.ToDto();
    }

    public async Task<RoomDto> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default)
    {
        if (await _db.Rooms.AnyAsync(r => r.Code == request.Code, cancellationToken))
            throw new ConflictException($"Mã phòng '{request.Code}' đã tồn tại.");

        var entity = new Room
        {
            Code = request.Code,
            Name = request.Name,
            Capacity = request.Capacity,
            Floor = request.Floor,
            Note = request.Note,
            IsActive = request.IsActive,
        };
        entity.SetCreated(_currentUser.UserId, _clock.UtcNow);
        _db.Rooms.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<RoomDto> UpdateAsync(Guid id, UpdateRoomRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy phòng học.");

        if (entity.Code != request.Code &&
            await _db.Rooms.AnyAsync(r => r.Code == request.Code && r.Id != id, cancellationToken))
            throw new ConflictException($"Mã phòng '{request.Code}' đã tồn tại.");

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.Capacity = request.Capacity;
        entity.Floor = request.Floor;
        entity.Note = request.Note;
        entity.IsActive = request.IsActive;
        entity.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<RoomDto> PatchAsync(Guid id, PatchRoomRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy phòng học.");

        if (request.Code is not null)
        {
            if (await _db.Rooms.AnyAsync(r => r.Code == request.Code && r.Id != id, cancellationToken))
                throw new ConflictException($"Mã phòng '{request.Code}' đã tồn tại.");
            entity.Code = request.Code;
        }
        if (request.Name is not null) entity.Name = request.Name;
        if (request.Capacity.HasValue) entity.Capacity = request.Capacity;
        if (request.Floor is not null) entity.Floor = request.Floor;
        if (request.Note is not null) entity.Note = request.Note;
        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;

        entity.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy phòng học.");
        entity.SoftDelete(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static PagedResult<RoomDto> MapPaged(PagedResult<Room> paged) =>
        PagedResult<RoomDto>.Create(
            paged.Items.Select(e => e.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);
}
