# 04 — Create the Client dashboard screen 🟡

The Client has a provider (`clientDashboardProvider`), a model (`ClientDashboard`), and a
backend endpoint (`/api/dashboard/client`) — but **no screen renders them**. Create one,
mirroring the artist dashboard exactly so it compiles against the same shared widgets.

**Create new file:** `smart_salon_app/lib/presentation/pages/client/client_dashboard_screen.dart`

**With this exact content:**
```dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/app_colors.dart';
import '../../../core/format/jalaali_helper.dart';
import '../../providers/dashboard_provider.dart';
import '../../widgets/dashboard_widgets.dart';

class ClientDashboardScreen extends ConsumerStatefulWidget {
  const ClientDashboardScreen({super.key});

  @override
  ConsumerState<ClientDashboardScreen> createState() => _ClientDashboardScreenState();
}

class _ClientDashboardScreenState extends ConsumerState<ClientDashboardScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(clientDashboardProvider.notifier).load();
    });
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(clientDashboardProvider);
    return Scaffold(
      appBar: AppBar(
        title: const Text('سالن هوشمند', style: TextStyle(fontWeight: FontWeight.bold)),
      ),
      body: _buildBody(state),
    );
  }

  Widget _buildBody(ClientDashboardState state) {
    if (state.loading) return const LoadingState();
    if (state.error != null) {
      return ErrorState(
        message: state.error!,
        onRetry: () => ref.read(clientDashboardProvider.notifier).load(),
      );
    }
    final data = state.data;
    if (data == null) return const EmptyState(message: 'داده‌ای یافت نشد');

    return RefreshIndicator(
      onRefresh: () => ref.read(clientDashboardProvider.notifier).load(),
      child: ListView(
        padding: AppSpacing.pagePadding,
        children: [
          SummaryCard(
            title: 'خلاصه',
            child: StatGrid(tiles: [
              StatTileConfig(icon: Icons.event_available, label: 'نوبت‌های پیش رو', value: '${data.upcomingBookings}'),
              StatTileConfig(icon: Icons.history, label: 'مجموع مراجعات', value: '${data.totalVisits}'),
              StatTileConfig(icon: Icons.star, label: 'امتیاز وفاداری', value: '${data.loyaltyPoints}', iconColor: AppColors.warning, valueColor: AppColors.warning),
              StatTileConfig(icon: Icons.notifications_none, label: 'پیام‌های خوانده‌نشده', value: '${data.unreadNotifications}'),
            ]),
          ),
          if (data.nextBooking != null)
            SummaryCard(
              title: 'نوبت بعدی',
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  StatTile(icon: Icons.store, label: 'سالن', value: data.nextBooking!.salonName),
                  StatTile(icon: Icons.content_cut, label: 'خدمت', value: data.nextBooking!.serviceName),
                  StatTile(icon: Icons.person, label: 'هنرمند', value: data.nextBooking!.artistName),
                  StatTile(icon: Icons.access_time, label: 'زمان', value: JalaaliHelper.formatDateTime(data.nextBooking!.startTime)),
                ],
              ),
            ),
          if (data.favoriteSalons.isNotEmpty)
            SummaryCard(
              title: 'سالن‌های مورد علاقه',
              child: Column(
                children: data.favoriteSalons.map((f) => ListTile(
                  contentPadding: EdgeInsets.zero,
                  leading: const Icon(Icons.favorite, color: AppColors.danger),
                  title: Text(f.salonName, style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w500)),
                  trailing: Text(f.ratingAvg.toStringAsFixed(1), style: const TextStyle(color: AppColors.textSecondary, fontSize: 12)),
                )).toList(),
              ),
            ),
        ],
      ),
    );
  }
}
```

**Verify (PowerShell):**
```powershell
Test-Path smart_salon_app/lib/presentation/pages/client/client_dashboard_screen.dart
cd smart_salon_app ; flutter analyze lib/presentation/pages/client/client_dashboard_screen.dart
```

> If `JalaaliHelper.formatDateTime` or any widget name does not resolve, STOP and report —
> do not invent replacements. (These are the same names the artist dashboard imports.)

**Done when:** the file analyzes clean. Routing it in is the next task.
