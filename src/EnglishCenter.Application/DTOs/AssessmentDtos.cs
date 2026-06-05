namespace EnglishCenter.Application.Dtos;

public record AssessmentDto(
    Guid Id,
    Guid ClassId,
    string Title,
    DateOnly? AssessmentDate,
    decimal? MaxScore,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateAssessmentRequest(
    string Title,
    DateOnly? AssessmentDate,
    decimal? MaxScore,
    string? Description);

public record GradeDto(
    Guid Id,
    Guid AssessmentId,
    Guid StudentId,
    decimal? Score,
    string? Comment,
    DateTime? GradedAt);

public record GradeItemRequest(
    Guid StudentId,
    decimal? Score,
    string? Comment);

public record ReplaceGradesRequest(IReadOnlyList<GradeItemRequest> Items);

public record ClassAssignmentDto(
    Guid Id,
    Guid ClassId,
    Guid TeacherId,
    string Role,
    DateOnly AssignedFrom,
    DateOnly? AssignedTo,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateClassAssignmentRequest(
    Guid TeacherId,
    string Role,
    DateOnly AssignedFrom,
    DateOnly? AssignedTo);

public record UpdateClassAssignmentRequest(
    Guid TeacherId,
    string Role,
    DateOnly AssignedFrom,
    DateOnly? AssignedTo,
    bool IsActive);

public record PatchClassAssignmentRequest(
    Guid? TeacherId,
    string? Role,
    DateOnly? AssignedFrom,
    DateOnly? AssignedTo,
    bool? IsActive);
