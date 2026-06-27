# 08 — Client dashboard: add quick actions (bookings + favorites) 🟡

Give the Client buttons to manage their bookings and open favorites
(`ClientHomeScreen`, currently orphaned). Edit the file created in task 04.

**File:** `smart_salon_app/lib/presentation/pages/client/client_dashboard_screen.dart`
> Requires task 04 done first.

**1) Add import** at the top, after the existing imports:
```dart
import '../client_home_screen.dart';
```

**2) Find (exact):**
```dart
          if (data.nextBooking != null)
```
**Replace with:**
```dart
          SummaryCard(
            title: 'دسترسی سریع',
            child: Column(
              children: [
                ListTile(
                  contentPadding: EdgeInsets.zero,
                  leading: const Icon(Icons.event_available, color: AppColors.primary),
                  title: const Text('نوبت‌های من', style: TextStyle(fontSize: 14)),
                  trailing: const Icon(Icons.chevron_left, color: AppColors.textSecondary),
                  onTap: () => Navigator.pushNamed(context, '/my-appointments'),
                ),
                ListTile(
                  contentPadding: EdgeInsets.zero,
                  leading: const Icon(Icons.favorite_outline, color: AppColors.danger),
                  title: const Text('سالن‌های مورد علاقه', style: TextStyle(fontSize: 14)),
                  trailing: const Icon(Icons.chevron_left, color: AppColors.textSecondary),
                  onTap: () => Navigator.push(
                    context,
                    MaterialPageRoute(builder: (_) => const ClientHomeScreen()),
                  ),
                ),
              ],
            ),
          ),
          if (data.nextBooking != null)
```

**Verify (PowerShell):**
```powershell
Select-String -Path smart_salon_app/lib/presentation/pages/client/client_dashboard_screen.dart -Pattern "ClientHomeScreen|/my-appointments"
# expect at least 2 matches
```

**Done when:** the Client dashboard has a "دسترسی سریع" card linking to their appointments
and their favorite salons.
