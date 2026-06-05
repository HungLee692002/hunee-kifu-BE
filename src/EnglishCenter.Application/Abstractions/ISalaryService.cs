using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;

namespace EnglishCenter.Application.Abstractions;

public interface ISalaryService
{
    Task<PagedResult<SalaryPeriodDto>> GetPeriodsPagedAsync(PagedQuery query, int? year, int? month, CancellationToken cancellationToken = default);
    Task<SalaryPeriodDto> GetPeriodByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SalaryPeriodDto> PatchPeriodAsync(Guid id, PatchSalaryPeriodRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<LessonPayRecordDto>> GetLessonPayRecordsByPeriodAsync(Guid periodId, PagedQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<LessonPayRecordDto>> GetLessonPayRecordsByTeacherAsync(Guid teacherId, PagedQuery query, CancellationToken cancellationToken = default);
}
