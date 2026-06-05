using EnglishCenter.Domain.Enums;

namespace EnglishCenter.Application.Common;

public static class EnumNames
{
    public static string ToApiName(this StudentStatus status) => status.ToString();
    public static string ToApiName(this TeacherStatus status) => status.ToString();
    public static string ToApiName(this TeacherType type) => type.ToString();
    public static string ToApiName(this ClassStatus status) => status.ToString();
    public static string ToApiName(this EnrollmentStatus status) => status.ToString();
    public static string ToApiName(this TuitionMonthStatus status) => status.ToString();
    public static string ToApiName(this PaymentMethod method) => method.ToString();
    public static string ToApiName(this TeachingMode mode) => mode.ToString();
    public static string ToApiName(this LessonSessionStatus status) => status.ToString();
    public static string ToApiName(this SessionStaffRole role) => role.ToString();
    public static string ToApiName(this AttendanceStatus status) => status.ToString();
    public static string ToApiName(this SalaryPeriodStatus status) => status.ToString();
    public static string ToApiName(this LessonPayStatus status) => status.ToString();
    public static string ToApiName(this ClassAssignmentRole role) => role.ToString();

    public static StudentStatus ParseStudentStatus(string value) =>
        Enum.Parse<StudentStatus>(value, ignoreCase: true);

    public static TeacherStatus ParseTeacherStatus(string value) =>
        Enum.Parse<TeacherStatus>(value, ignoreCase: true);

    public static TeacherType ParseTeacherType(string value) =>
        Enum.Parse<TeacherType>(value, ignoreCase: true);

    public static ClassStatus ParseClassStatus(string value) =>
        Enum.Parse<ClassStatus>(value, ignoreCase: true);

    public static EnrollmentStatus ParseEnrollmentStatus(string value) =>
        Enum.Parse<EnrollmentStatus>(value, ignoreCase: true);

    public static TuitionMonthStatus ParseTuitionMonthStatus(string value) =>
        Enum.Parse<TuitionMonthStatus>(value, ignoreCase: true);

    public static PaymentMethod ParsePaymentMethod(string value) =>
        Enum.Parse<PaymentMethod>(value, ignoreCase: true);

    public static TeachingMode ParseTeachingMode(string value) =>
        Enum.Parse<TeachingMode>(value, ignoreCase: true);

    public static LessonSessionStatus ParseLessonSessionStatus(string value) =>
        Enum.Parse<LessonSessionStatus>(value, ignoreCase: true);

    public static SessionStaffRole ParseSessionStaffRole(string value) =>
        Enum.Parse<SessionStaffRole>(value, ignoreCase: true);

    public static AttendanceStatus ParseAttendanceStatus(string value) =>
        Enum.Parse<AttendanceStatus>(value, ignoreCase: true);

    public static SalaryPeriodStatus ParseSalaryPeriodStatus(string value) =>
        Enum.Parse<SalaryPeriodStatus>(value, ignoreCase: true);

    public static ClassAssignmentRole ParseClassAssignmentRole(string value) =>
        Enum.Parse<ClassAssignmentRole>(value, ignoreCase: true);
}
