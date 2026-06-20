# Task C1 — Fix the misleading money comment 🟢

The comment claims "ریال" and `150000 → 150,000 ریال`, but the code divides by 10 and prints
"تومان". The code is fine; the comment is wrong.

**File:** `smart_salon_app/lib/core/format/money_formatter.dart`

**Find (exact):**
```dart
  /// Formats an integer amount in minor units (Rials) to a display string.
  /// 150000 → "۱۵۰,۰۰۰ ریال"
  static String format(int amount, {String currency = 'IRR'}) {
```

**Replace with:**
```dart
  /// Formats an integer Rial amount to a Toman display string (Rials ÷ 10).
  /// 150000 (Rials) → "۱۵,۰۰۰ تومان"
  static String format(int amount, {String currency = 'IRR'}) {
```

**Done when:** the comment matches the actual Toman output.
