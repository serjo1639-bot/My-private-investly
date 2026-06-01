 # Investly — Mobile App (myApp) Endpoints

Base URL: `http://localhost:5000/api`  
Auth: Bearer JWT token required on all routes unless marked **Public**

---

## Auth

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/auth/register` | Public | Register new user — body: `{ fullName, phone, email, password, role }` |
| POST | `/auth/login` | Public | Login with phone + password |
| POST | `/auth/login-email` | Public | Login with email + password |
| POST | `/auth/send-otp` | Public | Send OTP to phone number — body: `{ phone }` |
| POST | `/auth/verify-otp` | Public | Verify OTP code — body: `{ phone, code }` |
| POST | `/auth/forgot-password` | Public | Send password reset code to email — body: `{ email }` |
| POST | `/auth/verify-reset-code` | Public | Verify the reset code — body: `{ email, code }` |
| POST | `/auth/reset-password` | Public | Reset password with verified code — body: `{ email, code, newPassword }` |
| POST | `/auth/refresh-token` | Public | Refresh access token — body: `{ refreshToken }` |
| POST | `/auth/logout` | Required | Logout — body: `{ refreshToken }` |
| POST | `/auth/change-password` | Required | Change password — body: `{ oldPassword, newPassword }` |
| GET | `/auth/profile` | Required | Get current user profile |
| PUT | `/auth/profile` | Required | Update current user profile |

---

## Projects

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/projects` | Public | List all projects — query: `category`, `search`, `status`, `page`, `pageSize` |
| GET | `/projects/featured` | Public | Get featured projects |
| GET | `/projects/categories` | Public | Get all categories |
| GET | `/projects/{id}` | Public | Get project details by ID |
| GET | `/projects/{id}/stats` | Public | Get project funding stats |
| POST | `/projects/{id}/views` | Public | Record a project view |
| POST | `/projects` | Required | Create a new project (owner) |
| PUT | `/projects/{id}` | Required | Update a project (owner) |

---

## Investments

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/investments/checkout` | Required | Invest in a project — body: `{ projectId, amount }` |
| GET | `/investments/me` | Required | Get my active investments |
| GET | `/investments/history` | Required | Get investment history |
| GET | `/investments/wallet` | Required | Get my wallet balance |
| POST | `/investments/wallet/topup` | Required | Top up wallet — body: `{ amount, method }` |
| POST | `/investments/wallet/withdraw` | Required | Withdraw from wallet — body: `{ amount, bankDetails }` |
| GET | `/investments/funding-options` | Required | Get available funding/payment options |
| POST | `/investments/topup/redeem` | Required | Redeem a top-up code — body: `{ code }` |

---

## Payments

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/payments/methods` | Public | Get available payment methods |
| POST | `/payments/initiate` | Required | Initiate a payment — body: `{ amount, method, ... }` |
| POST | `/payments/verify` | Required | Verify a payment — body: `{ paymentId, transactionId }` |
| GET | `/payments/history` | Required | Get my payment history |
| GET | `/payments/{paymentId}` | Required | Get a specific payment by ID |
| GET | `/payments/{paymentId}/status` | Required | Get payment status |
| POST | `/payments/{paymentId}/refund` | Required | Request a refund |

---

## Notifications

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/notifications` | Required | Get all my notifications |
| GET | `/notifications/unread-count` | Required | Get unread notification count |
| POST | `/notifications/{id}/read` | Required | Mark a notification as read |
| POST | `/notifications/read-all` | Required | Mark all notifications as read |
| GET | `/notifications/settings` | Required | Get notification preferences |
| PUT | `/notifications/settings` | Required | Update notification preferences |

---

## Users

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/users/{userId}` | Required | Get user details by ID |
| PUT | `/users/{userId}` | Required | Update own profile (self only) |
| DELETE | `/users/{userId}` | Required | Delete own account (self only) |
| POST | `/users/{userId}/kyc` | Required | Submit KYC documents — body: `{ documentType, documentUrl }` |
| GET | `/users/{userId}/documents` | Required | Get KYC documents |
| GET | `/users/{userId}/investments` | Required | Get user's investments |

---

## Owners (Project Owners)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/owners/{ownerId}/projects` | Required | Get all projects for an owner |
| GET | `/owners/{ownerId}/stats` | Required | Get owner summary stats |
| GET | `/owners/{ownerId}/dashboard` | Required | Get owner dashboard (projects + stats) |

---

## Media

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/media/upload` | Required | Upload file (max 10 MB) — form-data: `file` |
| DELETE | `/media/{mediaId}` | Required | Delete a media file (owner only) |

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

---

## Auth Header

```
Authorization: Bearer <access_token>
```
