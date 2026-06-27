# 07 — Register routes by role 🟡

**File:** `smart_salon_app/lib/presentation/pages/register_screen.dart`

**Find (exact):**
```dart
    Navigator.of(context).pushReplacementNamed('/home');
```
**Replace with:**
```dart
    final ut = ref.read(authProvider).user?.userType ?? 4;
    Navigator.of(context).pushReplacementNamed(roleHome(ut));
```

**Add import** at the top:
```dart
import '../../core/role_router.dart';
```
> If `register_screen` isn't a `Consumer*` widget (no `ref`), use
> `ref` from its existing auth call; if there's truly no `ref`, keep `'/home'` and report.

**Done when:** registering routes correctly (new users are clients → `/home`).
