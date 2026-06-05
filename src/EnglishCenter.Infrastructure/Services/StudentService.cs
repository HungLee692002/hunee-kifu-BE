using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;
using EnglishCenter.Domain.Entities;
using EnglishCenter.Domain.Enums;
using EnglishCenter.Domain.Exceptions;
using EnglishCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Services;

public sealed class StudentService : IStudentService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public StudentService(AppDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<PagedResult<StudentDto>> GetPagedAsync(
        PagedQuery query,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var q = _db.Students.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(s => s.Status == EnumNames.ParseStudentStatus(status));
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(s =>
                s.FullName.Contains(term)
                || s.Code.Contains(term)
                || (s.Phone != null && s.Phone.Contains(term))
                || (s.Email != null && s.Email.Contains(term)));
        }

        var paged = await q.ToPagedAsync(query, cancellationToken);
        return PagedResult<StudentDto>.Create(
            paged.Items.Select(e => e.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);
    }

    public async Task<StudentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy học sinh.");
        return entity.ToDto();
    }

    public async Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Student
        {
            FullName = request.FullName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            Status = EnumNames.ParseStudentStatus(request.Status),
            PerLessonTuition = request.PerLessonTuition,
            Note = request.Note,
        };
        entity.SetCreated(_currentUser.UserId, _clock.UtcNow);
        entity.Code = entity.Id.ToString();
        _db.Students.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<StudentDto> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy học sinh.");

        entity.FullName = request.FullName;
        entity.DateOfBirth = request.DateOfBirth;
        entity.Gender = request.Gender;
        entity.Phone = request.Phone;
        entity.Email = request.Email;
        entity.Address = request.Address;
        entity.Status = EnumNames.ParseStudentStatus(request.Status);
        entity.PerLessonTuition = request.PerLessonTuition;
        entity.Note = request.Note;
        entity.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<IReadOnlyList<GuardianDto>> GetGuardiansAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == studentId, cancellationToken))
            throw new NotFoundException("Không tìm thấy học sinh.");

        var guardians = await _db.Guardians
            .Where(g => g.StudentId == studentId)
            .OrderByDescending(g => g.IsPrimary)
            .ToListAsync(cancellationToken);

        return guardians.Select(g => g.ToDto()).ToList();
    }

    public async Task<GuardianDto> CreateGuardianAsync(
        Guid studentId,
        CreateGuardianRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == studentId, cancellationToken))
            throw new NotFoundException("Không tìm thấy học sinh.");

        var entity = new Guardian
        {
            StudentId = studentId,
            FullName = request.FullName,
            Relationship = request.Relationship,
            Phone = request.Phone,
            Email = request.Email,
            IsPrimary = request.IsPrimary,
        };
        entity.SetCreated(_currentUser.UserId, _clock.UtcNow);
        _db.Guardians.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<PagedResult<EnrollmentDto>> GetEnrollmentsByStudentAsync(
        Guid studentId,
        PagedQuery query,
        string? status,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == studentId, cancellationToken))
            throw new NotFoundException("Không tìm thấy học sinh.");

        var q = _db.Enrollments.Where(e => e.StudentId == studentId);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(e => e.Status == EnumNames.ParseEnrollmentStatus(status));

        var paged = await q.ToPagedAsync(query, cancellationToken);
        return PagedResult<EnrollmentDto>.Create(
            paged.Items.Select(e => e.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);
    }
}
