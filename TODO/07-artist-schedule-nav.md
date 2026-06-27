# 07 — Artist dashboard: add "my schedule" action (un-orphan schedule) 🟡

`ArtistScheduleScreen` already has the confirm / complete appointment buttons wired to the
API — but nothing opens it. Add a button on the artist dashboard.

**File:** `smart_salon_app/lib/presentation/pages/artist/artist_dashboard_screen.dart`

**1) Add import** at the top, next to the other relative imports:
```dart
import 'artist_schedule_screen.dart';
```

**2) Find (exact):**
```dart
          SummaryCard(
            title: 'آمار ماه',
```
**Replace with:**
```dart
          SummaryCard(
            title: 'برنامه کاری',
            child: ListTile(
              contentPadding: EdgeInsets.zero,
              leading: const Icon(Icons.event_note_outlined, color: AppColors.primary),
              title: const Text('برنامه و نوبت‌های من', style: TextStyle(fontSize: 14)),
              subtitle: const Text('تأیید و تکمیل نوبت‌ها', style: TextStyle(fontSize: 12, color: AppColors.textSecondary)),
              trailing: const Icon(Icons.chevron_left, color: AppColors.textSecondary),
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const ArtistScheduleScreen()),
              ),
            ),
          ),
          SummaryCard(
            title: 'آمار ماه',
```

**Verify (PowerShell):**
```powershell
Select-String -Path smart_salon_app/lib/presentation/pages/artist/artist_dashboard_screen.dart -Pattern "ArtistScheduleScreen"
# expect 2 matches (import + Navigator)
```

**Done when:** the artist dashboard has a "برنامه کاری" card that opens the schedule,
where the artist can confirm and complete their own appointments.
