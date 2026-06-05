using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;

namespace EnglishCenter.Application.Abstractions;

public interface IStudentService
{
    Task<PagedResult<StudentDto>> GetPagedAsync(
        PagedQuery query,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default);
    Task<StudentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);
    Task<StudentDto> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GuardianDto>> GetGuardiansAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<GuardianDto> CreateGuardianAsync(Guid studentId, CreateGuardianRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<EnrollmentDto>> GetEnrollmentsByStudentAsync(Guid studentId, PagedQuery query, string? status, CancellationToken cancellationToken = default);
}
