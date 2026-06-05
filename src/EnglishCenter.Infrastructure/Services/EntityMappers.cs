using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;
using EnglishCenter.Domain.Entities;
using EnglishCenter.Domain.Enums;

namespace EnglishCenter.Infrastructure.Services;

internal static class EntityMappers
{
    public static RoomDto ToDto(this Room e) => new(
        e.Id, e.Code, e.Name, e.Capacity, e.Floor, e.Note, e.IsActive,
        e.CreatedAt, e.CreatedBy, e.UpdatedAt, e.UpdatedBy);

    public static CourseDto ToDto(this Course e) => new(
        e.Id, e.Code, e.Name, e.Description, e.IsActive,
        e.CreatedAt, e.CreatedBy, e.UpdatedAt, e.UpdatedBy);

    public static ClassDto ToDto(this Class e) => new(
        e.Id, e.Code, e.CourseId, e.Name, e.Status.ToApiName(), e.GradingEnabled,
        e.DefaultMonthlyTuition, e.StartDate, e.EndDate,
        e.CreatedAt, e.CreatedBy, e.UpdatedAt, e.UpdatedBy);

    public static EnrollmentDto ToDto(this Enrollment e) => new(
        e.Id, e.StudentId, e.ClassId, e.Status.ToApiName(), e.EnrolledAt, e.EndedAt,
        e.MonthlyTuitionAmount, e.CreatedAt, e.CreatedBy, e.UpdatedAt, e.UpdatedBy);

    public static StudentDto ToDto(this Student e) => new(
        e.Id, e.Code, e.FullName, e.DateOfBirth, e.Gender, e.Phone, e.Email, e.Address,
        e.Status.ToApiName(), e.CurrentEnrollmentId, e.PerLessonTuition, e.Note,
        e.CreatedAt, e.CreatedBy, e.UpdatedAt, e.UpdatedBy);

    public static GuardianDto ToDto(this Guardian e) => new(
        e.Id, e.StudentId, e.FullName, e.Relationship, e.Phone, e.Email, e.IsPrimary,
        e.CreatedAt, e.CreatedBy, e.UpdatedAt, e.UpdatedBy);

    public static TeacherDto ToDto(this Teacher e) => new(
        e.Id, e.Code, e.FullName, e.TeacherType.ToApiName(), e.CurrentLessonRate,
        e.Phone, e.Email, e.Status.ToApiName(), e.UserId, e.Note,
        e.CreatedAt, e.CreatedBy, e.UpdatedAt, e.UpdatedBy);

    public static TeacherLessonRateDto ToDto(this TeacherLessonRate e) => new(
        e.Id, e.TeacherId, e.LessonRate, e.EffectiveFrom, e.EffectiveTo, e.IsActive, e.Note,
        e.CreatedAt, e.UpdatedAt);

    public static StudentTuitionMonthDto ToDto(this StudentTuitionMonth e, Student? student = null) => new(
        e.Id, e.StudentId, e.EnrollmentId, e.BillingYear, e.BillingMonth,
        e.ExpectedAmount, e.AmountPaid, e.Status.ToApiName(), e.Note,
        student is null ? null : new StudentSummaryDto(student.Id, student.Code, student.FullName),
        e.CreatedAt, e.UpdatedAt);

    public static TuitionPaymentDto ToDto(this TuitionPayment e) => new(
        e.Id, e.StudentTuitionMonthId, e.StudentId, e.Amount, e.PaymentMethod.ToApiName(),
        e.PaidAt, e.ReferenceNo, e.ReceivedBy, e.Note, e.CreatedAt);

    public static TuitionMonthStatus ComputeTuitionStatus(decimal expected, decimal paid) =>
        paid <= 0 ? TuitionMonthStatus.Unpaid
        : paid >= expected ? TuitionMonthStatus.Paid
        : TuitionMonthStatus.Partial;

    public static WeeklyScheduleTemplateDto ToDto(this WeeklyScheduleTemplate e) => new(
        e.Id, e.ClassId, e.DayOfWeek, e.StartTime, e.EndTime, e.RoomId,
        e.TeachingMode.ToApiName(), e.PrimaryTeacherId, e.LocalSupportTeacherId,
        e.EffectiveFrom, e.EffectiveTo, e.IsActive, e.CreatedAt, e.UpdatedAt);

    public static LessonSessionStaffDto ToDto(this LessonSessionStaff e) => new(
        e.Id, e.TeacherId, e.StaffRole.ToApiName(), e.PayMultiplier);

    public static LessonSessionDto ToDto(this LessonSession e, IReadOnlyList<LessonSessionStaffDto> staff) => new(
        e.Id, e.ClassId, e.WeeklyScheduleTemplateId, e.SessionDate,
        e.TeachingMode.ToApiName(), e.Status.ToApiName(),
        e.EffectiveStartTime, e.EffectiveEndTime, e.EffectiveRoomId,
        e.HasOverride, staff, e.CreatedAt, e.UpdatedAt);

    public static LessonScheduleOverrideDto ToDto(this LessonScheduleOverride e) => new(
        e.Id, e.LessonSessionId, e.OverridePrimaryTeacherId, e.OverrideSupportTeacherId,
        e.OverrideRoomId, e.OverrideStartTime, e.OverrideEndTime,
        e.IsCancelled, e.Reason, e.IncidentAt);

    public static StudentAttendanceDto ToDto(this StudentAttendance e) => new(
        e.Id, e.LessonSessionId, e.StudentId, e.EnrollmentId,
        e.Status.ToApiName(), e.Note, e.RecordedAt);

    public static TeacherAttendanceDto ToDto(this TeacherAttendance e) => new(
        e.Id, e.LessonSessionId, e.TeacherId, e.LessonSessionStaffId,
        e.Status.ToApiName(), e.CheckInAt, e.CheckOutAt, e.Note);

    public static SalaryPeriodDto ToDto(this SalaryPeriod e) => new(
        e.Id, e.Year, e.Month, e.Status.ToApiName(), e.ClosedAt, e.ClosedBy,
        e.CreatedAt, e.UpdatedAt);

    public static LessonPayRecordDto ToDto(this LessonPayRecord e) => new(
        e.Id, e.SalaryPeriodId, e.LessonSessionId, e.TeacherId, e.ClassId,
        e.StaffRole.ToApiName(), e.TeachingMode.ToApiName(),
        e.BaseLessonRate, e.PayMultiplier, e.PayAmount, e.Status.ToApiName(),
        e.CalculatedAt);

    public static AssessmentDto ToDto(this Assessment e) => new(
        e.Id, e.ClassId, e.Title, e.AssessmentDate, e.MaxScore, e.Description,
        e.CreatedAt, e.UpdatedAt);

    public static GradeDto ToDto(this Grade e) => new(
        e.Id, e.AssessmentId, e.StudentId, e.Score, e.Comment, e.GradedAt);

    public static ClassAssignmentDto ToDto(this ClassAssignment e) => new(
        e.Id, e.ClassId, e.TeacherId, e.Role.ToApiName(),
        e.AssignedFrom, e.AssignedTo, e.IsActive, e.CreatedAt, e.UpdatedAt);
}
