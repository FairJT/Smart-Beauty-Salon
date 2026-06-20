# Task B2 — Fix wrong API paths 🟢

These three constants point to routes the backend doesn't have (they'd 404). Correct them to
the real backend routes. (They're currently unused, so this is safe — but it stops future 404s.)

**File:** `smart_salon_app/lib/data/datasources/api_constants.dart`

**Find (exact):**
```dart
  static const String catalogServices = '$baseUrl/api/catalog/services';
  static const String inventory = '$baseUrl/api/inventory';
  static const String marketplace = '$baseUrl/api/marketplace';
```

**Replace with:**
```dart
  static const String catalogServices = '$baseUrl/api/catalog-services';
  static const String inventory = '$baseUrl/api/inventory-items';
  static const String serviceTemplates = '$baseUrl/api/service-templates';
  static const String packageListings = '$baseUrl/api/package-listings';
```
(`marketplace` is dropped — the backend has no single marketplace route; it's `service-templates`
and `package-listings`.)

**Done when:** the constants match real backend routes.

**Verify (PowerShell):** make sure nothing referenced the dropped `marketplace`:
```powershell
Select-String -Path smart_salon_app\lib -Pattern "ApiConstants.marketplace" -Recurse
```
Expect **0 hits**. If there ARE hits, STOP and report — don't guess a replacement.
