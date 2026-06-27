import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/app_colors.dart';
import '../../../core/format/money_formatter.dart';
import '../../providers/dashboard_provider.dart';
import '../../widgets/dashboard_widgets.dart';
import '../home_screen.dart';
import 'artist_management_screen.dart';
import 'catalog_management_screen.dart';
import 'finance_screen.dart';

class ManagerDashboardScreen extends ConsumerStatefulWidget {
  final String? slug;

  const ManagerDashboardScreen({super.key, this.slug});

  @override
  ConsumerState<ManagerDashboardScreen> createState() =>
      _ManagerDashboardScreenState();
}

class _ManagerDashboardScreenState
    extends ConsumerState<ManagerDashboardScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(salonManagerDashboardProvider.notifier).load(widget.slug);
    });
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(salonManagerDashboardProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('داشبورد مدیریت',
            style: TextStyle(fontWeight: FontWeight.bold)),
        actions: [
          IconButton(
            icon: const Icon(Icons.home_outlined),
            onPressed: () => Navigator.pushReplacement(
              context,
              MaterialPageRoute(builder: (_) => const HomeScreen()),
            ),
          ),
        ],
      ),
      body: _buildBody(state),
    );
  }

  Widget _buildBody(SalonManagerDashboardState state) {
    if (state.loading) return const LoadingState();
    if (state.error != null) {
      return ErrorState(
          message: state.error!,
          onRetry: () => ref
              .read(salonManagerDashboardProvider.notifier)
              .load(widget.slug));
    }
    final data = state.data;
    if (data == null) return const EmptyState(message: 'داده‌ای یافت نشد');

    return RefreshIndicator(
      onRefresh: () =>
          ref.read(salonManagerDashboardProvider.notifier).load(widget.slug),
      child: ListView(
        padding: AppSpacing.pagePadding,
        children: [
          SummaryCard(
            title: 'خلاصه امروز',
            child: StatGrid(tiles: [
              StatTileConfig(
                  icon: Icons.calendar_today,
                  label: 'نوبت‌های امروز',
                  value: '${data.todayAppointments}'),
              StatTileConfig(
                  icon: Icons.upcoming,
                  label: 'نوبت‌های پیش رو',
                  value: '${data.upcomingAppointments}'),
              StatTileConfig(
                  icon: Icons.monetization_on_outlined,
                  label: 'درآمد امروز',
                  value: MoneyFormatter.format(data.revenue.amount),
                  iconColor: AppColors.success,
                  valueColor: AppColors.success),
              StatTileConfig(
                  icon: Icons.people_outline,
                  label: 'هنرمندان فعال',
                  value: '${data.activeArtistCount}'),
              StatTileConfig(
                  icon: Icons.content_cut,
                  label: 'خدمات فعال',
                  value: '${data.activeServiceCount}'),
            ]),
          ),
          if (data.artistUtilization.isNotEmpty)
            SummaryCard(
              title: 'عملکرد هنرمندان',
              child: Column(
                children: data.artistUtilization
                    .map((a) => Padding(
                          padding: const EdgeInsets.only(bottom: 8),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(
                                mainAxisAlignment:
                                    MainAxisAlignment.spaceBetween,
                                children: [
                                  Text(a.artistName,
                                      style: const TextStyle(
                                          fontWeight: FontWeight.w500,
                                          fontSize: 13)),
                                  Text('${a.todayAppointments} نوبت',
                                      style: const TextStyle(
                                          color: AppColors.textSecondary,
                                          fontSize: 12)),
                                ],
                              ),
                              const SizedBox(height: 4),
                              ClipRRect(
                                borderRadius: BorderRadius.circular(4),
                                child: LinearProgressIndicator(
                                  value: a.utilizationPercent / 100,
                                  minHeight: 6,
                                  backgroundColor: AppColors.border,
                                  valueColor: const AlwaysStoppedAnimation(
                                      AppColors.primary),
                                ),
                              ),
                            ],
                          ),
                        ))
                    .toList(),
              ),
            ),
          SummaryCard(
            title: 'مدیریت سالن',
            child: Column(
              children: [
                ListTile(
                  contentPadding: EdgeInsets.zero,
                  leading: const Icon(Icons.people_alt_outlined,
                      color: AppColors.primary),
                  title: const Text('مدیریت هنرمندان',
                      style: TextStyle(fontSize: 14)),
                  trailing: const Icon(Icons.chevron_left,
                      color: AppColors.textSecondary),
                  onTap: () => Navigator.push(
                    context,
                    MaterialPageRoute(
                        builder: (_) => const ArtistManagementScreen()),
                  ),
                ),
                ListTile(
                  contentPadding: EdgeInsets.zero,
                  leading:
                      const Icon(Icons.content_cut, color: AppColors.primary),
                  title: const Text('مدیریت خدمات',
                      style: TextStyle(fontSize: 14)),
                  trailing: const Icon(Icons.chevron_left,
                      color: AppColors.textSecondary),
                  onTap: () => Navigator.push(
                    context,
                    MaterialPageRoute(
                        builder: (_) => const CatalogManagementScreen()),
                  ),
                ),
                ListTile(
                  contentPadding: EdgeInsets.zero,
                  leading: const Icon(Icons.account_balance_wallet_outlined,
                      color: AppColors.success),
                  title:
                      const Text('امور مالی', style: TextStyle(fontSize: 14)),
                  trailing: const Icon(Icons.chevron_left,
                      color: AppColors.textSecondary),
                  onTap: () => Navigator.push(
                    context,
                    MaterialPageRoute(builder: (_) => const FinanceScreen()),
                  ),
                ),
              ],
            ),
          ),
          SummaryCard(
            title: 'اشتراک',
            child: StatTile(
              icon: Icons.workspace_premium,
              label: 'وضعیت اشتراک',
              value: data.subscriptionStatus == 'active' ? 'فعال' : 'غیرفعال',
              valueColor: data.subscriptionStatus == 'active'
                  ? AppColors.success
                  : AppColors.danger,
            ),
          ),
        ],
      ),
    );
  }
}
