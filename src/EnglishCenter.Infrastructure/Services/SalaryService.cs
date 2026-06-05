using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;
using EnglishCenter.Domain.Enums;
using EnglishCenter.Domain.Exceptions;
using EnglishCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Services;

public sealed class SalaryService : ISalaryService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public SalaryService(AppDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<PagedResult<SalaryPeriodDto>> GetPeriodsPagedAsync(
        PagedQuery query,
        int? year,
        int? month,
        CancellationToken cancellationToken = default)
    {
        var q = _db.SalaryPeriods.AsQueryable();
        if (year.HasValue) q = q.Where(p => p.Year == year);
        if (month.HasValue) q = q.Where(p => p.Month == month);

        var paged = await q.ToPagedAsync(query, cancellationToken);
        return PagedResult<SalaryPeriodDto>.Create(
            paged.Items.Select(p => p.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);
    }

    public async Task<SalaryPeriodDto> GetPeriodByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.SalaryPeriods.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy kỳ lương.");
        return entity.ToDto();
    }

    public async Task<SalaryPeriodDto> PatchPeriodAsync(
        Guid id,
        PatchSalaryPeriodRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.SalaryPeriods.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy kỳ lương.");

        var newStatus = EnumNames.ParseSalaryPeriodStatus(request.Status);
        if (newStatus == SalaryPeriodStatus.Closed)
        {
            if (entity.Status == SalaryPeriodStatus.Closed)
                throw new ConflictException("Kỳ lương đã được chốt.");

            entity.Status = SalaryPeriodStatus.Closed;
            entity.ClosedAt = _clock.UtcNow;
            entity.ClosedBy = _currentUser.UserId;
        }

        entity.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<PagedResult<LessonPayRecordDto>> GetLessonPayRecordsByPeriodAsync(
        Guid periodId,
        PagedQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.SalaryPeriods.AnyAsync(p => p.Id == periodId, cancellationToken))
            throw new NotFoundException("Không tìm thấy kỳ lương.");

        var q = _db.LessonPayRecords.Where(r => r.SalaryPeriodId == periodId);
        var paged = await q.ToPagedAsync(query, cancellationToken);
        return PagedResult<LessonPayRecordDto>.Create(
            paged.Items.Select(r => r.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);
    }

    public async Task<PagedResult<LessonPayRecordDto>> GetLessonPayRecordsByTeacherAsync(
        Guid teacherId,
        PagedQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Teachers.AnyAsync(t => t.Id == teacherId, cancellationToken))
            throw new NotFoundException("Không tìm thấy giáo viên.");

        var q = _db.LessonPayRecords.Where(r => r.TeacherId == teacherId);
        var paged = await q.ToPagedAsync(query, cancellationToken);
        return PagedResult<LessonPayRecordDto>.Create(
            paged.Items.Select(r => r.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);
    }
}
