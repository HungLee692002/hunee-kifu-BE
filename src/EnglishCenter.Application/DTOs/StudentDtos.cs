namespace EnglishCenter.Application.Dtos;

public record StudentDto(
    Guid Id,
    string Code,
    string FullName,
    DateOnly? DateOfBirth,
    int? Gender,
    string? Phone,
    string? Email,
    string? Address,
    string Status,
    Guid? CurrentEnrollmentId,
    decimal? PerLessonTuition,
    string? Note,
    DateTime CreatedAt,
    Guid? CreatedBy,
    DateTime? UpdatedAt,
    Guid? UpdatedBy);

public record CreateStudentRequest(
    string FullName,
    DateOnly? DateOfBirth,
    int? Gender,
    string? Phone,
    string? Email,
    string? Address,
    string Status,
    decimal? PerLessonTuition,
    string? Note);

public record UpdateStudentRequest(
    string FullName,
    DateOnly? DateOfBirth,
    int? Gender,
    string? Phone,
    string? Email,
    string? Address,
    string Status,
    decimal? PerLessonTuition,
    string? Note);

public record GuardianDto(
    Guid Id,
    Guid StudentId,
    string FullName,
    string? Relationship,
    string Phone,
    string? Email,
    bool IsPrimary,
    DateTime CreatedAt,
    Guid? CreatedBy,
    DateTime? UpdatedAt,
    Guid? UpdatedBy);

public record CreateGuardianRequest(
    string FullName,
    string? Relationship,
    string Phone,
    string? Email,
    bool IsPrimary);
