# Investly Backend — Documentation Index

This folder contains detailed documentation for the Investly backend API.
For the full project guide see the root [`README.md`](../README.md).

---

## Files in This Folder

| File | What It Covers |
|------|---------------|
| `FILE_STRUCTURE.md` | Every folder and file in the project explained in detail |
| `HOW_TO_RUN.md` | Full setup guide — PostgreSQL, connection string, frontend connection, error fixes |
| `ENDPOINTS_ALL.md` | All 81 API endpoints with request body examples |
| `ENDPOINTS_MYAPP.md` | Only the endpoints the mobile app (investors & owners) uses |
| `ENDPOINTS_DASHBOARD.md` | Only the endpoints the admin dashboard uses |
| `DASHBOARD_CONTROL.md` | How each admin action in the dashboard affects what users see in the app |
| `ENDPOINT_TESTS.http` | Runnable HTTP test file — open in VS Code with REST Client extension |

---

## System Overview

```
┌─────────────────────┐      HTTP/REST       ┌──────────────────────────┐
│  Mobile App (myApp) │ ──────────────────►  │                          │
│  React Native/Expo  │                       │   ASP.NET Core 8 API     │
│  investors & owners │ ◄──────────────────   │   http://localhost:5000  │
└─────────────────────┘      JSON             │                          │
                                              │   PostgreSQL — 14 tables │
┌─────────────────────┐      HTTP/REST        │                          │
│  Admin Dashboard    │ ──────────────────►   │                          │
│  Next.js            │                       └──────────────────────────┘
│  admin only         │ ◄──────────────────
└─────────────────────┘      JSON
```

Both frontends talk to the same API. The dashboard changes data in the database;
the mobile app reads that same data. See `DASHBOARD_CONTROL.md` for the full breakdown.

---

## Quick Start

```powershell
# 1. Edit appsettings.json — set your PostgreSQL password
# 2. Install EF tools (once per machine)
dotnet tool install --global dotnet-ef

# 3. Start the API (auto-creates tables + admin user on first run)
cd C:\Users\seraj\source\repos\investly_backendproject
dotnet run

# 4. Swagger: http://localhost:5000/swagger
# Admin login: admin@investly.ly / Admin@2024!
```

---

## API at a Glance

**Total: 81 endpoints** across 9 controllers

| Controller | Endpoints | Who Uses It |
|------------|-----------|-------------|
| Auth | 13 | Both app & dashboard |
| Projects | 8 | App (browse) + App owner (create) |
| Owners | 3 | App (owner role) |
| Investments | 8 | App (investor role) |
| Payments | 8 | App |
| Users | 6 | App |
| Notifications | 6 | App |
| Media | 2 | App |
| Admin | 26 | Dashboard only |

---

## Admin Credentials (auto-created on first run)

| Email | Password | Role |
|-------|----------|------|
| admin@investly.ly | Admin@2024! | admin |
