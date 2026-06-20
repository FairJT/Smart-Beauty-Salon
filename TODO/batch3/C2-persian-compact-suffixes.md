# Task C2 — Use Persian magnitude words in compact format 🟢

The compact format mixes Persian digits with English letters (`۱.۵M تومان`). Use Persian words.

**File:** `smart_salon_app/lib/core/format/money_formatter.dart`

**Find (exact):**
```dart
    if (toman >= 1000000000) {
      return '${_toPersianDigits((toman / 1000000000).toStringAsFixed(1))}B $label';
    } else if (toman >= 1000000) {
      return '${_toPersianDigits((toman / 1000000).toStringAsFixed(1))}M $label';
    } else if (toman >= 1000) {
      return '${_toPersianDigits((toman / 1000).toStringAsFixed(1))}K $label';
    }
```

**Replace with:**
```dart
    if (toman >= 1000000000) {
      return '${_toPersianDigits((toman / 1000000000).toStringAsFixed(1))} میلیارد $label';
    } else if (toman >= 1000000) {
      return '${_toPersianDigits((toman / 1000000).toStringAsFixed(1))} میلیون $label';
    } else if (toman >= 1000) {
      return '${_toPersianDigits((toman / 1000).toStringAsFixed(1))} هزار $label';
    }
```

**Done when:** compact amounts read like `۱.۵ میلیون تومان`.
