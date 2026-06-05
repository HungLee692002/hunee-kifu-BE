using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;

namespace EnglishCenter.Application.Abstractions;

public interface ITeacherService
{
    Task<PagedResult<TeacherDto>> GetPagedAsync(PagedQuery query, CancellationToken cancellationToken = default);
    Task<TeacherDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TeacherDto> CreateAsync(CreateTeacherRequest request, CancellationToken cancellationToken = default);
    Task<TeacherDto> UpdateAsync(Guid id, UpdateTeacherRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeacherLessonRateDto>> GetLessonRatesAsync(Guid teacherId, CancellationToken cancellationToken = default);
    Task<TeacherLessonRateDto> CreateLessonRateAsync(Guid teacherId, CreateTeacherLessonRateRequest request, CancellationToken cancellationToken = default);
}
