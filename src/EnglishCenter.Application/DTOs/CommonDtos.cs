namespace EnglishCenter.Application.DTOs;

/// <summary>
/// Audit metadata returned on business resources (API §1.6).
/// </summary>
public record AuditFieldsDto(
    DateTime CreatedAt,
    Guid? CreatedBy,
    DateTime? UpdatedAt,
    Guid? UpdatedBy);
