using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;
using EnglishCenter.Domain.Entities;
using EnglishCenter.Domain.Enums;
using EnglishCenter.Domain.Exceptions;
using EnglishCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Services;

public sealed class ScheduleService : IScheduleService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly ILessonSessionGeneratorService _sessionGenerator;

    public ScheduleService(
        AppDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock,
        ILessonSessionGeneratorService sessionGenerator)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _sessionGenerator = sessionGenerator;
    }

    public async Task<IReadOnlyList<ActiveScheduleTemplateDto>> GetActiveTemplatesAsync(
        CancellationToken cancellationToken = default)
    {
        return await (
            from t in _db.WeeklyScheduleTemplates.AsNoTracking()
            join c in _db.Classes.AsNoTracking() on t.ClassId equals c.Id
            join r in _db.Rooms.AsNoTracking() on t.RoomId equals r.Id
            where t.IsActive && c.Status == ClassStatus.Open
            orderby t.DayOfWeek, t.StartTime
            select new ActiveScheduleTemplateDto(
                t.Id,
                t.ClassId,
                c.Name,
                t.DayOfWeek,
                t.StartTime,
                t.EndTime,
                t.RoomId,
                r.Code,
                t.EffectiveFrom,
                t.EffectiveTo)
        ).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WeeklyScheduleTemplateDto>> GetTemplatesByClassAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        await EnsureClassExistsAsync(classId, cancellationToken);
        var items = await _db.WeeklyScheduleTemplates
            .Where(t => t.ClassId == classId)
            .OrderBy(t => t.DayOfWeek).ThenBy(t => t.StartTime)
            .ToListAsync(cancellationToken);
        return items.Select(t => t.ToDto()).ToList();
    }

    public async Task<WeeklyScheduleTemplateDto> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.WeeklyScheduleTemplates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy lịch tuần.");
        return entity.ToDto();
    }

    public async Task<WeeklyScheduleTemplateDto> CreateTemplateAsync(
        Guid classId,
        CreateWeeklyScheduleTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureClassExistsAsync(classId, cancellationToken);
        await ValidateTemplateRefsAsync(request.TeachingMode, request.RoomId, request.PrimaryTeacherId, request.LocalSupportTeacherId, cancellationToken);
        if (request.IsActive)
        {
            await EnsureNoRoomScheduleConflictAsync(
                request.RoomId,
                request.DayOfWeek,
                request.StartTime,
                request.EndTime,
                request.EffectiveFrom,
                request.EffectiveTo,
                excludeTemplateId: null,
                cancellationToken);
        }

        var entity = MapTemplate(new WeeklyScheduleTemplate { ClassId = classId }, request);
        entity.SetCreated(_currentUser.UserId, _clock.UtcNow);
        _db.WeeklyScheduleTemplates.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        await _sessionGenerator.GenerateAsync(classId, cancellationToken);
        return entity.ToDto();
    }

    public async Task<WeeklyScheduleTemplateDto> UpdateTemplateAsync(
        Guid id,
        UpdateWeeklyScheduleTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.WeeklyScheduleTemplates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy lịch tuần.");

        await ValidateTemplateRefsAsync(request.TeachingMode, request.RoomId, request.PrimaryTeacherId, request.LocalSupportTeacherId, cancellationToken);
        if (request.IsActive)
        {
            await EnsureNoRoomScheduleConflictAsync(
                request.RoomId,
                request.DayOfWeek,
                request.StartTime,
                request.EndTime,
                request.EffectiveFrom,
                request.EffectiveTo,
                excludeTemplateId: id,
                cancellationToken);
        }

        MapTemplate(entity, request);
        entity.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        await _sessionGenerator.GenerateAsync(entity.ClassId, cancellationToken);
        return entity.ToDto();
    }

    public async Task<WeeklyScheduleTemplateDto> PatchTemplateAsync(
        Guid id,
        PatchWeeklyScheduleTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.WeeklyScheduleTemplates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy lịch tuần.");

        if (request.DayOfWeek.HasValue) entity.DayOfWeek = request.DayOfWeek.Value;
        if (request.StartTime.HasValue) entity.StartTime = request.StartTime.Value;
        if (request.EndTime.HasValue) entity.EndTime = request.EndTime.Value;
        if (request.RoomId.HasValue) entity.RoomId = request.RoomId.Value;
        if (request.TeachingMode is not null) entity.TeachingMode = EnumNames.ParseTeachingMode(request.TeachingMode);
        if (request.PrimaryTeacherId.HasValue) entity.PrimaryTeacherId = request.PrimaryTeacherId.Value;
        if (request.LocalSupportTeacherId.HasValue || request.TeachingMode is not null)
            entity.LocalSupportTeacherId = request.LocalSupportTeacherId;
        if (request.EffectiveFrom.HasValue) entity.EffectiveFrom = request.EffectiveFrom;
        if (request.EffectiveTo.HasValue) entity.EffectiveTo = request.EffectiveTo;
        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;

        await ValidateTemplateRefsAsync(
            entity.TeachingMode.ToApiName(),
            entity.RoomId,
            entity.PrimaryTeacherId,
            entity.LocalSupportTeacherId,
            cancellationToken);

        if (entity.IsActive)
        {
            await EnsureNoRoomScheduleConflictAsync(
                entity.RoomId,
                entity.DayOfWeek,
                entity.StartTime,
                entity.EndTime,
                entity.EffectiveFrom,
                entity.EffectiveTo,
                excludeTemplateId: id,
                cancellationToken);
        }

        entity.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<PagedResult<LessonSessionDto>> GetSessionsPagedAsync(
        PagedQuery query,
        Guid? classId,
        Guid? teacherId,
        DateOnly? fromDate,
        DateOnly? toDate,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var q = _db.LessonSessions.AsQueryable();

        if (classId.HasValue) q = q.Where(s => s.ClassId == classId);
        if (fromDate.HasValue) q = q.Where(s => s.SessionDate >= fromDate);
        if (toDate.HasValue) q = q.Where(s => s.SessionDate <= toDate);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(s => s.Status == EnumNames.ParseLessonSessionStatus(status));
        if (teacherId.HasValue)
        {
            var sessionIds = _db.LessonSessionStaffs
                .Where(st => st.TeacherId == teacherId)
                .Select(st => st.LessonSessionId);
            q = q.Where(s => sessionIds.Contains(s.Id));
        }

        var paged = await q.ToPagedAsync(query, cancellationToken);
        var dtos = await MapSessionsAsync(paged.Items, cancellationToken);
        return PagedResult<LessonSessionDto>.Create(dtos, paged.Page, paged.PageSize, paged.TotalCount);
    }

    public async Task<LessonSessionDto> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var session = await _db.LessonSessions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy buổi học.");
        return (await MapSessionsAsync([session], cancellationToken)).Single();
    }

    public async Task<LessonScheduleOverrideDto?> GetOverrideAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        await EnsureSessionExistsAsync(sessionId, cancellationToken);
        var entity = await _db.LessonScheduleOverrides
            .FirstOrDefaultAsync(o => o.LessonSessionId == sessionId, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<LessonScheduleOverrideDto> UpsertOverrideAsync(
        Guid sessionId,
        UpsertLessonScheduleOverrideRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _db.LessonSessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy buổi học.");

        var entity = await _db.LessonScheduleOverrides
            .FirstOrDefaultAsync(o => o.LessonSessionId == sessionId, cancellationToken);

        if (entity is null)
        {
            entity = new LessonScheduleOverride { LessonSessionId = sessionId };
            entity.SetCreated(_currentUser.UserId, _clock.UtcNow);
            _db.LessonScheduleOverrides.Add(entity);
        }
        else
        {
            entity.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        }

        entity.OverridePrimaryTeacherId = request.OverridePrimaryTeacherId;
        entity.OverrideSupportTeacherId = request.OverrideSupportTeacherId;
        entity.OverrideRoomId = request.OverrideRoomId;
        entity.OverrideStartTime = request.OverrideStartTime;
        entity.OverrideEndTime = request.OverrideEndTime;
        entity.IsCancelled = request.IsCancelled;
        entity.Reason = request.Reason;
        entity.IncidentAt = _clock.UtcNow;

        session.HasOverride = true;
        session.EffectiveStartTime = request.OverrideStartTime ?? session.PlannedStartTime;
        session.EffectiveEndTime = request.OverrideEndTime ?? session.PlannedEndTime;
        session.EffectiveRoomId = request.OverrideRoomId ?? session.PlannedRoomId;
        session.Status = request.IsCancelled ? LessonSessionStatus.Cancelled : session.Status;
        session.SetUpdated(_currentUser.UserId, _clock.UtcNow);

        await ApplyStaffOverridesAsync(sessionId, request, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task DeleteOverrideAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _db.LessonSessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy buổi học.");

        var entity = await _db.LessonScheduleOverrides
            .FirstOrDefaultAsync(o => o.LessonSessionId == sessionId, cancellationToken);

        if (entity is not null)
        {
            entity.SoftDelete(_currentUser.UserId, _clock.UtcNow);
        }

        session.HasOverride = false;
        session.EffectiveStartTime = session.PlannedStartTime;
        session.EffectiveEndTime = session.PlannedEndTime;
        session.EffectiveRoomId = session.PlannedRoomId;
        if (session.Status == LessonSessionStatus.Cancelled)
            session.Status = LessonSessionStatus.Scheduled;
        session.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyStaffOverridesAsync(
        Guid sessionId,
        UpsertLessonScheduleOverrideRequest request,
        CancellationToken ct)
    {
        if (!request.OverridePrimaryTeacherId.HasValue && !request.OverrideSupportTeacherId.HasValue)
            return;

        var staff = await _db.LessonSessionStaffs
            .Where(s => s.LessonSessionId == sessionId)
            .ToListAsync(ct);

        if (request.OverridePrimaryTeacherId.HasValue)
        {
            var primary = staff.FirstOrDefault(s => s.StaffRole == SessionStaffRole.PrimaryInstructor);
            if (primary is not null)
                primary.TeacherId = request.OverridePrimaryTeacherId.Value;
        }

        if (request.OverrideSupportTeacherId.HasValue)
        {
            var support = staff.FirstOrDefault(s => s.StaffRole == SessionStaffRole.LocalSupport);
            if (support is not null)
                support.TeacherId = request.OverrideSupportTeacherId.Value;
        }
    }

    private async Task<IReadOnlyList<LessonSessionDto>> MapSessionsAsync(
        IReadOnlyList<LessonSession> sessions,
        CancellationToken ct)
    {
        if (sessions.Count == 0) return [];

        var ids = sessions.Select(s => s.Id).ToList();
        var staff = await _db.LessonSessionStaffs
            .Where(s => ids.Contains(s.LessonSessionId))
            .ToListAsync(ct);

        return sessions.Select(s =>
        {
            var sessionStaff = staff.Where(st => st.LessonSessionId == s.Id).Select(st => st.ToDto()).ToList();
            return s.ToDto(sessionStaff);
        }).ToList();
    }

    private static WeeklyScheduleTemplate MapTemplate(
        WeeklyScheduleTemplate entity,
        CreateWeeklyScheduleTemplateRequest request)
    {
        entity.DayOfWeek = request.DayOfWeek;
        entity.StartTime = request.StartTime;
        entity.EndTime = request.EndTime;
        entity.RoomId = request.RoomId;
        entity.TeachingMode = EnumNames.ParseTeachingMode(request.TeachingMode);
        entity.PrimaryTeacherId = request.PrimaryTeacherId;
        entity.LocalSupportTeacherId = request.LocalSupportTeacherId;
        entity.EffectiveFrom = request.EffectiveFrom;
        entity.EffectiveTo = request.EffectiveTo;
        entity.IsActive = request.IsActive;
        return entity;
    }

    private static WeeklyScheduleTemplate MapTemplate(
        WeeklyScheduleTemplate entity,
        UpdateWeeklyScheduleTemplateRequest request)
    {
        entity.DayOfWeek = request.DayOfWeek;
        entity.StartTime = request.StartTime;
        entity.EndTime = request.EndTime;
        entity.RoomId = request.RoomId;
        entity.TeachingMode = EnumNames.ParseTeachingMode(request.TeachingMode);
        entity.PrimaryTeacherId = request.PrimaryTeacherId;
        entity.LocalSupportTeacherId = request.LocalSupportTeacherId;
        entity.EffectiveFrom = request.EffectiveFrom;
        entity.EffectiveTo = request.EffectiveTo;
        entity.IsActive = request.IsActive;
        return entity;
    }

    private async Task ValidateTemplateRefsAsync(
        string teachingMode,
        Guid roomId,
        Guid primaryTeacherId,
        Guid? localSupportTeacherId,
        CancellationToken ct)
    {
        var mode = EnumNames.ParseTeachingMode(teachingMode);
        if (mode == TeachingMode.ForeignLed && !localSupportTeacherId.HasValue)
            throw new BusinessException("ForeignLed yêu cầu localSupportTeacherId.");

        if (!await _db.Rooms.AnyAsync(r => r.Id == roomId, ct))
            throw new NotFoundException("Không tìm thấy phòng học.");
        if (!await _db.Teachers.AnyAsync(t => t.Id == primaryTeacherId, ct))
            throw new NotFoundException("Không tìm thấy giáo viên chính.");
        if (localSupportTeacherId.HasValue &&
            !await _db.Teachers.AnyAsync(t => t.Id == localSupportTeacherId, ct))
            throw new NotFoundException("Không tìm thấy giáo viên hỗ trợ.");
    }

    private async Task EnsureNoRoomScheduleConflictAsync(
        Guid roomId,
        int dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        DateOnly? effectiveFrom,
        DateOnly? effectiveTo,
        Guid? excludeTemplateId,
        CancellationToken ct)
    {
        var candidates = await (
            from t in _db.WeeklyScheduleTemplates.AsNoTracking()
            join c in _db.Classes.AsNoTracking() on t.ClassId equals c.Id
            join r in _db.Rooms.AsNoTracking() on t.RoomId equals r.Id
            where t.IsActive
                && c.Status == ClassStatus.Open
                && t.RoomId == roomId
                && t.DayOfWeek == dayOfWeek
                && (excludeTemplateId == null || t.Id != excludeTemplateId)
            select new
            {
                t.StartTime,
                t.EndTime,
                t.EffectiveFrom,
                t.EffectiveTo,
                ClassName = c.Name,
                RoomCode = r.Code,
            }
        ).ToListAsync(ct);

        foreach (var other in candidates)
        {
            if (!ScheduleConflictHelper.TemplatesConflict(
                    dayOfWeek,
                    roomId,
                    startTime,
                    endTime,
                    effectiveFrom,
                    effectiveTo,
                    dayOfWeek,
                    roomId,
                    other.StartTime,
                    other.EndTime,
                    other.EffectiveFrom,
                    other.EffectiveTo))
            {
                continue;
            }

            throw new ConflictException(
                $"Phòng {other.RoomCode} đã có lớp \"{other.ClassName}\" " +
                $"(thứ {dayOfWeek}, {other.StartTime:HH\\:mm}–{other.EndTime:HH\\:mm}).");
        }
    }

    private async Task EnsureClassExistsAsync(Guid classId, CancellationToken ct)
    {
        if (!await _db.Classes.AnyAsync(c => c.Id == classId, ct))
            throw new NotFoundException("Không tìm thấy lớp học.");
    }

    private async Task EnsureSessionExistsAsync(Guid sessionId, CancellationToken ct)
    {
        if (!await _db.LessonSessions.AnyAsync(s => s.Id == sessionId, ct))
            throw new NotFoundException("Không tìm thấy buổi học.");
    }
}
