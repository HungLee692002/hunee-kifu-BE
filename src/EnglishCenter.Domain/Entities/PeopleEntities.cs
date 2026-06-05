using EnglishCenter.Domain.Common;
using EnglishCenter.Domain.Enums;

namespace EnglishCenter.Domain.Entities;

public class Student : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public int? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public StudentStatus Status { get; set; } = StudentStatus.Active;
    public Guid? CurrentEnrollmentId { get; set; }
    /// <summary>Học phí mỗi buổi (VND) — dùng khi tính học phí tháng theo điểm danh.</summary>
    public decimal? PerLessonTuition { get; set; }
    public string? Note { get; set; }
}

public class Guardian : AuditableEntity
{
    public Guid StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsPrimary { get; set; }
}

public class Teacher : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public TeacherType TeacherType { get; set; }
    public decimal? CurrentLessonRate { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public TeacherStatus Status { get; set; } = TeacherStatus.Active;
    public Guid? UserId { get; set; }
    public string? Note { get; set; }
}

public class Enrollment : AuditableEntity
{
    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public DateOnly EnrolledAt { get; set; }
    public DateOnly? EndedAt { get; set; }
    public decimal? MonthlyTuitionAmount { get; set; }
}

public class ClassAssignment : AuditableEntity
{
    public Guid ClassId { get; set; }
    public Guid TeacherId { get; set; }
    public ClassAssignmentRole Role { get; set; }
    public DateOnly AssignedFrom { get; set; }
    public DateOnly? AssignedTo { get; set; }
    public bool IsActive { get; set; } = true;
}

public class TeacherLessonRate : AuditableEntity
{
    public Guid TeacherId { get; set; }
    public decimal LessonRate { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}
