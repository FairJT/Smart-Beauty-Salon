# 01 — Remove /otp route from main.dart 🟡

**File:** `smart_salon_app/lib/main.dart`

**Find (exact) and DELETE:**
```dart
          case '/otp':
            return MaterialPageRoute(builder: (_) => const OtpScreen());
```

**Then DELETE the import line:**
```dart
import 'presentation/pages/otp_screen.dart';
```

**Done when:** no `/otp` case and no `otp_screen` import remain in main.dart.
