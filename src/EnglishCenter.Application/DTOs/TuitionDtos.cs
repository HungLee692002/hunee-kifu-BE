namespace EnglishCenter.Application.Dtos;

public record StudentTuitionMonthDto(
    Guid Id,
    Guid StudentId,
    Guid EnrollmentId,
    int BillingYear,
    int BillingMonth,
    decimal ExpectedAmount,
    decimal AmountPaid,
    string Status,
    string? Note,
    StudentSummaryDto? Student,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record StudentSummaryDto(Guid Id, string Code, string FullName);

public record TuitionPaymentDto(
    Guid Id,
    Guid StudentTuitionMonthId,
    Guid StudentId,
    decimal Amount,
    string PaymentMethod,
    DateTime PaidAt,
    string? ReferenceNo,
    Guid? ReceivedBy,
    string? Note,
    DateTime CreatedAt);

public record CreateTuitionPaymentRequest(
    int BillingYear,
    int BillingMonth,
    decimal Amount,
    string PaymentMethod,
    DateTime PaidAt,
    string? ReferenceNo,
    string? Note);

public record StudentTuitionMonthListFilter(
    int? Year = null,
    int? Month = null,
    string? Status = null,
    Guid? StudentId = null,
    Guid? ClassId = null);

public record ClassStudentTuitionBillingDto(
    Guid StudentId,
    string StudentCode,
    string FullName,
    Guid EnrollmentId,
    decimal? PerLessonTuition,
    int BillableSessions,
    decimal ExpectedAmount,
    decimal AmountPaid,
    string? MonthStatus,
    Guid? StudentTuitionMonthId);
