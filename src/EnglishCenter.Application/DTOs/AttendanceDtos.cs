namespace EnglishCenter.Application.Dtos;

public record StudentAttendanceDto(
    Guid Id,
    Guid LessonSessionId,
    Guid StudentId,
    Guid? EnrollmentId,
    string Status,
    string? Note,
    DateTime RecordedAt);

public record TeacherAttendanceDto(
    Guid Id,
    Guid LessonSessionId,
    Guid TeacherId,
    Guid? LessonSessionStaffId,
    string Status,
    DateTime? CheckInAt,
    DateTime? CheckOutAt,
    string? Note);

public record StudentAttendanceItemRequest(
    Guid StudentId,
    string Status,
    string? Note);

public record UpsertStudentAttendancesRequest(
    IReadOnlyList<StudentAttendanceItemRequest> Items);

public record TeacherAttendanceItemRequest(
    Guid TeacherId,
    string Status,
    DateTime? CheckInAt,
    DateTime? CheckOutAt,
    string? Note);

public record UpsertTeacherAttendancesRequest(
    IReadOnlyList<TeacherAttendanceItemRequest> Items);
