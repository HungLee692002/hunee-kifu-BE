using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;
using EnglishCenter.Domain.Entities;
using EnglishCenter.Domain.Exceptions;
using EnglishCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Services;

public sealed class TeacherService : ITeacherService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public TeacherService(AppDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<PagedResult<TeacherDto>> GetPagedAsync(PagedQuery query, CancellationToken cancellationToken = default)
    {
        var paged = await _db.Teachers.AsQueryable().ToPagedAsync(query, cancellationToken);
        return PagedResult<TeacherDto>.Create(
            paged.Items.Select(e => e.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);
    }

    public async Task<TeacherDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Teachers.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy giáo viên.");
        return entity.ToDto();
    }

    public async Task<TeacherDto> CreateAsync(CreateTeacherRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Teacher
        {
            FullName = request.FullName,
            TeacherType = EnumNames.ParseTeacherType(request.TeacherType),
            Phone = request.Phone,
            Email = request.Email,
            Status = EnumNames.ParseTeacherStatus(request.Status),
            UserId = request.UserId,
            Note = request.Note,
        };
        entity.SetCreated(_currentUser.UserId, _clock.UtcNow);
        entity.Code = entity.Id.ToString();
        _db.Teachers.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<TeacherDto> UpdateAsync(Guid id, UpdateTeacherRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Teachers.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy giáo viên.");

        entity.FullName = request.FullName;
        entity.TeacherType = EnumNames.ParseTeacherType(request.TeacherType);
        entity.Phone = request.Phone;
        entity.Email = request.Email;
        entity.Status = EnumNames.ParseTeacherStatus(request.Status);
        entity.UserId = request.UserId;
        entity.Note = request.Note;
        entity.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<IReadOnlyList<TeacherLessonRateDto>> GetLessonRatesAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Teachers.AnyAsync(t => t.Id == teacherId, cancellationToken))
            throw new NotFoundException("Không tìm thấy giáo viên.");

        var rates = await _db.TeacherLessonRates
            .Where(r => r.TeacherId == teacherId)
            .OrderByDescending(r => r.EffectiveFrom)
            .ToListAsync(cancellationToken);

        return rates.Select(r => r.ToDto()).ToList();
    }

    public async Task<TeacherLessonRateDto> CreateLessonRateAsync(
        Guid teacherId,
        CreateTeacherLessonRateRequest request,
        CancellationToken cancellationToken = default)
    {
        var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Id == teacherId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy giáo viên.");

        var activeRates = await _db.TeacherLessonRates
            .Where(r => r.TeacherId == teacherId && r.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var rate in activeRates)
        {
            rate.IsActive = false;
            rate.EffectiveTo = request.EffectiveFrom.AddDays(-1);
            rate.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        }

        var entity = new TeacherLessonRate
        {
            TeacherId = teacherId,
            LessonRate = request.LessonRate,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            IsActive = true,
            Note = request.Note,
        };
        entity.SetCreated(_currentUser.UserId, _clock.UtcNow);
        _db.TeacherLessonRates.Add(entity);

        teacher.CurrentLessonRate = request.LessonRate;
        teacher.SetUpdated(_currentUser.UserId, _clock.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
