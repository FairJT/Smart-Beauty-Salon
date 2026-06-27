import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/app_colors.dart';
import '../../../domain/entities/appointment_entity.dart';
import '../../../data/datasources/dio_client.dart';
import '../../../data/datasources/api_constants.dart';
import '../../providers/artist_schedule_provider.dart';
import '../../providers/auth_provider.dart';
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

    return Scaffold(
      appBar: AppBar(
        title: Text('برنامه کاری - $userName',
            style: const TextStyle(fontWeight: FontWeight.bold)),
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
    return const Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.event_available_outlined, size: 60, color: Colors.grey),
          SizedBox(height: 12),
          Text('هیچ نوبتی در برنامه شما نیست',
              style: TextStyle(color: Colors.grey, fontSize: 16)),
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
            const Icon(Icons.wifi_off, size: 60, color: Colors.grey),
            const SizedBox(height: 12),
            Text(error,
                textAlign: TextAlign.center,
                style: const TextStyle(color: Colors.grey)),
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
      padding: const EdgeInsets.all(16),
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
    final status = appointment.status;
    final statusColor = AppColors.statusColor(status);

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: Padding(
        padding: const EdgeInsets.all(16),
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
                        style: const TextStyle(
                            fontWeight: FontWeight.bold, fontSize: 16),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        appointment.serviceName ?? '',
                        style:
                            const TextStyle(color: Colors.grey, fontSize: 14),
                      ),
                    ],
                  ),
                ),
                Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: statusColor.withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(color: statusColor),
                  ),
                  child: Text(
                    appointment.statusText,
                    style: TextStyle(
                        color: statusColor,
                        fontSize: 12,
                        fontWeight: FontWeight.bold),
                  ),
                ),
              ],
            ),
            const Divider(height: 20),
            Row(
              children: [
                const Icon(Icons.calendar_today_outlined,
                    size: 16, color: AppColors.primary),
                const SizedBox(width: 8),
                Text(
                  '${appointment.startTime.year}/${appointment.startTime.month.toString().padLeft(2, '0')}/${appointment.startTime.day.toString().padLeft(2, '0')}',
                  style: const TextStyle(fontSize: 14),
                ),
                const SizedBox(width: 16),
                const Icon(Icons.access_time,
                    size: 16, color: AppColors.primary),
                const SizedBox(width: 8),
                Text(
                  '${appointment.startTime.hour}:${appointment.startTime.minute.toString().padLeft(2, '0')} - ${appointment.endTime.hour}:${appointment.endTime.minute.toString().padLeft(2, '0')}',
                  style: const TextStyle(fontSize: 14),
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
                  style: const TextStyle(fontSize: 14),
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
