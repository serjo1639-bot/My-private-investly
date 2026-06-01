# How the Admin Dashboard Controls the Mobile App (myApp)

This document explains every action the admin dashboard can take and exactly what effect it has on users in the mobile app.

---

## The Core Relationship

```
Admin Dashboard                 Backend API                   Mobile App (myApp)
     |                               |                               |
     |  Admin clicks "Approve"       |                               |
     |------------------------------>|                               |
     |  POST /admin/projects/{id}/approve                           |
     |                               |  project.status = "active"   |
     |                               |----------------------------->|
     |                               |                               |  Project now appears
     |                               |                               |  in browse screen
```

Both the admin dashboard and the mobile app talk to the SAME backend. The dashboard changes data in the database; the mobile app reads that same data. That is how the dashboard "controls" what users see in the app.

---

## 1. Project Approval / Rejection

### What admin does:
**Dashboard → Projects → Pending → Approve or Reject**

| Admin Action | API Call | Effect in myApp |
|---|---|---|
| Approve project | `POST /admin/projects/{id}/approve` | Project `status` changes to `active`. It now appears when investors browse `/projects`. |
| Reject project | `POST /admin/projects/{id}/reject` | Project `status` changes to `rejected`. It never shows to investors. The owner sees "rejected" in their dashboard. |
| Mark as Featured | `PATCH /admin/projects/{id}/featured` | Project appears in the **featured carousel** at the top of the home screen (`GET /projects/featured`). |

### What the app does:
- `GET /projects` — filters out everything that is NOT `status = active`
- `GET /projects/featured` — returns only projects where `is_featured = true AND status = active`
- An owner who created a project sees its status in `/owners/{id}/projects` — they can see if it is `pending`, `active`, or `rejected`

---

## 2. User Account Control

### What admin does:
**Dashboard → Users → Select user → Ban / Suspend / Unsuspend**

| Admin Action | API Call | Effect in myApp |
|---|---|---|
| Ban user | `POST /admin/users/{id}/ban` | `user.status = banned`. The banned user's next login attempt returns 401. Their active sessions do NOT immediately die — but on token refresh, they will be blocked. |
| Suspend user | `POST /admin/users/{id}/suspend` | `user.status = suspended`. Same behaviour as ban — next login is blocked. |
| Unsuspend user | `POST /admin/users/{id}/unsuspend` | `user.status = active`. User can log in again. |

### What the app does:
- `POST /auth/login-email` and `POST /auth/login` check `user.status` before issuing a token
- Banned/suspended users receive: `{ "success": false, "message": "Your account has been suspended." }`
- The app shows the error message and keeps the user on the login screen

---

## 3. KYC Approval / Rejection

### What admin does:
**Dashboard → Users → Select user → View KYC Documents → Approve or Reject**

| Admin Action | API Call | Effect in myApp |
|---|---|---|
| Approve KYC | `POST /admin/users/{id}/kyc/approve` | `user.kyc_status = approved`. User's profile now shows "verified" badge. |
| Reject KYC | `POST /admin/users/{id}/kyc/reject` | `user.kyc_status = rejected`. User is asked to re-submit documents. |

### What the app does:
- KYC status is returned on every profile call (`GET /auth/profile`)
- The app uses `kycStatus` to display: a green "Verified" badge, a "Pending" badge, or a "Please complete KYC" prompt
- Certain features (e.g. large investments or withdrawals) may be blocked for users without `kycStatus = approved`

### Flow:
```
Investor opens app → goes to Profile → uploads passport image via POST /media/upload
    → calls POST /users/{id}/kyc with the returned image URL
        → kycStatus becomes "pending"
            → Admin sees pending KYC in dashboard
                → Admin reviews & approves → kycStatus becomes "approved"
                    → App shows green "Verified" badge
```

---

## 4. Wallet & Fund Management

### What admin does:
**Dashboard → Users → Select user → Add Funds**
**Dashboard → Wallets → Transfer**

| Admin Action | API Call | Effect in myApp |
|---|---|---|
| Add funds to a user's wallet | `POST /admin/users/{id}/wallet/add` | Increases `wallet.balance`. User sees updated balance on their wallet screen. |
| Transfer between wallets | `POST /admin/wallet/transfer` | Moves balance from one user to another. Both users see updated balances. |

### What the app does:
- `GET /investments/wallet` — returns the user's current `wallet.balance`
- The wallet screen refreshes balance on focus/pull-to-refresh
- After admin adds funds, the user immediately sees the new balance next time they check

---

## 5. Payment Control

### What admin does:
**Dashboard → Payments → Select payment → Approve / Reject / Refund**

| Admin Action | API Call | Effect in myApp |
|---|---|---|
| Approve payment | `POST /admin/payments/{id}/approve` | `payment.status = completed`. Investment becomes fully confirmed. |
| Reject payment | `POST /admin/payments/{id}/reject` | `payment.status = failed`. Investment is cancelled. |
| Refund payment | `POST /admin/payments/{id}/refund` | `payment.status = refunded`. Money is returned to user's wallet. |
| Update status manually | `PUT /admin/payments/{id}/status` | Override payment status to any value. |

### What the app does:
- `GET /payments/history` — user sees updated payment status
- `GET /investments/me` — investment status reflects payment outcome
- If refunded, wallet balance increases automatically

---

## 6. Notifications — Direct Control Over App Content

### What admin does:
**Dashboard → Notifications → Send**

```json
POST /api/admin/notifications/send
{
  "userId": null,           // null = broadcast to ALL users
  "titleAr": "تحديث مهم",
  "titleEn": "Important Update",
  "messageAr": "تم إطلاق مشاريع جديدة",
  "messageEn": "New projects have been launched",
  "type": "system"
}
```

| Scenario | `userId` field | Effect |
|---|---|---|
| Send to one user | Specific user UUID | Only that user sees the notification |
| Broadcast to all | `null` | Every user sees it in their notifications list |
| Alert about a project | Any user or null | Appears in notifications with type `"project"` |

### What the app does:
- `GET /notifications` — returns notifications targeted to this user (or broadcast to all)
- `GET /notifications/unread-count` — returns the badge number shown on the bell icon
- The bell icon badge count updates whenever the user opens the app
- User can mark individual or all notifications as read

---

## 7. Platform Statistics — Dashboard Reads App Data

The dashboard reads aggregated data about what is happening in the app:

```
GET /admin/stats
```

Returns:
- Total users (by role: investors, owners)
- Total projects (by status: pending, active, completed, rejected)
- Total investments made
- Total revenue collected
- Recent activity

This gives the admin a live view of all activity happening in the mobile app.

---

## 8. Activity Logs — Admin Audit Trail

```
GET /admin/activity-logs
```

Every admin action (approve, reject, ban, etc.) is logged. The dashboard displays a full audit trail so you can see which admin did what and when.

---

## Summary: Dashboard → API → App Flow

```
DASHBOARD ACTION          API ENDPOINT                     APP RESULT
─────────────────────────────────────────────────────────────────────
Approve project     → POST /admin/projects/{id}/approve  → Project visible in browse
Reject project      → POST /admin/projects/{id}/reject   → Project hidden from browse
Feature project     → PATCH /admin/projects/{id}/featured → Project in home carousel
Approve KYC         → POST /admin/users/{id}/kyc/approve  → Green "Verified" badge
Reject KYC          → POST /admin/users/{id}/kyc/reject   → "Re-submit" prompt shown
Ban user            → POST /admin/users/{id}/ban          → User blocked from login
Suspend user        → POST /admin/users/{id}/suspend      → User blocked from login
Unsuspend user      → POST /admin/users/{id}/unsuspend    → User can login again
Add wallet funds    → POST /admin/users/{id}/wallet/add   → Balance updated in app
Transfer funds      → POST /admin/wallet/transfer         → Both wallets updated
Approve payment     → POST /admin/payments/{id}/approve   → Payment shows "completed"
Refund payment      → POST /admin/payments/{id}/refund    → Balance credited to wallet
Send notification   → POST /admin/notifications/send      → Notification + badge in app
```

---

## Key Point: Role Separation

The **dashboard** logs in with `role = admin`.
The **mobile app** users log in with `role = investor` or `role = owner`.

The backend checks the role on every request:
- Admin endpoints (`/admin/*`) reject any token that is not `role = admin`
- The dashboard cannot accidentally be used as an investor account
- The mobile app cannot accidentally call admin endpoints (it would get 403 Forbidden)
