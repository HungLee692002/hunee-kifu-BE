# English Center Backend

API quản lý trung tâm tiếng Anh — **.NET 8**, **Clean Architecture**, **MySQL**, **JWT**.

## Cấu trúc

```
src/
  EnglishCenter.Domain/          # Entities, enums, business exceptions
  EnglishCenter.Application/     # DTOs, service interfaces
  EnglishCenter.Infrastructure/  # EF Core, services, jobs
  EnglishCenter.Api/             # REST API (entry point)
docs/                            # ARCHITECTURE.md, DATABASE.md, api-overview.md
```

## Yêu cầu

- .NET 8 SDK
- MySQL 8

## Cấu hình

**Không commit secret.** Copy file mẫu rồi điền giá trị máy dev:

```powershell
cd src/EnglishCenter.Api
copy appsettings.Development.local.json.example appsettings.Development.local.json
```

Sửa `appsettings.Development.local.json` (file này nằm trong `.gitignore`):

```json
"ConnectionStrings": {
  "Default": "Server=localhost;Port=3306;Database=english_center;User=root;Password=YOUR_PASSWORD;"
},
"Jwt": {
  "Secret": "YOUR_DEV_JWT_SECRET_MIN_32_CHARS"
}
```

**Production (Railway):** set biến môi trường `ConnectionStrings__Default`, `Jwt__Secret`, … — không dùng file local.

## Chạy lần đầu

```powershell
cd "d:\Personal Project\EnglishCenter.Backend"

# Tạo migration (cần MySQL đang chạy)
dotnet ef migrations add InitialCreate `
  -p src/EnglishCenter.Infrastructure `
  -s src/EnglishCenter.Api `
  -o Persistence/Migrations

# Chạy API (tự migrate + seed admin)
dotnet run --project src/EnglishCenter.Api
```

- Swagger: https://localhost:7155/swagger (hoặc port trong `launchSettings.json`)
- Health: `GET /health`

## Tài khoản mặc định (seed)

| Username | Password   | Role  |
|----------|------------|-------|
| admin    | Admin@123  | Admin |

## Đăng nhập (REST)

```http
POST /api/v1/auth/tokens
Content-Type: application/json

{
  "grantType": "password",
  "username": "admin",
  "password": "Admin@123"
}
```

Dùng `accessToken` trong header: `Authorization: Bearer {token}`

## API chính

Xem [docs/api-overview.md](docs/api-overview.md).

| Nhóm | Path |
|------|------|
| Auth | `POST/DELETE /api/v1/auth/tokens` |
| Rooms, Courses, Classes | CRUD |
| Students, Teachers | CRUD + nested resources |
| Enrollments | `PATCH /api/v1/enrollments/{id}` |
| Tuition | `POST /api/v1/students/{id}/tuition-payments` |
| Dashboard | `GET /api/v1/dashboard/summary` |

Paging: `?page=1&pageSize=20&sortBy=updatedAt&sortDir=desc` (pageSize: 10, 20, 50, 100)
