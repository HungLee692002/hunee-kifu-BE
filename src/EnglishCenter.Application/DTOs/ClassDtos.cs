namespace EnglishCenter.Application.Dtos;

public record ClassDto(
    Guid Id,
    string Code,
    Guid CourseId,
    string Name,
    string Status,
    bool GradingEnabled,
    decimal? DefaultMonthlyTuition,
    DateOnly? StartDate,
    DateOnly? EndDate,
    DateTime CreatedAt,
    Guid? CreatedBy,
    DateTime? UpdatedAt,
    Guid? UpdatedBy);

public record CreateClassRequest(
    Guid CourseId,
    string Name,
    string Status,
    bool GradingEnabled,
    decimal? DefaultMonthlyTuition,
    DateOnly? StartDate,
    DateOnly? EndDate);

public record UpdateClassRequest(
    Guid CourseId,
    string Name,
    string Status,
    bool GradingEnabled,
    decimal? DefaultMonthlyTuition,
    DateOnly? StartDate,
    DateOnly? EndDate);

public record PatchClassRequest(
    Guid? CourseId = null,
    string? Name = null,
    string? Status = null,
    bool? GradingEnabled = null,
    decimal? DefaultMonthlyTuition = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null);

public record EnrollmentDto(
    Guid Id,
    Guid StudentId,
    Guid ClassId,
    string Status,
    DateOnly EnrolledAt,
    DateOnly? EndedAt,
    decimal? MonthlyTuitionAmount,
    DateTime CreatedAt,
    Guid? CreatedBy,
    DateTime? UpdatedAt,
    Guid? UpdatedBy);

public record CreateEnrollmentRequest(
    Guid StudentId,
    DateOnly EnrolledAt,
    decimal? MonthlyTuitionAmount);

public record PatchEnrollmentRequest(string? Status = null, DateOnly? EndedAt = null);
