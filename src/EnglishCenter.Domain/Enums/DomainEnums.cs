namespace EnglishCenter.Domain.Enums;

public enum UserStatus { Inactive = 0, Active = 1 }
public enum StudentStatus { Inactive = 0, Active = 1, Graduated = 2 }
public enum TeacherType { Local = 1, Foreign = 2 }
public enum TeacherStatus { Inactive = 0, Active = 1 }
public enum ClassStatus { Draft = 0, Open = 1, Closed = 2 }
public enum EnrollmentStatus { Ended = 0, Active = 1 }
public enum ClassAssignmentRole { Main = 1, Assistant = 2 }
public enum TeachingMode { LocalLed = 1, ForeignLed = 2 }
public enum SessionStaffRole { PrimaryInstructor = 1, LocalSupport = 2 }
public enum LessonSessionStatus { Scheduled = 0, Completed = 1, Cancelled = 2 }
public enum AttendanceStatus { Absent = 0, Present = 1, Late = 2, Excused = 3 }
public enum TuitionMonthStatus { Unpaid = 0, Partial = 1, Paid = 2 }
public enum PaymentMethod { Cash = 1, BankTransfer = 2 }
public enum SalaryPeriodStatus { Open = 0, Closed = 1 }
public enum LessonPayStatus { Pending = 0, Confirmed = 1, Reversed = 2 }
