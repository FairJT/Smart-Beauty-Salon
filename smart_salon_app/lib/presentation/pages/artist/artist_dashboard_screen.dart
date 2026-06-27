import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/app_colors.dart';
import '../../../core/format/money_formatter.dart';
import '../../../core/format/jalaali_helper.dart';
import '../../providers/dashboard_provider.dart';
import '../../widgets/dashboard_widgets.dart';
import '../../widgets/ui_kit.dart';
import 'artist_schedule_screen.dart';

class ArtistDashboardScreen extends ConsumerStatefulWidget {
  const ArtistDashboardScreen({super.key});

  @override
  ConsumerState<ArtistDashboardScreen> createState() =>
      _ArtistDashboardScreenState();
}

class _ArtistDashboardScreenState extends ConsumerState<ArtistDashboardScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(artistDashboardProvider.notifier).load();
    });
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(artistDashboardProvider);
    final textTheme = Theme.of(context).textTheme;

    return Scaffold(
      appBar: AppBar(
        title: Text('داشبورد هنرمند',
            style: textTheme.titleLarge
                ?.copyWith(fontWeight: FontWeight.bold, color: Colors.white)),
      ),
      body: _buildBody(state),
    );
  }

  Widget _buildBody(ArtistDashboardState state) {
    if (state.loading) return const LoadingState();
    if (state.error != null) {
      return ErrorState(
          message: state.error!,
          onRetry: () => ref.read(artistDashboardProvider.notifier).load());
    }
    final data = state.data;
    if (data == null) return const EmptyState(message: 'داده‌ای یافت نشد');

    return RefreshIndicator(
      onRefresh: () => ref.read(artistDashboardProvider.notifier).load(),
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
            ]),
          ),
          if (data.nextAppointment != null)
            SummaryCard(
              title: 'نوبت بعدی',
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  StatTile(
                      icon: Icons.person,
                      label: 'مشتری',
                      value: data.nextAppointment!.clientName),
                  StatTile(
                      icon: Icons.content_cut,
                      label: 'خدمت',
                      value: data.nextAppointment!.serviceName),
                  StatTile(
                      icon: Icons.access_time,
                      label: 'زمان',
                      value: JalaaliHelper.formatDateTime(
                          data.nextAppointment!.startTime)),
                  const SizedBox(height: 4),
                  StatusPill(data.nextAppointment!.status),
                ],
              ),
            ),
          SummaryCard(
            title: 'برنامه کاری',
            child: AppListRow(
              icon: Icons.event_note_outlined,
              title: 'برنامه و نوبت‌های من',
              subtitle: 'تأیید و تکمیل نوبت‌ها',
              tint: AppColors.primary,
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const ArtistScheduleScreen()),
              ),
            ),
          ),
          SummaryCard(
            title: 'آمار ماه',
            child: StatGrid(tiles: [
              StatTileConfig(
                  icon: Icons.date_range,
                  label: 'نوبت‌های ماه',
                  value: '${data.monthAppointments}'),
              if (data.monthRevenue != null)
                StatTileConfig(
                    icon: Icons.monetization_on_outlined,
                    label: 'درآمد ماه',
                    value: MoneyFormatter.format(data.monthRevenue!.amount),
                    iconColor: AppColors.success,
                    valueColor: AppColors.success),
              StatTileConfig(
                  icon: Icons.star_half,
                  label: 'امتیاز',
                  value: data.ratingAvg.toStringAsFixed(1),
                  iconColor: AppColors.warning,
                  valueColor: AppColors.warning),
            ]),
          ),
        ],
      ),
    );
  }
}
