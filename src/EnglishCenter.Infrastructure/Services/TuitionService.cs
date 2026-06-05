using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;
using EnglishCenter.Domain.Entities;
using EnglishCenter.Domain.Enums;
using EnglishCenter.Domain.Exceptions;
using EnglishCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Services;

public sealed class TuitionService : ITuitionService
{
    private static readonly AttendanceStatus[] BillableAttendanceStatuses =
    {
        AttendanceStatus.Present,
        AttendanceStatus.Late,
        AttendanceStatus.Excused,
    };

    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public TuitionService(AppDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<PagedResult<StudentTuitionMonthDto>> GetTuitionMonthsPagedAsync(
        PagedQuery query,
        StudentTuitionMonthListFilter filter,
        CancellationToken cancellationToken = default)
    {
        var q = _db.StudentTuitionMonths.AsQueryable();

        if (filter.Year.HasValue)
            q = q.Where(m => m.BillingYear == filter.Year.Value);
        if (filter.Month.HasValue)
            q = q.Where(m => m.BillingMonth == filter.Month.Value);
        if (!string.IsNullOrWhiteSpace(filter.Status))
            q = q.Where(m => m.Status == EnumNames.ParseTuitionMonthStatus(filter.Status));
        if (filter.StudentId.HasValue)
            q = q.Where(m => m.StudentId == filter.StudentId.Value);
        if (filter.ClassId.HasValue)
        {
            q = q.Where(m =>
                _db.Enrollments.Any(e =>
                    e.Id == m.EnrollmentId && e.ClassId == filter.ClassId.Value));
        }

        var paged = await q.ToPagedAsync(query, cancellationToken);
        var studentIds = paged.Items.Select(m => m.StudentId).Distinct().ToList();
        var students = await _db.Students
            .Where(s => studentIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        return PagedResult<StudentTuitionMonthDto>.Create(
            paged.Items.Select(m => m.ToDto(students.GetValueOrDefault(m.StudentId))).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);
    }

    public async Task<StudentTuitionMonthDto> GetTuitionMonthByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.StudentTuitionMonths.FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy học phí tháng.");

        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == entity.StudentId, cancellationToken);
        return entity.ToDto(student);
    }

    public async Task<PagedResult<TuitionPaymentDto>> GetPaymentsPagedAsync(
        PagedQuery query,
        CancellationToken cancellationToken = default)
    {
        var paged = await _db.TuitionPayments.AsQueryable().ToPagedAsync(query, cancellationToken);
        return PagedResult<TuitionPaymentDto>.Create(
            paged.Items.Select(e => e.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);
    }

    public async Task<TuitionPaymentDto> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.TuitionPayments.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy phiếu thu.");
        return entity.ToDto();
    }

    public async Task<IReadOnlyList<ClassStudentTuitionBillingDto>> GetClassTuitionBillingAsync(
        Guid classId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        _ = await _db.Classes.FirstOrDefaultAsync(c => c.Id == classId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy lớp học.");

        var enrollments = await _db.Enrollments
            .Where(e => e.ClassId == classId && e.Status == EnrollmentStatus.Active)
            .ToListAsync(cancellationToken);

        var studentIds = enrollments.Select(e => e.StudentId).ToList();
        var students = await _db.Students
            .Where(s => studentIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var results = new List<ClassStudentTuitionBillingDto>();

        foreach (var enrollment in enrollments)
        {
            if (!students.TryGetValue(enrollment.StudentId, out var student))
                continue;

            var billableSessions = await CountBillableSessionsAsync(
                student.Id, classId, year, month, cancellationToken);

            var expectedAmount = student.PerLessonTuition is { } rate
                ? Math.Round(rate * billableSessions, 0)
                : 0m;

            var monthRow = await EnsureTuitionMonthAsync(
                student.Id,
                enrollment.Id,
                year,
                month,
                expectedAmount,
                cancellationToken);

            results.Add(new ClassStudentTuitionBillingDto(
                student.Id,
                student.Code,
                student.FullName,
                enrollment.Id,
                student.PerLessonTuition,
                billableSessions,
                monthRow.ExpectedAmount,
                monthRow.AmountPaid,
                monthRow.Status.ToApiName(),
                monthRow.Id));
        }

        await _db.SaveChangesAsync(cancellationToken);
        return results.OrderBy(r => r.FullName).ToList();
    }

    public async Task<TuitionPaymentDto> RecordPaymentAsync(
        Guid studentId,
        CreateTuitionPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy học sinh.");

        var enrollment = await _db.Enrollments.FirstOrDefaultAsync(
            e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active,
            cancellationToken)
            ?? throw new ConflictException("Học sinh chưa có ghi danh đang hoạt động.");

        var billableSessions = await CountBillableSessionsAsync(
            studentId, enrollment.ClassId, request.BillingYear, request.BillingMonth, cancellationToken);

        var expectedAmount = student.PerLessonTuition is { } rate
            ? Math.Round(rate * billableSessions, 0)
            : throw new ConflictException("Học sinh chưa cấu hình học phí theo buổi.");

        var month = await EnsureTuitionMonthAsync(
            studentId,
            enrollment.Id,
            request.BillingYear,
            request.BillingMonth,
            expectedAmount,
            cancellationToken);

        var payment = new TuitionPayment
        {
            StudentTuitionMonthId = month.Id,
            StudentId = studentId,
            Amount = request.Amount,
            PaymentMethod = EnumNames.ParsePaymentMethod(request.PaymentMethod),
            PaidAt = request.PaidAt,
            ReferenceNo = request.ReferenceNo,
            ReceivedBy = _currentUser.UserId,
            Note = request.Note,
        };
        payment.SetCreated(_currentUser.UserId, _clock.UtcNow);
        _db.TuitionPayments.Add(payment);

        month.AmountPaid += request.Amount;
        month.Status = EntityMappers.ComputeTuitionStatus(month.ExpectedAmount, month.AmountPaid);
        month.SetUpdated(_currentUser.UserId, _clock.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);
        return payment.ToDto();
    }

    private async Task<int> CountBillableSessionsAsync(
        Guid studentId,
        Guid classId,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        return await (
            from a in _db.StudentAttendances
            join s in _db.LessonSessions on a.LessonSessionId equals s.Id
            where a.StudentId == studentId
                  && s.ClassId == classId
                  && s.SessionDate >= start
                  && s.SessionDate <= end
                  && s.Status != LessonSessionStatus.Cancelled
                  && BillableAttendanceStatuses.Contains(a.Status)
            select a.Id
        ).CountAsync(cancellationToken);
    }

    private async Task<StudentTuitionMonth> EnsureTuitionMonthAsync(
        Guid studentId,
        Guid enrollmentId,
        int year,
        int month,
        decimal expectedAmount,
        CancellationToken cancellationToken)
    {
        var monthRow = await _db.StudentTuitionMonths.FirstOrDefaultAsync(
            m => m.StudentId == studentId && m.BillingYear == year && m.BillingMonth == month,
            cancellationToken);

        if (monthRow is null)
        {
            monthRow = new StudentTuitionMonth
            {
                StudentId = studentId,
                EnrollmentId = enrollmentId,
                BillingYear = year,
                BillingMonth = month,
                ExpectedAmount = expectedAmount,
                AmountPaid = 0,
                Status = TuitionMonthStatus.Unpaid,
            };
            monthRow.SetCreated(_currentUser.UserId, _clock.UtcNow);
            _db.StudentTuitionMonths.Add(monthRow);
        }
        else
        {
            monthRow.ExpectedAmount = expectedAmount;
            monthRow.EnrollmentId = enrollmentId;
            monthRow.Status = EntityMappers.ComputeTuitionStatus(monthRow.ExpectedAmount, monthRow.AmountPaid);
            monthRow.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        }

        return monthRow;
    }
}
