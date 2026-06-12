import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/app_colors.dart';
import '../../domain/entities/appointment_entity.dart';
import '../providers/appointment_provider.dart';

class AppointmentList extends ConsumerStatefulWidget {
  const AppointmentList({super.key});

  @override
  ConsumerState<AppointmentList> createState() => _AppointmentListState();
}

class _AppointmentListState extends ConsumerState<AppointmentList> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(appointmentListProvider.notifier).load();
    });
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(appointmentListProvider);

    if (state.loading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (state.error != null) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.error_outline, size: 60, color: Colors.grey),
            const SizedBox(height: 12),
            Text(state.error!, style: const TextStyle(color: Colors.grey)),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: () => ref.read(appointmentListProvider.notifier).load(),
              child: const Text('تلاش مجدد'),
            ),
          ],
        ),
      );
    }

    if (state.appointments.isEmpty) {
      return const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.calendar_today_outlined, size: 60, color: Colors.grey),
            SizedBox(height: 12),
            Text('هنوز رزروی ندارید', style: TextStyle(color: Colors.grey, fontSize: 16)),
          ],
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: () => ref.read(appointmentListProvider.notifier).load(),
      child: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: state.appointments.length,
        itemBuilder: (_, i) => _AppointmentCard(
          appointment: state.appointments[i],
          onCancel: () => _cancel(state.appointments[i].id),
          onRate: () => _showRateDialog(state.appointments[i].id),
        ),
      ),
    );
  }

  Future<void> _cancel(int id) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('لغو نوبت'),
        content: const Text('آیا مطمئنید می‌خواهید این نوبت را لغو کنید؟'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('خیر')),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.danger),
            onPressed: () => Navigator.pop(context, true),
            child: const Text('بله، لغو کن'),
          ),
        ],
      ),
    );

    if (confirm == true) {
      try {
        await ref.read(appointmentListProvider.notifier).cancel(id);
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('نوبت با موفقیت لغو شد'), backgroundColor: Colors.green),
        );
      } catch (e) {
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(e.toString()), backgroundColor: Colors.red),
        );
      }
    }
  }

  void _showRateDialog(int id) {
    int selectedRating = 5;
    final commentController = TextEditingController();

    showDialog(
      context: context,
      builder: (_) => StatefulBuilder(
        builder: (context, setStateDialog) => AlertDialog(
          title: const Text('ثبت امتیاز'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text('به این هنرمند چند ستاره می‌دهید؟'),
              const SizedBox(height: 12),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: List.generate(5, (i) {
                  return GestureDetector(
                    onTap: () => setStateDialog(() => selectedRating = i + 1),
                    child: Icon(
                      i < selectedRating ? Icons.star : Icons.star_border,
                      color: Colors.amber,
                      size: 36,
                    ),
                  );
                }),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: commentController,
                decoration: const InputDecoration(labelText: 'نظر شما (اختیاری)', border: OutlineInputBorder()),
                maxLines: 2,
              ),
            ],
          ),
          actions: [
            TextButton(onPressed: () => Navigator.pop(context), child: const Text('انصراف')),
            ElevatedButton(
              style: ElevatedButton.styleFrom(backgroundColor: Colors.amber),
              onPressed: () async {
                Navigator.pop(context);
                try {
                  await ApiService.post(
                    '${ApiConstants.appointments}/$id/rate',
                    {'rating': selectedRating, 'comment': commentController.text},
                  );
                  if (!context.mounted) return;
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('امتیاز با موفقیت ثبت شد'), backgroundColor: Colors.amber),
                  );
                  ref.read(appointmentListProvider.notifier).load();
                } catch (e) {
                  if (!context.mounted) return;
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(content: Text(e.toString()), backgroundColor: Colors.red),
                  );
                }
              },
              child: const Text('ثبت امتیاز'),
            ),
          ],
        ),
      ),
    );
  }
}

class _AppointmentCard extends StatelessWidget {
  final AppointmentEntity appointment;
  final VoidCallback onCancel;
  final VoidCallback onRate;

  const _AppointmentCard({
    required this.appointment,
    required this.onCancel,
    required this.onRate,
  });

  @override
  Widget build(BuildContext context) {
    final status = appointment.status;
    final statusColor = AppColors.statusColor(status);
    final isRated = appointment.isRated;

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
                  child: Text(appointment.salonName,
                      style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: statusColor.withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(color: statusColor),
                  ),
                  child: Text(
                    appointment.statusText,
                    style: TextStyle(color: statusColor, fontSize: 12, fontWeight: FontWeight.bold),
                  ),
                ),
              ],
            ),
            const Divider(height: 20),
            _row(Icons.spa_outlined, appointment.serviceName),
            _row(Icons.person_outline, appointment.artistName),
            _row(Icons.calendar_today_outlined,
                '${appointment.startTime.year}/${appointment.startTime.month}/${appointment.startTime.day}  ساعت  ${appointment.startTime.hour}:${appointment.startTime.minute.toString().padLeft(2, '0')}'),
            _row(Icons.attach_money,
                '${appointment.estimatedPrice} تومان  |  بیعانه: ${appointment.depositAmount} تومان'),
            if ((status == 1 || status == 2) && !isRated)
              Padding(
                padding: const EdgeInsets.only(top: 12),
                child: SizedBox(
                  width: double.infinity,
                  child: OutlinedButton.icon(
                    style: OutlinedButton.styleFrom(
                      foregroundColor: AppColors.danger,
                      side: const BorderSide(color: AppColors.danger),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                    ),
                    icon: const Icon(Icons.cancel_outlined, size: 18),
                    label: const Text('لغو نوبت'),
                    onPressed: onCancel,
                  ),
                ),
              ),
            if (status == 4 && !isRated)
              Padding(
                padding: const EdgeInsets.only(top: 8),
                child: SizedBox(
                  width: double.infinity,
                  child: ElevatedButton.icon(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.primary,
                      foregroundColor: Colors.white,
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                    ),
                    icon: const Icon(Icons.star, size: 18),
                    label: const Text('ثبت امتیاز'),
                    onPressed: onRate,
                  ),
                ),
              ),
            if (status == 4 && isRated)
              Padding(
                padding: const EdgeInsets.only(top: 8),
                child: Row(
                  children: [
                    const Icon(Icons.star, color: Colors.amber, size: 18),
                    const SizedBox(width: 4),
                    Text('امتیاز شما: ${appointment.rating}',
                        style: const TextStyle(color: Colors.amber)),
                  ],
                ),
              ),
          ],
        ),
      ),
    );
  }

  Widget _row(IconData icon, String text) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 3),
      child: Row(
        children: [
          Icon(icon, size: 16, color: AppColors.primary),
          const SizedBox(width: 8),
          Expanded(child: Text(text, style: const TextStyle(fontSize: 14))),
        ],
      ),
    );
  }
}
