# 06 — Manager dashboard: add quick actions (un-orphan staff management) 🟡

The manager dashboard is read-only. `ArtistManagementScreen` (full staff add/edit screen)
already exists but nothing navigates to it. Add a "quick actions" card with a working
button to it. (Catalog and Finance buttons get added in tasks 09 and 10, once those
screens exist — do **not** add them here.)

**File:** `smart_salon_app/lib/presentation/pages/manager/manager_dashboard_screen.dart`

**1) Add import** at the top, next to the other relative imports:
```dart
import 'artist_management_screen.dart';
```

**2) Find (exact):**
```dart
          SummaryCard(
            title: 'اشتراک',
```
**Replace with:**
```dart
          SummaryCard(
            title: 'مدیریت سالن',
            child: Column(
              children: [
                ListTile(
                  contentPadding: EdgeInsets.zero,
                  leading: const Icon(Icons.people_alt_outlined, color: AppColors.primary),
                  title: const Text('مدیریت هنرمندان', style: TextStyle(fontSize: 14)),
                  trailing: const Icon(Icons.chevron_left, color: AppColors.textSecondary),
                  onTap: () => Navigator.push(
                    context,
                    MaterialPageRoute(builder: (_) => const ArtistManagementScreen()),
                  ),
                ),
              ],
            ),
          ),
          SummaryCard(
            title: 'اشتراک',
```

**Verify (PowerShell):**
```powershell
Select-String -Path smart_salon_app/lib/presentation/pages/manager/manager_dashboard_screen.dart -Pattern "ArtistManagementScreen"
# expect 2 matches (import + Navigator)
```

**Done when:** the manager dashboard shows a "مدیریت سالن" card whose "مدیریت هنرمندان"
row opens the staff management screen.
