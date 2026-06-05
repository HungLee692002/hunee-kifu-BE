using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using EnglishCenter.Domain.Enums;
using EnglishCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    private readonly IDateTimeProvider _clock;

    public DashboardService(AppDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(
        int? year,
        int? month,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;
        var (prevYear, prevMonth) = PreviousMonth(y, m);

        var periodStart = new DateOnly(y, m, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);
        var prevStart = new DateOnly(prevYear, prevMonth, 1);
        var prevEnd = prevStart.AddMonths(1).AddDays(-1);

        var activeStudents = await _db.Students.CountAsync(
            s => s.Status == StudentStatus.Active, cancellationToken);

        var activeStudentsPrevious = await _db.Enrollments
            .Where(e => e.Status == EnrollmentStatus.Active && e.EnrolledAt <= prevEnd)
            .Select(e => e.StudentId)
            .Distinct()
            .CountAsync(cancellationToken);

        var newEnrollments = await _db.Enrollments.CountAsync(
            e => e.EnrolledAt >= periodStart && e.EnrolledAt <= periodEnd,
            cancellationToken);

        var attendanceRate = await ComputeAttendanceRateAsync(periodStart, periodEnd, cancellationToken);
        var attendanceRatePrevious = await ComputeAttendanceRateAsync(prevStart, prevEnd, cancellationToken);

        var tuition = await AggregateTuitionAsync(y, m, cancellationToken);
        var tuitionPrevious = await AggregateTuitionAsync(prevYear, prevMonth, cancellationToken);

        var completedSessions = await _db.LessonSessions.CountAsync(
            s => s.SessionDate >= periodStart && s.SessionDate <= periodEnd && s.Status == LessonSessionStatus.Completed,
            cancellationToken);

        var completedSessionsPrevious = await _db.LessonSessions.CountAsync(
            s => s.SessionDate >= prevStart && s.SessionDate <= prevEnd && s.Status == LessonSessionStatus.Completed,
            cancellationToken);

        var salaryTotal = await AggregateSalaryAsync(y, m, cancellationToken);
        var salaryTotalPrevious = await AggregateSalaryAsync(prevYear, prevMonth, cancellationToken);

        return new DashboardSummaryDto(
            new PeriodDto(y, m),
            new PeriodDto(prevYear, prevMonth),
            new DashboardStudentsDto(activeStudents, activeStudentsPrevious, newEnrollments),
            new DashboardAttendanceDto(attendanceRate, attendanceRatePrevious),
            new DashboardTuitionDto(
                tuition.Expected,
                tuition.Paid,
                Math.Max(tuition.Expected - tuition.Paid, 0),
                tuitionPrevious.Expected,
                tuitionPrevious.Paid),
            new DashboardTeachingDto(completedSessions, completedSessionsPrevious),
            new DashboardSalaryDto(salaryTotal, salaryTotalPrevious));
    }

    private async Task<decimal> ComputeAttendanceRateAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        var records = await (
            from sa in _db.StudentAttendances
            join ls in _db.LessonSessions on sa.LessonSessionId equals ls.Id
            where ls.SessionDate >= fromDate && ls.SessionDate <= toDate && ls.Status != LessonSessionStatus.Cancelled
            select sa.Status
        ).ToListAsync(cancellationToken);

        if (records.Count == 0)
            return 0;

        var present = records.Count(s => s is AttendanceStatus.Present or AttendanceStatus.Late);
        return Math.Round(present * 100m / records.Count, 1);
    }

    private async Task<(decimal Expected, decimal Paid)> AggregateTuitionAsync(
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var rows = await _db.StudentTuitionMonths
            .Where(t => t.BillingYear == year && t.BillingMonth == month)
            .Select(t => new { t.ExpectedAmount, t.AmountPaid })
            .ToListAsync(cancellationToken);

        return (rows.Sum(r => r.ExpectedAmount), rows.Sum(r => r.AmountPaid));
    }

    private async Task<decimal> AggregateSalaryAsync(int year, int month, CancellationToken cancellationToken)
    {
        return await (
            from lpr in _db.LessonPayRecords
            join sp in _db.SalaryPeriods on lpr.SalaryPeriodId equals sp.Id
            where sp.Year == year && sp.Month == month && lpr.Status == LessonPayStatus.Confirmed
            select lpr.PayAmount
        ).SumAsync(cancellationToken);
    }

    private static (int Year, int Month) PreviousMonth(int year, int month) =>
        month == 1 ? (year - 1, 12) : (year, month - 1);
}
