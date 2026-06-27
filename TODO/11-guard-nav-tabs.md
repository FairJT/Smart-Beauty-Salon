# 11 — Guest can't open رزروها / پروفایل 🟡

**File:** the client home with the bottom nav (`home_screen.dart` or `client_home_screen.dart`).

When a guest taps the **رزروها** (appointments) or **پروفایل** (profile) tabs, guard them:
in the bottom-nav `onTap`, before switching to those tabs:
```dart
if ((index == /* رزروها */ 1 || index == /* پروفایل */ 3) &&
    !requireLogin(context, ref, reason: 'برای دیدن این بخش وارد شوید')) {
  return;
}
```
(Adjust the indexes to match the actual tab order. The **خانه** (browse) tab stays open to guests.)

**Add import:**
```dart
import '../../core/auth_guard.dart';
```

**Done when:** a guest can browse on خانه but رزروها/پروفایل send them to login.
