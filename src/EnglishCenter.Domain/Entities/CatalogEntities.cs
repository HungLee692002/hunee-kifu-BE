using EnglishCenter.Domain.Common;
using EnglishCenter.Domain.Enums;

namespace EnglishCenter.Domain.Entities;

public class Room : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? Capacity { get; set; }
    public string? Floor { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Course : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Class : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ClassStatus Status { get; set; } = ClassStatus.Open;
    public bool GradingEnabled { get; set; }
    public decimal? DefaultMonthlyTuition { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
