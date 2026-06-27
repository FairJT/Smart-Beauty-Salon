# 05 — Route the Client to their dashboard 🟡

Two small edits: register the route in `main.dart`, then send Client (userType 4) there.

### 5.1 — Add the route
**File:** `smart_salon_app/lib/main.dart`

**Add import** next to the other dashboard imports (after the admin dashboard import):
```dart
import 'presentation/pages/client/client_dashboard_screen.dart';
```

**Find (exact):**
```dart
          case '/admin-dashboard':
            return MaterialPageRoute(
                builder: (_) => const AdminDashboard());
```
**Replace with:**
```dart
          case '/admin-dashboard':
            return MaterialPageRoute(
                builder: (_) => const AdminDashboard());
          case '/client-dashboard':
            return MaterialPageRoute(
                builder: (_) => const ClientDashboardScreen());
```
> This assumes task 03 is done (so `AdminDashboard` is the anchor). If task 03 is NOT done,
> STOP and do it first.

### 5.2 — Send Client there after login
**File:** `smart_salon_app/lib/core/role_router.dart`

**Find (exact):**
```dart
    case 3:
      return '/artist-dashboard';
    default:
      return '/home';
```
**Replace with:**
```dart
    case 3:
      return '/artist-dashboard';
    case 4:
      return '/client-dashboard';
    default:
      return '/home';
```

**Verify (PowerShell):**
```powershell
Select-String -Path smart_salon_app/lib/main.dart -Pattern "/client-dashboard"
Select-String -Path smart_salon_app/lib/core/role_router.dart -Pattern "case 4:"
```

**Done when:** logging in as a Client lands on the new dashboard with their loyalty points,
upcoming bookings, next booking, and favorite salons.
