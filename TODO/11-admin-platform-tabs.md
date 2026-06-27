# 11 — SuperAdmin: tenants / billing / platform tabs 🔴 (review before commit)

The real admin dashboard (`admin/admin_dashboard.dart`) already has **کاربران** and
**سالن‌ها** tabs with working actions. It's missing the platform-owner duties from the
access-control matrix. Extend it — don't rewrite it.

**Backend already exists:**
- `PlatformAdminController` (`/admin/...`, 10 endpoints) — users, salons, and more.
- `PlatformAccountingController` — platform billing / accounting.
- `TenantController` (`/tenant`) — tenant management.
- `Marketplace/.../ServiceTemplateController` — `marketplace.template.manage` (admin-only).

**Edit:** `smart_salon_app/lib/presentation/pages/admin/admin_dashboard.dart`
- Bump the `TabController(length: 2 ...)` to `length: 3` (or 4) and add tab(s):
  - "اشتراک‌ها / صورتحساب" — list tenants + subscription status, suspend/reactivate,
    backed by `PlatformAccountingController` / `TenantController`.
  - (optional) "قالب‌های مارکت" — manage marketplace service templates.
- Add the matching `_build…Tab(state)` method(s) and the data loads in `initState`
  (follow the existing `loadUsers()` / `loadSalons()` pattern in `admin_provider.dart`;
  add `loadTenants()` etc. to the provider).

**Guardrails:**
- This is the **only** scope allowed to cross tenants. Every cross-tenant call goes through
  the platform admin endpoints — never by passing a tenantId into a normal salon endpoint.
- Keep the existing two tabs and their toggle actions intact (rule R6: add, don't break).

**Done when:** SuperAdmin can view tenants and their billing/subscription state and act on
them from a new tab, alongside the existing user and salon tabs.

---

## Final check after all tasks
```powershell
cd smart_salon_app
flutter pub get
flutter analyze
```
Run the app and log in as each of the 4 roles. Each should land on a real, data-backed
dashboard and be able to reach its core duties.
