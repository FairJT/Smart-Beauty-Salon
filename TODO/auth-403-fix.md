# Fix: revert wrong baseUrl + grant platform owner access (403) — Agent TODO

**For:** a free / local AI coding agent (Continue.dev + deepseek-coder) on Windows / PowerShell
**Context:** Login WORKS. Two issues remain:
  1. The earlier `baseUrl: ''` change in `dio_client.dart` is WRONG. nginx strips one `/api`
     (`proxy_pass http://salonos-api:5016/;`), so the client MUST send `/api/api/...` (double) to reach
     `:5016/api/...`. With `baseUrl: ''` the client sends single `/api/...` → nginx → `:5016/...` → 404.
     The running container still has the old (correct, double) build, which is why login works now — but
     rebuilding with the current source would break it. Revert it.
  2. SuperAdmin (platform owner) gets 403 on `/api/admin/*` (stats/users/salons). The `PermissionHandler`
     only succeeds on a `permission` claim, but a SuperAdmin token carries `is_platform_owner=true` and
     NO permission claims → forbidden. The handler must also let platform owners through.
**Generated:** 2026-06-18

Flags: 🟢 safe · 🟡 agent does it, human reviews · 🔴 human-only / hand-off.

---

### A1 — Revert the Dio baseUrl 🟡
**File:** `smart_salon_app/lib/data/datasources/dio_client.dart`
**Find:**
```dart
      baseUrl: '',
```
**Replace with:**
```dart
      baseUrl: ApiConstants.baseUrl,
```
**Done when:** the Dio `baseUrl` is `ApiConstants.baseUrl` again (so requests are `/api/api/...`, which
this nginx maps to `:5016/api/...`). Verify: `Select-String -Path smart_salon_app\lib\data\datasources\dio_client.dart -Pattern "baseUrl:"`.

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
**Done when:** the handler succeeds when the token has `is_platform_owner=true` OR the required permission claim.

### C1 — Rebuild both containers 🟢
```powershell
cd D:\PR\Smart-Beauty-Salon-Claude-v1
docker compose build --no-cache flutter-web salonos-api
docker compose up -d --force-recreate flutter-web salonos-api
docker compose ps
```
**Done when:** both `flutter-web` and `salonos-api` are Up; report build errors if any.

### C2 — HAND OFF: human verifies 🔴
**Agent stops.** Post to the human and wait:
> Open http://localhost:5080, hard-reload (Ctrl+Shift+R), log in as SuperAdmin (09110000001 / Test@1234).
> 1. Does login still work?
> 2. On the admin dashboard, do the counts (revenue / appointments / salons / users) now load instead
>    of all showing 0? In DevTools → Network, is `admin/stats` now 200 (not 403)?
**Done when:** the human confirms stats load (200) or reports the new status/error.

### C3 — Commit (after C2 confirms) 🟡
```powershell
git add smart_salon_app/lib/data/datasources/dio_client.dart src/SalonOS.Shared/Authorization/PermissionHandler.cs
git commit -m "fix: revert dio baseUrl to match nginx; allow platform owner through permission checks"
git push
```

## Order
A1 → B1 → C1 → **C2 (hand off, stop)** → C3.
