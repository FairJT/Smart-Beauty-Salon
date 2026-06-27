# 09 — Create the guest guard helper 🟢

**New file:** `smart_salon_app/lib/core/auth_guard.dart`
```dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../presentation/providers/auth_provider.dart';

/// True if the user may proceed. If a guest, prompts + sends to login, returns false.
bool requireLogin(BuildContext context, WidgetRef ref, {String? reason}) {
  if (ref.read(authProvider).isLoggedIn) return true;
  ScaffoldMessenger.of(context).showSnackBar(
    SnackBar(content: Text(reason ?? 'برای این کار باید وارد شوید')),
  );
  Navigator.of(context).pushNamed('/login');
  return false;
}
```
> If the `auth_provider` import path differs, fix it. The screen using this must be a `Consumer*`
> widget (have a `ref`).

**Done when:** the file compiles.
