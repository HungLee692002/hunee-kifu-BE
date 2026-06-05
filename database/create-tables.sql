-- =============================================================================
-- English Center — Tạo schema MySQL 8
-- Cập nhật: 2026-06-01
-- Tham chiếu: docs/DATABASE.md, EF migration InitialCreate
--
-- Lưu ý:
--   - Không tạo FOREIGN KEY (toàn vẹn do Application layer).
--   - Tên cột PascalCase khớp Entity Framework Core.
--   - Một số bảng dùng PascalCase (EF mặc định), một số dùng snake_case (đã cấu hình).
--
-- Cách chạy:
--   mysql -u root -p < database/create-tables.sql
--   hoặc mở file trong MySQL Workbench / DBeaver.
--
-- Nếu dùng EF Migrate thay vì script này, KHÔNG chạy song song — chọn một cách.
-- =============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

CREATE DATABASE IF NOT EXISTS `english_center`
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE `english_center`;

-- -----------------------------------------------------------------------------
-- Auth
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS `users` (
  `Id`              CHAR(36)     NOT NULL,
  `Username`        VARCHAR(64)  NOT NULL,
  `Email`           LONGTEXT     NULL,
  `PasswordHash`    VARCHAR(512) NOT NULL,
  `FullName`        VARCHAR(200) NOT NULL,
  `Status`          INT          NOT NULL COMMENT '0=Inactive, 1=Active',
  `LastLoginAt`     DATETIME(6)  NULL,
  `CreatedAt`       DATETIME(6)  NOT NULL,
  `CreatedBy`       CHAR(36)     NULL,
  `UpdatedAt`       DATETIME(6)  NULL,
  `UpdatedBy`       CHAR(36)     NULL,
  `IsDeleted`       TINYINT(1)   NOT NULL DEFAULT 0,
  `DeletedAt`       DATETIME(6)  NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_users_Username` (`Username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `roles` (
  `Id`          CHAR(36)     NOT NULL,
  `Code`        VARCHAR(255) NOT NULL,
  `Name`        LONGTEXT     NOT NULL,
  `Description` LONGTEXT     NULL,
  `CreatedAt`   DATETIME(6)  NOT NULL,
  `CreatedBy`   CHAR(36)     NULL,
  `UpdatedAt`   DATETIME(6)  NULL,
  `UpdatedBy`   CHAR(36)     NULL,
  `IsDeleted`   TINYINT(1)   NOT NULL DEFAULT 0,
  `DeletedAt`   DATETIME(6)  NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_roles_Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `user_roles` (
  `Id`        CHAR(36)    NOT NULL,
  `UserId`    CHAR(36)    NOT NULL,
  `RoleId`    CHAR(36)    NOT NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  `CreatedBy` CHAR(36)    NULL,
  `UpdatedAt` DATETIME(6) NULL,
  `UpdatedBy` CHAR(36)    NULL,
  `IsDeleted` TINYINT(1)  NOT NULL DEFAULT 0,
  `DeletedAt` DATETIME(6) NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_user_roles_UserId_RoleId` (`UserId`, `RoleId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `refresh_tokens` (
  `Id`          CHAR(36)     NOT NULL,
  `UserId`      CHAR(36)     NOT NULL,
  `TokenHash`   VARCHAR(255) NOT NULL,
  `ExpiresAt`   DATETIME(6)  NOT NULL,
  `RevokedAt`   DATETIME(6)  NULL,
  `CreatedAt`   DATETIME(6)  NOT NULL,
  `CreatedByIp` LONGTEXT     NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_refresh_tokens_TokenHash` (`TokenHash`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Danh mục
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS `courses` (
  `Id`          CHAR(36)     NOT NULL,
  `Code`        VARCHAR(255) NOT NULL,
  `Name`        LONGTEXT     NOT NULL,
  `Description` LONGTEXT     NULL,
  `IsActive`    TINYINT(1)   NOT NULL DEFAULT 1,
  `CreatedAt`   DATETIME(6)  NOT NULL,
  `CreatedBy`   CHAR(36)     NULL,
  `UpdatedAt`   DATETIME(6)  NULL,
  `UpdatedBy`   CHAR(36)     NULL,
  `IsDeleted`   TINYINT(1)   NOT NULL DEFAULT 0,
  `DeletedAt`   DATETIME(6)  NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_courses_Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `rooms` (
  `Id`        CHAR(36)     NOT NULL,
  `Code`      VARCHAR(255) NOT NULL,
  `Name`      LONGTEXT     NOT NULL,
  `Capacity`  INT          NULL,
  `Floor`     LONGTEXT     NULL,
  `Note`      LONGTEXT     NULL,
  `IsActive`  TINYINT(1)   NOT NULL DEFAULT 1,
  `CreatedAt` DATETIME(6)  NOT NULL,
  `CreatedBy` CHAR(36)     NULL,
  `UpdatedAt` DATETIME(6)  NULL,
  `UpdatedBy` CHAR(36)     NULL,
  `IsDeleted` TINYINT(1)   NOT NULL DEFAULT 0,
  `DeletedAt` DATETIME(6)  NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_rooms_Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `classes` (
  `Id`                      CHAR(36)       NOT NULL,
  `Code`                    VARCHAR(255)   NOT NULL,
  `CourseId`                CHAR(36)       NOT NULL,
  `Name`                    LONGTEXT       NOT NULL,
  `Status`                  INT            NOT NULL COMMENT '0=Draft, 1=Open, 2=Closed',
  `GradingEnabled`          TINYINT(1)     NOT NULL DEFAULT 0,
  `DefaultMonthlyTuition`   DECIMAL(18,2)  NULL,
  `StartDate`               DATE           NULL,
  `EndDate`                 DATE           NULL,
  `CreatedAt`               DATETIME(6)    NOT NULL,
  `CreatedBy`               CHAR(36)       NULL,
  `UpdatedAt`               DATETIME(6)    NULL,
  `UpdatedBy`               CHAR(36)       NULL,
  `IsDeleted`               TINYINT(1)     NOT NULL DEFAULT 0,
  `DeletedAt`               DATETIME(6)    NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_classes_Code` (`Code`),
  KEY `idx_classes_course` (`CourseId`),
  KEY `idx_classes_status` (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Học sinh & giáo viên
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS `students` (
  `Id`                    CHAR(36)     NOT NULL,
  `Code`                  VARCHAR(255) NOT NULL,
  `FullName`              LONGTEXT     NOT NULL,
  `DateOfBirth`           DATE         NULL,
  `Gender`                INT          NULL,
  `Phone`                 LONGTEXT     NULL,
  `Email`                 LONGTEXT     NULL,
  `Address`               LONGTEXT     NULL,
  `Status`                INT          NOT NULL COMMENT '0=Inactive, 1=Active, 2=Graduated',
  `CurrentEnrollmentId`   CHAR(36)     NULL,
  `Note`                  LONGTEXT     NULL,
  `CreatedAt`             DATETIME(6)  NOT NULL,
  `CreatedBy`             CHAR(36)     NULL,
  `UpdatedAt`             DATETIME(6)  NULL,
  `UpdatedBy`             CHAR(36)     NULL,
  `IsDeleted`             TINYINT(1)   NOT NULL DEFAULT 0,
  `DeletedAt`             DATETIME(6)  NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_students_Code` (`Code`),
  KEY `idx_students_status` (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Guardians` (
  `Id`           CHAR(36)    NOT NULL,
  `StudentId`    CHAR(36)    NOT NULL,
  `FullName`     LONGTEXT    NOT NULL,
  `Relationship` LONGTEXT    NULL,
  `Phone`        LONGTEXT    NOT NULL,
  `Email`        LONGTEXT    NULL,
  `IsPrimary`    TINYINT(1)  NOT NULL DEFAULT 0,
  `CreatedAt`    DATETIME(6) NOT NULL,
  `CreatedBy`    CHAR(36)    NULL,
  `UpdatedAt`    DATETIME(6) NULL,
  `UpdatedBy`    CHAR(36)    NULL,
  `IsDeleted`    TINYINT(1)  NOT NULL DEFAULT 0,
  `DeletedAt`    DATETIME(6) NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_guardians_student` (`StudentId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `teachers` (
  `Id`                  CHAR(36)       NOT NULL,
  `Code`                VARCHAR(255)   NOT NULL,
  `FullName`            LONGTEXT       NOT NULL,
  `TeacherType`         INT            NOT NULL COMMENT '1=Local, 2=Foreign',
  `CurrentLessonRate`   DECIMAL(18,2)  NULL,
  `Phone`               LONGTEXT       NULL,
  `Email`               LONGTEXT       NULL,
  `Status`              INT            NOT NULL COMMENT '0=Inactive, 1=Active',
  `UserId`              CHAR(36)       NULL,
  `Note`                LONGTEXT       NULL,
  `CreatedAt`           DATETIME(6)    NOT NULL,
  `CreatedBy`           CHAR(36)       NULL,
  `UpdatedAt`           DATETIME(6)    NULL,
  `UpdatedBy`           CHAR(36)       NULL,
  `IsDeleted`           TINYINT(1)     NOT NULL DEFAULT 0,
  `DeletedAt`           DATETIME(6)    NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_teachers_Code` (`Code`),
  KEY `idx_teachers_user` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `enrollments` (
  `Id`                     CHAR(36)       NOT NULL,
  `StudentId`              CHAR(36)       NOT NULL,
  `ClassId`                CHAR(36)       NOT NULL,
  `Status`                 INT            NOT NULL COMMENT '0=Ended, 1=Active',
  `EnrolledAt`             DATE           NOT NULL,
  `EndedAt`                DATE           NULL,
  `MonthlyTuitionAmount`   DECIMAL(18,2)  NULL,
  `CreatedAt`              DATETIME(6)    NOT NULL,
  `CreatedBy`              CHAR(36)       NULL,
  `UpdatedAt`              DATETIME(6)    NULL,
  `UpdatedBy`              CHAR(36)       NULL,
  `IsDeleted`              TINYINT(1)     NOT NULL DEFAULT 0,
  `DeletedAt`              DATETIME(6)    NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_enrollments_student_status` (`StudentId`, `Status`),
  KEY `idx_enrollments_class` (`ClassId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `ClassAssignments` (
  `Id`           CHAR(36)    NOT NULL,
  `ClassId`      CHAR(36)    NOT NULL,
  `TeacherId`    CHAR(36)    NOT NULL,
  `Role`         INT         NOT NULL COMMENT '1=Main, 2=Assistant',
  `AssignedFrom` DATE        NOT NULL,
  `AssignedTo`   DATE        NULL,
  `IsActive`     TINYINT(1)  NOT NULL DEFAULT 1,
  `CreatedAt`    DATETIME(6) NOT NULL,
  `CreatedBy`    CHAR(36)    NULL,
  `UpdatedAt`    DATETIME(6) NULL,
  `UpdatedBy`    CHAR(36)    NULL,
  `IsDeleted`    TINYINT(1)  NOT NULL DEFAULT 0,
  `DeletedAt`    DATETIME(6) NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_class_assignments_class` (`ClassId`),
  KEY `idx_class_assignments_teacher` (`TeacherId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `TeacherLessonRates` (
  `Id`             CHAR(36)       NOT NULL,
  `TeacherId`      CHAR(36)       NOT NULL,
  `LessonRate`     DECIMAL(18,2)  NOT NULL,
  `EffectiveFrom`  DATE           NOT NULL,
  `EffectiveTo`    DATE           NULL,
  `IsActive`       TINYINT(1)     NOT NULL DEFAULT 1,
  `Note`           LONGTEXT       NULL,
  `CreatedAt`      DATETIME(6)    NOT NULL,
  `CreatedBy`      CHAR(36)       NULL,
  `UpdatedAt`      DATETIME(6)    NULL,
  `UpdatedBy`      CHAR(36)       NULL,
  `IsDeleted`      TINYINT(1)     NOT NULL DEFAULT 0,
  `DeletedAt`      DATETIME(6)    NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_teacher_lesson_rates` (`TeacherId`, `IsActive`, `EffectiveFrom`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Lịch học
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS `WeeklyScheduleTemplates` (
  `Id`                      CHAR(36)    NOT NULL,
  `ClassId`                 CHAR(36)    NOT NULL,
  `DayOfWeek`               INT         NOT NULL COMMENT '1=Mon … 7=Sun (ISO)',
  `StartTime`               TIME(6)     NOT NULL,
  `EndTime`                 TIME(6)     NOT NULL,
  `RoomId`                  CHAR(36)    NOT NULL,
  `TeachingMode`            INT         NOT NULL COMMENT '1=LocalLed, 2=ForeignLed',
  `PrimaryTeacherId`        CHAR(36)    NOT NULL,
  `LocalSupportTeacherId`   CHAR(36)    NULL,
  `EffectiveFrom`           DATE        NULL,
  `EffectiveTo`             DATE        NULL,
  `IsActive`                TINYINT(1)  NOT NULL DEFAULT 1,
  `CreatedAt`               DATETIME(6) NOT NULL,
  `CreatedBy`               CHAR(36)    NULL,
  `UpdatedAt`               DATETIME(6) NULL,
  `UpdatedBy`               CHAR(36)    NULL,
  `IsDeleted`               TINYINT(1)  NOT NULL DEFAULT 0,
  `DeletedAt`               DATETIME(6) NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_schedule_templates_class` (`ClassId`, `IsActive`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `lesson_sessions` (
  `Id`                        CHAR(36)    NOT NULL,
  `ClassId`                   CHAR(36)    NOT NULL,
  `WeeklyScheduleTemplateId`  CHAR(36)    NULL,
  `SessionDate`               DATE        NOT NULL,
  `Status`                    INT         NOT NULL COMMENT '0=Scheduled, 1=Completed, 2=Cancelled',
  `TeachingMode`              INT         NOT NULL,
  `PlannedStartTime`          TIME(6)     NOT NULL,
  `PlannedEndTime`            TIME(6)     NOT NULL,
  `PlannedRoomId`             CHAR(36)    NOT NULL,
  `EffectiveStartTime`        TIME(6)     NOT NULL,
  `EffectiveEndTime`          TIME(6)     NOT NULL,
  `EffectiveRoomId`           CHAR(36)    NOT NULL,
  `HasOverride`               TINYINT(1)  NOT NULL DEFAULT 0,
  `CompletedAt`               DATETIME(6) NULL,
  `CreatedAt`                 DATETIME(6) NOT NULL,
  `CreatedBy`                 CHAR(36)    NULL,
  `UpdatedAt`                 DATETIME(6) NULL,
  `UpdatedBy`                 CHAR(36)    NULL,
  `IsDeleted`                 TINYINT(1)  NOT NULL DEFAULT 0,
  `DeletedAt`                 DATETIME(6) NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_lesson_sessions_ClassId_SessionDate_PlannedStartTime` (`ClassId`, `SessionDate`, `PlannedStartTime`),
  KEY `idx_lesson_sessions_date` (`SessionDate`, `Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `LessonSessionStaffs` (
  `Id`               CHAR(36)       NOT NULL,
  `LessonSessionId`  CHAR(36)       NOT NULL,
  `TeacherId`        CHAR(36)       NOT NULL,
  `StaffRole`        INT            NOT NULL COMMENT '1=PrimaryInstructor, 2=LocalSupport',
  `PayMultiplier`    DECIMAL(18,4)  NOT NULL DEFAULT 1.0000,
  `CreatedAt`        DATETIME(6)    NOT NULL,
  `CreatedBy`        CHAR(36)       NULL,
  `UpdatedAt`        DATETIME(6)    NULL,
  `UpdatedBy`        CHAR(36)       NULL,
  `IsDeleted`        TINYINT(1)     NOT NULL DEFAULT 0,
  `DeletedAt`        DATETIME(6)    NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_session_staff_session` (`LessonSessionId`),
  KEY `idx_session_staff_teacher` (`TeacherId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `LessonScheduleOverrides` (
  `Id`                        CHAR(36)    NOT NULL,
  `LessonSessionId`           CHAR(36)    NOT NULL,
  `OverridePrimaryTeacherId`  CHAR(36)    NULL,
  `OverrideSupportTeacherId`  CHAR(36)    NULL,
  `OverrideRoomId`            CHAR(36)    NULL,
  `OverrideStartTime`         TIME(6)     NULL,
  `OverrideEndTime`           TIME(6)     NULL,
  `IsCancelled`               TINYINT(1)  NOT NULL DEFAULT 0,
  `Reason`                    LONGTEXT    NULL,
  `IncidentAt`                DATETIME(6) NOT NULL,
  `CreatedAt`                 DATETIME(6) NOT NULL,
  `CreatedBy`                 CHAR(36)    NULL,
  `UpdatedAt`                 DATETIME(6) NULL,
  `UpdatedBy`                 CHAR(36)    NULL,
  `IsDeleted`                 TINYINT(1)  NOT NULL DEFAULT 0,
  `DeletedAt`                 DATETIME(6) NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_lesson_schedule_override_session` (`LessonSessionId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Điểm danh
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS `StudentAttendances` (
  `Id`               CHAR(36)    NOT NULL,
  `LessonSessionId`  CHAR(36)    NOT NULL,
  `StudentId`        CHAR(36)    NOT NULL,
  `EnrollmentId`     CHAR(36)    NULL,
  `Status`           INT         NOT NULL COMMENT '0=Absent, 1=Present, 2=Late, 3=Excused',
  `Note`             LONGTEXT    NULL,
  `RecordedAt`       DATETIME(6) NOT NULL,
  `RecordedBy`       CHAR(36)    NULL,
  `CreatedAt`        DATETIME(6) NOT NULL,
  `CreatedBy`        CHAR(36)    NULL,
  `UpdatedAt`        DATETIME(6) NULL,
  `UpdatedBy`        CHAR(36)    NULL,
  `IsDeleted`        TINYINT(1)  NOT NULL DEFAULT 0,
  `DeletedAt`        DATETIME(6) NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_student_attendance_session_student` (`LessonSessionId`, `StudentId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `TeacherAttendances` (
  `Id`                   CHAR(36)    NOT NULL,
  `LessonSessionId`      CHAR(36)    NOT NULL,
  `TeacherId`            CHAR(36)    NOT NULL,
  `LessonSessionStaffId` CHAR(36)    NULL,
  `Status`               INT         NOT NULL,
  `CheckInAt`            DATETIME(6) NULL,
  `CheckOutAt`           DATETIME(6) NULL,
  `Note`                 LONGTEXT    NULL,
  `CreatedAt`            DATETIME(6) NOT NULL,
  `CreatedBy`            CHAR(36)    NULL,
  `UpdatedAt`            DATETIME(6) NULL,
  `UpdatedBy`            CHAR(36)    NULL,
  `IsDeleted`            TINYINT(1)  NOT NULL DEFAULT 0,
  `DeletedAt`            DATETIME(6) NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_teacher_attendance_session_teacher` (`LessonSessionId`, `TeacherId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Điểm / đánh giá
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS `Assessments` (
  `Id`              CHAR(36)    NOT NULL,
  `ClassId`         CHAR(36)    NOT NULL,
  `Title`           LONGTEXT    NOT NULL,
  `AssessmentDate`  DATE        NULL,
  `MaxScore`        DECIMAL(18,2) NULL,
  `Description`     LONGTEXT    NULL,
  `CreatedAt`       DATETIME(6) NOT NULL,
  `CreatedBy`       CHAR(36)    NULL,
  `UpdatedAt`       DATETIME(6) NULL,
  `UpdatedBy`       CHAR(36)    NULL,
  `IsDeleted`       TINYINT(1)  NOT NULL DEFAULT 0,
  `DeletedAt`       DATETIME(6) NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_assessments_class` (`ClassId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Grades` (
  `Id`            CHAR(36)       NOT NULL,
  `AssessmentId`  CHAR(36)       NOT NULL,
  `StudentId`     CHAR(36)       NOT NULL,
  `Score`         DECIMAL(18,2)  NULL,
  `Comment`       LONGTEXT       NULL,
  `GradedAt`      DATETIME(6)    NULL,
  `GradedBy`      CHAR(36)       NULL,
  `CreatedAt`     DATETIME(6)    NOT NULL,
  `CreatedBy`     CHAR(36)       NULL,
  `UpdatedAt`     DATETIME(6)    NULL,
  `UpdatedBy`     CHAR(36)       NULL,
  `IsDeleted`     TINYINT(1)     NOT NULL DEFAULT 0,
  `DeletedAt`     DATETIME(6)    NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_grades_assessment_student` (`AssessmentId`, `StudentId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Học phí
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS `student_tuition_months` (
  `Id`              CHAR(36)       NOT NULL,
  `StudentId`       CHAR(36)       NOT NULL,
  `EnrollmentId`    CHAR(36)       NOT NULL,
  `BillingYear`     INT            NOT NULL,
  `BillingMonth`    INT            NOT NULL,
  `ExpectedAmount`  DECIMAL(18,2)  NOT NULL,
  `AmountPaid`      DECIMAL(18,2)  NOT NULL DEFAULT 0,
  `Status`          INT            NOT NULL COMMENT '0=Unpaid, 1=Partial, 2=Paid',
  `Note`            LONGTEXT       NULL,
  `CreatedAt`       DATETIME(6)    NOT NULL,
  `CreatedBy`       CHAR(36)       NULL,
  `UpdatedAt`       DATETIME(6)    NULL,
  `UpdatedBy`       CHAR(36)       NULL,
  `IsDeleted`       TINYINT(1)     NOT NULL DEFAULT 0,
  `DeletedAt`       DATETIME(6)    NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_student_tuition_months_StudentId_BillingYear_BillingMonth` (`StudentId`, `BillingYear`, `BillingMonth`),
  KEY `idx_tuition_months_period` (`BillingYear`, `BillingMonth`, `Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `TuitionPayments` (
  `Id`                     CHAR(36)       NOT NULL,
  `StudentTuitionMonthId`  CHAR(36)       NOT NULL,
  `StudentId`              CHAR(36)       NOT NULL,
  `Amount`                 DECIMAL(18,2)  NOT NULL,
  `PaymentMethod`          INT            NOT NULL COMMENT '1=Cash, 2=BankTransfer',
  `PaidAt`                 DATETIME(6)    NOT NULL,
  `ReferenceNo`            LONGTEXT       NULL,
  `ReceivedBy`             CHAR(36)       NULL,
  `Note`                   LONGTEXT       NULL,
  `CreatedAt`              DATETIME(6)    NOT NULL,
  `CreatedBy`              CHAR(36)       NULL,
  `UpdatedAt`              DATETIME(6)    NULL,
  `UpdatedBy`              CHAR(36)       NULL,
  `IsDeleted`              TINYINT(1)     NOT NULL DEFAULT 0,
  `DeletedAt`              DATETIME(6)    NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_tuition_payments_student` (`StudentId`, `PaidAt`),
  KEY `idx_tuition_payments_month` (`StudentTuitionMonthId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Lương
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS `SalaryPeriods` (
  `Id`        CHAR(36)    NOT NULL,
  `Year`      INT         NOT NULL,
  `Month`     INT         NOT NULL,
  `Status`    INT         NOT NULL COMMENT '0=Open, 1=Closed',
  `ClosedAt`  DATETIME(6) NULL,
  `ClosedBy`  CHAR(36)    NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  `CreatedBy` CHAR(36)    NULL,
  `UpdatedAt` DATETIME(6) NULL,
  `UpdatedBy` CHAR(36)    NULL,
  `IsDeleted` TINYINT(1)  NOT NULL DEFAULT 0,
  `DeletedAt` DATETIME(6) NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_salary_period_year_month` (`Year`, `Month`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `LessonPayRecords` (
  `Id`                   CHAR(36)       NOT NULL,
  `SalaryPeriodId`       CHAR(36)       NOT NULL,
  `LessonSessionId`      CHAR(36)       NOT NULL,
  `LessonSessionStaffId` CHAR(36)       NOT NULL,
  `TeacherId`            CHAR(36)       NOT NULL,
  `ClassId`              CHAR(36)       NOT NULL,
  `StaffRole`            INT            NOT NULL,
  `TeachingMode`         INT            NOT NULL,
  `BaseLessonRate`       DECIMAL(18,2)  NOT NULL,
  `PayMultiplier`        DECIMAL(18,4)  NOT NULL,
  `PayAmount`            DECIMAL(18,2)  NOT NULL,
  `Status`               INT            NOT NULL COMMENT '0=Pending, 1=Confirmed, 2=Reversed',
  `CalculatedAt`         DATETIME(6)    NOT NULL,
  `Note`                 LONGTEXT       NULL,
  `CreatedAt`            DATETIME(6)    NOT NULL,
  `CreatedBy`            CHAR(36)       NULL,
  `UpdatedAt`            DATETIME(6)    NULL,
  `UpdatedBy`            CHAR(36)       NULL,
  `IsDeleted`            TINYINT(1)     NOT NULL DEFAULT 0,
  `DeletedAt`            DATETIME(6)    NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_lesson_pay_staff` (`LessonSessionStaffId`),
  KEY `idx_lesson_pay_period` (`SalaryPeriodId`, `TeacherId`),
  KEY `idx_lesson_pay_teacher` (`TeacherId`, `Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- EF Core migration history (chỉ khi dùng script thay cho dotnet ef database update)
-- Bỏ comment block dưới nếu cần đánh dấu migration đã apply:
-- -----------------------------------------------------------------------------
-- CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
--   `MigrationId`    VARCHAR(150) NOT NULL,
--   `ProductVersion` VARCHAR(32)  NOT NULL,
--   PRIMARY KEY (`MigrationId`)
-- ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
--
-- INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
-- VALUES ('20260601152143_InitialCreate', '8.0.11');

SET FOREIGN_KEY_CHECKS = 1;

-- =============================================================================
-- Gợi ý seed roles (tùy chọn — API DbSeeder cũng tự seed khi chạy lần đầu)
-- =============================================================================
-- INSERT INTO `roles` (`Id`, `Code`, `Name`, `CreatedAt`, `IsDeleted`) VALUES
--   (UUID(), 'Admin', 'Quản trị viên', UTC_TIMESTAMP(6), 0),
--   (UUID(), 'AcademicStaff', 'Giáo vụ', UTC_TIMESTAMP(6), 0),
--   (UUID(), 'Accountant', 'Kế toán', UTC_TIMESTAMP(6), 0),
--   (UUID(), 'Teacher', 'Giáo viên', UTC_TIMESTAMP(6), 0),
--   (UUID(), 'Receptionist', 'Lễ tân', UTC_TIMESTAMP(6), 0);
