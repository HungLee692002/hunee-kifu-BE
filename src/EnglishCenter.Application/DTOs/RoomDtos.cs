namespace EnglishCenter.Application.Dtos;

public record RoomDto(
    Guid Id,
    string Code,
    string Name,
    int? Capacity,
    string? Floor,
    string? Note,
    bool IsActive,
    DateTime CreatedAt,
    Guid? CreatedBy,
    DateTime? UpdatedAt,
    Guid? UpdatedBy);

public record CreateRoomRequest(
    string Code,
    string Name,
    int? Capacity,
    string? Floor,
    string? Note,
    bool IsActive = true);

public record UpdateRoomRequest(
    string Code,
    string Name,
    int? Capacity,
    string? Floor,
    string? Note,
    bool IsActive);

public record PatchRoomRequest(
    string? Code = null,
    string? Name = null,
    int? Capacity = null,
    string? Floor = null,
    string? Note = null,
    bool? IsActive = null);
