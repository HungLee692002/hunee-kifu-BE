# Hướng dẫn deploy Backend (.NET) lên Railway

Tài liệu mẫu tái sử dụng cho dự án **ASP.NET Core + MySQL** (hoặc tương tự).  
Áp dụng cho English Center Backend; khi deploy dự án khác, thay `{TênProject}`, `{Repo}`, `{ApiProject}` cho phù hợp.

---

## 1. Nguyên tắc bảo mật (làm trước khi push Git)

| Môi trường | Lưu secret ở đâu |
|------------|------------------|
| **Máy dev** | `appsettings.Development.local.json` (gitignore) hoặc `dotnet user-secrets` |
| **Production (Railway)** | **Variables** trên dashboard Railway |
| **Git** | Chỉ placeholder / cấu hình không nhạy cảm |

### Cấu trúc file config khuyến nghị

```
src/{ApiProject}/
  appsettings.json                      ← commit: không secret
  appsettings.Development.json          ← commit: logging, không secret
  appsettings.Development.local.json    ← KHÔNG commit (gitignore)
  appsettings.Development.local.json.example  ← commit: mẫu cho team
```

### ASP.NET Core đọc biến môi trường

Dùng `__` thay cho `:` trong JSON:

| JSON | Biến môi trường Railway |
|------|-------------------------|
| `ConnectionStrings:Default` | `ConnectionStrings__Default` |
| `Jwt:Secret` | `Jwt__Secret` |
| `Jwt:Issuer` | `Jwt__Issuer` |

### Load file local (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.local.json",
    optional: true,
    reloadOnChange: true);
```

### .gitignore (Backend .NET)

```gitignore
[Bb]in/
[Oo]bj/
**/appsettings.*.local.json
**/secrets.json
.env
.env.*
!.env.example
```

### Checklist trước khi push

- [ ] `appsettings.json` không chứa password / JWT thật
- [ ] File `.local.json` không nằm trong `git status`
- [ ] Secret từng lộ trên Git → **rotate** (đổi password DB, JWT mới)

---

## 2. Đưa code lên GitHub

### Repo chỉ Backend

```powershell
cd "d:\path\to\{BackendFolder}"

git init
git add .
git status   # kiểm tra không có bin/, obj/, *.local.json

git commit -m "Initial commit: {TênProject} API"
git branch -M main
git remote add origin https://github.com/{username}/{repo}.git
git push -u origin main
```

### Repo monorepo (BE + FE)

```powershell
cd "d:\path\to\{RootFolder}"
git init
git add {BackendFolder} {FrontendFolder}
git commit -m "Initial commit"
git remote add origin https://github.com/{username}/{repo}.git
git push -u origin main
```

Trên Railway, set **Root Directory** = `{BackendFolder}` (ví dụ `EnglishCenter.Backend`).

---

## 3. Deploy lên Railway — giai đoạn 1 (chưa DB / JWT)

Mục tiêu: link repo, build thành công, có public URL. Service có thể **crash** cho đến khi thêm connection string — bình thường.

### 3.1 Tạo project

1. [railway.app](https://railway.app) → **New Project**
2. **Deploy from GitHub repo** → chọn repo
3. Railway tạo service API

### 3.2 Build & Start

> **Lỗi `MSB1003: Specify a project or solution file`**  
> Railway đang build ở thư mục **không có** `.sln`/`.csproj`.  
> - Repo GitHub **chỉ Backend** (root có `EnglishCenter.Backend.sln`) → **Root Directory để trống** (`/`).  
> - **Không** set Root Directory = `EnglishCenter.Backend` (trừ khi repo monorepo có folder con đó).  
> - Commit file `railway.toml` ở root repo (đã có trong project này).

**Settings → Build** (Railway đọc `railway.toml` hoặc set tay):

| Mục | Giá trị mẫu (.NET 8) |
|-----|----------------------|
| Root Directory | **`/` (trống)** nếu repo chỉ BE; **`EnglishCenter.Backend`** nếu monorepo |
| Build Command | `dotnet restore EnglishCenter.Backend.sln && dotnet publish src/EnglishCenter.Api/EnglishCenter.Api.csproj -c Release --no-restore -o out` |
| Start Command | `dotnet out/EnglishCenter.Api.dll` |

Ví dụ English Center:

```bash
dotnet publish src/EnglishCenter.Api/EnglishCenter.Api.csproj -c Release -o out
dotnet out/EnglishCenter.Api.dll
```

### 3.3 Biến môi trường tối thiểu (chưa secret)

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:${PORT}
```

Railway tự inject `PORT`. Không set JWT / connection string ở bước này.

### 3.4 Public URL

**Settings → Networking → Generate Domain** → `https://{service}.up.railway.app`

### 3.5 Deploy lần đầu

- Xem **Deployments → Logs**
- Nếu lỗi *Connection string ... not configured* → chuyển sang mục 4

---

## 4. Giai đoạn 2 — MySQL + Connection string + JWT

### 4.1 Thêm MySQL

1. Cùng **Project** Railway → **+ New** → **Database** → **MySQL**
2. Đợi trạng thái Active
3. Tab **Variables** của MySQL: `MYSQLHOST`, `MYSQLPORT`, `MYSQLUSER`, `MYSQLPASSWORD`, `MYSQLDATABASE`

### 4.2 Gắn DB vào service API

Service **API** → **Variables** → **Add Variable Reference** (hoặc ghép tay):

```env
ConnectionStrings__Default=Server=${{MySQL.MYSQLHOST}};Port=${{MySQL.MYSQLPORT}};Database=${{MySQL.MYSQLDATABASE}};User=${{MySQL.MYSQLUSER}};Password=${{MySQL.MYSQLPASSWORD}};
```

> `${{MySQL....}}` đổi theo **tên service MySQL** trên project (Railway UI gợi ý khi Add Reference).  
> API và MySQL cùng project → dùng host private (`*.railway.internal`).

### 4.3 JWT (và config liên quan)

```env
Jwt__Secret=<chuỗi-ngẫu-nhiên-≥32-ký-tự>
Jwt__Issuer={TênProject}
Jwt__Audience={TênProject}.Api
```

Tạo secret (PowerShell):

```powershell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }) -as [byte[]])
```

**Production dùng secret khác dev** — không copy từ file `.local.json`.

### 4.4 Redeploy

**Deploy** lại hoặc **Restart** service API.

Nếu app có EF migrations + seed lúc startup, DB sẽ được migrate/seed tự động (tùy code dự án).

---

## 5. Kiểm tra sau deploy

### Health

```http
GET https://{domain}/health
```

Kỳ vọng: `Healthy`.

### Login (nếu có JWT + seed admin)

```http
POST https://{domain}/api/v1/auth/tokens
Content-Type: application/json

{
  "grantType": "password",
  "username": "admin",
  "password": "Admin@123"
}
```

Swagger thường **chỉ bật Development** — Production test bằng Postman, curl hoặc FE.

---

## 6. Deploy Frontend (tham khảo)

Khi có FE (Vite/React):

| Biến Railway (FE) | Ý nghĩa |
|-------------------|---------|
| `VITE_API_BASE_URL` | URL API production, ví dụ `https://{api}.up.railway.app` |

Build FE trên Railway cần biến này **lúc build** (Vite embed vào bundle).

CORS: nếu FE và API khác domain, cấu hình CORS trên BE cho origin FE.

---

## 7. Checklist tổng hợp

| # | Bước | Secret? |
|---|------|---------|
| 1 | Dọn secret khỏi file commit | — |
| 2 | `.gitignore` + file `.local.json` / `.example` | Local only |
| 3 | Push GitHub | Không |
| 4 | Railway: New Project + link repo | Không |
| 5 | Build / Start command | Không |
| 6 | `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_URLS` | Không |
| 7 | Generate domain | Không |
| 8 | Deploy lần 1 (có thể lỗi DB) | — |
| 9 | Add MySQL service | — |
| 10 | `ConnectionStrings__Default` | **Có** |
| 11 | `Jwt__Secret` (+ Issuer/Audience) | **Có** |
| 12 | Redeploy | — |
| 13 | Test `/health`, login | — |
| 14 | Deploy FE + `VITE_API_BASE_URL` | URL public |

---

## 8. Xử lý sự cố thường gặp

| Triệu chứng | Nguyên nhân | Cách xử lý |
|-------------|-------------|------------|
| Build fail | Sai path project | Kiểm tra Root Directory, lệnh `dotnet publish` |
| `MSB1003` không tìm thấy `.sln` | Root Directory sai | Repo chỉ BE → Root Directory **trống**; monorepo → `EnglishCenter.Backend` |
| `MSB1003` | Thiếu `railway.toml` | Commit `railway.toml` + buildCommand có `dotnet restore ...sln` |
| Crash ngay khi start | Thiếu connection string | Thêm `ConnectionStrings__Default` |
| Unhealthy / timeout | Sai port | Set `ASPNETCORE_URLS=http://0.0.0.0:${PORT}` |
| DB connection fail | Sai host / firewall | Dùng biến MySQL reference, cùng Railway project |
| Login 401 / token lỗi | Thiếu hoặc JWT ngắn | `Jwt__Secret` ≥ 32 ký tự |
| CORS từ browser | Chưa cấu hình CORS BE | Thêm policy cho origin FE |

---

## 9. Tùy biến cho dự án mới

Thay các placeholder:

| Placeholder | Ví dụ English Center |
|-------------|----------------------|
| `{BackendFolder}` | `EnglishCenter.Backend` |
| `{ApiProject}` | `EnglishCenter.Api` |
| `{TênProject}` | `EnglishCenter` |
| `{Repo}` | `english-center-api` |

Điều chỉnh thêm nếu dự án khác:

- **PostgreSQL** → plugin Postgres, connection string format khác
- **Redis / S3** → thêm Variables tương ứng (`Redis__...`, `AWS__...`)
- **Docker** → thêm `Dockerfile`, Railway build từ Docker thay Nixpacks
- **Không dùng EF migrate lúc startup** → chạy migration tay hoặc CI riêng

---

## 10. Tài liệu tham khảo

- [Railway — Deploy .NET](https://docs.railway.com/guides/dotnet)
- [ASP.NET Core — Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [ASP.NET Core — Safe storage of app secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)

---

*Cập nhật: 2025-06-03 — English Center Backend*
