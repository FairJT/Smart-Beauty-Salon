# 02 — Fill the theme gaps (so widgets inherit, not hand-style) 🟡

Add a few missing theme entries to the central theme. Once these exist, Dividers, TabBars,
ListTiles and Chips look uniform everywhere with no per-page styling.

**File:** `smart_salon_app/lib/main.dart`

**Find (exact):**
```dart
        snackBarTheme: const SnackBarThemeData(
          behavior: SnackBarBehavior.floating,
          shape: RoundedRectangleBorder(
            borderRadius: AppSpacing.borderRadiusMd,
          ),
        ),
      ),
```
**Replace with:**
```dart
        snackBarTheme: const SnackBarThemeData(
          behavior: SnackBarBehavior.floating,
          shape: RoundedRectangleBorder(
            borderRadius: AppSpacing.borderRadiusMd,
          ),
        ),
        dividerTheme: const DividerThemeData(
          color: AppColors.border,
          thickness: 1,
          space: 1,
        ),
        listTileTheme: const ListTileThemeData(
          iconColor: AppColors.textSecondary,
          contentPadding: EdgeInsets.symmetric(horizontal: AppSpacing.xs),
        ),
        tabBarTheme: const TabBarThemeData(
          labelColor: AppColors.primary,
          unselectedLabelColor: AppColors.textMuted,
          indicatorColor: AppColors.primary,
          dividerColor: AppColors.border,
          labelStyle: TextStyle(fontWeight: FontWeight.w600, fontSize: 14),
        ),
        chipTheme: ChipThemeData(
          backgroundColor: AppColors.background,
          side: const BorderSide(color: AppColors.border),
          labelStyle: const TextStyle(fontSize: 12, color: AppColors.textSecondary),
          shape: RoundedRectangleBorder(borderRadius: AppSpacing.borderRadiusSm),
          padding: const EdgeInsets.symmetric(horizontal: AppSpacing.xs, vertical: 4),
        ),
      ),
```

> Note: in some Flutter versions the class is `TabBarTheme` (not `TabBarThemeData`). If
> `flutter analyze` complains, change `TabBarThemeData(` → `TabBarTheme(` and re-run.

**Verify (PowerShell):**
```powershell
Select-String -Path smart_salon_app/lib/main.dart -Pattern "dividerTheme|tabBarTheme|chipTheme"
cd smart_salon_app ; flutter analyze
```

**Done when:** analyze passes and the admin dashboard tabs + in-card dividers look uniform.
