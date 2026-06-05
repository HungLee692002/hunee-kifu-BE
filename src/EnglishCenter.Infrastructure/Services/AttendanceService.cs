using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;
using EnglishCenter.Domain.Entities;
using EnglishCenter.Domain.Enums;
using EnglishCenter.Domain.Exceptions;
using EnglishCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Services;

public sealed class AttendanceService : IAttendanceService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public AttendanceService(AppDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<IReadOnlyList<StudentAttendanceDto>> GetStudentAttendancesAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        await EnsureSessionAsync(sessionId, cancellationToken);
        var items = await _db.StudentAttendances
            .Where(a => a.LessonSessionId == sessionId)
            .ToListAsync(cancellationToken);
        return items.Select(a => a.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<StudentAttendanceDto>> UpsertStudentAttendancesAsync(
        Guid sessionId,
        UpsertStudentAttendancesRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await EnsureSessionAsync(sessionId, cancellationToken);

        foreach (var item in request.Items)
        {
            var enrollment = await _db.Enrollments.FirstOrDefaultAsync(
                e => e.StudentId == item.StudentId && e.ClassId == session.ClassId && e.Status == EnrollmentStatus.Active,
                cancellationToken);

            var existing = await _db.StudentAttendances.FirstOrDefaultAsync(
                a => a.LessonSessionId == sessionId && a.StudentId == item.StudentId,
                cancellationToken);

            if (existing is null)
            {
                existing = new StudentAttendance
                {
                    LessonSessionId = sessionId,
                    StudentId = item.StudentId,
                    EnrollmentId = enrollment?.Id,
                    Status = EnumNames.ParseAttendanceStatus(item.Status),
                    Note = item.Note,
                    RecordedAt = _clock.UtcNow,
                    RecordedBy = _currentUser.UserId,
                };
                existing.SetCreated(_currentUser.UserId, _clock.UtcNow);
                _db.StudentAttendances.Add(existing);
            }
            else
            {
                existing.Status = EnumNames.ParseAttendanceStatus(item.Status);
                existing.Note = item.Note;
                existing.RecordedAt = _clock.UtcNow;
                existing.RecordedBy = _currentUser.UserId;
                existing.SetUpdated(_currentUser.UserId, _clock.UtcNow);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return await GetStudentAttendancesAsync(sessionId, cancellationToken);
    }

    public async Task<IReadOnlyList<TeacherAttendanceDto>> UpsertTeacherAttendancesAsync(
        Guid sessionId,
        UpsertTeacherAttendancesRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await EnsureSessionAsync(sessionId, cancellationToken);
        var staffList = await _db.LessonSessionStaffs
            .Where(s => s.LessonSessionId == sessionId)
            .ToListAsync(cancellationToken);

        foreach (var item in request.Items)
        {
            var staff = staffList.FirstOrDefault(s => s.TeacherId == item.TeacherId);
            var existing = await _db.TeacherAttendances.FirstOrDefaultAsync(
                a => a.LessonSessionId == sessionId && a.TeacherId == item.TeacherId,
                cancellationToken);

            var status = EnumNames.ParseAttendanceStatus(item.Status);

            if (existing is null)
            {
                existing = new TeacherAttendance
                {
                    LessonSessionId = sessionId,
                    TeacherId = item.TeacherId,
                    LessonSessionStaffId = staff?.Id,
                    Status = status,
                    CheckInAt = item.CheckInAt,
                    CheckOutAt = item.CheckOutAt,
                    Note = item.Note,
                };
                existing.SetCreated(_currentUser.UserId, _clock.UtcNow);
                _db.TeacherAttendances.Add(existing);
            }
            else
            {
                existing.Status = status;
                existing.CheckInAt = item.CheckInAt;
                existing.CheckOutAt = item.CheckOutAt;
                existing.Note = item.Note;
                existing.LessonSessionStaffId = staff?.Id;
                existing.SetUpdated(_currentUser.UserId, _clock.UtcNow);
            }

            if (IsPresentLike(status) && staff is not null)
                await TryCreateLessonPayRecordAsync(session, staff, cancellationToken);
        }

        if (request.Items.Any(i => IsPresentLike(EnumNames.ParseAttendanceStatus(i.Status))))
        {
            session.Status = LessonSessionStatus.Completed;
            session.CompletedAt ??= _clock.UtcNow;
            session.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        }

        await _db.SaveChangesAsync(cancellationToken);
        var results = await _db.TeacherAttendances
            .Where(a => a.LessonSessionId == sessionId)
            .ToListAsync(cancellationToken);
        return results.Select(a => a.ToDto()).ToList();
    }

    private static bool IsPresentLike(AttendanceStatus status) =>
        status is AttendanceStatus.Present or AttendanceStatus.Late or AttendanceStatus.Excused;

    private async Task TryCreateLessonPayRecordAsync(
        LessonSession session,
        LessonSessionStaff staff,
        CancellationToken ct)
    {
        var period = await GetOrCreateSalaryPeriodAsync(session.SessionDate.Year, session.SessionDate.Month, ct);
        if (period.Status == SalaryPeriodStatus.Closed)
            return;

        var exists = await _db.LessonPayRecords.AnyAsync(
            r => r.LessonSessionId == session.Id && r.LessonSessionStaffId == staff.Id,
            ct);
        if (exists) return;

        var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Id == staff.TeacherId, ct);
        if (teacher?.CurrentLessonRate is not { } rate)
            return;

        var record = new LessonPayRecord
        {
            SalaryPeriodId = period.Id,
            LessonSessionId = session.Id,
            LessonSessionStaffId = staff.Id,
            TeacherId = staff.TeacherId,
            ClassId = session.ClassId,
            StaffRole = staff.StaffRole,
            TeachingMode = session.TeachingMode,
            BaseLessonRate = rate,
            PayMultiplier = staff.PayMultiplier,
            PayAmount = Math.Round(rate * staff.PayMultiplier, 0),
            Status = LessonPayStatus.Confirmed,
            CalculatedAt = _clock.UtcNow,
        };
        record.SetCreated(_currentUser.UserId, _clock.UtcNow);
        _db.LessonPayRecords.Add(record);
    }

    private async Task<SalaryPeriod> GetOrCreateSalaryPeriodAsync(int year, int month, CancellationToken ct)
    {
        var period = await _db.SalaryPeriods
            .FirstOrDefaultAsync(p => p.Year == year && p.Month == month, ct);

        if (period is not null)
            return period;

        period = new SalaryPeriod { Year = year, Month = month, Status = SalaryPeriodStatus.Open };
        period.SetCreated(_currentUser.UserId, _clock.UtcNow);
        _db.SalaryPeriods.Add(period);
        await _db.SaveChangesAsync(ct);
        return period;
    }

    private async Task<LessonSession> EnsureSessionAsync(Guid sessionId, CancellationToken ct) =>
        await _db.LessonSessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct)
        ?? throw new NotFoundException("Không tìm thấy buổi học.");
}
