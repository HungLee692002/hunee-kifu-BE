namespace EnglishCenter.Application.Dtos;

public record TeacherDto(
    Guid Id,
    string Code,
    string FullName,
    string TeacherType,
    decimal? CurrentLessonRate,
    string? Phone,
    string? Email,
    string Status,
    Guid? UserId,
    string? Note,
    DateTime CreatedAt,
    Guid? CreatedBy,
    DateTime? UpdatedAt,
    Guid? UpdatedBy);

public record CreateTeacherRequest(
    string FullName,
    string TeacherType,
    string? Phone,
    string? Email,
    string Status,
    Guid? UserId,
    string? Note);

public record UpdateTeacherRequest(
    string FullName,
    string TeacherType,
    string? Phone,
    string? Email,
    string Status,
    Guid? UserId,
    string? Note);

public record TeacherLessonRateDto(
    Guid Id,
    Guid TeacherId,
    decimal LessonRate,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive,
    string? Note,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateTeacherLessonRateRequest(
    decimal LessonRate,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string? Note);
