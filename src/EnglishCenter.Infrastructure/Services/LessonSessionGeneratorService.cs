using EnglishCenter.Application.Abstractions;
using EnglishCenter.Domain.Entities;
using EnglishCenter.Domain.Enums;
using EnglishCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnglishCenter.Infrastructure.Services;

public sealed class LessonSessionGeneratorService : ILessonSessionGeneratorService
{
    private readonly AppDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<LessonSessionGeneratorService> _logger;

    public LessonSessionGeneratorService(
        AppDbContext db,
        IDateTimeProvider clock,
        ILogger<LessonSessionGeneratorService> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task<int> GenerateAsync(Guid? classId = null, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(_clock.UtcNow);
        var end = today.AddDays(30);

        var templatesQuery = _db.WeeklyScheduleTemplates
            .Where(t => t.IsActive && !t.IsDeleted);

        if (classId.HasValue)
            templatesQuery = templatesQuery.Where(t => t.ClassId == classId.Value);

        var templates = await templatesQuery.ToListAsync(cancellationToken);

        var created = 0;
        for (var date = today; date <= end; date = date.AddDays(1))
        {
            var dow = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;

            foreach (var t in templates.Where(x => x.DayOfWeek == dow))
            {
                if (t.EffectiveFrom.HasValue && date < t.EffectiveFrom) continue;
                if (t.EffectiveTo.HasValue && date > t.EffectiveTo) continue;

                var exists = await _db.LessonSessions.AnyAsync(s =>
                    s.ClassId == t.ClassId &&
                    s.SessionDate == date &&
                    s.PlannedStartTime == t.StartTime &&
                    !s.IsDeleted, cancellationToken);

                if (exists) continue;

                var session = new LessonSession
                {
                    ClassId = t.ClassId,
                    WeeklyScheduleTemplateId = t.Id,
                    SessionDate = date,
                    Status = LessonSessionStatus.Scheduled,
                    TeachingMode = t.TeachingMode,
                    PlannedStartTime = t.StartTime,
                    PlannedEndTime = t.EndTime,
                    PlannedRoomId = t.RoomId,
                    EffectiveStartTime = t.StartTime,
                    EffectiveEndTime = t.EndTime,
                    EffectiveRoomId = t.RoomId,
                    HasOverride = false,
                };
                session.SetCreated(null, _clock.UtcNow);
                _db.LessonSessions.Add(session);

                var staffEntries = new List<LessonSessionStaff>
                {
                    new()
                    {
                        LessonSessionId = session.Id,
                        TeacherId = t.PrimaryTeacherId,
                        StaffRole = SessionStaffRole.PrimaryInstructor,
                        PayMultiplier = 1.0m,
                    },
                };

                if (t.TeachingMode == TeachingMode.ForeignLed && t.LocalSupportTeacherId.HasValue)
                {
                    staffEntries.Add(new LessonSessionStaff
                    {
                        LessonSessionId = session.Id,
                        TeacherId = t.LocalSupportTeacherId.Value,
                        StaffRole = SessionStaffRole.LocalSupport,
                        PayMultiplier = 0.7m,
                    });
                }

                foreach (var s in staffEntries)
                {
                    s.SetCreated(null, _clock.UtcNow);
                    _db.LessonSessionStaffs.Add(s);
                }

                created++;
            }
        }

        if (created > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Generated {Count} lesson sessions{Scope}.",
                created,
                classId.HasValue ? $" for class {classId}" : string.Empty);
        }

        return created;
    }
}
