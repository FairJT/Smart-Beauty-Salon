# Fix: unify /api routing (nginx + clients) + admin 403 — Agent TODO

**For:** a free / local AI coding agent (Continue.dev + deepseek-coder) on Windows / PowerShell
**Supersedes:** auth-403-fix.md (its "revert baseUrl to /api" step was WRONG — it leaves the dashboards
broken. Use THIS file instead.)

## Root cause (systemic)
The app has TWO HTTP clients with inconsistent `/api` handling, and nginx strips one `/api`:
- nginx: `proxy_pass http://salonos-api:5016/;` (trailing slash) → strips the `/api/` prefix.
- `DioClient` (baseUrl = `/api`) → sends `/api/api/...` → nginx strips one → `:5016/api/...` → WORKS.
- `ApiService` (no base) → sends `/api/...` → nginx strips → `:5016/...` → 404.

So everything on DioClient works (admin only has a 403 permission issue), but everything on
`ApiService` — the **manager/artist/client/platform dashboards** and **favorites** — returns 404.

## The fix (make it ALL single `/api`, and make nginx preserve `/api`)
1. nginx: stop stripping `/api` (remove the trailing slash) → `/api/x` reaches `:5016/api/x`.
2. `DioClient.baseUrl` must be `''` (so it sends single `/api/...`, not double). It should already be `''`.
3. `ApiService` already sends single `/api/...` → no change needed; it just works once nginx preserves.
4. Backend `PermissionHandler`: let platform owners (SuperAdmin) pass any permission check.
**Generated:** 2026-06-18

Flags: 🟢 safe · 🟡 agent does it, human reviews · 🔴 human-only / hand-off.

---

### A1 — Make nginx preserve `/api` (remove the trailing slash) 🟡
**File:** `nginx.conf`
**Find (inside `location /api/ {`):**
```nginx
        proxy_pass http://salonos-api:5016/;
```
**Replace with:**
```nginx
        proxy_pass http://salonos-api:5016;
```
**Why:** with no trailing slash / URI, nginx forwards the full path unchanged, so `/api/auth/login`
reaches `:5016/api/auth/login` (instead of `:5016/auth/login`). Do NOT change the `/auth/` or
`/swagger/` location blocks.
**Done when:** the `/api/` block's `proxy_pass` has no trailing slash. (Leave the other locations.)

### A2 — Confirm DioClient sends single `/api` (baseUrl empty) 🟡
**File:** `smart_salon_app/lib/data/datasources/dio_client.dart`
**Required state:** the line must be:
```dart
      baseUrl: '',
```
If it currently says `baseUrl: ApiConstants.baseUrl,`, change it to `baseUrl: '',`.
**Why:** combined with A1, every request is a single `/api/...` that nginx forwards as-is.
**Done when:** `baseUrl: '',`. Verify: `Select-String -Path smart_salon_app\lib\data\datasources\dio_client.dart -Pattern "baseUrl:"`.

### B1 — Let platform owners pass any permission check 🟡
**File:** `src/SalonOS.Shared/Authorization/PermissionHandler.cs`
**Find:**
```csharp
        if (context.User.HasClaim("permission", requirement.Permission))
            context.Succeed(requirement);
```
**Replace with:**
```csharp
        if (context.User.HasClaim("is_platform_owner", "true") ||
            context.User.HasClaim("permission", requirement.Permission))
            context.Succeed(requirement);
```
**Done when:** the handler succeeds for `is_platform_owner=true` OR the required permission claim.

### C1 — Rebuild both containers 🟢
nginx.conf and the Dart build are both baked into the flutter-web image, and the handler into the API.
```powershell
cd D:\PR\Smart-Beauty-Salon-Claude-v1
docker compose build --no-cache flutter-web salonos-api
docker compose up -d --force-recreate flutter-web salonos-api
docker compose ps
```
**Done when:** both are Up; report build errors if any.

### C2 — HAND OFF: human verifies all the parts 🔴
**Agent stops.** Post to the human and wait:
> Open http://localhost:5080, hard-reload (Ctrl+Shift+R). In DevTools → Network, every `/api/...` request
> should now be a SINGLE `/api/` (not `/api/api/`) and return 200.
> 1. Log in as SuperAdmin (09110000001 / Test@1234) → admin dashboard counts load (not all 0)? `admin/stats` = 200?
> 2. Log in as SalonManager (09110000002 / Test@1234) → manager dashboard loads? `dashboard/manager` = 200?
> 3. Log in as Client (09110000004 / Test@1234) → client dashboard / favorites load?
> Report any request still returning 404, 403, or 500 (with its URL + status).
**Done when:** the human confirms dashboards load, or reports the failing URLs/status.

### C3 — Commit (after C2 confirms) 🟡
```powershell
git add nginx.conf smart_salon_app/lib/data/datasources/dio_client.dart src/SalonOS.Shared/Authorization/PermissionHandler.cs
git commit -m "fix: nginx preserves /api; single-/api on all clients; platform owner passes permission checks"
git push
```

## Order
A1 → A2 → B1 → C1 → **C2 (hand off, stop)** → C3.

## Still to audit after this (report, don't fix yet)
Once routing is consistent, watch the Network tab for any endpoint returning **500** — that would point to
a response-shape / field-name mismatch in that feature's parsing (the same class of bug as the earlier
`phoneNumber` vs `mobile`). Likely suspects to eyeball: salon list, appointments, services. List any 500s
with the endpoint so they can be fixed one by one.
