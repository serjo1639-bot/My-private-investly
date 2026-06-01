# All API Endpoints — Complete Reference

**Base URL:** `http://localhost:5000/api`

**Legend:**
- No badge = Public (no token needed)
- 🔒 = Requires `Authorization: Bearer {token}` header
- 👑 = Admin role required (`role = "admin"` in token)

**Total: 81 endpoints**

---

## Auth — 13 endpoints

`POST /api/auth/login-email` — Login with email + password — **Public**
```json
Body: { "email": "user@example.com", "password": "Pass123!", "role": "investor" }
```

`POST /api/auth/login` — Login with phone + password — **Public**
```json
Body: { "phone": "+218910000000", "password": "Pass123!", "role": "investor" }
```

`POST /api/auth/register` — Create new account — **Public**
```json
Body: {
  "name": "Ahmed Ali", "phone": "+218910000001", "email": "ahmed@example.com",
  "role": "investor", "password": "Pass123!", "age": 28,
  "gender": "male", "location": "Tripoli", "termsAccepted": true
}
```

`POST /api/auth/send-otp` — Send OTP code to phone — **Public**
```json
Body: { "phone": "+218910000001", "purpose": "login" }
```

`POST /api/auth/verify-otp` — Verify OTP and login — **Public**
```json
Body: { "phone": "+218910000001", "code": "123456" }
```

`POST /api/auth/forgot-password` — Send password reset code to email — **Public**
```json
Body: { "email": "ahmed@example.com" }
```

`POST /api/auth/verify-reset-code` — Validate the reset code — **Public**
```json
Body: { "email": "ahmed@example.com", "code": "123456" }
```

`POST /api/auth/reset-password` — Reset password using the code — **Public**
```json
Body: { "email": "ahmed@example.com", "code": "123456", "newPassword": "NewPass123!" }
```

`POST /api/auth/refresh-token` — Get a new access token — **Public**
```json
Body: { "refreshToken": "your-refresh-token-here" }
```

`POST /api/auth/logout` — Revoke refresh token — 🔒
```json
Body: { "refreshToken": "your-refresh-token-here" }
```

`POST /api/auth/change-password` — Change current password — 🔒
```json
Body: { "oldPassword": "OldPass123!", "newPassword": "NewPass456!" }
```

`GET /api/auth/profile` — Get current user's profile — 🔒

`PUT /api/auth/profile` — Update current user's profile — 🔒
```json
Body: { "name": "Updated Name", "location": "Benghazi", "age": 30 }
```

---

## Projects — 8 endpoints

`GET /api/projects/featured` — Get featured projects for the home screen carousel — **Public**

`GET /api/projects` — Get all projects with optional filters — **Public**
```
Query params: category=tech | search=startup | status=active | page=1 | pageSize=10
```

`GET /api/projects/categories` — Get all project categories — **Public**

`GET /api/projects/{id}` — Get single project details — **Public**

`GET /api/projects/{id}/stats` — Get project funding statistics — **Public**
```
Returns: goal, raised, progress %, investor count, avg/min/max investment
```

`POST /api/projects/{id}/views` — Increment project view counter — **Public**

`POST /api/projects` — Create a new project — 🔒 (owner role)
```json
Body: {
  "titleAr": "مشروع تقني", "titleEn": "Tech Project",
  "descriptionAr": "وصف", "descriptionEn": "Description",
  "category": "tech", "cityAr": "طرابلس", "cityEn": "Tripoli",
  "goal": 100000, "minInvestment": 500, "maxInvestment": 50000,
  "currencyCode": "LYD", "duration": 12,
  "startDate": "2026-06-01", "endDate": "2027-06-01",
  "founderName": "Sara", "founderEmail": "sara@email.com", "founderPhone": "+218910000002"
}
```

`PUT /api/projects/{id}` — Update a project — 🔒

---

## Owners — 3 endpoints

`GET /api/owners/{ownerId}/projects` — Get all projects belonging to an owner — 🔒

`GET /api/owners/{ownerId}/stats` — Get owner statistics (total raised, investor count, etc.) — 🔒

`GET /api/owners/{ownerId}/dashboard` — Get full owner dashboard (projects + stats combined) — 🔒

---

## Investments — 8 endpoints

`POST /api/investments/checkout` — Invest in one or more projects — 🔒
```json
Body: {
  "currency": "LYD",
  "contributions": [
    { "projectId": "uuid", "amount": 1000, "currency": "LYD", "paymentMethod": "wallet" }
  ]
}
```

`GET /api/investments/me` — Get my current active investments — 🔒

`GET /api/investments/history` — Get my full investment history — 🔒

`GET /api/investments/wallet` — Get my wallet balance and details — 🔒

`POST /api/investments/wallet/topup` — Add money to my wallet — 🔒
```json
Body: { "amount": 5000, "method": "credit_card" }
```

`POST /api/investments/wallet/withdraw` — Withdraw from my wallet — 🔒
```json
Body: { "amount": 1000, "bankDetails": { "bankName": "Bank", "accountNumber": "123" } }
```

`GET /api/investments/funding-options` — Get available payment methods — 🔒

`POST /api/investments/topup/redeem` — Redeem a top-up code — 🔒
```json
Body: { "code": "TOPUP-XXXX-YYYY" }
```

---

## Payments — 8 endpoints

`GET /api/payments/methods` — Get available payment methods — **Public**

`POST /api/payments/initiate` — Start a payment — 🔒
```json
Body: { "amount": 2000, "method": "credit_card", "currency": "LYD" }
```

`POST /api/payments/verify` — Verify a payment with a transaction ID — 🔒
```json
Body: { "paymentId": "uuid", "transactionId": "TXN-12345" }
```

`GET /api/payments/history` — Get my payment history — 🔒

`GET /api/payments/{id}` — Get a single payment by ID — 🔒

`GET /api/payments/{id}/status` — Get payment status — 🔒

`POST /api/payments/{id}/refund` — Request a refund for a payment — 🔒

`GET /api/payments/wallet` — Get wallet info (convenience endpoint) — 🔒

---

## Users — 7 endpoints

`GET /api/users/{userId}` — Get user profile — 🔒

`PUT /api/users/{userId}` — Update own profile — 🔒
```json
Body: { "name": "New Name", "location": "Tripoli", "age": 29 }
```

`DELETE /api/users/{userId}` — Delete own account — 🔒

`POST /api/users/{userId}/kyc` — Submit KYC document — 🔒
```json
Body: { "documentType": "passport", "documentUrl": "http://localhost:5000/uploads/passport.jpg" }
```

`GET /api/users/{userId}/documents` — Get user's KYC documents — 🔒

`GET /api/users/{userId}/investments` — Get user's investments — 🔒

---

## Notifications — 6 endpoints

`GET /api/notifications` — Get all my notifications — 🔒

`GET /api/notifications/unread-count` — Get unread notification count (for badge) — 🔒

`POST /api/notifications/{id}/read` — Mark a single notification as read — 🔒

`POST /api/notifications/read-all` — Mark all notifications as read — 🔒

`GET /api/notifications/settings` — Get my notification preferences — 🔒

`PUT /api/notifications/settings` — Update my notification preferences — 🔒
```json
Body: { "investmentAlerts": true, "projectUpdates": true, "systemMessages": true, "emailNotifications": false }
```

---

## Media — 2 endpoints

`POST /api/media/upload` — Upload a file — 🔒
```
Content-Type: multipart/form-data
Field name: file
Max size: 10 MB
Allowed types: jpeg, png, webp, pdf
Returns: { "url": "http://localhost:5000/uploads/abc123.jpg" }
```

`DELETE /api/media/{mediaId}` — Delete a file — 🔒

---

## Admin — 24 endpoints 👑 (all require admin role)

### Dashboard & Logs

`GET /api/admin/stats` — Platform overview statistics 👑
```
Returns: TotalUsers, ActiveUsers, TotalProjects, TotalInvestments, TotalRevenue,
         ActiveProjects, PendingProjects, CompletedProjects,
         NewUsersToday, NewUsersThisWeek, TotalTransactions, SuccessRate
```

`GET /api/admin/chart-data` — Chart data for dashboard graphs 👑
```
Returns:
  UserGrowth  — array of { Month, Investors, Owners } (monthly user sign-ups by role)
  Revenue     — array of { Month, Revenue, Investments } (monthly revenue figures)
  ProjectStatus — { Active, Pending, Completed, Inactive, Rejected } counts
```

`GET /api/admin/recent-activity` — Recent platform activity feed 👑
```
Query: count (default 10)
Returns: array of { Id, Type, UserName, Action, ProjectTitle, Amount, Date, Status }
```

`GET /api/admin/activity-logs` — Full paginated admin action log 👑
```
Query: adminId | page | pageSize
```

### User Management

`GET /api/admin/users` — List all users 👑
```
Query: search | status | kycStatus | page | pageSize
```

`PUT /api/admin/users/{id}` — Update a user 👑

`POST /api/admin/users/{id}/ban` — Permanently ban a user 👑

`POST /api/admin/users/{id}/suspend` — Temporarily suspend a user 👑
```json
Body: { "reason": "Violation of terms" }
```

`POST /api/admin/users/{id}/unsuspend` — Lift a suspension 👑

`POST /api/admin/users/{id}/kyc/approve` — Approve a user's KYC documents 👑

`POST /api/admin/users/{id}/kyc/reject` — Reject KYC with a reason 👑
```json
Body: { "reason": "Document is blurry. Please re-submit." }
```

`POST /api/admin/users/{id}/wallet/add` — Add funds to a user's wallet 👑
```json
Body: { "amount": 10000, "reason": "Promotional bonus" }
```

### Project Management

`GET /api/admin/projects` — List all projects 👑
```
Query: status | page | pageSize
```

`POST /api/admin/projects/{id}/approve` — Approve a pending project 👑

`POST /api/admin/projects/{id}/reject` — Reject a project 👑
```json
Body: { "reason": "Insufficient documentation." }
```

`PATCH /api/admin/projects/{id}/featured` — Toggle featured status 👑
```json
Body: { "isFeatured": true }
```

### Investment Management

`GET /api/admin/investments` — List all investments 👑
```
Query: status | userId | projectId | page | pageSize
```

### Payment Management

`GET /api/admin/payments` — List all payments 👑
```
Query: status | page | pageSize
```

`POST /api/admin/payments/{id}/approve` — Approve a payment 👑

`POST /api/admin/payments/{id}/reject` — Reject a payment 👑
```json
Body: { "reason": "Verification failed." }
```

`POST /api/admin/payments/{id}/refund` — Refund a payment 👑

`PUT /api/admin/payments/{id}/status` — Update payment status manually 👑
```json
Body: { "status": "completed" }
```

### Wallet Management

`GET /api/admin/wallets` — List all wallets 👑
```
Query: search
```

`POST /api/admin/wallet/transfer` — Transfer funds between two wallets 👑
```json
Body: { "fromUserId": "uuid", "toUserId": "uuid", "amount": 500, "reason": "Manual correction" }
```

### Notifications

`POST /api/admin/notifications/send` — Send notification to one user or broadcast to all 👑
```json
// To one user:
Body: { "userId": "uuid", "titleAr": "إشعار", "titleEn": "Notice", "messageAr": "رسالة", "messageEn": "Message", "type": "system" }

// Broadcast to all (set userId to null):
Body: { "userId": null, "titleAr": "للجميع", "titleEn": "For Everyone", "messageAr": "رسالة", "messageEn": "Message", "type": "system" }
```

---

## Standard Response Format

Every endpoint returns the same JSON structure:

**Success:**
```json
{
  "success": true,
  "message": "Login successful.",
  "data": { ... }
}
```

**Error:**
```json
{
  "success": false,
  "message": "Project not found.",
  "errors": ["optional list of field errors"]
}
```

## HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success (GET, PUT) |
| 201 | Created (POST) |
| 400 | Bad request (validation error, business rule violated) |
| 401 | Unauthorized (missing or expired token) |
| 403 | Forbidden (wrong role) |
| 404 | Not found |
| 500 | Server error |
