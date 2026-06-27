# 04 — Migrate inline text styles to the type scale 🟡

Pages hand-write `TextStyle(fontSize: …, fontWeight: …)` ~158 times. Replace them with the
existing `textTheme` so every screen shares one scale. Work **one page file at a time**, run
`flutter analyze` after each, and report the file when done. Do NOT bulk-edit all pages at once.

## Mapping (inline → theme)
Inside a widget with a `BuildContext context`, use `Theme.of(context).textTheme`:

| Inline style | Replace with |
|---|---|
| `fontSize: 22, bold` | `textTheme.headlineLarge` |
| `fontSize: 18, w600` | `textTheme.headlineMedium` |
| `fontSize: 16, w600` | `textTheme.titleLarge` |
| `fontSize: 16, normal` | `textTheme.bodyLarge` |
| `fontSize: 14, w600` | `textTheme.titleMedium` |
| `fontSize: 14, normal` | `textTheme.bodyMedium` |
| `fontSize: 13` (label/secondary) | `textTheme.bodySmall` + `color: AppColors.textSecondary` |
| `fontSize: 12` | `textTheme.bodySmall` |
| `fontSize: 10/11` | `textTheme.labelSmall` |

To tweak only color/weight, use `.copyWith(...)`:
```dart
Text('درآمد', style: Theme.of(context).textTheme.titleLarge?.copyWith(color: AppColors.success))
```

## Rules
- Keep colored values (success/danger/warning) — only the size/weight comes from the theme.
- Do NOT touch numbers that need tabular alignment if they already render fine; the kit's
  `MetricCard` handles those.
- If a style doesn't map cleanly, leave it and note it in your report — don't force it.

## Suggested order (one per step)
1. `presentation/pages/login_screen.dart`
2. `presentation/pages/register_screen.dart`
3. `presentation/pages/manager/manager_dashboard_screen.dart`
4. `presentation/pages/artist/artist_dashboard_screen.dart`
5. `presentation/pages/client/client_dashboard_screen.dart` (if present)
6. `presentation/pages/manager/artist_management_screen.dart`
7. `presentation/pages/artist/artist_schedule_screen.dart`
8. remaining files under `presentation/pages/`

**Verify per file (PowerShell):**
```powershell
cd smart_salon_app ; flutter analyze
```

**Done when:** `Select-String -Path smart_salon_app/lib/presentation/pages -Recurse -Pattern "fontSize:"`
returns only intentional exceptions (e.g. inside the kit), not scattered across every page.
