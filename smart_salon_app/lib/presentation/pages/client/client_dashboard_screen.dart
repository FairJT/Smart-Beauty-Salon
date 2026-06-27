import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/app_colors.dart';
import '../../../core/format/jalaali_helper.dart';
import '../../providers/dashboard_provider.dart';
import '../../widgets/dashboard_widgets.dart';
import '../client_home_screen.dart';

class ClientDashboardScreen extends ConsumerStatefulWidget {
  const ClientDashboardScreen({super.key});

  @override
  ConsumerState<ClientDashboardScreen> createState() =>
      _ClientDashboardScreenState();
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
        title: const Text('سالن هوشمند',
            style: TextStyle(fontWeight: FontWeight.bold)),
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
              StatTileConfig(
                  icon: Icons.event_available,
                  label: 'نوبت‌های پیش رو',
                  value: '${data.upcomingBookings}'),
              StatTileConfig(
                  icon: Icons.history,
                  label: 'مجموع مراجعات',
                  value: '${data.totalVisits}'),
              StatTileConfig(
                  icon: Icons.star,
                  label: 'امتیاز وفاداری',
                  value: '${data.loyaltyPoints}',
                  iconColor: AppColors.warning,
                  valueColor: AppColors.warning),
              StatTileConfig(
                  icon: Icons.notifications_none,
                  label: 'پیام‌های خوانده‌نشده',
                  value: '${data.unreadNotifications}'),
            ]),
          ),
          SummaryCard(
            title: 'دسترسی سریع',
            child: Column(
              children: [
                ListTile(
                  contentPadding: EdgeInsets.zero,
                  leading: const Icon(Icons.event_available,
                      color: AppColors.primary),
                  title:
                      const Text('نوبت‌های من', style: TextStyle(fontSize: 14)),
                  trailing: const Icon(Icons.chevron_left,
                      color: AppColors.textSecondary),
                  onTap: () => Navigator.pushNamed(context, '/my-appointments'),
                ),
                ListTile(
                  contentPadding: EdgeInsets.zero,
                  leading: const Icon(Icons.favorite_outline,
                      color: AppColors.danger),
                  title: const Text('سالن‌های مورد علاقه',
                      style: TextStyle(fontSize: 14)),
                  trailing: const Icon(Icons.chevron_left,
                      color: AppColors.textSecondary),
                  onTap: () => Navigator.push(
                    context,
                    MaterialPageRoute(builder: (_) => const ClientHomeScreen()),
                  ),
                ),
              ],
            ),
          ),
          if (data.nextBooking != null)
            SummaryCard(
              title: 'نوبت بعدی',
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  StatTile(
                      icon: Icons.store,
                      label: 'سالن',
                      value: data.nextBooking!.salonName),
                  StatTile(
                      icon: Icons.content_cut,
                      label: 'خدمت',
                      value: data.nextBooking!.serviceName),
                  StatTile(
                      icon: Icons.person,
                      label: 'هنرمند',
                      value: data.nextBooking!.artistName),
                  StatTile(
                      icon: Icons.access_time,
                      label: 'زمان',
                      value: JalaaliHelper.formatDateTime(
                          data.nextBooking!.startTime)),
                ],
              ),
            ),
          if (data.favoriteSalons.isNotEmpty)
            SummaryCard(
              title: 'سالن‌های مورد علاقه',
              child: Column(
                children: data.favoriteSalons
                    .map((f) => ListTile(
                          contentPadding: EdgeInsets.zero,
                          leading: const Icon(Icons.favorite,
                              color: AppColors.danger),
                          title: Text(f.salonName,
                              style: const TextStyle(
                                  fontSize: 13, fontWeight: FontWeight.w500)),
                          trailing: Text(f.ratingAvg.toStringAsFixed(1),
                              style: const TextStyle(
                                  color: AppColors.textSecondary,
                                  fontSize: 12)),
                        ))
                    .toList(),
              ),
            ),
        ],
      ),
    );
  }
}
