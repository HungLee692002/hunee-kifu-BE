using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;
using EnglishCenter.Domain.Entities;
using EnglishCenter.Domain.Enums;
using EnglishCenter.Domain.Exceptions;
using EnglishCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Services;

public sealed class ClassService : IClassService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public ClassService(AppDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<PagedResult<ClassDto>> GetPagedAsync(PagedQuery query, CancellationToken cancellationToken = default)
    {
        var paged = await _db.Classes.AsQueryable().ToPagedAsync(query, cancellationToken);
        return PagedResult<ClassDto>.Create(
            paged.Items.Select(e => e.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);
    }

    public async Task<ClassDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Classes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy lớp học.");
        return entity.ToDto();
    }

    public async Task<ClassDto> CreateAsync(CreateClassRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _db.Courses.AnyAsync(c => c.Id == request.CourseId, cancellationToken))
            throw new NotFoundException("Không tìm thấy khóa học.");

        var entity = new Class
        {
            CourseId = request.CourseId,
            Name = request.Name,
            Status = EnumNames.ParseClassStatus(request.Status),
            GradingEnabled = request.GradingEnabled,
            DefaultMonthlyTuition = request.DefaultMonthlyTuition,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
        };
        entity.SetCreated(_currentUser.UserId, _clock.UtcNow);
        entity.Code = entity.Id.ToString();
        _db.Classes.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<ClassDto> UpdateAsync(Guid id, UpdateClassRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Classes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy lớp học.");

        if (!await _db.Courses.AnyAsync(c => c.Id == request.CourseId, cancellationToken))
            throw new NotFoundException("Không tìm thấy khóa học.");

        entity.CourseId = request.CourseId;
        entity.Name = request.Name;
        entity.Status = EnumNames.ParseClassStatus(request.Status);
        entity.GradingEnabled = request.GradingEnabled;
        entity.DefaultMonthlyTuition = request.DefaultMonthlyTuition;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<ClassDto> PatchAsync(Guid id, PatchClassRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Classes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy lớp học.");

        if (request.CourseId.HasValue)
        {
            if (!await _db.Courses.AnyAsync(c => c.Id == request.CourseId, cancellationToken))
                throw new NotFoundException("Không tìm thấy khóa học.");
            entity.CourseId = request.CourseId.Value;
        }
        if (request.Name is not null) entity.Name = request.Name;
        if (request.Status is not null) entity.Status = EnumNames.ParseClassStatus(request.Status);
        if (request.GradingEnabled.HasValue) entity.GradingEnabled = request.GradingEnabled.Value;
        if (request.DefaultMonthlyTuition.HasValue) entity.DefaultMonthlyTuition = request.DefaultMonthlyTuition;
        if (request.StartDate.HasValue) entity.StartDate = request.StartDate;
        if (request.EndDate.HasValue) entity.EndDate = request.EndDate;

        entity.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<PagedResult<EnrollmentDto>> GetEnrollmentsByClassAsync(
        Guid classId,
        PagedQuery query,
        string? status,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Classes.AnyAsync(c => c.Id == classId, cancellationToken))
            throw new NotFoundException("Không tìm thấy lớp học.");

        var q = _db.Enrollments.Where(e => e.ClassId == classId);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(e => e.Status == EnumNames.ParseEnrollmentStatus(status));

        var paged = await q.ToPagedAsync(query, cancellationToken);
        return PagedResult<EnrollmentDto>.Create(
            paged.Items.Select(e => e.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);
    }

    public async Task<EnrollmentDto> GetEnrollmentByIdAsync(Guid enrollmentId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Enrollments.FirstOrDefaultAsync(e => e.Id == enrollmentId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy ghi danh.");
        return entity.ToDto();
    }

    public async Task<EnrollmentDto> CreateEnrollmentAsync(
        Guid classId,
        CreateEnrollmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Classes.AnyAsync(c => c.Id == classId, cancellationToken))
            throw new NotFoundException("Không tìm thấy lớp học.");

        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy học sinh.");

        if (await _db.Enrollments.AnyAsync(
                e => e.StudentId == request.StudentId && e.Status == EnrollmentStatus.Active,
                cancellationToken))
            throw new ConflictException("Học sinh đã có ghi danh đang hoạt động.");

        var enrollment = new Enrollment
        {
            StudentId = request.StudentId,
            ClassId = classId,
            Status = EnrollmentStatus.Active,
            EnrolledAt = request.EnrolledAt,
            MonthlyTuitionAmount = request.MonthlyTuitionAmount,
        };
        enrollment.SetCreated(_currentUser.UserId, _clock.UtcNow);
        _db.Enrollments.Add(enrollment);

        student.CurrentEnrollmentId = enrollment.Id;
        student.SetUpdated(_currentUser.UserId, _clock.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);
        return enrollment.ToDto();
    }

    public async Task<EnrollmentDto> PatchEnrollmentAsync(
        Guid enrollmentId,
        PatchEnrollmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _db.Enrollments.FirstOrDefaultAsync(e => e.Id == enrollmentId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy ghi danh.");

        if (request.Status is not null)
            enrollment.Status = EnumNames.ParseEnrollmentStatus(request.Status);
        if (request.EndedAt.HasValue)
            enrollment.EndedAt = request.EndedAt;

        if (enrollment.Status == EnrollmentStatus.Ended)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == enrollment.StudentId, cancellationToken);
            if (student?.CurrentEnrollmentId == enrollment.Id)
            {
                student.CurrentEnrollmentId = null;
                student.SetUpdated(_currentUser.UserId, _clock.UtcNow);
            }
        }

        enrollment.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        return enrollment.ToDto();
    }
}
