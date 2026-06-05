# API Overview — English Center Backend

> **Version:** v1  
> **Base URL:** `https://{host}/api/v1`  
> **Cập nhật:** 2026-06-01  
> **Tham chiếu:** [ARCHITECTURE.md](./ARCHITECTURE.md), [DATABASE.md](./DATABASE.md)  
> **Chuẩn:** **RESTful API** (resource-oriented, đúng HTTP verb & status code)

---

## 1. Quy ước chung

### 1.0 RESTful — bắt buộc khi thiết kế API

| Nguyên tắc | Áp dụng |
|------------|---------|
| **Danh từ, số nhiều** | `/students`, `/lesson-sessions` — không dùng `/getStudents`, `/createStudent` |
| **Không động từ trên URL** | Không `/login`, `/close`, `/end` — dùng **POST collection** hoặc **PATCH resource** |
| **HTTP method đúng nghĩa** | `GET` đọc, `POST` tạo, `PUT` thay thế toàn bộ, `PATCH` sửa một phần, `DELETE` xóa |
| **Quan hệ cha–con** | `/classes/{classId}/enrollments`, `/students/{studentId}/tuition-payments` |
| **Lọc / sort / page** | Query string: `?page=1&pageSize=20&status=Active` |
| **Idempotent** | `PUT`, `DELETE` idempotent; `POST` không idempotent |
| **Response code** | `201` + `Location` khi tạo; `204` khi xóa không body; `404` không tồn tại |

**Ví dụ thay thế pattern không REST:**

| Tránh | Dùng |
|-------|------|
| `POST /enrollments/{id}/end` | `PATCH /enrollments/{id}` body `{ "status": "Ended" }` |
| `POST /salary/periods/2026/6/close` | `PATCH /salary-periods/{id}` body `{ "status": "Closed" }` |
| `POST /lesson-sessions/{id}/override` | `PUT /lesson-sessions/{id}/schedule-override` |
| `POST /auth/login` | `POST /auth/tokens` (credentials trong body) |

**Đường dẫn:** `kebab-case`, chữ thường.

---

### 1.1 Định dạng

| Mục | Giá trị |
|-----|---------|
| Content-Type | `application/json; charset=utf-8` |
| Ngôn ngữ lỗi | Tiếng Việt (`detail`, `title`) |
| ID | **GUID** string (`550e8400-e29b-41d4-a716-446655440000`) |
| Mã HS / GV / lớp (`code`) | **Server tự sinh** = `id.ToString()` khi tạo; **không** nhận từ client |
| Thời gian | ISO 8601 UTC (`2026-06-01T10:30:00.000Z`) |
| Tiền | `number` (decimal), đơn vị VND |

### 1.2 Xác thực

Mọi endpoint (trừ Auth) yêu cầu header:

```http
Authorization: Bearer {access_token}
```

| Token | TTL |
|-------|-----|
| Access | **30 phút** |
| Refresh | **1 ngày** (opaque, lưu DB) |

### 1.3 Phân trang & sắp xếp (list)

Query áp dụng cho mọi `GET` trả danh sách:

| Param | Mặc định | Ràng buộc |
|-------|----------|-----------|
| `page` | `1` | integer ≥ 1 |
| `pageSize` | `20` | **10 \| 20 \| 50 \| 100** |
| `sortBy` | `updatedAt` | Whitelist theo từng resource |
| `sortDir` | `desc` | `asc` \| `desc` |

**Response:**

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8
}
```

### 1.4 Lỗi (RFC 7807 Problem Details)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Dữ liệu không hợp lệ",
  "status": 400,
  "detail": "pageSize chỉ được 10, 20, 50 hoặc 100.",
  "traceId": "00-abc..."
}
```

| HTTP | Ý nghĩa |
|------|---------|
| 400 | Validation |
| 401 | Chưa đăng nhập / token hết hạn |
| 403 | Không đủ quyền |
| 404 | Không tìm thấy |
| 409 | Xung đột nghiệp vụ (vd. HS đã có lớp Active) |
| 500 | Lỗi server |

### 1.5 HTTP method & status (REST)

| Method | Khi nào | Success |
|--------|---------|---------|
| `GET` | Lấy 1 hoặc danh sách | `200` |
| `POST` | Tạo resource mới | `201` + header `Location: .../{id}` |
| `PUT` | Thay thế toàn bộ resource / collection con | `200` hoặc `204` |
| `PATCH` | Cập nhật một phần (status, vài field) | `200` |
| `DELETE` | Xóa / thu hồi | `204` |

### 1.6 Audit fields (response)

Các resource nghiệp vụ có thể trả:

```json
{
  "createdAt": "2026-06-01T08:00:00.000Z",
  "createdBy": "guid-or-null",
  "updatedAt": "2026-06-01T09:00:00.000Z",
  "updatedBy": "guid-or-null"
}
```

---

## 2. Enum (API dùng string)

| Tên | Giá trị |
|-----|---------|
| `StudentStatus` | `Inactive`, `Active`, `Graduated` |
| `TeacherType` | `Local`, `Foreign` |
| `TeacherStatus` | `Inactive`, `Active` |
| `ClassStatus` | `Draft`, `Open`, `Closed` |
| `EnrollmentStatus` | `Ended`, `Active` |
| `TeachingMode` | `LocalLed`, `ForeignLed` |
| `SessionStaffRole` | `PrimaryInstructor`, `LocalSupport` |
| `LessonSessionStatus` | `Scheduled`, `Completed`, `Cancelled` |
| `AttendanceStatus` | `Absent`, `Present`, `Late`, `Excused` |
| `TuitionMonthStatus` | `Unpaid`, `Partial`, `Paid` |
| `PaymentMethod` | `Cash`, `BankTransfer` |
| `SalaryPeriodStatus` | `Open`, `Closed` |
| `UserRole` | `Admin`, `AcademicStaff`, `Accountant`, `Teacher`, `Receptionist` |

---

## 3. Auth — Resource `auth/tokens`

Đăng nhập / refresh / logout qua **cùng resource token**, không dùng URL động từ.

### POST `/auth/tokens`

**Roles:** Public  
**Mục đích:** Tạo cặp token (đăng nhập hoặc làm mới).

**Đăng nhập** (`grantType: password`):

```json
{
  "grantType": "password",
  "username": "admin",
  "password": "********"
}
```

**Refresh** (`grantType: refresh_token`):

```json
{
  "grantType": "refresh_token",
  "refreshToken": "opaque-token-string"
}
```

**Response `201 Created`:**

```json
{
  "accessToken": "eyJhbG...",
  "expiresIn": 1800,
  "refreshToken": "opaque-token-string",
  "tokenType": "Bearer",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "admin",
    "fullName": "Quản trị viên",
    "roles": ["Admin"]
  }
}
```

Header: `Location: /api/v1/auth/tokens` (optional).

---

### DELETE `/auth/tokens`

**Roles:** Authenticated (hoặc Public + body refresh)  
**Mục đích:** Thu hồi refresh token (logout).

**Request body:**

```json
{
  "refreshToken": "opaque-token-string"
}
```

**Response:** `204 No Content`

---

## 4. Rooms — Danh mục phòng

| Method | Path | Roles |
|--------|------|-------|
| GET | `/rooms` | Admin, AcademicStaff |
| GET | `/rooms/{id}` | Admin, AcademicStaff |
| POST | `/rooms` | Admin |
| PUT | `/rooms/{id}` | Admin |
| PATCH | `/rooms/{id}` | Admin |
| DELETE | `/rooms/{id}` | Admin |

`PUT` = thay thế toàn bộ; `PATCH` = đổi vài field (vd. `isActive`).

**POST body** (`code` phòng: admin nhập — khác HS/GV/lớp tự sinh GUID):

```json
{
  "code": "P101",
  "name": "Phòng 101",
  "capacity": 20,
  "floor": "1",
  "note": null,
  "isActive": true
}
```

**Room response:**

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "code": "P101",
  "name": "Phòng 101",
  "capacity": 20,
  "floor": "1",
  "note": null,
  "isActive": true,
  "updatedAt": "2026-06-01T09:00:00.000Z"
}
```

---

## 5. Courses

| Method | Path | Roles |
|--------|------|-------|
| GET | `/courses` | Admin, AcademicStaff |
| GET | `/courses/{id}` | Admin, AcademicStaff |
| POST | `/courses` | Admin |
| PUT | `/courses/{id}` | Admin |
| PATCH | `/courses/{id}` | Admin |

**POST:**

```json
{
  "code": "ENG-A1",
  "name": "Tiếng Anh giao tiếp A1",
  "description": null,
  "isActive": true
}
```

---

## 6. Classes — Lớp học

| Method | Path | Roles |
|--------|------|-------|
| GET | `/classes` | Admin, AcademicStaff, Teacher* |
| GET | `/classes/{id}` | Admin, AcademicStaff, Teacher* |
| POST | `/classes` | Admin, AcademicStaff |
| PUT | `/classes/{id}` | Admin, AcademicStaff |
| PATCH | `/classes/{id}` | Admin, AcademicStaff |

\* Teacher: chỉ lớp được phân công.

**POST** — `code` **không gửi** (server sinh GUID):

```json
{
  "courseId": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Lớp A1 - Tối 2-4-6",
  "status": "Open",
  "gradingEnabled": false,
  "defaultMonthlyTuition": 2000000,
  "startDate": "2026-06-01",
  "endDate": null
}
```

**Response** (có `code` tự sinh):

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "code": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "courseId": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Lớp A1 - Tối 2-4-6",
  "status": "Open",
  "gradingEnabled": false,
  "defaultMonthlyTuition": 2000000,
  "startDate": "2026-06-01",
  "endDate": null
}
```

---

## 7. Students — Học sinh

| Method | Path | Roles |
|--------|------|-------|
| GET | `/students` | Admin, AcademicStaff, Receptionist |
| GET | `/students/{id}` | Admin, AcademicStaff, Receptionist |
| POST | `/students` | Admin, Receptionist |
| PUT | `/students/{id}` | Admin, Receptionist |
| GET | `/students/{id}/guardians` | Admin, AcademicStaff, Receptionist |
| POST | `/students/{id}/guardians` | Admin, Receptionist |

**POST student** — không gửi `code`:

```json
{
  "fullName": "Nguyễn Văn A",
  "dateOfBirth": "2015-03-10",
  "gender": 1,
  "phone": "0901234567",
  "email": null,
  "address": "Hà Nội",
  "status": "Active",
  "note": null
}
```

---

## 8. Enrollments — Ghi danh (1 HS = 1 lớp Active)

| Method | Path | Roles |
|--------|------|-------|
| GET | `/enrollments/{id}` | Admin, AcademicStaff, Receptionist |
| GET | `/students/{studentId}/enrollments` | Admin, AcademicStaff, Receptionist |
| GET | `/classes/{classId}/enrollments` | Admin, AcademicStaff |
| POST | `/classes/{classId}/enrollments` | Admin, AcademicStaff, Receptionist |
| PATCH | `/enrollments/{id}` | Admin, AcademicStaff |

**GET** `/students/{studentId}/enrollments?status=Active` — lớp hiện tại.

**PATCH** kết thúc ghi danh (thay cho `/end`):

```json
{
  "status": "Ended",
  "endedAt": "2026-06-15"
}
```

**POST** tạo enrollment — `/classes/{classId}/enrollments`:

```json
{
  "studentId": "550e8400-e29b-41d4-a716-446655440000",
  "enrolledAt": "2026-06-01",
  "monthlyTuitionAmount": 2000000
}
```

| Quy tắc | Hành vi API |
|---------|-------------|
| 1 HS = 1 lớp Active | Nếu HS đã có enrollment Active → **409** |
| Chuyển lớp | Tự `end` enrollment cũ + tạo mới (hoặc 2 bước API) |
| `monthlyTuitionAmount` | Dùng làm **`expected_amount`** khi ghi học phí tháng |

**Enrollment response:**

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "studentId": "550e8400-e29b-41d4-a716-446655440000",
  "classId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "status": "Active",
  "enrolledAt": "2026-06-01",
  "endedAt": null,
  "monthlyTuitionAmount": 2000000
}
```

---

## 9. Teachers — Giáo viên

| Method | Path | Roles |
|--------|------|-------|
| GET | `/teachers` | Admin, AcademicStaff |
| GET | `/teachers/{id}` | Admin, AcademicStaff |
| POST | `/teachers` | Admin |
| PUT | `/teachers/{id}` | Admin |
| GET | `/teachers/{id}/lesson-rates` | Admin, Accountant |
| POST | `/teachers/{id}/lesson-rates` | Admin, Accountant |

**POST teacher** — không gửi `code`:

```json
{
  "fullName": "John Smith",
  "teacherType": "Foreign",
  "phone": null,
  "email": "john@example.com",
  "status": "Active",
  "userId": null,
  "note": null
}
```

**POST lesson-rate** (hợp đồng 300k/buổi):

```json
{
  "lessonRate": 300000,
  "effectiveFrom": "2026-06-01",
  "effectiveTo": null,
  "note": "Hợp đồng 2026"
}
```

→ Cập nhật `teachers.currentLessonRate`.

---

## 10. Teacher assignments — GV phụ trách lớp

| Method | Path | Roles |
|--------|------|-------|
| GET | `/classes/{classId}/teacher-assignments` | Admin, AcademicStaff |
| POST | `/classes/{classId}/teacher-assignments` | Admin, AcademicStaff |
| GET | `/teacher-assignments/{id}` | Admin, AcademicStaff |
| PUT | `/teacher-assignments/{id}` | Admin, AcademicStaff |
| PATCH | `/teacher-assignments/{id}` | Admin, AcademicStaff |

**POST:**

```json
{
  "teacherId": "550e8400-e29b-41d4-a716-446655440000",
  "role": "Main",
  "assignedFrom": "2026-06-01",
  "assignedTo": null
}
```

---

## 11. Schedule — Lịch tuần & buổi học

### 11.1 Weekly templates

| Method | Path | Roles |
|--------|------|-------|
| GET | `/classes/{classId}/schedule-templates` | Admin, AcademicStaff, Teacher* |
| POST | `/classes/{classId}/schedule-templates` | Admin, AcademicStaff |
| GET | `/schedule-templates/{id}` | Admin, AcademicStaff, Teacher* |
| PUT | `/schedule-templates/{id}` | Admin, AcademicStaff |
| PATCH | `/schedule-templates/{id}` | Admin, AcademicStaff |

**POST** (1 lớp, nhiều ca):

```json
{
  "dayOfWeek": 3,
  "startTime": "18:00:00",
  "endTime": "19:30:00",
  "roomId": "550e8400-e29b-41d4-a716-446655440000",
  "teachingMode": "ForeignLed",
  "primaryTeacherId": "guid-gvnn",
  "localSupportTeacherId": "guid-gv-viet",
  "effectiveFrom": "2026-06-01",
  "effectiveTo": null,
  "isActive": true
}
```

| `teachingMode` | `primaryTeacherId` | `localSupportTeacherId` |
|----------------|--------------------|-------------------------|
| `LocalLed` | GV Việt dạy chính | `null` |
| `ForeignLed` | GVNN | GV Việt phụ trách lớp (bắt buộc) |

---

### 11.2 Lesson sessions

| Method | Path | Roles |
|--------|------|-------|
| GET | `/lesson-sessions` | Admin, AcademicStaff, Teacher* |
| GET | `/lesson-sessions/{id}` | Admin, AcademicStaff, Teacher* |

**Query gợi ý:** `classId`, `teacherId`, `fromDate`, `toDate`, `status`

> Buổi học do **job** sinh 30 ngày tới; không expose POST tạo tay (trừ admin đặc biệt sau).

**Lesson session response:**

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "classId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "sessionDate": "2026-06-04",
  "teachingMode": "ForeignLed",
  "status": "Scheduled",
  "effectiveStartTime": "18:00:00",
  "effectiveEndTime": "19:30:00",
  "effectiveRoomId": "...",
  "hasOverride": false,
  "staff": [
    {
      "teacherId": "guid-gvnn",
      "staffRole": "PrimaryInstructor",
      "payMultiplier": 1.0
    },
    {
      "teacherId": "guid-gv-viet",
      "staffRole": "LocalSupport",
      "payMultiplier": 0.7
    }
  ]
}
```

---

### 11.3 Schedule override (singleton sub-resource)

| Method | Path | Roles |
|--------|------|-------|
| GET | `/lesson-sessions/{id}/schedule-override` | Admin, AcademicStaff, Teacher* |
| PUT | `/lesson-sessions/{id}/schedule-override` | Admin, AcademicStaff |
| DELETE | `/lesson-sessions/{id}/schedule-override` | Admin, AcademicStaff |

- `PUT` — tạo hoặc thay thế override (sự cố).  
- `DELETE` — gỡ override; buổi học trở lại theo template (tuần sau vẫn generate từ template).

**PUT body:**

```json
{
  "overridePrimaryTeacherId": null,
  "overrideSupportTeacherId": "guid-thay",
  "overrideRoomId": null,
  "overrideStartTime": null,
  "overrideEndTime": null,
  "isCancelled": false,
  "reason": "GV Việt nghỉ, GV dự phòng"
}
```

---

## 12. Attendance — Điểm danh

### 12.1 Học sinh

| Method | Path | Roles |
|--------|------|-------|
| GET | `/lesson-sessions/{id}/student-attendances` | Admin, AcademicStaff, Teacher* |
| PUT | `/lesson-sessions/{id}/student-attendances` | Admin, AcademicStaff, Teacher* |

**PUT** (bulk):

```json
{
  "items": [
    {
      "studentId": "550e8400-e29b-41d4-a716-446655440000",
      "status": "Present",
      "note": null
    }
  ]
}
```

### 12.2 Giáo viên

| Method | Path | Roles |
|--------|------|-------|
| PUT | `/lesson-sessions/{id}/teacher-attendances` | Admin, AcademicStaff, Teacher* |

---

## 13. Tuition — Học phí (nhập tay)

Resource: **`student-tuition-months`**, **`tuition-payments`** (collection).

| Method | Path | Roles |
|--------|------|-------|
| GET | `/student-tuition-months` | Admin, Accountant, Receptionist |
| GET | `/student-tuition-months/{id}` | Admin, Accountant, Receptionist |
| GET | `/students/{studentId}/student-tuition-months` | Admin, Accountant, Receptionist |
| GET | `/tuition-payments` | Admin, Accountant |
| GET | `/tuition-payments/{id}` | Admin, Accountant |
| POST | `/students/{studentId}/tuition-payments` | Admin, Accountant, Receptionist |

**GET `/student-tuition-months` query:**

| Param | Mô tả |
|-------|--------|
| `year` | VD: 2026 |
| `month` | 1–12 |
| `status` | `Unpaid` \| `Partial` \| `Paid` |

**Không có** API sinh hóa đơn tự động.

**POST `/students/{studentId}/tuition-payments`** — `201 Created` — admin nhập khi thu tiền:

```json
{
  "billingYear": 2026,
  "billingMonth": 6,
  "amount": 1000000,
  "paymentMethod": "Cash",
  "paidAt": "2026-06-05T14:00:00.000Z",
  "referenceNo": null,
  "note": "Thu đợt 1"
}
```

**Logic server:**

1. Lấy enrollment **Active** của HS.  
2. `expectedAmount` = `enrollment.monthlyTuitionAmount` ?? `class.defaultMonthlyTuition`.  
3. Upsert `student_tuition_months` (lazy).  
4. Insert `tuition_payments`; cộng `amountPaid`; tính `status`.

**Tuition month response:**

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "studentId": "550e8400-e29b-41d4-a716-446655440001",
  "enrollmentId": "550e8400-e29b-41d4-a716-446655440002",
  "billingYear": 2026,
  "billingMonth": 6,
  "expectedAmount": 2000000,
  "amountPaid": 1000000,
  "status": "Partial",
  "student": {
    "id": "...",
    "code": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "fullName": "Nguyễn Văn A"
  }
}
```

---

## 14. Salary — Lương theo buổi

| Method | Path | Roles |
|--------|------|-------|
| GET | `/salary-periods` | Admin, Accountant |
| GET | `/salary-periods/{id}` | Admin, Accountant |
| PATCH | `/salary-periods/{id}` | Admin, Accountant |
| GET | `/salary-periods/{id}/lesson-pay-records` | Admin, Accountant |
| GET | `/teachers/{teacherId}/lesson-pay-records` | Admin, Accountant, Teacher* |

**PATCH** chốt kỳ lương (thay URL `/close`):

```json
{
  "status": "Closed"
}
```

Chỉ chấp nhận khi `status` hiện tại là `Open` → `409` nếu đã đóng.

**GET `/salary-periods` query:** `?year=2026&month=6`

**Tính lương:** sau buổi `Completed` + điểm danh GV → `payAmount = teacher.lessonRate × payMultiplier`.

| Vai trò | `payMultiplier` |
|---------|-----------------|
| Primary (mọi ca) | 1.0 |
| LocalSupport + ForeignLed | 0.7 |

**Lesson pay record:**

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "lessonSessionId": "...",
  "teacherId": "...",
  "staffRole": "LocalSupport",
  "teachingMode": "ForeignLed",
  "baseLessonRate": 300000,
  "payMultiplier": 0.7,
  "payAmount": 210000,
  "status": "Confirmed"
}
```

---

## 15. Assessments & Grades (tùy chọn)

| Method | Path | Roles |
|--------|------|-------|
| GET | `/classes/{classId}/assessments` | Admin, AcademicStaff, Teacher* |
| POST | `/classes/{classId}/assessments` | Admin, AcademicStaff |
| GET | `/assessments/{id}/grades` | Admin, AcademicStaff, Teacher* |
| PUT | `/assessments/{id}/grades` | Admin, AcademicStaff, Teacher* |

`PUT` = thay thế toàn bộ điểm của bài kiểm tra (collection). Chỉ khi `class.gradingEnabled = true`.

**Grade item:**

```json
{
  "studentId": "550e8400-e29b-41d4-a716-446655440000",
  "score": 8.5,
  "comment": "Good participation"
}
```

`score` có thể `null` (chỉ nhận xét).

---

## 16. Dashboard

| Method | Path | Roles |
|--------|------|-------|
| GET | `/dashboard/summary` | Admin, Accountant, AcademicStaff |

**Query:**

| Param | Mặc định |
|-------|----------|
| `year` | Năm hiện tại |
| `month` | Tháng hiện tại |

**Response 200:**

```json
{
  "period": { "year": 2026, "month": 6 },
  "previousPeriod": { "year": 2026, "month": 5 },
  "students": {
    "activeCount": 120,
    "activeCountPrevious": 115,
    "newEnrollmentsThisMonth": 8
  },
  "attendance": {
    "ratePercent": 92.5,
    "ratePercentPrevious": 90.1
  },
  "tuition": {
    "expectedTotal": 240000000,
    "paidTotal": 180000000,
    "outstandingTotal": 60000000,
    "expectedTotalPrevious": 230000000,
    "paidTotalPrevious": 220000000
  },
  "teaching": {
    "completedSessionsCount": 180,
    "completedSessionsCountPrevious": 175
  },
  "salary": {
    "estimatedPayTotal": 45000000,
    "estimatedPayTotalPrevious": 43000000
  }
}
```

---

## 17. Health

| Method | Path | Auth |
|--------|------|------|
| GET | `/health` | Public |
| GET | `/health/ready` | Public (check DB) |

---

## 18. Ma trận endpoint (tóm tắt — REST)

| Module | Resources |
|--------|-----------|
| Auth | `POST/DELETE /auth/tokens` |
| Rooms | `/rooms` |
| Courses | `/courses` |
| Classes | `/classes`, `.../enrollments`, `.../schedule-templates`, `.../teacher-assignments` |
| Students | `/students`, `.../guardians`, `.../enrollments`, `.../tuition-payments` |
| Teachers | `/teachers`, `.../lesson-rates`, `.../lesson-pay-records` |
| Lessons | `/lesson-sessions`, `.../schedule-override`, `.../student-attendances` |
| Tuition | `/student-tuition-months`, `/tuition-payments` |
| Salary | `/salary-periods`, `.../lesson-pay-records` |
| Grades | `/assessments`, `.../grades` |
| Dashboard | `GET /dashboard/summary` (read-only aggregate) |

---

## 19. Thứ tự implement gợi ý (API)

1. REST base: paging, ProblemDetails, `201`+`Location`  
2. `POST/DELETE /auth/tokens`  
3. Rooms, Courses, Classes  
4. Students, Enrollments (`PATCH` status)  
5. Teachers, teacher-assignments, lesson-rates  
6. Schedule-templates, lesson-sessions, schedule-override (`PUT`/`DELETE`)  
7. Attendances (`PUT` collections)  
8. `POST .../tuition-payments`  
9. `PATCH /salary-periods/{id}`  
10. Dashboard, Grades (optional)  

---

*Khi implement, đồng bộ contract này với Swagger (`Swashbuckle`) và DTO trong `EnglishCenter.Application`.*
