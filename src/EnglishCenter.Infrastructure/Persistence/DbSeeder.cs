using EnglishCenter.Domain.Entities;
using EnglishCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EnglishCenter.Infrastructure.Persistence;

public static class DbSeeder
{
    private static readonly (string Code, string Name)[] Roles =
    [
        ("Admin", "Quản trị viên"),
        ("AcademicStaff", "Giáo vụ"),
        ("Accountant", "Kế toán"),
        ("Teacher", "Giáo viên"),
        ("Receptionist", "Lễ tân"),
    ];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        var pending = await db.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pending.Any())
            await db.Database.MigrateAsync(cancellationToken);

        var roleIds = new Dictionary<string, Guid>();
        foreach (var (code, name) in Roles)
        {
            var existing = await db.Roles.IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Code == code, cancellationToken);

            if (existing is not null)
            {
                roleIds[code] = existing.Id;
                continue;
            }

            var role = new Role { Code = code, Name = name };
            role.SetCreated(null, DateTime.UtcNow);
            db.Roles.Add(role);
            roleIds[code] = role.Id;
        }

        await db.SaveChangesAsync(cancellationToken);

        const string adminUsername = "admin";
        var admin = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Username == adminUsername, cancellationToken);

        if (admin is null)
        {
            admin = new User
            {
                Username = adminUsername,
                FullName = "Quản trị viên",
                Status = UserStatus.Active,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            };
            admin.SetCreated(null, DateTime.UtcNow);
            db.Users.Add(admin);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded admin user '{Username}'.", adminUsername);
        }

        if (roleIds.TryGetValue("Admin", out var adminRoleId))
        {
            var hasAdminRole = await db.UserRoles.AnyAsync(
                ur => ur.UserId == admin.Id && ur.RoleId == adminRoleId,
                cancellationToken);

            if (!hasAdminRole)
            {
                var userRole = new UserRole { UserId = admin.Id, RoleId = adminRoleId };
                userRole.SetCreated(null, DateTime.UtcNow);
                db.UserRoles.Add(userRole);
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
