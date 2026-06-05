using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;

namespace EnglishCenter.Application.Abstractions;

public interface ICourseService
{
    Task<PagedResult<CourseDto>> GetPagedAsync(PagedQuery query, CancellationToken cancellationToken = default);
    Task<CourseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CourseDto> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken = default);
    Task<CourseDto> UpdateAsync(Guid id, UpdateCourseRequest request, CancellationToken cancellationToken = default);
    Task<CourseDto> PatchAsync(Guid id, PatchCourseRequest request, CancellationToken cancellationToken = default);
}
