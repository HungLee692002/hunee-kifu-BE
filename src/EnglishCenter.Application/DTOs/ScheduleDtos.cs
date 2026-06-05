namespace EnglishCenter.Application.Dtos;

public record ActiveScheduleTemplateDto(
    Guid Id,
    Guid ClassId,
    string ClassName,
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid RoomId,
    string RoomCode,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo);

public record WeeklyScheduleTemplateDto(
    Guid Id,
    Guid ClassId,
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid RoomId,
    string TeachingMode,
    Guid PrimaryTeacherId,
    Guid? LocalSupportTeacherId,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateWeeklyScheduleTemplateRequest(
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid RoomId,
    string TeachingMode,
    Guid PrimaryTeacherId,
    Guid? LocalSupportTeacherId,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive);

public record UpdateWeeklyScheduleTemplateRequest(
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid RoomId,
    string TeachingMode,
    Guid PrimaryTeacherId,
    Guid? LocalSupportTeacherId,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive);

public record PatchWeeklyScheduleTemplateRequest(
    int? DayOfWeek,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    Guid? RoomId,
    string? TeachingMode,
    Guid? PrimaryTeacherId,
    Guid? LocalSupportTeacherId,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    bool? IsActive);

public record LessonSessionStaffDto(
    Guid Id,
    Guid TeacherId,
    string StaffRole,
    decimal PayMultiplier);

public record LessonSessionDto(
    Guid Id,
    Guid ClassId,
    Guid? WeeklyScheduleTemplateId,
    DateOnly SessionDate,
    string TeachingMode,
    string Status,
    TimeOnly EffectiveStartTime,
    TimeOnly EffectiveEndTime,
    Guid EffectiveRoomId,
    bool HasOverride,
    IReadOnlyList<LessonSessionStaffDto> Staff,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record LessonScheduleOverrideDto(
    Guid Id,
    Guid LessonSessionId,
    Guid? OverridePrimaryTeacherId,
    Guid? OverrideSupportTeacherId,
    Guid? OverrideRoomId,
    TimeOnly? OverrideStartTime,
    TimeOnly? OverrideEndTime,
    bool IsCancelled,
    string? Reason,
    DateTime IncidentAt);

public record UpsertLessonScheduleOverrideRequest(
    Guid? OverridePrimaryTeacherId,
    Guid? OverrideSupportTeacherId,
    Guid? OverrideRoomId,
    TimeOnly? OverrideStartTime,
    TimeOnly? OverrideEndTime,
    bool IsCancelled,
    string? Reason);
