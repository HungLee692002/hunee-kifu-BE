using EnglishCenter.Application.Dtos;

namespace EnglishCenter.Application.Abstractions;

public interface IAttendanceService
{
    Task<IReadOnlyList<StudentAttendanceDto>> GetStudentAttendancesAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentAttendanceDto>> UpsertStudentAttendancesAsync(Guid sessionId, UpsertStudentAttendancesRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeacherAttendanceDto>> UpsertTeacherAttendancesAsync(Guid sessionId, UpsertTeacherAttendancesRequest request, CancellationToken cancellationToken = default);
}
