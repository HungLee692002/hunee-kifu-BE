namespace EnglishCenter.Application.Dtos;

public record SalaryPeriodDto(
    Guid Id,
    int Year,
    int Month,
    string Status,
    DateTime? ClosedAt,
    Guid? ClosedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record PatchSalaryPeriodRequest(string Status);

public record LessonPayRecordDto(
    Guid Id,
    Guid SalaryPeriodId,
    Guid LessonSessionId,
    Guid TeacherId,
    Guid ClassId,
    string StaffRole,
    string TeachingMode,
    decimal BaseLessonRate,
    decimal PayMultiplier,
    decimal PayAmount,
    string Status,
    DateTime CalculatedAt);
