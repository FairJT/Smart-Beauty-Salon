# 01 — Soften the hairline border (classic, tidy) 🟡

The card/divider border is currently black @ 10% (`0x1A000000`), which reads a touch heavy.
A lighter, neutral hairline is the single biggest "tidy" win because every Card, Divider,
and input border uses it.

**File:** `smart_salon_app/lib/core/app_colors.dart`

**Find (exact):**
```dart
  static const Color border = Color(0x1A000000);
```
**Replace with:**
```dart
  static const Color border = Color(0xFFECEDEF); // hairline
  static const Color borderStrong = Color(0xFFDDE0E4); // dividers under headers
```

**Verify (PowerShell):**
```powershell
Select-String -Path smart_salon_app/lib/core/app_colors.dart -Pattern "ECEDEF"
cd smart_salon_app ; flutter analyze
```

> If any screen layered `AppColors.border` over a dark/colored background expecting it to be
> translucent, it will now look slightly lighter — that's intended. If something looks wrong,
> report it rather than reverting the token.

**Done when:** cards and dividers show a clean light hairline instead of a grey-black line.
