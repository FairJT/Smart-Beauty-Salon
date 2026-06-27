# 03 — Add the component kit 🟢

Copy the provided `ui_kit.dart` into the project. It builds only on the existing
`AppColors` / `AppSpacing` / `textTheme` — no new design system.

**Create:** `smart_salon_app/lib/presentation/widgets/ui_kit.dart`
**Content:** use the `ui_kit.dart` shipped with these tasks, verbatim.

It exports: `Gap`, `SectionHeader`, `AppCard`, `AppListRow`, `StatusPill`, `InfoChip`,
`MetricCard`, `AppDivider`.

**Verify (PowerShell):**
```powershell
Test-Path smart_salon_app/lib/presentation/widgets/ui_kit.dart
cd smart_salon_app ; flutter analyze lib/presentation/widgets/ui_kit.dart
```

> If `withValues(alpha:)` is flagged (older Flutter), change it to `.withOpacity(...)` and
> re-run. Otherwise leave as-is.

**Done when:** the file analyzes clean. Tasks 04–05 start using it.
