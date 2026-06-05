namespace EnglishCenter.Application.DTOs;

public record CreateEnrollmentRequest(
    Guid StudentId,
    DateOnly EnrolledAt,
    decimal? MonthlyTuitionAmount);

public record PatchEnrollmentRequest(
    string Status,
    DateOnly? EndedAt);

public record EnrollmentListFilter(string? Status = null);

public record EnrollmentResponse(
    Guid Id,
    Guid StudentId,
    Guid ClassId,
    string Status,
    DateOnly EnrolledAt,
    DateOnly? EndedAt,
    decimal? MonthlyTuitionAmount,
    AuditFieldsDto? Audit = null);
