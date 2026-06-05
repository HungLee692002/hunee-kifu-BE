using EnglishCenter.Domain.Common;
using EnglishCenter.Domain.Enums;

namespace EnglishCenter.Domain.Entities;

public class StudentTuitionMonth : AuditableEntity
{
    public Guid StudentId { get; set; }
    public Guid EnrollmentId { get; set; }
    public int BillingYear { get; set; }
    public int BillingMonth { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public TuitionMonthStatus Status { get; set; }
    public string? Note { get; set; }
}

public class TuitionPayment : AuditableEntity
{
    public Guid StudentTuitionMonthId { get; set; }
    public Guid StudentId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime PaidAt { get; set; }
    public string? ReferenceNo { get; set; }
    public Guid? ReceivedBy { get; set; }
    public string? Note { get; set; }
}

public class SalaryPeriod : AuditableEntity
{
    public int Year { get; set; }
    public int Month { get; set; }
    public SalaryPeriodStatus Status { get; set; } = SalaryPeriodStatus.Open;
    public DateTime? ClosedAt { get; set; }
    public Guid? ClosedBy { get; set; }
}

public class LessonPayRecord : AuditableEntity
{
    public Guid SalaryPeriodId { get; set; }
    public Guid LessonSessionId { get; set; }
    public Guid LessonSessionStaffId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid ClassId { get; set; }
    public SessionStaffRole StaffRole { get; set; }
    public TeachingMode TeachingMode { get; set; }
    public decimal BaseLessonRate { get; set; }
    public decimal PayMultiplier { get; set; }
    public decimal PayAmount { get; set; }
    public LessonPayStatus Status { get; set; }
    public DateTime CalculatedAt { get; set; }
    public string? Note { get; set; }
}

public class Assessment : AuditableEntity
{
    public Guid ClassId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly? AssessmentDate { get; set; }
    public decimal? MaxScore { get; set; }
    public string? Description { get; set; }
}

public class Grade : AuditableEntity
{
    public Guid AssessmentId { get; set; }
    public Guid StudentId { get; set; }
    public decimal? Score { get; set; }
    public string? Comment { get; set; }
    public DateTime? GradedAt { get; set; }
    public Guid? GradedBy { get; set; }
}
