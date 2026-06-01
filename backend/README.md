# Investly Backend API — Complete Project Guide

## Table of Contents
1. [What Is This Project?](#1-what-is-this-project)
2. [Full Project Structure](#2-full-project-structure)
3. [Every File Explained](#3-every-file-explained)
4. [How The System Works](#4-how-the-system-works)
5. [Step-by-Step Setup](#5-step-by-step-setup)
6. [How to Run](#6-how-to-run)
7. [All API Endpoints](#7-all-api-endpoints)
8. [Database Tables](#8-database-tables)
9. [User Roles & Permissions](#9-user-roles--permissions)
10. [Authentication Flow](#10-authentication-flow)
11. [Testing With Swagger](#11-testing-with-swagger)
12. [Connecting The Frontends](#12-connecting-the-frontends)
13. [Common Errors & Fixes](#13-common-errors--fixes)

> **Full documentation:** See the [`docs/`](./docs/) folder for dedicated files covering endpoints, file structure, dashboard control, and runnable HTTP tests.

---

## 1. What Is This Project?

**Investly** is a crowdfunding investment platform for Libya.

There are 3 parts to the full system:

| Part | Technology | Location |
|------|-----------|----------|
| Mobile App (investors & owners) | React Native / Expo | `مشروع التخرج/project_code/myApp` |
| Admin Dashboard | Next.js / React | `مشروع التخرج/project_code/dashbord` |
| **Backend API (this project)** | **ASP.NET Core 8** | `investly_backendproject/` |
| Database | PostgreSQL | runs locally or on a server |

This backend is the "brain" — the mobile app and dashboard both call this API to get data, save data, handle payments, and manage users. Without this backend, both frontends only show fake (mock) data.

---

## 2. Full Project Structure

```
investly_backendproject/
│
├── Program.cs                          ← App entry point, startup config
├── appsettings.json                    ← Main config (DB, JWT, CORS)
├── appsettings.Development.json        ← Dev-only config
├── investly_backendproject.csproj      ← Project file (NuGet packages)
│
├── Controllers/                        ← HTTP endpoints (what the apps call)
│   ├── AuthController.cs               ← Login, register, password reset
│   ├── ProjectsController.cs           ← Browse & create projects
│   ├── OwnersController.cs             ← Owner dashboard & stats
│   ├── InvestmentsController.cs        ← Invest, wallet, checkout
│   ├── PaymentsController.cs           ← Payments history & status
│   ├── UsersController.cs              ← User profiles & KYC
│   ├── NotificationsController.cs      ← Notifications & settings
│   ├── MediaController.cs              ← File uploads
│   └── AdminController.cs              ← Admin-only operations (26 endpoints)
│
├── Models/
│   ├── Entities/                       ← Database table classes (14 tables)
│   │   ├── User.cs
│   │   ├── Wallet.cs
│   │   ├── Category.cs
│   │   ├── Project.cs
│   │   ├── Investment.cs
│   │   ├── Payment.cs
│   │   ├── WalletTransaction.cs
│   │   ├── Notification.cs
│   │   ├── UserNotificationRead.cs
│   │   ├── NotificationSettings.cs
│   │   ├── RefreshToken.cs
│   │   ├── OtpCode.cs
│   │   ├── PasswordResetCode.cs
│   │   └── Media.cs
│   │
│   ├── DTOs/                           ← Request/response shapes
│   │   ├── Auth/AuthDtos.cs
│   │   ├── Projects/ProjectDtos.cs
│   │   ├── Investments/InvestmentDtos.cs
│   │   ├── Payments/PaymentDtos.cs
│   │   ├── Users/UserDtos.cs
│   │   ├── Notifications/NotificationDtos.cs
│   │   └── Admin/AdminDtos.cs
│   │
│   └── Enums/Enums.cs                  ← All enum types (roles, statuses, etc.)
│
├── Data/
│   ├── AppDbContext.cs                 ← EF Core database context
│   └── DbSeeder.cs                     ← Seeds the first admin user
│
├── Services/
│   ├── Interfaces/                     ← Service contracts
│   │   ├── IAuthService.cs
│   │   ├── IProjectService.cs
│   │   ├── IInvestmentService.cs
│   │   ├── IPaymentService.cs
│   │   ├── IUserService.cs
│   │   ├── INotificationService.cs
│   │   ├── IMediaService.cs
│   │   ├── IAdminService.cs
│   │   └── IJwtService.cs
│   │
│   └── Implementations/                ← Actual business logic
│       ├── AuthService.cs
│       ├── ProjectService.cs
│       ├── InvestmentService.cs
│       ├── PaymentService.cs
│       ├── UserService.cs
│       ├── NotificationService.cs
│       ├── MediaService.cs
│       ├── AdminService.cs
│       └── JwtService.cs
│
├── Infrastructure/
│   ├── Extensions/
│   │   └── ServiceExtensions.cs        ← Registers all services with DI container
│   └── Middleware/
│       └── ExceptionMiddleware.cs      ← Global error handler (returns JSON errors)
│
├── Helpers/
│   ├── ApiResponse.cs                  ← Standard JSON response wrapper
│   └── ReferenceGenerator.cs          ← Generates reference codes (PRJ-xxx, INV-xxx)
│
├── Migrations/                         ← Auto-generated database migration files
│   ├── 20260521125913_InitialCreate.cs
│   └── AppDbContextModelSnapshot.cs
│
├── wwwroot/uploads/                    ← Uploaded files stored here
│
└── docs/                               ← Full project documentation
    ├── README.md                       ← Docs index
    ├── FILE_STRUCTURE.md               ← Every file explained
    ├── HOW_TO_RUN.md                   ← Setup & run guide
    ├── ENDPOINTS_ALL.md                ← All 81 endpoints
    ├── ENDPOINTS_MYAPP.md              ← Mobile app endpoints
    ├── ENDPOINTS_DASHBOARD.md          ← Admin dashboard endpoints
    ├── DASHBOARD_CONTROL.md            ← How dashboard controls the app
    └── ENDPOINT_TESTS.http             ← Runnable HTTP tests
```

---

## 3. Every File Explained

### `Program.cs`
The application entry point. It registers all services (database, JWT, CORS, controllers), builds the middleware pipeline, and on startup automatically applies database migrations and seeds the admin user.

### `appsettings.json`
Main configuration. Contains:
- **ConnectionStrings.DefaultConnection** — PostgreSQL connection
- **Jwt.Secret** — token signing key (keep this private, must be ≥32 chars)
- **Jwt.AccessTokenMinutes** — access token lifetime (default: 60 min)
- **Jwt.RefreshTokenDays** — refresh token lifetime (default: 30 days)
- **Cors.AllowedOrigins** — which frontend URLs are allowed to call the API

### Controllers
| Controller | What It Handles |
|------------|----------------|
| `AuthController` | Login, register, OTP, forgot/reset password, token refresh, profile |
| `ProjectsController` | Browse, filter, create, update projects; view counter; stats |
| `OwnersController` | Owner's projects list, owner stats, owner dashboard |
| `InvestmentsController` | Checkout (invest), my investments, wallet balance/topup/withdraw |
| `PaymentsController` | Initiate, verify, history, status, refund |
| `UsersController` | Get/update profile, delete account, submit KYC, documents |
| `NotificationsController` | Get notifications, unread count, mark read, settings |
| `MediaController` | Upload files (max 10 MB: jpeg/png/webp/pdf), delete files |
| `AdminController` | All 26 admin operations — see section 7 below |

### `Models/Entities/`
Each file maps to one database table. Entity Framework reads these and creates the tables automatically.

### `Models/DTOs/`
Data Transfer Objects — define exactly what JSON the API accepts and returns. They are NOT stored in the database.

### `Models/Enums/Enums.cs`
All enum types: `UserRole`, `UserStatus`, `KycStatus`, `Gender`, `ProjectStatus`, `InvestmentStatus`, `PaymentStatus`, `WalletTransactionType`, `NotificationType`, `OtpPurpose`.

### `Data/AppDbContext.cs`
Entity Framework context. Configures table relationships, column types, check constraints, indexes, and seeds the 6 project categories.

### `Data/DbSeeder.cs`
Runs on first startup and creates the initial admin account (`admin@investly.ly` / `Admin@2024!`).

### `Services/Implementations/InvestmentService.cs`
The core money flow:
1. Validates project is `active` and amount is within min/max range
2. For wallet payments: checks balance, atomically debits wallet
3. Creates Investment + Payment + WalletTransaction records
4. Updates project's `raised` and `investors_count`
5. Sends notification to investor

### `Services/Implementations/AdminService.cs`
Handles dashboard stats, chart data (monthly user growth/revenue, project status breakdown), recent activity feed, activity logs, wallet listing, and inter-wallet transfers.

### `Infrastructure/Middleware/ExceptionMiddleware.cs`
Catches all unhandled exceptions and converts them to consistent JSON:

| Exception | HTTP Code |
|-----------|----------|
| `KeyNotFoundException` | 404 Not Found |
| `UnauthorizedAccessException` | 401 Unauthorized |
| `InvalidOperationException` | 400 Bad Request |
| `ArgumentException` | 400 Bad Request |
| Any other | 500 Internal Server Error |

### `Helpers/ApiResponse.cs`
Every response is wrapped in:
```json
{ "success": true, "message": "...", "data": { ... } }
```

### `Helpers/ReferenceGenerator.cs`
Generates human-readable codes: `PRJ-202606-0001`, `INV-2026-0001`, 16-char hex transaction IDs, 8-digit member IDs from phone numbers.

---

## 4. How The System Works

### Registration Flow
```
User fills register form in mobile app
    → POST /api/auth/register
        → AuthService creates User record
        → Creates Wallet (balance = 0)
        → Creates NotificationSettings (all defaults on)
        → Returns JWT token + user data
```

### Login Flow
```
User enters email + password
    → POST /api/auth/login-email
        → BCrypt.Verify(password, stored_hash)
        → Checks user is not banned/suspended
        → Creates RefreshToken in DB
        → Returns: { token, refreshToken, user }
```

### Investment Flow
```
Investor taps "Invest" → Checkout
    → POST /api/investments/checkout
        → Validates project is active, amount within min/max
        → Debits wallet atomically (if payment = wallet)
        → Creates Investment + Payment + WalletTransaction
        → Updates project.raised and project.investors_count
        → Sends notification to investor
```

### Token Refresh Flow
```
App gets 401 (token expired, 60 min default)
    → POST /api/auth/refresh-token { refreshToken }
        → Validates refresh token in DB (30 day lifetime)
        → Issues new access token + new refresh token
```

---

## 5. Step-by-Step Setup

### Prerequisites

| Tool | Check | Download |
|------|-------|----------|
| .NET 8 SDK | `dotnet --version` → `8.x.x` | https://dotnet.microsoft.com/download/dotnet/8.0 |
| PostgreSQL | Open pgAdmin or run `psql --version` | https://www.postgresql.org/download/windows/ |

### Step 1 — Create the database

Open pgAdmin → right-click Databases → Create → name it `InvestlyDB` → Save.

Or in terminal:
```powershell
psql -U postgres -c "CREATE DATABASE InvestlyDB;"
```

### Step 2 — Set your PostgreSQL password

Open `appsettings.json` and update:
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=InvestlyDB;Username=postgres;Password=YOUR_PASSWORD"
```

### Step 3 — Install EF tools (once per machine)

```powershell
dotnet tool install --global dotnet-ef
```

---

## 6. How to Run

```powershell
cd C:\Users\seraj\source\repos\investly_backendproject
dotnet run
```

On first run: tables are created automatically, admin user is seeded.

The API runs at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:7000`
- **Swagger**: `http://localhost:5000/swagger`

**Admin credentials:** `admin@investly.ly` / `Admin@2024!`

Stop with `Ctrl + C`.

> See [`docs/HOW_TO_RUN.md`](./docs/HOW_TO_RUN.md) for the full setup guide, frontend connection instructions, and common error fixes.

---

## 7. All API Endpoints

Base URL: `http://localhost:5000/api`  
🔒 = requires Bearer token | 👑 = admin role required

**Total: 81 endpoints**

### Auth — 13 endpoints

| Method | URL | Auth |
|--------|-----|------|
| POST | `/auth/login-email` | Public |
| POST | `/auth/login` | Public |
| POST | `/auth/register` | Public |
| POST | `/auth/send-otp` | Public |
| POST | `/auth/verify-otp` | Public |
| POST | `/auth/forgot-password` | Public |
| POST | `/auth/verify-reset-code` | Public |
| POST | `/auth/reset-password` | Public |
| POST | `/auth/refresh-token` | Public |
| POST | `/auth/logout` | 🔒 |
| POST | `/auth/change-password` | 🔒 |
| GET | `/auth/profile` | 🔒 |
| PUT | `/auth/profile` | 🔒 |

### Projects — 8 endpoints

| Method | URL | Auth |
|--------|-----|------|
| GET | `/projects/featured` | Public |
| GET | `/projects` | Public |
| GET | `/projects/categories` | Public |
| GET | `/projects/{id}` | Public |
| GET | `/projects/{id}/stats` | Public |
| POST | `/projects/{id}/views` | Public |
| POST | `/projects` | 🔒 owner |
| PUT | `/projects/{id}` | 🔒 |

### Owners — 3 endpoints

| Method | URL | Auth |
|--------|-----|------|
| GET | `/owners/{ownerId}/projects` | 🔒 |
| GET | `/owners/{ownerId}/stats` | 🔒 |
| GET | `/owners/{ownerId}/dashboard` | 🔒 |

### Investments — 8 endpoints

| Method | URL | Auth |
|--------|-----|------|
| POST | `/investments/checkout` | 🔒 |
| GET | `/investments/me` | 🔒 |
| GET | `/investments/history` | 🔒 |
| GET | `/investments/wallet` | 🔒 |
| POST | `/investments/wallet/topup` | 🔒 |
| POST | `/investments/wallet/withdraw` | 🔒 |
| GET | `/investments/funding-options` | 🔒 |
| POST | `/investments/topup/redeem` | 🔒 |

### Payments — 8 endpoints

| Method | URL | Auth |
|--------|-----|------|
| GET | `/payments/methods` | Public |
| POST | `/payments/initiate` | 🔒 |
| POST | `/payments/verify` | 🔒 |
| GET | `/payments/history` | 🔒 |
| GET | `/payments/wallet` | 🔒 |
| GET | `/payments/{id}` | 🔒 |
| GET | `/payments/{id}/status` | 🔒 |
| POST | `/payments/{id}/refund` | 🔒 |

### Users — 6 endpoints

| Method | URL | Auth |
|--------|-----|------|
| GET | `/users/{userId}` | 🔒 |
| PUT | `/users/{userId}` | 🔒 |
| DELETE | `/users/{userId}` | 🔒 |
| POST | `/users/{userId}/kyc` | 🔒 |
| GET | `/users/{userId}/documents` | 🔒 |
| GET | `/users/{userId}/investments` | 🔒 |

### Notifications — 6 endpoints

| Method | URL | Auth |
|--------|-----|------|
| GET | `/notifications` | 🔒 |
| GET | `/notifications/unread-count` | 🔒 |
| POST | `/notifications/{id}/read` | 🔒 |
| POST | `/notifications/read-all` | 🔒 |
| GET | `/notifications/settings` | 🔒 |
| PUT | `/notifications/settings` | 🔒 |

### Media — 2 endpoints

| Method | URL | Auth |
|--------|-----|------|
| POST | `/media/upload` | 🔒 |
| DELETE | `/media/{mediaId}` | 🔒 |

### Admin — 26 endpoints 👑

#### Dashboard
| Method | URL | Description |
|--------|-----|-------------|
| GET | `/admin/stats` | Platform stats: users, projects, revenue, success rate, new users today/this week |
| GET | `/admin/chart-data` | Chart data: monthly user growth by role, monthly revenue, project status breakdown |
| GET | `/admin/recent-activity` | Recent activity feed — query: `count` (default 10) |
| GET | `/admin/activity-logs` | Full admin action audit log — query: `adminId`, `page`, `pageSize` |

#### User Management
| Method | URL | Description |
|--------|-----|-------------|
| GET | `/admin/users` | List all users — query: `search`, `status`, `kycStatus`, `page`, `pageSize` |
| PUT | `/admin/users/{id}` | Update a user |
| POST | `/admin/users/{id}/ban` | Permanently ban user |
| POST | `/admin/users/{id}/suspend` | Suspend user — body: `{ reason }` |
| POST | `/admin/users/{id}/unsuspend` | Lift suspension |
| POST | `/admin/users/{id}/kyc/approve` | Approve KYC |
| POST | `/admin/users/{id}/kyc/reject` | Reject KYC — body: `{ reason }` |
| POST | `/admin/users/{id}/wallet/add` | Add funds to wallet — body: `{ amount, reason }` |

#### Investment Management
| Method | URL | Description |
|--------|-----|-------------|
| GET | `/admin/investments` | List all investments — query: `status`, `userId`, `projectId`, `page`, `pageSize` |

#### Project Management
| Method | URL | Description |
|--------|-----|-------------|
| GET | `/admin/projects` | List all projects — query: `status`, `page`, `pageSize` |
| POST | `/admin/projects/{id}/approve` | Approve pending project |
| POST | `/admin/projects/{id}/reject` | Reject project — body: `{ reason }` |
| PATCH | `/admin/projects/{id}/featured` | Toggle featured — body: `{ isFeatured }` |

#### Payment Management
| Method | URL | Description |
|--------|-----|-------------|
| GET | `/admin/payments` | List all payments — query: `status`, `page`, `pageSize` |
| POST | `/admin/payments/{id}/approve` | Approve payment |
| POST | `/admin/payments/{id}/reject` | Reject payment — body: `{ reason }` |
| POST | `/admin/payments/{id}/refund` | Refund payment |
| PUT | `/admin/payments/{id}/status` | Update status — body: `{ status }` |

#### Wallet Management
| Method | URL | Description |
|--------|-----|-------------|
| GET | `/admin/wallets` | List all wallets — query: `search` |
| POST | `/admin/wallet/transfer` | Transfer between wallets — body: `{ fromUserId, toUserId, amount, reason }` |

#### Notifications
| Method | URL | Description |
|--------|-----|-------------|
| POST | `/admin/notifications/send` | Send to one user or broadcast — body: `{ userId?, titleAr, titleEn, messageAr, messageEn, type }` |

---

## 8. Database Tables

| Table | Purpose |
|-------|---------|
| `users` | All accounts (investors, owners, admins) |
| `wallets` | One wallet per user, stores balance |
| `categories` | 6 project categories (pre-seeded) |
| `projects` | Investment projects created by owners |
| `investments` | Records of each investment made |
| `payments` | Payment transaction records |
| `wallet_transactions` | Full ledger of every wallet credit/debit |
| `notifications` | System & admin notifications |
| `user_notification_reads` | Tracks which user has read which notification |
| `notification_settings` | Per-user notification preferences |
| `refresh_tokens` | Stored refresh tokens for session management |
| `otp_codes` | One-time passwords for phone login |
| `password_reset_codes` | Email password reset codes |
| `media` | Uploaded file records |

### Pre-seeded Data

**Categories:**

| ID | Arabic | English |
|----|--------|---------|
| tech | تقنية | Technology |
| energy | طاقة متجددة | Renewable Energy |
| agri | زراعة | Agriculture |
| health | صحة | Health |
| edu | تعليم | Education |
| realestate | عقارات | Real Estate |

**Admin user (created on first run):**

| Email | Password | Role |
|-------|----------|------|
| admin@investly.ly | Admin@2024! | admin |

---

## 9. User Roles & Permissions

| Action | investor | owner | admin |
|--------|----------|-------|-------|
| Browse projects | ✅ | ✅ | ✅ |
| Register / Login | ✅ | ✅ | ✅ |
| Invest in projects | ✅ | ❌ | ❌ |
| Manage own wallet | ✅ | ✅ | ❌ |
| Create projects | ❌ | ✅ | ❌ |
| View owner dashboard | ❌ | ✅ | ❌ |
| Approve/reject projects | ❌ | ❌ | ✅ |
| Ban/suspend users | ❌ | ❌ | ✅ |
| Approve KYC | ❌ | ❌ | ✅ |
| Send notifications | ❌ | ❌ | ✅ |
| View platform charts | ❌ | ❌ | ✅ |
| Manage all payments | ❌ | ❌ | ✅ |

---

## 10. Authentication Flow

All protected endpoints require:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

1. Login → receive `token` (60 min) + `refreshToken` (30 days)
2. App stores both tokens (AsyncStorage on mobile, localStorage on dashboard)
3. Every API call includes `Authorization: Bearer {token}`
4. When access token expires → call `POST /api/auth/refresh-token` with refresh token
5. On logout → refresh token is revoked in the database

---

## 11. Testing With Swagger

1. `dotnet run`
2. Open `http://localhost:5000/swagger`
3. `POST /api/auth/login-email` → body: `{ "email": "admin@investly.ly", "password": "Admin@2024!", "role": "admin" }`
4. Copy the `token` from the response
5. Click **Authorize** at the top → enter `Bearer eyJ...`
6. All 🔒 endpoints now work

---

## 12. Connecting The Frontends

### Mobile App (React Native)

Open `مشروع التخرج/project_code/myApp/.env` and set:
```
EXPO_PUBLIC_API_BASE_URL=http://192.168.1.X:5000/api
EXPO_PUBLIC_USE_MOCK_API=false
```

Replace `192.168.1.X` with your computer's local IP (`ipconfig` → IPv4 Address).

### Admin Dashboard (Next.js)

Open `مشروع التخرج/project_code/dashbord/.env.local` and set:
```
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000/api
NEXT_PUBLIC_MOCK_MODE=false
```

Start the dashboard:
```powershell
cd "مشروع التخرج/project_code/dashbord"
npm install
npm run dev
```

Dashboard at `http://localhost:3000` — login with `admin@investly.ly` / `Admin@2024!`

---

## 13. Common Errors & Fixes

**"Connection refused" or "database does not exist"**
→ Make sure PostgreSQL is running and the password in `appsettings.json` is correct.

**"Invalid token"**
→ Token expired. Call `POST /api/auth/login-email` again. Use `Bearer ` prefix in the header.

**"Not authorized to create projects"**
→ Only `role = "owner"` accounts can create projects.

**"Insufficient wallet balance"**
→ As admin: `POST /api/admin/users/{id}/wallet/add`. As user: `POST /api/investments/wallet/topup`.

**"dotnet-ef not found"**
→ Run `dotnet tool install --global dotnet-ef` then reopen terminal.

**"Port already in use"**
→ Change port in `Properties/launchSettings.json`.

---

## Quick Start

```powershell
# 1. Set your PostgreSQL password in appsettings.json

# 2. Install EF tools (once per machine)
dotnet tool install --global dotnet-ef

# 3. Start the API
cd C:\Users\seraj\source\repos\investly_backendproject
dotnet run

# 4. Swagger: http://localhost:5000/swagger
# Admin: admin@investly.ly / Admin@2024!
```
