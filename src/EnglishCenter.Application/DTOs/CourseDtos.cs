namespace EnglishCenter.Application.Dtos;

public record CourseDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    Guid? CreatedBy,
    DateTime? UpdatedAt,
    Guid? UpdatedBy);

public record CreateCourseRequest(string Code, string Name, string? Description, bool IsActive = true);

public record UpdateCourseRequest(string Code, string Name, string? Description, bool IsActive);

public record PatchCourseRequest(
    string? Code = null,
    string? Name = null,
    string? Description = null,
    bool? IsActive = null);
