# 08 — Splash resumes a logged-in session 🟡

Today splash always goes to `/login`. Make it route a logged-in user to their dashboard.

**File:** `smart_salon_app/lib/presentation/pages/splash_screen.dart`

**1) Add imports** at the top (after the existing imports):
```dart
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/role_router.dart';
import '../providers/auth_provider.dart';
```

**2) Find (exact):**
```dart
class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen> {
```
**Replace with:**
```dart
class SplashScreen extends ConsumerStatefulWidget {
  const SplashScreen({super.key});

  @override
  ConsumerState<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends ConsumerState<SplashScreen> {
```

**3) Find (exact):**
```dart
    await Future.delayed(const Duration(seconds: 2));
    if (!mounted) return;
    Navigator.of(context).pushReplacementNamed('/login');
```
**Replace with:**
```dart
    await Future.delayed(const Duration(seconds: 2));
    if (!mounted) return;
    final auth = ref.read(authProvider);
    if (auth.isLoggedIn && auth.user != null) {
      Navigator.of(context).pushReplacementNamed(roleHome(auth.user!.userType));
    } else {
      Navigator.of(context).pushReplacementNamed('/login');
    }
```

**Done when:** reopening the app while logged in skips `/login` and lands on the right dashboard.
