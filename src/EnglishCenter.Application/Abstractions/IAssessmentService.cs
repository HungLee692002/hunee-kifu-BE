using EnglishCenter.Application.Dtos;

namespace EnglishCenter.Application.Abstractions;

public interface IAssessmentService
{
    Task<IReadOnlyList<AssessmentDto>> GetByClassAsync(Guid classId, CancellationToken cancellationToken = default);
    Task<AssessmentDto> CreateAsync(Guid classId, CreateAssessmentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GradeDto>> GetGradesAsync(Guid assessmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GradeDto>> ReplaceGradesAsync(Guid assessmentId, ReplaceGradesRequest request, CancellationToken cancellationToken = default);
}
