# 10 — Booking requires login 🟡

Guests may VIEW a salon but must log in to book.

**1) Find the book button** (salon detail / booking entry):
```powershell
Select-String -Path smart_salon_app\lib\presentation\pages -Pattern "/booking|رزرو نوبت|onTap|onPressed" -Recurse
```
Pick the salon-detail "رزرو" button's `onTap`/`onPressed`.

**2) At the START of that callback add:**
```dart
if (!requireLogin(context, ref, reason: 'برای رزرو وارد شوید')) return;
```

**3) Add import** to that file:
```dart
import '../../core/auth_guard.dart';
```

**Done when:** as a guest, tapping "رزرو" shows the prompt and opens login; as a logged-in user it books normally.
