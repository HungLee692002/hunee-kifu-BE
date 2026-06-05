using EnglishCenter.Application.Abstractions;
using EnglishCenter.Infrastructure.Jobs;
using EnglishCenter.Infrastructure.Options;
using EnglishCenter.Infrastructure.Persistence;
using EnglishCenter.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishCenter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        var serverVersion = ServerVersion.Parse("8.0.36-mysql");

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(
                connectionString,
                serverVersion,
                mySql => mySql
                    .MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                    .EnableStringComparisonTranslations()));

        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        // ICurrentUserService registered in Api layer
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<ITuitionService, TuitionService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ISalaryService, SalaryService>();
        services.AddScoped<IAssessmentService, AssessmentService>();
        services.AddScoped<IClassAssignmentService, ClassAssignmentService>();
        services.AddScoped<ILessonSessionGeneratorService, LessonSessionGeneratorService>();

        services.AddHostedService<LessonSessionGeneratorJob>();

        return services;
    }
}
