# File Structure — Every File Explained

This document describes every folder and file in the project and what it does.

---

## Full Directory Tree

```
investly_backendproject/
│
├── Program.cs                          ← App entry point, startup config
├── appsettings.json                    ← Main config (DB connection, JWT, CORS)
├── appsettings.Development.json        ← Dev-only overrides
├── investly_backendproject.csproj      ← Project file (NuGet packages listed here)
│
├── Controllers/                        ← HTTP endpoints (what the apps call)
│   ├── AuthController.cs               ← Login, register, OTP, password reset, profile
│   ├── ProjectsController.cs           ← Browse & create projects
│   ├── OwnersController.cs             ← Owner dashboard & stats
│   ├── InvestmentsController.cs        ← Invest, wallet, checkout
│   ├── PaymentsController.cs           ← Payment history & status
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
│   ├── DTOs/                           ← Request/response shapes (not stored in DB)
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
│   └── DbSeeder.cs                     ← Seeds initial admin user on first run
│
├── Services/
│   ├── Interfaces/                     ← Service contracts (what each service can do)
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
│   └── ReferenceGenerator.cs           ← Generates reference codes (PRJ-xxx, INV-xxx)
│
├── Migrations/                         ← Auto-generated EF Core migration files
│   ├── 20260521125913_InitialCreate.cs
│   └── AppDbContextModelSnapshot.cs
│
├── wwwroot/uploads/                    ← Uploaded files stored here (images, PDFs)
│
└── docs/                               ← This documentation folder
```

---

## Entry Point & Configuration

### `Program.cs`
The application entry point. Runs on startup and:
- Registers all services (database, JWT, CORS, controllers, Swagger)
- Adds the global exception middleware to the pipeline
- On startup: automatically applies database migrations
- On startup: runs `DbSeeder` to create the admin user if not yet created
- Starts the HTTP server on port 5000

### `appsettings.json`
The main configuration file. Every value here can be changed without touching code.

| Section | Key | What It Controls |
|---------|-----|-----------------|
| `ConnectionStrings` | `DefaultConnection` | PostgreSQL host, port, database name, username, password |
| `Jwt` | `Secret` | The key used to sign JWT tokens (keep private, min 32 chars) |
| `Jwt` | `AccessTokenMinutes` | How long access tokens live (default: 60 minutes) |
| `Jwt` | `RefreshTokenDays` | How long refresh tokens live (default: 30 days) |
| `Cors` | `AllowedOrigins` | Which frontend URLs can call the API |

---

## Controllers (HTTP Layer)

Controllers receive HTTP requests and call services. They do NOT contain business logic.

### `Controllers/AuthController.cs`
13 endpoints for authentication:
- Login (email or phone), register, OTP, forgot/reset password
- Token refresh and logout
- Get and update current user profile

### `Controllers/ProjectsController.cs`
8 endpoints for projects:
- Browse all projects with filters (category, search, status, pagination)
- Get featured projects for the home screen carousel
- Get categories
- Get single project details and stats
- Create and update projects (owner only)
- Increment view counter

### `Controllers/OwnersController.cs`
3 endpoints for project owners:
- Get owner's projects list
- Get owner statistics (total raised, investor count, etc.)
- Get full owner dashboard (projects + stats combined)

### `Controllers/InvestmentsController.cs`
8 endpoints — the core money flow:
- Checkout (invest in one or more projects)
- View my investments and investment history
- Wallet balance, top-up, and withdraw
- Get available payment methods
- Redeem a top-up code

### `Controllers/PaymentsController.cs`
8 endpoints for payment records:
- Initiate and verify a payment
- Get payment history, single payment, and payment status
- Request a refund

### `Controllers/UsersController.cs`
7 endpoints for user management:
- Get and update user profile
- Delete account
- Submit KYC document (passport image URL)
- Get KYC documents
- Get user's investments

### `Controllers/NotificationsController.cs`
6 endpoints for notifications:
- Get all notifications
- Get unread count (badge number)
- Mark one or all as read
- Get and update notification preferences

### `Controllers/MediaController.cs`
2 endpoints for file uploads:
- Upload a file (jpeg, png, webp, pdf — max 10 MB). Saves to `wwwroot/uploads/`.
- Delete a file

### `Controllers/AdminController.cs`
26 endpoints — ALL require `admin` role:
- Platform stats, chart data for graphs, recent activity feed, activity logs
- User management: list, ban, suspend, unsuspend, KYC approve/reject, add wallet funds
- Project management: list, approve, reject, toggle featured
- Investment management: list all
- Payment management: list, approve, reject, refund, update status
- Wallet management: list all, transfer between wallets
- Send targeted or broadcast notifications

---

## Models

### `Models/Entities/` — Database Tables

Each class maps exactly to one database table. Entity Framework reads these and creates the tables.

| File | Table | Purpose |
|------|-------|---------|
| `User.cs` | `users` | All user accounts. Has: id, name, email, phone, role, status, kyc_status, age, gender, location, member_id |
| `Wallet.cs` | `wallets` | One wallet per user. Has: balance, total_deposits, total_withdrawals |
| `Category.cs` | `categories` | 6 pre-seeded project categories (tech, energy, agri, health, edu, realestate) |
| `Project.cs` | `projects` | Investment projects. Has: title (AR+EN), description (AR+EN), goal, raised, status, is_featured, min/max_investment, start/end_date |
| `Investment.cs` | `investments` | One record per investment made. Has: investor_id, project_id, amount, status, reference |
| `Payment.cs` | `payments` | Payment transaction records. Has: user_id, amount, method, status, reference |
| `WalletTransaction.cs` | `wallet_transactions` | Full ledger of every credit and debit to a wallet |
| `Notification.cs` | `notifications` | System and admin notifications. Can target a specific user or all users |
| `UserNotificationRead.cs` | `user_notification_reads` | Tracks which user has read which notification |
| `NotificationSettings.cs` | `notification_settings` | Per-user notification preferences (investment alerts, project updates, etc.) |
| `RefreshToken.cs` | `refresh_tokens` | Stored refresh tokens for session management |
| `OtpCode.cs` | `otp_codes` | One-time passwords for phone login/verify flows |
| `PasswordResetCode.cs` | `password_reset_codes` | Email-based password reset codes |
| `Media.cs` | `media` | Records of every uploaded file (URL, file name, size, uploader) |

### `Models/DTOs/` — Request & Response Shapes

DTOs (Data Transfer Objects) define exactly what JSON the API expects and returns. They are separate from the database entities — this keeps the API surface clean and safe.

| File | Contains |
|------|---------|
| `Auth/AuthDtos.cs` | Login request, register request, profile response, token response |
| `Projects/ProjectDtos.cs` | Create/update project request, project list response, project detail response |
| `Investments/InvestmentDtos.cs` | Checkout request, investment response |
| `Payments/PaymentDtos.cs` | Initiate payment request, payment response |
| `Users/UserDtos.cs` | Update profile request, user detail response, KYC request |
| `Notifications/NotificationDtos.cs` | Notification response, settings request/response |
| `Admin/AdminDtos.cs` | Admin stats response (`DashboardStatsResponse`), chart data responses (`ChartDataResponse`, `MonthlyUserPoint`, `MonthlyRevenuePoint`, `ProjectStatusBreakdown`), recent activity (`RecentActivityItem`), user/payment/wallet management requests |

### `Models/Enums/Enums.cs`

All enum types used across the system:

```
UserRole:               investor | owner | admin | guest
UserStatus:             active | pending | suspended | banned
KycStatus:              none | pending | approved | rejected
Gender:                 male | female | other
WalletStatus:           active | frozen | inactive
WalletTransactionType:  credit | debit
WalletTransactionStatus: completed | pending | failed | refunded
ProjectStatus:          pending | active | completed | inactive | rejected
InvestmentStatus:       pending | completed | failed | cancelled
PaymentMethod:          wallet | credit_card
PaymentStatus:          pending | completed | failed | refunded
NotificationType:       investment | project | system | user
OtpPurpose:             login | register | reset
```

---

## Data Layer

### `Data/AppDbContext.cs`
The Entity Framework "database context". It:
- Declares all 14 table sets (`DbSet<User>`, `DbSet<Project>`, etc.)
- Configures snake_case column naming (e.g., `created_at` not `CreatedAt`)
- Sets up table relationships (foreign keys, indexes)
- Seeds the 6 project categories via `HasData()`

### `Data/DbSeeder.cs`
Runs once when the app starts. Checks if the admin user exists — if not, creates it:
- Email: `admin@investly.ly`
- Password: `Admin@2024!` (BCrypt hashed)
- Role: `admin`

---

## Services (Business Logic Layer)

### `Services/Interfaces/` — What Each Service Can Do
The interface files define the contract — what methods each service must implement. This allows easy testing and swapping of implementations.

### `Services/Implementations/AuthService.cs`
- Verifies email/password using BCrypt hash comparison
- Blocks banned/suspended accounts before issuing tokens
- Creates JWT access token + refresh token on login
- On register: creates User + Wallet (balance=0) + NotificationSettings
- Handles OTP codes and password reset codes with expiry

### `Services/Implementations/ProjectService.cs`
- Validates only `owner`-role users can create projects (new projects start as `pending`)
- Generates project reference codes (e.g., `PRJ-202606-0001`)
- Returns projects joined with owner info, category info, and stats
- Updates `project.raised` and `project.investors_count` after an investment

### `Services/Implementations/InvestmentService.cs`
The most critical service — handles the full money flow atomically:
1. Validates project is `active`
2. Validates amount is within `min_investment` and `max_investment`
3. For wallet payments: checks `wallet.balance >= amount`, then debits the wallet
4. Creates an `Investment` record, a `Payment` record, and a `WalletTransaction` record
5. Updates `project.raised` and `project.investors_count`
6. Sends a notification to the investor

### `Services/Implementations/JwtService.cs`
- Creates JWT access tokens (60 min lifetime, contains user ID + email + role)
- Creates refresh tokens (30-day lifetime, stored in the database)
- Validates tokens on incoming requests

### `Services/Implementations/MediaService.cs`
- Validates file size (max 10 MB) and file type (jpeg, png, webp, pdf only)
- Saves file to `wwwroot/uploads/` with a UUID filename to prevent collisions
- Returns the public URL the frontend can display

### `Services/Implementations/AdminService.cs`
Implements all 26 admin operations. Key ones:
- `ApproveProject` / `RejectProject` — changes `project.status` which controls visibility in the app
- `BanUser` / `SuspendUser` — changes `user.status` which blocks login
- `ApproveKyc` — changes `user.kyc_status` which controls the verified badge in the app
- `AddWalletFunds` — directly credits a user's wallet
- `GetChartDataAsync` — returns monthly user growth by role, monthly revenue, and project status breakdown for dashboard charts
- `GetRecentActivityAsync` — returns the N most recent platform events (investments, registrations, project approvals)
- `SendNotification` — creates notification records (targeted or broadcast)

---

## Infrastructure

### `Infrastructure/Extensions/ServiceExtensions.cs`
Registers everything with the dependency injection container:
- Database context (EF Core + PostgreSQL)
- All 9 services as `Scoped` (one instance per HTTP request)
- JWT authentication middleware
- CORS policy from config
- Swagger

### `Infrastructure/Middleware/ExceptionMiddleware.cs`
Global error handler. Every unhandled exception is caught here and converted to a consistent JSON response instead of ASP.NET's default HTML error page.

| Exception Type | HTTP Status | Example |
|---------------|------------|---------|
| `KeyNotFoundException` | 404 Not Found | "Project not found." |
| `UnauthorizedAccessException` | 401 Unauthorized | "You are not authorized." |
| `InvalidOperationException` | 400 Bad Request | "Insufficient wallet balance." |
| `ArgumentException` | 400 Bad Request | "Invalid email format." |
| Any other exception | 500 Internal Server Error | "An unexpected error occurred." |

---

## Helpers

### `Helpers/ApiResponse.cs`
Every single API response — success or error — is wrapped in this standard format:
```json
{
  "success": true,
  "message": "Login successful.",
  "data": { ... }
}
```
This makes it easy for the frontends to handle responses consistently.

### `Helpers/ReferenceGenerator.cs`
Generates human-readable reference codes:
- Projects: `PRJ-202606-0001`
- Investments: `INV-2026-0001`
- Wallet transactions: random 16-character hex string
- Member IDs: last 8 digits of user's phone number
