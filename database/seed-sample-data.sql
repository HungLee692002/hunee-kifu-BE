-- =============================================================================
-- English Center — Dữ liệu mẫu (seed)
-- Cập nhật: 2026-06-01
--
-- Yêu cầu: Đã chạy database/create-tables.sql (hoặc EF migrate).
-- Database mặc định: english_center
--
-- Cách chạy:
--   mysql -u root -p english_center < database/seed-sample-data.sql
--
-- Lưu ý: Nếu API đã seed user `admin` (DbSeeder), xóa user đó trước hoặc
--         comment block mục 2 (Users) để tránh trùng username.
--
-- Tài khoản đăng nhập API (POST /api/v1/auth/tokens):
--   admin     / Admin@123   (Admin)
--   giaovu    / Admin@123   (AcademicStaff)
--   ketoan    / Admin@123   (Accountant)
--   gvnn1     / Admin@123   (Teacher — GVNN)
--   gvviet1   / Admin@123   (Teacher — GV Việt)
--
-- Mật khẩu tất cả user mẫu: Admin@123 (BCrypt)
-- =============================================================================

SET NAMES utf8mb4;
USE `english_center`;

SET @now = UTC_TIMESTAMP(6);
SET @pwd  = '$2a$11$J6xohwCn.OvDjxUEdgOR/.hmwHsmB/exD33T1sJZckx19OHflvMr6';

-- Xóa dữ liệu mẫu cũ (cùng bộ Id) để chạy lại script an toàn
DELETE FROM `LessonPayRecords`       WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `TuitionPayments`      WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `student_tuition_months` WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `Grades`               WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `Assessments`          WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `TeacherAttendances`   WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `StudentAttendances`   WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `LessonScheduleOverrides` WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `LessonSessionStaffs`  WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `lesson_sessions`      WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `WeeklyScheduleTemplates` WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `TeacherLessonRates`   WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `ClassAssignments`     WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `enrollments`          WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `Guardians`            WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `students`             WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `teachers`             WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `classes`              WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `courses`              WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `rooms`                WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `SalaryPeriods`        WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `refresh_tokens`       WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `user_roles`           WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `users`                WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';
DELETE FROM `roles`                WHERE `Id` LIKE 'a9999999-9999-4999-8999-%';

-- -----------------------------------------------------------------------------
-- 1. Roles
-- -----------------------------------------------------------------------------
INSERT INTO `roles` (`Id`, `Code`, `Name`, `Description`, `CreatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000001', 'Admin',          'Quản trị viên', NULL, @now, 0),
('a9999999-9999-4999-8999-000000000002', 'AcademicStaff',  'Giáo vụ',       NULL, @now, 0),
('a9999999-9999-4999-8999-000000000003', 'Accountant',     'Kế toán',       NULL, @now, 0),
('a9999999-9999-4999-8999-000000000004', 'Teacher',        'Giáo viên',     NULL, @now, 0),
('a9999999-9999-4999-8999-000000000005', 'Receptionist',   'Lễ tân',        NULL, @now, 0);

-- -----------------------------------------------------------------------------
-- 2. Users
-- -----------------------------------------------------------------------------
INSERT INTO `users` (`Id`, `Username`, `Email`, `PasswordHash`, `FullName`, `Status`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000101', 'admin',   'admin@englishcenter.local',   @pwd, 'Quản trị viên',     1, @now, @now, 0),
('a9999999-9999-4999-8999-000000000102', 'giaovu',  'giaovu@englishcenter.local',  @pwd, 'Trần Thị Giáo Vụ',  1, @now, @now, 0),
('a9999999-9999-4999-8999-000000000103', 'ketoan',  'ketoan@englishcenter.local',  @pwd, 'Lê Văn Kế Toán',    1, @now, @now, 0),
('a9999999-9999-4999-8999-000000000104', 'gvnn1',   'john.smith@englishcenter.local', @pwd, 'John Smith',     1, @now, @now, 0),
('a9999999-9999-4999-8999-000000000105', 'gvviet1', 'nguyen.van.b@englishcenter.local', @pwd, 'Nguyễn Văn B', 1, @now, @now, 0);

INSERT INTO `user_roles` (`Id`, `UserId`, `RoleId`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000201', 'a9999999-9999-4999-8999-000000000101', 'a9999999-9999-4999-8999-000000000001', @now, @now, 0),
('a9999999-9999-4999-8999-000000000202', 'a9999999-9999-4999-8999-000000000102', 'a9999999-9999-4999-8999-000000000002', @now, @now, 0),
('a9999999-9999-4999-8999-000000000203', 'a9999999-9999-4999-8999-000000000103', 'a9999999-9999-4999-8999-000000000003', @now, @now, 0),
('a9999999-9999-4999-8999-000000000204', 'a9999999-9999-4999-8999-000000000104', 'a9999999-9999-4999-8999-000000000004', @now, @now, 0),
('a9999999-9999-4999-8999-000000000205', 'a9999999-9999-4999-8999-000000000105', 'a9999999-9999-4999-8999-000000000004', @now, @now, 0);

-- -----------------------------------------------------------------------------
-- 3. Danh mục: phòng, khóa, lớp
-- -----------------------------------------------------------------------------
INSERT INTO `rooms` (`Id`, `Code`, `Name`, `Capacity`, `Floor`, `Note`, `IsActive`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000301', 'a9999999-9999-4999-8999-000000000301', 'Phòng 101', 20, 'Tầng 1', NULL, 1, @now, @now, 0),
('a9999999-9999-4999-8999-000000000302', 'a9999999-9999-4999-8999-000000000302', 'Phòng 202', 15, 'Tầng 2', 'Có máy chiếu', 1, @now, @now, 0);

INSERT INTO `courses` (`Id`, `Code`, `Name`, `Description`, `IsActive`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000401', 'a9999999-9999-4999-8999-000000000401', 'Tiếng Anh Thiếu nhi', 'Khóa 6–11 tuổi', 1, @now, @now, 0),
('a9999999-9999-4999-8999-000000000402', 'a9999999-9999-4999-8999-000000000402', 'Tiếng Anh Giao tiếp', 'Khóa người lớn', 1, @now, @now, 0);

INSERT INTO `classes` (`Id`, `Code`, `CourseId`, `Name`, `Status`, `GradingEnabled`, `DefaultMonthlyTuition`, `StartDate`, `EndDate`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000501', 'a9999999-9999-4999-8999-000000000501', 'a9999999-9999-4999-8999-000000000401', 'Starters A1 — Ca tối', 1, 1, 2000000.00, '2026-01-15', '2026-12-31', @now, @now, 0),
('a9999999-9999-4999-8999-000000000502', 'a9999999-9999-4999-8999-000000000502', 'a9999999-9999-4999-8999-000000000402', 'Communication B1', 1, 0, 2500000.00, '2026-03-01', NULL, @now, @now, 0);

-- -----------------------------------------------------------------------------
-- 4. Giáo viên & đơn giá buổi
-- -----------------------------------------------------------------------------
INSERT INTO `teachers` (`Id`, `Code`, `FullName`, `TeacherType`, `CurrentLessonRate`, `Phone`, `Email`, `Status`, `UserId`, `Note`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000601', 'a9999999-9999-4999-8999-000000000601', 'John Smith',   2, 400000.00, '0901000001', 'john.smith@englishcenter.local', 1, 'a9999999-9999-4999-8999-000000000104', 'GVNN', @now, @now, 0),
('a9999999-9999-4999-8999-000000000602', 'a9999999-9999-4999-8999-000000000602', 'Nguyễn Văn B', 1, 300000.00, '0901000002', 'nguyen.van.b@englishcenter.local', 1, 'a9999999-9999-4999-8999-000000000105', 'GV Việt — hỗ trợ lớp', @now, @now, 0),
('a9999999-9999-4999-8999-000000000603', 'a9999999-9999-4999-8999-000000000603', 'Trần Thị C',   1, 300000.00, '0901000003', NULL, 1, NULL, 'GV Việt dạy chính', @now, @now, 0);

INSERT INTO `TeacherLessonRates` (`Id`, `TeacherId`, `LessonRate`, `EffectiveFrom`, `EffectiveTo`, `IsActive`, `Note`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000611', 'a9999999-9999-4999-8999-000000000601', 400000.00, '2026-01-01', NULL, 1, 'Hợp đồng GVNN 2026', @now, @now, 0),
('a9999999-9999-4999-8999-000000000612', 'a9999999-9999-4999-8999-000000000602', 300000.00, '2026-01-01', NULL, 1, 'Hợp đồng GV Việt 2026', @now, @now, 0),
('a9999999-9999-4999-8999-000000000613', 'a9999999-9999-4999-8999-000000000603', 300000.00, '2026-01-01', NULL, 1, NULL, @now, @now, 0);

INSERT INTO `ClassAssignments` (`Id`, `ClassId`, `TeacherId`, `Role`, `AssignedFrom`, `AssignedTo`, `IsActive`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000621', 'a9999999-9999-4999-8999-000000000501', 'a9999999-9999-4999-8999-000000000602', 1, '2026-01-15', NULL, 1, @now, @now, 0),
('a9999999-9999-4999-8999-000000000622', 'a9999999-9999-4999-8999-000000000502', 'a9999999-9999-4999-8999-000000000603', 1, '2026-03-01', NULL, 1, @now, @now, 0);

-- -----------------------------------------------------------------------------
-- 5. Học sinh & ghi danh (1 HS chỉ 1 enrollment Active)
-- -----------------------------------------------------------------------------
INSERT INTO `students` (`Id`, `Code`, `FullName`, `DateOfBirth`, `Gender`, `Phone`, `Email`, `Address`, `Status`, `CurrentEnrollmentId`, `Note`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000701', 'a9999999-9999-4999-8999-000000000701', 'Nguyễn Văn An',  '2015-03-10', 1, '0912000001', NULL, 'Hà Nội', 1, 'a9999999-9999-4999-8999-000000000711', NULL, @now, @now, 0),
('a9999999-9999-4999-8999-000000000702', 'a9999999-9999-4999-8999-000000000702', 'Trần Thị Bình', '2014-08-22', 2, '0912000002', NULL, 'Hà Nội', 1, 'a9999999-9999-4999-8999-000000000712', NULL, @now, @now, 0),
('a9999999-9999-4999-8999-000000000703', 'a9999999-9999-4999-8999-000000000703', 'Lê Văn Cường',  '1998-11-05', 1, '0912000003', 'cuong.le@email.com', 'Hà Nội', 1, 'a9999999-9999-4999-8999-000000000713', NULL, @now, @now, 0);

INSERT INTO `Guardians` (`Id`, `StudentId`, `FullName`, `Relationship`, `Phone`, `Email`, `IsPrimary`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000705', 'a9999999-9999-4999-8999-000000000701', 'Nguyễn Văn Phúc', 'Bố', '0988000001', NULL, 1, @now, @now, 0),
('a9999999-9999-4999-8999-000000000706', 'a9999999-9999-4999-8999-000000000702', 'Trần Thị Mai',   'Mẹ', '0988000002', NULL, 1, @now, @now, 0);

INSERT INTO `enrollments` (`Id`, `StudentId`, `ClassId`, `Status`, `EnrolledAt`, `EndedAt`, `MonthlyTuitionAmount`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000711', 'a9999999-9999-4999-8999-000000000701', 'a9999999-9999-4999-8999-000000000501', 1, '2026-01-20', NULL, 2000000.00, @now, @now, 0),
('a9999999-9999-4999-8999-000000000712', 'a9999999-9999-4999-8999-000000000702', 'a9999999-9999-4999-8999-000000000501', 1, '2026-02-01', NULL, NULL, @now, @now, 0),
('a9999999-9999-4999-8999-000000000713', 'a9999999-9999-4999-8999-000000000703', 'a9999999-9999-4999-8999-000000000502', 1, '2026-03-05', NULL, 2500000.00, @now, @now, 0);

-- -----------------------------------------------------------------------------
-- 6. Lịch tuần & buổi học (Thứ 4, 18:00–19:30, ForeignLed: GVNN + GV Việt 70%)
-- -----------------------------------------------------------------------------
INSERT INTO `WeeklyScheduleTemplates` (`Id`, `ClassId`, `DayOfWeek`, `StartTime`, `EndTime`, `RoomId`, `TeachingMode`, `PrimaryTeacherId`, `LocalSupportTeacherId`, `EffectiveFrom`, `EffectiveTo`, `IsActive`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000801', 'a9999999-9999-4999-8999-000000000501', 3, '18:00:00', '19:30:00', 'a9999999-9999-4999-8999-000000000301', 2, 'a9999999-9999-4999-8999-000000000601', 'a9999999-9999-4999-8999-000000000602', '2026-01-15', NULL, 1, @now, @now, 0),
('a9999999-9999-4999-8999-000000000802', 'a9999999-9999-4999-8999-000000000502', 5, '19:00:00', '20:30:00', 'a9999999-9999-4999-8999-000000000302', 1, 'a9999999-9999-4999-8999-000000000603', NULL, '2026-03-01', NULL, 1, @now, @now, 0);

INSERT INTO `lesson_sessions` (`Id`, `ClassId`, `WeeklyScheduleTemplateId`, `SessionDate`, `Status`, `TeachingMode`, `PlannedStartTime`, `PlannedEndTime`, `PlannedRoomId`, `EffectiveStartTime`, `EffectiveEndTime`, `EffectiveRoomId`, `HasOverride`, `CompletedAt`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000811', 'a9999999-9999-4999-8999-000000000501', 'a9999999-9999-4999-8999-000000000801', '2026-06-04', 0, 2, '18:00:00', '19:30:00', 'a9999999-9999-4999-8999-000000000301', '18:00:00', '19:30:00', 'a9999999-9999-4999-8999-000000000301', 0, NULL, @now, @now, 0),
('a9999999-9999-4999-8999-000000000812', 'a9999999-9999-4999-8999-000000000501', 'a9999999-9999-4999-8999-000000000801', '2026-06-11', 1, 2, '18:00:00', '19:30:00', 'a9999999-9999-4999-8999-000000000301', '18:00:00', '19:30:00', 'a9999999-9999-4999-8999-000000000301', 0, @now, @now, @now, 0),
('a9999999-9999-4999-8999-000000000813', 'a9999999-9999-4999-8999-000000000502', 'a9999999-9999-4999-8999-000000000802', '2026-06-06', 0, 1, '19:00:00', '20:30:00', 'a9999999-9999-4999-8999-000000000302', '19:00:00', '20:30:00', 'a9999999-9999-4999-8999-000000000302', 0, NULL, @now, @now, 0);

INSERT INTO `LessonSessionStaffs` (`Id`, `LessonSessionId`, `TeacherId`, `StaffRole`, `PayMultiplier`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000821', 'a9999999-9999-4999-8999-000000000811', 'a9999999-9999-4999-8999-000000000601', 1, 1.0000, @now, @now, 0),
('a9999999-9999-4999-8999-000000000822', 'a9999999-9999-4999-8999-000000000811', 'a9999999-9999-4999-8999-000000000602', 2, 0.7000, @now, @now, 0),
('a9999999-9999-4999-8999-000000000823', 'a9999999-9999-4999-8999-000000000812', 'a9999999-9999-4999-8999-000000000601', 1, 1.0000, @now, @now, 0),
('a9999999-9999-4999-8999-000000000824', 'a9999999-9999-4999-8999-000000000812', 'a9999999-9999-4999-8999-000000000602', 2, 0.7000, @now, @now, 0),
('a9999999-9999-4999-8999-000000000825', 'a9999999-9999-4999-8999-000000000813', 'a9999999-9999-4999-8999-000000000603', 1, 1.0000, @now, @now, 0);

-- -----------------------------------------------------------------------------
-- 7. Học phí tháng 6/2026
-- -----------------------------------------------------------------------------
INSERT INTO `student_tuition_months` (`Id`, `StudentId`, `EnrollmentId`, `BillingYear`, `BillingMonth`, `ExpectedAmount`, `AmountPaid`, `Status`, `Note`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000901', 'a9999999-9999-4999-8999-000000000701', 'a9999999-9999-4999-8999-000000000711', 2026, 6, 2000000.00, 1000000.00, 1, 'Thu đợt 1', @now, @now, 0),
('a9999999-9999-4999-8999-000000000902', 'a9999999-9999-4999-8999-000000000702', 'a9999999-9999-4999-8999-000000000712', 2026, 6, 2000000.00, 0.00, 0, NULL, @now, @now, 0),
('a9999999-9999-4999-8999-000000000903', 'a9999999-9999-4999-8999-000000000703', 'a9999999-9999-4999-8999-000000000713', 2026, 6, 2500000.00, 2500000.00, 2, 'Đã thanh toán đủ', @now, @now, 0);

INSERT INTO `TuitionPayments` (`Id`, `StudentTuitionMonthId`, `StudentId`, `Amount`, `PaymentMethod`, `PaidAt`, `ReferenceNo`, `ReceivedBy`, `Note`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000911', 'a9999999-9999-4999-8999-000000000901', 'a9999999-9999-4999-8999-000000000701', 1000000.00, 1, '2026-06-05 14:00:00.000000', NULL, 'a9999999-9999-4999-8999-000000000103', 'Thu đợt 1', @now, @now, 0),
('a9999999-9999-4999-8999-000000000912', 'a9999999-9999-4999-8999-000000000903', 'a9999999-9999-4999-8999-000000000703', 2500000.00, 2, '2026-06-03 09:30:00.000000', 'CK-20260603-001', 'a9999999-9999-4999-8999-000000000103', NULL, @now, @now, 0);

-- -----------------------------------------------------------------------------
-- 8. Kỳ lương & bản ghi lương (buổi 11/06 đã Completed)
-- -----------------------------------------------------------------------------
INSERT INTO `SalaryPeriods` (`Id`, `Year`, `Month`, `Status`, `ClosedAt`, `ClosedBy`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000951', 2026, 6, 0, NULL, NULL, @now, @now, 0);

INSERT INTO `LessonPayRecords` (`Id`, `SalaryPeriodId`, `LessonSessionId`, `LessonSessionStaffId`, `TeacherId`, `ClassId`, `StaffRole`, `TeachingMode`, `BaseLessonRate`, `PayMultiplier`, `PayAmount`, `Status`, `CalculatedAt`, `Note`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000961', 'a9999999-9999-4999-8999-000000000951', 'a9999999-9999-4999-8999-000000000812', 'a9999999-9999-4999-8999-000000000823', 'a9999999-9999-4999-8999-000000000601', 'a9999999-9999-4999-8999-000000000501', 1, 2, 400000.00, 1.0000, 400000.00, 1, @now, NULL, @now, @now, 0),
('a9999999-9999-4999-8999-000000000962', 'a9999999-9999-4999-8999-000000000951', 'a9999999-9999-4999-8999-000000000812', 'a9999999-9999-4999-8999-000000000824', 'a9999999-9999-4999-8999-000000000602', 'a9999999-9999-4999-8999-000000000501', 2, 2, 300000.00, 0.7000, 210000.00, 1, @now, 'LocalSupport 70%', @now, @now, 0);

-- -----------------------------------------------------------------------------
-- 9. Điểm danh & bài kiểm tra mẫu
-- -----------------------------------------------------------------------------
INSERT INTO `StudentAttendances` (`Id`, `LessonSessionId`, `StudentId`, `EnrollmentId`, `Status`, `Note`, `RecordedAt`, `RecordedBy`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000971', 'a9999999-9999-4999-8999-000000000812', 'a9999999-9999-4999-8999-000000000701', 'a9999999-9999-4999-8999-000000000711', 1, NULL, @now, 'a9999999-9999-4999-8999-000000000102', @now, @now, 0),
('a9999999-9999-4999-8999-000000000972', 'a9999999-9999-4999-8999-000000000812', 'a9999999-9999-4999-8999-000000000702', 'a9999999-9999-4999-8999-000000000712', 1, NULL, @now, 'a9999999-9999-4999-8999-000000000102', @now, @now, 0);

INSERT INTO `TeacherAttendances` (`Id`, `LessonSessionId`, `TeacherId`, `LessonSessionStaffId`, `Status`, `CheckInAt`, `CheckOutAt`, `Note`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000981', 'a9999999-9999-4999-8999-000000000812', 'a9999999-9999-4999-8999-000000000601', 'a9999999-9999-4999-8999-000000000823', 1, @now, @now, NULL, @now, @now, 0),
('a9999999-9999-4999-8999-000000000982', 'a9999999-9999-4999-8999-000000000812', 'a9999999-9999-4999-8999-000000000602', 'a9999999-9999-4999-8999-000000000824', 1, @now, @now, NULL, @now, @now, 0);

INSERT INTO `Assessments` (`Id`, `ClassId`, `Title`, `AssessmentDate`, `MaxScore`, `Description`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000991', 'a9999999-9999-4999-8999-000000000501', 'Kiểm tra giữa kỳ — Speaking', '2026-05-20', 10.00, 'Đánh giá nói', @now, @now, 0);

INSERT INTO `Grades` (`Id`, `AssessmentId`, `StudentId`, `Score`, `Comment`, `GradedAt`, `GradedBy`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) VALUES
('a9999999-9999-4999-8999-000000000992', 'a9999999-9999-4999-8999-000000000991', 'a9999999-9999-4999-8999-000000000701', 8.50, 'Good participation', @now, 'a9999999-9999-4999-8999-000000000102', @now, @now, 0),
('a9999999-9999-4999-8999-000000000993', 'a9999999-9999-4999-8999-000000000991', 'a9999999-9999-4999-8999-000000000702', 7.00, 'Needs more practice', @now, 'a9999999-9999-4999-8999-000000000102', @now, @now, 0);

-- =============================================================================
-- Tóm tắt Id tham chiếu nhanh
-- =============================================================================
-- Lớp Starters A1: a9999999-9999-4999-8999-000000000501
-- GVNN John:       a9999999-9999-4999-8999-000000000601
-- GV Việt B:       a9999999-9999-4999-8999-000000000602
-- HS An:           a9999999-9999-4999-8999-000000000701
-- Buổi đã học:     a9999999-9999-4999-8999-000000000812 (2026-06-11)
