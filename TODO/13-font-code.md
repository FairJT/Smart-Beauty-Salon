# 13 — Switch font in code: Vazirmatn → IRANSans 🟢

Replace every `'Vazirmatn'` string with `'IRANSans'` in these two files:
- `smart_salon_app/lib/main.dart` (2 places: `fontFamily` and `fontFamilyFallback`)
- `smart_salon_app/lib/core/app_colors.dart` (~13 places in `AppTextTheme`)

i.e. find `'Vazirmatn'` → replace with `'IRANSans'` (all occurrences).

**Verify:** `Select-String -Path smart_salon_app\lib -Pattern "Vazirmatn" -Recurse` → 0 hits.
**Done when:** the app renders in IRANSans.
