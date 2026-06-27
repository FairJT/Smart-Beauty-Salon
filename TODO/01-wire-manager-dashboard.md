# 01 — Point main.dart at the REAL manager dashboard 🟡

**File:** `smart_salon_app/lib/main.dart`

The real `ManagerDashboardScreen` (provider-wired) and the fake stub have the **same
class name**, so we only change the import path. No other edit is needed — the route
`case '/manager-dashboard'` already builds `const ManagerDashboardScreen()`.

**Find (exact):**
```dart
import 'presentation/pages/generated/manager_dashboard_screen.dart';
```
**Replace with:**
```dart
import 'presentation/pages/manager/manager_dashboard_screen.dart';
```

**Verify (PowerShell):**
```powershell
Select-String -Path smart_salon_app/lib/main.dart -Pattern "pages/manager/manager_dashboard_screen"
# expect exactly 1 match, and NO match for "generated/manager_dashboard_screen"
```

**Done when:** logging in as a SalonManager (userType 2) shows the dashboard with live
"خلاصه امروز" stats from the API instead of the fixed `12 / 350 / 120k` numbers.
