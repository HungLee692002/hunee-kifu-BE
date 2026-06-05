using System.Linq.Expressions;
using EnglishCenter.Domain.Common;
using EnglishCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Guardian> Guardians => Set<Guardian>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<ClassAssignment> ClassAssignments => Set<ClassAssignment>();
    public DbSet<TeacherLessonRate> TeacherLessonRates => Set<TeacherLessonRate>();
    public DbSet<WeeklyScheduleTemplate> WeeklyScheduleTemplates => Set<WeeklyScheduleTemplate>();
    public DbSet<LessonSession> LessonSessions => Set<LessonSession>();
    public DbSet<LessonSessionStaff> LessonSessionStaffs => Set<LessonSessionStaff>();
    public DbSet<LessonScheduleOverride> LessonScheduleOverrides => Set<LessonScheduleOverride>();
    public DbSet<StudentAttendance> StudentAttendances => Set<StudentAttendance>();
    public DbSet<TeacherAttendance> TeacherAttendances => Set<TeacherAttendance>();
    public DbSet<StudentTuitionMonth> StudentTuitionMonths => Set<StudentTuitionMonth>();
    public DbSet<TuitionPayment> TuitionPayments => Set<TuitionPayment>();
    public DbSet<SalaryPeriod> SalaryPeriods => Set<SalaryPeriod>();
    public DbSet<LessonPayRecord> LessonPayRecords => Set<LessonPayRecord>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(Guid) || property.ClrType == typeof(Guid?))
                    property.SetColumnType("char(36)");
            }
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var isDeleted = Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
            var filter = Expression.Lambda(
                Expression.Equal(isDeleted, Expression.Constant(false)),
                parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }

        // No FK constraints — logical references only (per DATABASE.md)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var fk in entityType.GetForeignKeys().ToList())
                entityType.RemoveForeignKey(fk);
        }
    }
}
