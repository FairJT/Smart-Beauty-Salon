# 12 — Register IRANSans in pubspec 🟡

First put the font files in `smart_salon_app/assets/fonts/`:
`IRANSansX-Regular.ttf` and `IRANSansX-Bold.ttf` (you supply these `.ttf` files).

**File:** `smart_salon_app/pubspec.yaml`
**Find (exact):**
```yaml
  fonts:
    - family: Vazirmatn
      fonts:
        - asset: assets/fonts/Vazirmatn-Regular.ttf
        - asset: assets/fonts/Vazirmatn-Bold.ttf
          weight: 700
```
**Replace with:**
```yaml
  fonts:
    - family: IRANSans
      fonts:
        - asset: assets/fonts/IRANSansX-Regular.ttf
        - asset: assets/fonts/IRANSansX-Bold.ttf
          weight: 700
```
Then run `flutter pub get`.

**Done when:** pubspec lists IRANSans and `pub get` succeeds (font files present).
