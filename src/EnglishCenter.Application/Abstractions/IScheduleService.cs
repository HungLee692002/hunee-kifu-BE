using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;

namespace EnglishCenter.Application.Abstractions;

public interface IScheduleService
{
    Task<IReadOnlyList<ActiveScheduleTemplateDto>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WeeklyScheduleTemplateDto>> GetTemplatesByClassAsync(Guid classId, CancellationToken cancellationToken = default);
    Task<WeeklyScheduleTemplateDto> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WeeklyScheduleTemplateDto> CreateTemplateAsync(Guid classId, CreateWeeklyScheduleTemplateRequest request, CancellationToken cancellationToken = default);
    Task<WeeklyScheduleTemplateDto> UpdateTemplateAsync(Guid id, UpdateWeeklyScheduleTemplateRequest request, CancellationToken cancellationToken = default);
    Task<WeeklyScheduleTemplateDto> PatchTemplateAsync(Guid id, PatchWeeklyScheduleTemplateRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<LessonSessionDto>> GetSessionsPagedAsync(
        PagedQuery query,
        Guid? classId,
        Guid? teacherId,
        DateOnly? fromDate,
        DateOnly? toDate,
        string? status,
        CancellationToken cancellationToken = default);

    Task<LessonSessionDto> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<LessonScheduleOverrideDto?> GetOverrideAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<LessonScheduleOverrideDto> UpsertOverrideAsync(Guid sessionId, UpsertLessonScheduleOverrideRequest request, CancellationToken cancellationToken = default);
    Task DeleteOverrideAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
