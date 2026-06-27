# Agent Tasks — UI polish (classic / tidy)

Your foundation is already good: one central `ThemeData`, a shared `dashboard_widgets`,
and pages use `AppColors` (zero hardcoded colors). So this is **polish, not a rebuild**.
The goal is consistency: kill the ~158 ad-hoc `TextStyle`s and the hand-rolled rows/pills,
and route every page through one type scale + a small component kit.

**Brand is unchanged** — navy `#1B3A5C`, gold for admin/finance, rose accent. We only make
the system around it tidier.

## Delegation legend
- 🟢 trivial / safe — agent does it unattended
- 🟡 mechanical but exact — agent does it, you skim the diff
- 🔴 needs judgment — agent scaffolds, you review

## Order
- `01-soften-hairline.md` 🟡 — one-token change: lighter, classic card/divider borders
- `02-theme-gaps.md` 🟡 — add divider / tab / listTile / chip themes so widgets inherit
- `03-add-ui-kit.md` 🟢 — drop in `ui_kit.dart` (SectionHeader, AppCard, AppListRow, StatusPill, InfoChip, MetricCard)
- `04-text-scale-migration.md` 🟡 — replace inline `TextStyle(fontSize: …)` with the existing `textTheme`
- `05-adopt-components.md` 🟡 — swap hand-rolled rows/status chips for `AppListRow` / `StatusPill`

Files `app_theme.dart` (reference) and `ui_kit.dart` (drop-in) ship alongside these tasks.

## After each task
```powershell
cd smart_salon_app ; flutter analyze
```
After 03–05, run the app and eyeball the manager/artist/client dashboards and the
management screens — they should look like one app, not five.
