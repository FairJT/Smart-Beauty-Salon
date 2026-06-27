import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/app_colors.dart';
import '../../core/auth_guard.dart';
import '../../core/format/jalaali_helper.dart';
import '../providers/dashboard_provider.dart';
import '../providers/favorites_provider.dart';
import '../widgets/dashboard_widgets.dart';
import 'generated/home_screen.dart';
import 'profile_screen.dart';
import 'appointment_list.dart';
import 'salon_detail_screen.dart';

class ClientHomeScreen extends ConsumerStatefulWidget {
  const ClientHomeScreen({super.key});

  @override
  ConsumerState<ClientHomeScreen> createState() => _ClientHomeScreenState();
}

class _ClientHomeScreenState extends ConsumerState<ClientHomeScreen> {
  int _currentTab = 0;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(clientDashboardProvider.notifier).load();
      ref.read(favoritesProvider.notifier).load();
    });
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(clientDashboardProvider);
    final favState = ref.watch(favoritesProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('سالن هوشمند',
            style: TextStyle(fontWeight: FontWeight.bold)),
        actions: [
          IconButton(
            icon: const Icon(Icons.store),
            onPressed: () => Navigator.push(
                context, MaterialPageRoute(builder: (_) => const HomeScreen())),
          ),
        ],
      ),
      body: _currentTab == 0
          ? _buildDashboard(state)
          : _currentTab == 1
              ? const AppointmentList()
              : _currentTab == 2
                  ? _buildFavorites(favState)
                  : const ProfileScreen(),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _currentTab,
        onDestinationSelected: (i) {
          if ((i == 1 || i == 3) &&
              !requireLogin(context, ref,
                  reason: 'برای دیدن این بخش وارد شوید')) {
            return;
          }
          setState(() => _currentTab = i);
        },
        destinations: const [
          NavigationDestination(
              icon: Icon(Icons.dashboard_outlined),
              selectedIcon: Icon(Icons.dashboard),
              label: 'خانه'),
          NavigationDestination(
              icon: Icon(Icons.calendar_month_outlined),
              selectedIcon: Icon(Icons.calendar_month),
              label: 'رزروها'),
          NavigationDestination(
              icon: Icon(Icons.favorite_outline),
              selectedIcon: Icon(Icons.favorite),
              label: 'علاقه‌مندی‌ها'),
          NavigationDestination(
              icon: Icon(Icons.person_outline),
              selectedIcon: Icon(Icons.person),
              label: 'پروفایل'),
        ],
      ),
    );
  }

  Widget _buildFavorites(FavoritesState state) {
    if (state.loading) return const LoadingState();
    if (state.error != null) {
      return ErrorState(
          message: state.error!,
          onRetry: () => ref.read(favoritesProvider.notifier).load());
    }
    if (state.favorites.isEmpty) {
      return const EmptyState(
          message: 'هنوز سالنی را به علاقه‌مندی‌ها اضافه نکرده‌اید',
          icon: Icons.favorite_outline);
    }

    return RefreshIndicator(
      onRefresh: () => ref.read(favoritesProvider.notifier).load(),
      child: ListView.builder(
        padding: AppSpacing.pagePadding,
        itemCount: state.favorites.length,
        itemBuilder: (_, i) {
          final fav = state.favorites[i];
          return Card(
            margin: const EdgeInsets.only(bottom: 8),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(12),
              side: const BorderSide(color: AppColors.border),
            ),
            child: ListTile(
              contentPadding:
                  const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              leading: CircleAvatar(
                backgroundColor: AppColors.primary50,
                child: Text(
                  fav.salonName.isNotEmpty ? fav.salonName[0] : '؟',
                  style: const TextStyle(
                      color: AppColors.primary, fontWeight: FontWeight.bold),
                ),
              ),
              title: Row(
                children: [
                  Expanded(
                      child: Text(fav.salonName,
                          style: const TextStyle(fontWeight: FontWeight.bold))),
                  if (fav.isVip)
                    const Icon(Icons.verified, color: Colors.amber, size: 16),
                ],
              ),
              subtitle: fav.ratingAvg > 0
                  ? Row(
                      children: [
                        const Icon(Icons.star, size: 14, color: Colors.amber),
                        const SizedBox(width: 4),
                        Text(fav.ratingAvg.toStringAsFixed(1),
                            style: const TextStyle(fontSize: 12)),
                      ],
                    )
                  : null,
              trailing: IconButton(
                icon: const Icon(Icons.favorite, color: AppColors.danger),
                onPressed: () =>
                    ref.read(favoritesProvider.notifier).remove(fav.slug),
              ),
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(
                    builder: (_) => SalonDetailScreen(slug: fav.slug)),
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _buildDashboard(ClientDashboardState state) {
    if (state.loading) return const LoadingState();
    if (state.error != null) {
      return ErrorState(
          message: state.error!,
          onRetry: () => ref.read(clientDashboardProvider.notifier).load());
    }
    final data = state.data;
    if (data == null) return const EmptyState(message: 'داده‌ای یافت نشد');

    return RefreshIndicator(
      onRefresh: () => ref.read(clientDashboardProvider.notifier).load(),
      child: ListView(
        padding: AppSpacing.pagePadding,
        children: [
          Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              gradient: AppColors.heroGradient,
              borderRadius: BorderRadius.circular(16),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text('خوش آمدید',
                    style: TextStyle(color: Colors.white70, fontSize: 14)),
                const SizedBox(height: 8),
                const Text('به سالن هوشمند خوش آمدید',
                    style: TextStyle(
                        color: Colors.white,
                        fontSize: 18,
                        fontWeight: FontWeight.bold)),
                const SizedBox(height: 16),
                Row(
                  children: [
                    _badge(Icons.star, '${data.loyaltyPoints} امتیاز'),
                    const SizedBox(width: 12),
                    _badge(Icons.visibility, '${data.totalVisits} بازدید'),
                  ],
                ),
              ],
            ),
          ),
          const SizedBox(height: 12),
          if (data.upcomingBookings > 0 && data.nextBooking != null)
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
          SummaryCard(
            title: 'خلاصه حساب',
            child: StatGrid(tiles: [
              StatTileConfig(
                  icon: Icons.calendar_month,
                  label: 'رزروهای پیش رو',
                  value: '${data.upcomingBookings}'),
              StatTileConfig(
                  icon: Icons.notifications_outlined,
                  label: 'اعلان‌های خوانده نشده',
                  value: '${data.unreadNotifications}',
                  iconColor: AppColors.warning,
                  valueColor: AppColors.warning),
            ]),
          ),
        ],
      ),
    );
  }

  Widget _badge(IconData icon, String text) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: Colors.white.withValues(alpha: 0.2),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 16, color: Colors.white),
          const SizedBox(width: 6),
          Text(text, style: const TextStyle(color: Colors.white, fontSize: 13)),
        ],
      ),
    );
  }
}
