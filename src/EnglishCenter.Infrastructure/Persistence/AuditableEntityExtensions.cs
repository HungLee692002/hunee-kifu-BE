using EnglishCenter.Domain.Common;

namespace EnglishCenter.Infrastructure.Persistence;

public static class AuditableEntityExtensions
{
    public static void SetCreated(this AuditableEntity entity, Guid? userId, DateTime utcNow)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = utcNow;
        entity.CreatedBy = userId;
        entity.UpdatedAt = utcNow;
        entity.UpdatedBy = userId;
        entity.IsDeleted = false;
    }

    public static void SetUpdated(this AuditableEntity entity, Guid? userId, DateTime utcNow)
    {
        entity.UpdatedAt = utcNow;
        entity.UpdatedBy = userId;
    }

    public static void SoftDelete(this AuditableEntity entity, Guid? userId, DateTime utcNow)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = utcNow;
        SetUpdated(entity, userId, utcNow);
    }

    public static string NewGuidCode() => Guid.NewGuid().ToString();
}
