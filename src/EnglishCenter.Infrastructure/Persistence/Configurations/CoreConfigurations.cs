using EnglishCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishCenter.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnType("char(36)");
        b.HasIndex(x => x.Username).IsUnique();
        b.Property(x => x.Username).HasMaxLength(64);
        b.Property(x => x.PasswordHash).HasMaxLength(512);
        b.Property(x => x.FullName).HasMaxLength(200);
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("roles");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Code).IsUnique();
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> b)
    {
        b.ToTable("user_roles");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_tokens");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.TokenHash);
    }
}

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> b)
    {
        b.ToTable("rooms");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Code).IsUnique();
    }
}

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> b)
    {
        b.ToTable("courses");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Code).IsUnique();
    }
}

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> b)
    {
        b.ToTable("classes");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.DefaultMonthlyTuition).HasPrecision(18, 2);
    }
}

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> b)
    {
        b.ToTable("students");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Code).IsUnique();
    }
}

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> b)
    {
        b.ToTable("enrollments");
        b.HasKey(x => x.Id);
        b.Property(x => x.MonthlyTuitionAmount).HasPrecision(18, 2);
    }
}

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> b)
    {
        b.ToTable("teachers");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.CurrentLessonRate).HasPrecision(18, 2);
    }
}

public class StudentTuitionMonthConfiguration : IEntityTypeConfiguration<StudentTuitionMonth>
{
    public void Configure(EntityTypeBuilder<StudentTuitionMonth> b)
    {
        b.ToTable("student_tuition_months");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.StudentId, x.BillingYear, x.BillingMonth }).IsUnique();
        b.Property(x => x.ExpectedAmount).HasPrecision(18, 2);
        b.Property(x => x.AmountPaid).HasPrecision(18, 2);
    }
}

public class LessonSessionConfiguration : IEntityTypeConfiguration<LessonSession>
{
    public void Configure(EntityTypeBuilder<LessonSession> b)
    {
        b.ToTable("lesson_sessions");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ClassId, x.SessionDate, x.PlannedStartTime }).IsUnique();
    }
}
