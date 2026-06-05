using EnglishCenter.Domain.Common;
using EnglishCenter.Domain.Enums;

namespace EnglishCenter.Domain.Entities;

public class WeeklyScheduleTemplate : AuditableEntity
{
    public Guid ClassId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public Guid RoomId { get; set; }
    public TeachingMode TeachingMode { get; set; }
    public Guid PrimaryTeacherId { get; set; }
    public Guid? LocalSupportTeacherId { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}

public class LessonSession : AuditableEntity
{
    public Guid ClassId { get; set; }
    public Guid? WeeklyScheduleTemplateId { get; set; }
    public DateOnly SessionDate { get; set; }
    public LessonSessionStatus Status { get; set; } = LessonSessionStatus.Scheduled;
    public TeachingMode TeachingMode { get; set; }
    public TimeOnly PlannedStartTime { get; set; }
    public TimeOnly PlannedEndTime { get; set; }
    public Guid PlannedRoomId { get; set; }
    public TimeOnly EffectiveStartTime { get; set; }
    public TimeOnly EffectiveEndTime { get; set; }
    public Guid EffectiveRoomId { get; set; }
    public bool HasOverride { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class LessonSessionStaff : AuditableEntity
{
    public Guid LessonSessionId { get; set; }
    public Guid TeacherId { get; set; }
    public SessionStaffRole StaffRole { get; set; }
    public decimal PayMultiplier { get; set; } = 1.0m;
}

public class LessonScheduleOverride : AuditableEntity
{
    public Guid LessonSessionId { get; set; }
    public Guid? OverridePrimaryTeacherId { get; set; }
    public Guid? OverrideSupportTeacherId { get; set; }
    public Guid? OverrideRoomId { get; set; }
    public TimeOnly? OverrideStartTime { get; set; }
    public TimeOnly? OverrideEndTime { get; set; }
    public bool IsCancelled { get; set; }
    public string? Reason { get; set; }
    public DateTime IncidentAt { get; set; }
}

public class StudentAttendance : AuditableEntity
{
    public Guid LessonSessionId { get; set; }
    public Guid StudentId { get; set; }
    public Guid? EnrollmentId { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime RecordedAt { get; set; }
    public Guid? RecordedBy { get; set; }
}

public class TeacherAttendance : AuditableEntity
{
    public Guid LessonSessionId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid? LessonSessionStaffId { get; set; }
    public AttendanceStatus Status { get; set; }
    public DateTime? CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public string? Note { get; set; }
}
