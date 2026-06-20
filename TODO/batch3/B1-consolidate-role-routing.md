# Task B1 — Remove the inconsistent role gate from home_screen 🟡 (review after)

`splash_screen` and `login_screen` already route each role to its own dashboard. But
`home_screen` has its OWN role gate that disagrees (Artist → `ArtistScheduleScreen` instead of
`ArtistDashboardScreen`; Manager → generic screen instead of `ManagerDashboardScreen`). Since
`home_screen` is really just the "browse salons" screen, remove its role gate so there's ONE
source of truth for routing.

---

## Step 1 — remove the role-gate blocks

**File:** `smart_salon_app/lib/presentation/pages/home_screen.dart`

**Find (exact):**
```dart
    final auth = ref.watch(authProvider);

    if (auth.isSuperAdmin) {
      return const AdminDashboard();
    }

    if (auth.isArtist) {
      return const ArtistScheduleScreen();
    }

    final salonState = ref.watch(salonListProvider);
```

**Replace with:**
```dart
    final auth = ref.watch(authProvider);
    final salonState = ref.watch(salonListProvider);
```

---

## Step 2 — remove the now-unused imports

**File:** same.
**Delete these two import lines:**
```dart
import 'admin/admin_dashboard.dart';
import 'artist/artist_schedule_screen.dart';
```
(Keep `import 'manager/artist_management_screen.dart';` — the manager button still uses it.)

**Done when:** `home_screen` no longer references `AdminDashboard` or `ArtistScheduleScreen`,
and `flutter analyze` reports no unused-import / undefined-name errors for this file.

**⚠️ Human review:** confirm `auth` is still used in the file (it is — the manager button uses
`auth.isSalonManager`). If `flutter analyze` flags `auth` as unused, tell Claude.
