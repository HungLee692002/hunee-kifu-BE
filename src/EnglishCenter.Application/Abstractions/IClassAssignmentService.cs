using EnglishCenter.Application.Dtos;

namespace EnglishCenter.Application.Abstractions;

public interface IClassAssignmentService
{
    Task<IReadOnlyList<ClassAssignmentDto>> GetByClassAsync(Guid classId, CancellationToken cancellationToken = default);
    Task<ClassAssignmentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClassAssignmentDto> CreateAsync(Guid classId, CreateClassAssignmentRequest request, CancellationToken cancellationToken = default);
    Task<ClassAssignmentDto> UpdateAsync(Guid id, UpdateClassAssignmentRequest request, CancellationToken cancellationToken = default);
    Task<ClassAssignmentDto> PatchAsync(Guid id, PatchClassAssignmentRequest request, CancellationToken cancellationToken = default);
}
