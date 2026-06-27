import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/app_colors.dart';
import '../../../domain/entities/appointment_entity.dart';
import '../../../data/datasources/dio_client.dart';
import '../../../data/datasources/api_constants.dart';
import '../../providers/artist_schedule_provider.dart';
import '../../providers/auth_provider.dart';
import '../../widgets/ui_kit.dart';
import '../login_screen.dart';

class ArtistScheduleScreen extends ConsumerStatefulWidget {
  const ArtistScheduleScreen({super.key});

  @override
  ConsumerState<ArtistScheduleScreen> createState() =>
      _ArtistScheduleScreenState();
}

class _ArtistScheduleScreenState extends ConsumerState<ArtistScheduleScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(artistScheduleProvider.notifier).load();
    });
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(artistScheduleProvider);
    final auth = ref.watch(authProvider);
    final userName = auth.user?.fullName ?? 'هنرمند';
    final textTheme = Theme.of(context).textTheme;

    return Scaffold(
      appBar: AppBar(
        title: Text('برنامه کاری - $userName',
            style: textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () async {
              await ref.read(authProvider.notifier).logout();
              if (!context.mounted) return;
              Navigator.pushReplacement(
                context,
                MaterialPageRoute(builder: (_) => const LoginScreen()),
              );
            },
          ),
        ],
      ),
      body: state.loading
          ? const Center(child: CircularProgressIndicator())
          : state.error != null
              ? _buildError(state.error!)
              : state.appointments.isEmpty
                  ? _buildEmpty()
                  : RefreshIndicator(
                      onRefresh: () =>
                          ref.read(artistScheduleProvider.notifier).load(),
                      child: _buildList(state),
                    ),
    );
  }

  Widget _buildEmpty() {
    final textTheme = Theme.of(context).textTheme;
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.event_available_outlined,
              size: 60, color: AppColors.primary),
          const SizedBox(height: 12),
          Text('هیچ نوبتی در برنامه شما نیست',
              style: textTheme.bodyLarge?.copyWith(color: AppColors.primary)),
        ],
      ),
    );
  }

  Widget _buildError(String error) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(40),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.wifi_off, size: 60, color: AppColors.primary),
            const SizedBox(height: 12),
            Text(error,
                textAlign: TextAlign.center,
                style: TextStyle(color: AppColors.primary)),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: () => ref.read(artistScheduleProvider.notifier).load(),
              child: const Text('تلاش مجدد'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildList(ArtistScheduleState state) {
    return ListView.builder(
      padding: AppSpacing.pagePadding,
      itemCount: state.appointments.length,
      itemBuilder: (_, i) => _AppointmentCard(
        appointment: state.appointments[i],
        onConfirm: () => _confirm(state.appointments[i].id),
        onComplete: () => _complete(state.appointments[i].id),
      ),
    );
  }

  Future<void> _confirm(String id) async {
    try {
      await DioClient.instance.put('${ApiConstants.appointments}/$id/confirm');
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text('نوبت تایید شد'), backgroundColor: AppColors.success),
      );
      ref.read(artistScheduleProvider.notifier).load();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
            content: Text(e.toString()), backgroundColor: AppColors.danger),
      );
    }
  }

  Future<void> _complete(String id) async {
    try {
      await DioClient.instance.put('${ApiConstants.appointments}/$id/complete');
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text('نوبت تمام شد'), backgroundColor: AppColors.success),
      );
      ref.read(artistScheduleProvider.notifier).load();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
            content: Text(e.toString()), backgroundColor: AppColors.danger),
      );
    }
  }
}

class _AppointmentCard extends StatelessWidget {
  final AppointmentEntity appointment;
  final VoidCallback onConfirm;
  final VoidCallback onComplete;

  const _AppointmentCard({
    required this.appointment,
    required this.onConfirm,
    required this.onComplete,
  });

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final status = appointment.status;

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: Padding(
        padding: AppSpacing.pagePadding,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        appointment.salonName ?? '',
                        style: textTheme.titleMedium
                            ?.copyWith(fontWeight: FontWeight.bold),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        appointment.serviceName ?? '',
                        style: textTheme.bodyMedium
                            ?.copyWith(color: AppColors.primary),
                      ),
                    ],
                  ),
                ),
                StatusPill(status),
              ],
            ),
            const AppDivider(),
            Row(
              children: [
                const Icon(Icons.calendar_today_outlined,
                    size: 16, color: AppColors.primary),
                const SizedBox(width: 8),
                Text(
                  '${appointment.startTime.year}/${appointment.startTime.month.toString().padLeft(2, '0')}/${appointment.startTime.day.toString().padLeft(2, '0')}',
                  style: textTheme.bodyMedium,
                ),
                const SizedBox(width: 16),
                const Icon(Icons.access_time,
                    size: 16, color: AppColors.primary),
                const SizedBox(width: 8),
                Text(
                  '${appointment.startTime.hour}:${appointment.startTime.minute.toString().padLeft(2, '0')} - ${appointment.endTime.hour}:${appointment.endTime.minute.toString().padLeft(2, '0')}',
                  style: textTheme.bodyMedium,
                ),
              ],
            ),
            const SizedBox(height: 8),
            Row(
              children: [
                const Icon(Icons.attach_money,
                    size: 16, color: AppColors.primary),
                const SizedBox(width: 8),
                Text(
                  '${appointment.estimatedPrice.toStringAsFixed(0)} تومان',
                  style: textTheme.bodyMedium,
                ),
              ],
            ),
            if (status == 1 || status == 2)
              Padding(
                padding: const EdgeInsets.only(top: 12),
                child: Row(
                  children: [
                    if (status == 1)
                      Expanded(
                        child: ElevatedButton.icon(
                          style: ElevatedButton.styleFrom(
                            backgroundColor: AppColors.success,
                            foregroundColor: Colors.white,
                            shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(8)),
                          ),
                          icon: const Icon(Icons.check, size: 18),
                          label: const Text('تایید'),
                          onPressed: onConfirm,
                        ),
                      ),
                    if (status == 2) ...[
                      const SizedBox(width: 8),
                      Expanded(
                        child: ElevatedButton.icon(
                          style: ElevatedButton.styleFrom(
                            backgroundColor: AppColors.info,
                            foregroundColor: Colors.white,
                            shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(8)),
                          ),
                          icon: const Icon(Icons.done_all, size: 18),
                          label: const Text('انجام شد'),
                          onPressed: onComplete,
                        ),
                      ),
                    ],
                  ],
                ),
              ),
          ],
        ),
      ),
    );
  }
}
