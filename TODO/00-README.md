# Agent Tasks — Auth / Guest / Font (atomic)

Each file = ONE small change. Do in order. If a "Find" isn't found exactly, STOP and report.

### Remove OTP
- `01-otp-main-route.md` — drop `/otp` route + import from main.dart
- `02-otp-guest-booking.md` — drop OTP from guest_booking_screen
- `03-otp-skeletons.md` — (optional) drop OTP class from skeletons
- `04-otp-delete-file.md` — delete otp_screen.dart

### Fix login routing
- `05-role-router-helper.md` — create role_router.dart
- `06-login-routing.md` — login_screen routes by role
- `07-register-routing.md` — register_screen routes by role
- `08-splash-routing.md` — splash resumes session by role

### Guest restriction
- `09-auth-guard-helper.md` — create auth_guard.dart
- `10-guard-book-button.md` — booking needs login
- `11-guard-nav-tabs.md` — رزروها/پروفایل need login

### Font + Persian
- `12-pubspec-font.md` — IRANSans in pubspec
- `13-font-code.md` — Vazirmatn → IRANSans in code
- `14-rtl-alignment.md` — RTL-aware alignment
- `15-no-english.md` — translate English UI text

After all: `flutter pub get` → `flutter analyze` → run & log in as each role.
