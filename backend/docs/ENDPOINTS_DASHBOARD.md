# Investly — Dashboard (Admin Panel) Endpoints

Base URL: `http://localhost:5000/api`  
Auth: Bearer JWT token required on all routes unless marked **Public**  
Role: All `/admin/*` routes require **admin** role

---

## Auth

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/auth/login-email` | Public | Admin login with email + password |
| POST | `/auth/logout` | Required | Logout (revoke refresh token) |
| POST | `/auth/refresh-token` | Public | Refresh access token |
| POST | `/auth/change-password` | Required | Change own password |
| GET | `/auth/profile` | Required | Get own profile |
| PUT | `/auth/profile` | Required | Update own profile |

---

## Dashboard Stats

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/admin/stats` | Admin | Platform overview — returns `TotalUsers`, `ActiveUsers`, `TotalProjects`, `TotalInvestments`, `TotalRevenue`, `ActiveProjects`, `PendingProjects`, `CompletedProjects`, `NewUsersToday`, `NewUsersThisWeek`, `TotalTransactions`, `SuccessRate` |
| GET | `/admin/chart-data` | Admin | Chart data — `UserGrowth` (monthly Investors+Owners), `Revenue` (monthly), `ProjectStatus` (counts per status) |
| GET | `/admin/recent-activity` | Admin | Recent activity feed — query: `count` (default 10), returns `{ Id, Type, UserName, Action, ProjectTitle, Amount, Date, Status }` |
| GET | `/admin/activity-logs` | Admin | Full paginated admin action log — query: `adminId`, `page`, `pageSize` |

---

## Users Management

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/admin/users` | Admin | List all users — query: `search`, `status`, `kycStatus`, `page`, `pageSize` |
| POST | `/admin/users/{userId}/ban` | Admin | Permanently ban a user |
| POST | `/admin/users/{userId}/suspend` | Admin | Suspend user — body: `{ reason }` |
| POST | `/admin/users/{userId}/unsuspend` | Admin | Unsuspend a user |
| POST | `/admin/users/{userId}/kyc/approve` | Admin | Approve KYC submission |
| POST | `/admin/users/{userId}/kyc/reject` | Admin | Reject KYC — body: `{ reason }` |
| POST | `/admin/users/{userId}/wallet/add` | Admin | Add funds to user wallet — body: `{ amount, reason }` |

---

## Projects Management

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/admin/projects` | Admin | List all projects — query: `status`, `page`, `pageSize` |
| POST | `/admin/projects/{id}/approve` | Admin | Approve a pending project |
| POST | `/admin/projects/{id}/reject` | Admin | Reject a project — body: `{ reason }` |
| PATCH | `/admin/projects/{id}/featured` | Admin | Toggle featured flag — body: `{ isFeatured }` |

---

## Investments Management

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/admin/investments` | Admin | List all investments — query: `status`, `userId`, `projectId`, `page`, `pageSize` |

---

## Payments Management

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/admin/payments` | Admin | List all payments — query: `status`, `page`, `pageSize` |
| POST | `/admin/payments/{id}/approve` | Admin | Approve a payment |
| POST | `/admin/payments/{id}/reject` | Admin | Reject a payment — body: `{ reason }` |
| POST | `/admin/payments/{id}/refund` | Admin | Process a refund |
| PUT | `/admin/payments/{id}/status` | Admin | Manually update payment status — body: `{ status }` |

---

## Wallets Management

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/admin/wallets` | Admin | List all wallets — query: `search` |
| POST | `/admin/wallet/transfer` | Admin | Transfer funds between wallets — body: `{ fromUserId, toUserId, amount, reason }` |

---

## Notifications

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/admin/notifications/send` | Admin | Send notification to user(s) — body: `{ userId?, title, message, type }` |

---

## Response Format

All endpoints return:
```json
{
  "success": true,
  "message": "...",
  "data": { ... }
}
```

Errors return with appropriate HTTP status code and `"success": false`.
