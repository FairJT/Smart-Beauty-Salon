# 06 — Login routes by role 🟡

**File:** `smart_salon_app/lib/presentation/pages/login_screen.dart`

**1) Find (exact):**
```dart
    if (success && mounted) {
      Navigator.of(context).pushReplacementNamed('/home');
    } else if (mounted) {
```
**Replace with:**
```dart
    if (success && mounted) {
      final ut = ref.read(authProvider).user?.userType ?? 4;
      Navigator.of(context).pushReplacementNamed(roleHome(ut));
    } else if (mounted) {
```

**2) Add import** near the other imports at the top:
```dart
import '../../core/role_router.dart';
```

> Do NOT touch `_guestBrowse()` — guests stay on `/home`.

**Done when:** logging in as a manager/artist/admin lands on their dashboard.
