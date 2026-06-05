namespace EnglishCenter.Application.Dtos;

public record DashboardSummaryDto(
    PeriodDto Period,
    PeriodDto PreviousPeriod,
    DashboardStudentsDto Students,
    DashboardAttendanceDto Attendance,
    DashboardTuitionDto Tuition,
    DashboardTeachingDto Teaching,
    DashboardSalaryDto Salary);

public record PeriodDto(int Year, int Month);

public record DashboardStudentsDto(
    int ActiveCount,
    int ActiveCountPrevious,
    int NewEnrollmentsThisMonth);

public record DashboardAttendanceDto(
    decimal RatePercent,
    decimal RatePercentPrevious);

public record DashboardTuitionDto(
    decimal ExpectedTotal,
    decimal PaidTotal,
    decimal OutstandingTotal,
    decimal ExpectedTotalPrevious,
    decimal PaidTotalPrevious);

public record DashboardTeachingDto(
    int CompletedSessionsCount,
    int CompletedSessionsCountPrevious);

public record DashboardSalaryDto(
    decimal EstimatedPayTotal,
    decimal EstimatedPayTotalPrevious);
