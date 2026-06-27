# 02 — Point main.dart at the REAL artist dashboard 🟡

**File:** `smart_salon_app/lib/main.dart`

Same situation as task 01: real and stub `ArtistDashboardScreen` share a class name, so
only the import path changes. Route `case '/artist-dashboard'` already builds
`const ArtistDashboardScreen()`.

**Find (exact):**
```dart
import 'presentation/pages/generated/artist_dashboard_screen.dart';
```
**Replace with:**
```dart
import 'presentation/pages/artist/artist_dashboard_screen.dart';
```

**Verify (PowerShell):**
```powershell
Select-String -Path smart_salon_app/lib/main.dart -Pattern "pages/artist/artist_dashboard_screen"
# expect exactly 1 match, and NO match for "generated/artist_dashboard_screen"
```

**Done when:** logging in as an Artist (userType 3) shows live "خلاصه امروز" /
"نوبت بعدی" / "آمار ماه" from the API instead of the fixed `5 / 2 / 10` numbers.
