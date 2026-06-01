# How to Run the Investly Backend API

---

## Prerequisites

Make sure these are installed on your computer before you start.

| Tool | How to Check | Download |
|------|-------------|----------|
| .NET 8 SDK | Run `dotnet --version` — should show `8.x.x` | https://dotnet.microsoft.com/download/dotnet/8.0 |
| PostgreSQL | Open pgAdmin or run `psql --version` | https://www.postgresql.org/download/windows/ |

---

## Step 1 — Create the Database

Open **pgAdmin** (installed with PostgreSQL).

1. In the left panel, right-click **Databases** → **Create** → **Database**
2. Set the name to: `InvestlyDB`
3. Click **Save**

Or use the command line:
```powershell
psql -U postgres -c "CREATE DATABASE InvestlyDB;"
```

---

## Step 2 — Set Your PostgreSQL Password

Open this file:
```
investly_backendproject/appsettings.json
```

Find and update this line — replace `123456` with your actual PostgreSQL password:
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=InvestlyDB;Username=postgres;Password=123456"
```

If your PostgreSQL username is not `postgres`, change that too.

---

## Step 3 — Install EF Core Tools (once per machine)

Open PowerShell and run:
```powershell
dotnet tool install --global dotnet-ef
```

You only need to do this once. Skip it if you already have `dotnet-ef` installed.

---

## Step 4 — Start the API

```powershell
cd C:\Users\seraj\source\repos\investly_backendproject
dotnet run
```

On first run, the API will automatically:
1. Apply the database migration (creates all 14 tables)
2. Seed the 6 project categories
3. Create the admin user account

You will see output like:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7000
```

---

## Step 5 — Open Swagger UI

Open your browser and go to:
```
http://localhost:5000/swagger
```

This shows every API endpoint with a form to test it.

---

## Admin Account (created automatically on first run)

| Field | Value |
|-------|-------|
| Email | admin@investly.ly |
| Password | Admin@2024! |
| Role | admin |

Use these to log into the admin dashboard.

---

## How to Test in Swagger

1. Go to `http://localhost:5000/swagger`
2. Find `POST /api/auth/login-email`
3. Click **Try it out**
4. Enter:
   ```json
   {
     "email": "admin@investly.ly",
     "password": "Admin@2024!",
     "role": "admin"
   }
   ```
5. Click **Execute**
6. Copy the `token` value from the response
7. Click the **Authorize** button at the top of the Swagger page
8. Enter: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...` (your full token)
9. Click **Authorize** then close
10. All locked endpoints now work with your admin token

---

## How to Test with VS Code REST Client

1. Install the **REST Client** extension in VS Code (by Huachao Mao)
2. Open `docs/ENDPOINT_TESTS.http`
3. Run the **Login as Admin** request at the top
4. Copy the `token` from the response
5. Paste it into the `@token = ` variable at the top of the file
6. Click **Send Request** above any other request in the file

---

## Connecting the Mobile App

Open the mobile app's `.env` file:
```
مشروع التخرج/project_code/myApp/.env
```

Set:
```
EXPO_PUBLIC_API_BASE_URL=http://192.168.1.X:5000/api
EXPO_PUBLIC_USE_MOCK_API=false
```

**Important:** Replace `192.168.1.X` with your computer's local IP address.
The phone/emulator cannot reach `localhost` — it needs the actual IP.

Find your local IP:
```powershell
ipconfig
```
Look for **IPv4 Address** under your active network adapter (e.g., `192.168.1.45`).

---

## Connecting the Admin Dashboard

Open the dashboard's `.env.local` file:
```
مشروع التخرج/project_code/dashbord/.env.local
```

Set:
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

Dashboard runs at `http://localhost:3000`

---

## Stopping the API

Press `Ctrl + C` in the terminal where `dotnet run` is running.

---

## Common Errors & Fixes

### "Connection refused" or "database does not exist"
- Make sure PostgreSQL is running (check pgAdmin or Windows Services)
- Check the connection string in `appsettings.json` — verify the password and database name
- Make sure `InvestlyDB` database exists

### "Invalid token" on API calls
- Your access token expired (lasts 60 minutes)
- Log in again with `POST /api/auth/login-email` to get a fresh token
- Make sure the header format is exactly: `Authorization: Bearer eyJ...`

### "Not authorized to create projects"
- Only users with `role = "owner"` can create projects
- Register a new account with role `"owner"` to test project creation

### "Insufficient wallet balance"
- The investor has no money in their wallet
- As admin: call `POST /api/admin/users/{id}/wallet/add` with an amount to add funds
- As user: call `POST /api/investments/wallet/topup`

### "dotnet-ef not found"
- Run: `dotnet tool install --global dotnet-ef`
- Close and reopen your terminal

### "Port already in use"
- Another process is using port 5000
- Change the port in `Properties/launchSettings.json`:
  ```json
  "applicationUrl": "http://localhost:5001"
  ```
- Remember to update `.env` files in the frontends to use the new port

---

## Quick Command Reference

```powershell
# Start API
dotnet run

# Apply migrations manually (not needed — dotnet run does this automatically)
dotnet ef database update

# Create a new migration after model changes
dotnet ef migrations add MigrationName

# Restore NuGet packages
dotnet restore

# Build without running
dotnet build
```
