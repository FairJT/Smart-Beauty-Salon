# 02 — Remove OTP from guest_booking_screen 🟡

**File:** `smart_salon_app/lib/presentation/pages/guest_booking_screen.dart`

**1) DELETE the import:**
```dart
import 'otp_screen.dart';
```

**2) Find (exact):**
```dart
          builder: (_) => OtpScreen(phoneNumber: _phoneController.text.trim()),
```
**Replace with:**
```dart
          builder: (_) => throw UnimplementedError(), // OTP removed
```
> If the above line is inside a `MaterialPageRoute(...)`, instead replace the whole navigation
> with: `Navigator.of(context).pushReplacementNamed('/login');` (remove the MaterialPageRoute).

**3) Find (exact):**
```dart
                MaterialPageRoute(builder: (_) => OtpScreen(phoneNumber: _phoneController.text.isNotEmpty ? _phoneController.text.trim() : '')),
```
**Replace the enclosing `Navigator.push(... )` with:**
```dart
                Navigator.of(context).pushReplacementNamed('/login');
```
(guests must log in to book — see task 10).

**Done when:** `Select-String guest_booking_screen.dart -Pattern "Otp"` → 0 hits, and `flutter analyze` is clean.
