# 03 — Point main.dart at the REAL admin dashboard 🟡

**File:** `smart_salon_app/lib/main.dart`

Here the real class is named `AdminDashboard` (in `admin/admin_dashboard.dart`), while
the stub is `AdminDashboardScreen`. So we change **two** things: the import, and the
widget the route builds.

**1) Find (exact):**
```dart
import 'presentation/pages/generated/admin_dashboard_screen.dart';
```
**Replace with:**
```dart
import 'presentation/pages/admin/admin_dashboard.dart';
```

**2) Find (exact):**
```dart
          case '/admin-dashboard':
            return MaterialPageRoute(
                builder: (_) => const AdminDashboardScreen());
```
**Replace with:**
```dart
          case '/admin-dashboard':
            return MaterialPageRoute(
                builder: (_) => const AdminDashboard());
```

**Verify (PowerShell):**
```powershell
Select-String -Path smart_salon_app/lib/main.dart -Pattern "AdminDashboard\(\)"
# expect 1 match (const AdminDashboard()), and NO match for "AdminDashboardScreen"
```

**Done when:** logging in as SuperAdmin (userType 1) shows the real admin panel with the
**کاربران / سالن‌ها** tabs, live stats, and working toggle actions — not the static
`5 / 120 / 200k / 12` grid.
