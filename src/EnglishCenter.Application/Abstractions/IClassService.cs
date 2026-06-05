using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;

namespace EnglishCenter.Application.Abstractions;

public interface IClassService
{
    Task<PagedResult<ClassDto>> GetPagedAsync(PagedQuery query, CancellationToken cancellationToken = default);
    Task<ClassDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClassDto> CreateAsync(CreateClassRequest request, CancellationToken cancellationToken = default);
    Task<ClassDto> UpdateAsync(Guid id, UpdateClassRequest request, CancellationToken cancellationToken = default);
    Task<ClassDto> PatchAsync(Guid id, PatchClassRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<EnrollmentDto>> GetEnrollmentsByClassAsync(Guid classId, PagedQuery query, string? status, CancellationToken cancellationToken = default);
    Task<EnrollmentDto> GetEnrollmentByIdAsync(Guid enrollmentId, CancellationToken cancellationToken = default);
    Task<EnrollmentDto> CreateEnrollmentAsync(Guid classId, CreateEnrollmentRequest request, CancellationToken cancellationToken = default);
    Task<EnrollmentDto> PatchEnrollmentAsync(Guid enrollmentId, PatchEnrollmentRequest request, CancellationToken cancellationToken = default);
}
