# 05 — Adopt the kit components 🟡

Replace hand-rolled rows and status chips with the shared kit so navigation lists and
appointment states look identical everywhere. One file per step; analyze after each.

`import '../../widgets/ui_kit.dart';` (adjust depth per file).

## 5a — Navigation / action rows → `AppListRow`
Anywhere a page builds a tappable row by hand (Container + Row + Icon + Text + chevron, or a
`ListTile` used for navigation), replace with:
```dart
AppListRow(
  icon: Icons.people_alt_outlined,
  title: 'مدیریت هنرمندان',
  subtitle: 'افزودن و ویرایش پرسنل',
  tint: AppColors.primary,          // use AppColors.accent or adminGold to vary
  onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ArtistManagementScreen())),
)
```
Targets: the manager dashboard "مدیریت سالن" card, client "دسترسی سریع" card, any settings lists.

## 5b — Appointment status chips → `StatusPill`
Replace inline status containers (the `Container` with `statusColor(...).withOpacity` + text)
with:
```dart
StatusPill(appointment.status)
```
Targets: `artist_schedule_screen.dart`, `appointment_list.dart`, dashboards' "نوبت بعدی".

## 5c — Titled groupings → `AppCard` / `SectionHeader`
Where a screen wraps content in `Card`+`Padding`+a title `Text`, use `AppCard(title: '…', child: …)`.
For a heading above ungrouped content, use `SectionHeader('…', actionLabel: 'مشاهده همه', onAction: …)`.

## Rules
- Don't change behavior or data — this is presentation only.
- `SummaryCard` / `StatGrid` already exist and are fine; leave them, or migrate to `AppCard`
  only if it reduces duplication. Don't churn working dashboards for no visual gain.
- If a row has custom trailing widgets (toggles, menus), pass them via `AppListRow(trailing: …)`.

**Verify (PowerShell):**
```powershell
cd smart_salon_app ; flutter analyze
```

**Done when:** every navigation list uses `AppListRow`, every appointment state uses
`StatusPill`, and the dashboards/management screens read as one consistent app.
