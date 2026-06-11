import 'package:flutter/material.dart';
import 'package:shamsi_date/shamsi_date.dart';
import '../../core/app_colors.dart';
import '../../domain/entities/slot_entity.dart';
import '../../domain/entities/appointment_entity.dart';
import '../../presentation/providers/appointment_provider.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class BookingScreen extends StatefulWidget {
  final int salonId;
  final int artistId;
  final String artistName;
  final int serviceId;
  final String serviceName;
  final int durationMinutes;
  final double price;

  const BookingScreen({
    super.key,
    required this.salonId,
    required this.artistId,
    required this.artistName,
    required this.serviceId,
    required this.serviceName,
    required this.durationMinutes,
    required this.price,
  });

  @override
  State<BookingScreen> createState() => _BookingScreenState();
}

class _BookingScreenState extends State<BookingScreen> {
  DateTime _selectedDate = DateTime.now().add(const Duration(days: 1));
  String? _selectedSlot;
  List<SlotEntity> _slots = [];
  bool _loadingSlots = false;
  bool _booking = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadSlots();
  }

  Future<void> _loadSlots() async {
    setState(() { _loadingSlots = true; _slots = []; _selectedSlot = null; });
    try {
      final dateStr = '${_selectedDate.year}-${_selectedDate.month.toString().padLeft(2, '0')}-${_selectedDate.day.toString().padLeft(2, '0')}';
      // TODO: Implement slot loading using the new repository
      setState(() {
        _slots = [];
        _loadingSlots = false;
      });
    } catch (e) {
      setState(() { _error = e.toString().replaceAll('Exception: ', ''); _loadingSlots = false; });
    }
  }

  Future<void> _book() async {
    if (_selectedSlot == null) {
      setState(() => _error = 'لطفاً یک تایم انتخاب کنید');
      return;
    }
    setState(() { _booking = true; _error = null; });
    try {
      // TODO: Implement booking using the new repository
      if (!mounted) return;
      final jalali = _selectedDate.toJalali();

      showDialog(
        context: context,
        barrierDismissible: false,
        builder: (_) => AlertDialog(
          title: const Text('رزرو موفق!'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text('تاریخ: ${jalali.day} ${_jalaliMonthName(jalali.month)} ${jalali.year}'),
              Text('ساعت: $_selectedSlot'),
              Text('خدمت: ${widget.serviceName}'),
              Text('هنرمند: ${widget.artistName}'),
              const SizedBox(height: 12),
              const Text(
                'مبلغ بیعانه: تومان',
                style: TextStyle(color: AppColors.green, fontWeight: FontWeight.bold, fontSize: 16),
              ),
            ],
          ),
          actions: [
            ElevatedButton(
              onPressed: () { Navigator.pop(context); Navigator.pop(context); },
              child: const Text('باشه'),
            ),
          ],
        ),
      );
    } catch (e) {
      setState(() => _error = e.toString().replaceAll('Exception: ', ''));
    } finally {
      if (mounted) setState(() => _booking = false);
    }
  }

  String _jalaliMonthName(int month) {
    const months = ['فروردین','اردیبهشت','خرداد','تیر','مرداد','شهریور','مهر','آبان','آذر','دی','بهمن','اسفند'];
    return months[month - 1];
  }

  String _weekDayName(DateTime date) {
    const days = ['دوشنبه','سه‌شنبه','چهارشنبه','پنجشنبه','جمعه','شنبه','یکشنبه'];
    return days[date.weekday - 1];
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('رزرو نوبت')),
      body: SingleChildScrollView(
        padding: EdgeInsets.fromLTRB(16, 16, 16, MediaQuery.of(context).padding.bottom + 16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _buildInfoCard(),
            const SizedBox(height: 20),
            const Text('انتخاب تاریخ:', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
            const SizedBox(height: 8),
            _buildDateSelector(),
            const SizedBox(height: 20),
            const Text('انتخاب تایم:', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
            const SizedBox(height: 8),
            _buildSlots(),
            const SizedBox(height: 20),
            if (_error != null)
              Container(
                padding: const EdgeInsets.all(12),
                margin: const EdgeInsets.only(bottom: 16),
                decoration: BoxDecoration(color: Colors.red.shade100, borderRadius: BorderRadius.circular(10)),
                child: Text(_error!, textAlign: TextAlign.center, style: const TextStyle(color: Colors.red)),
              ),
            ElevatedButton(
              onPressed: _booking ? null : _book,
              child: _booking
                  ? const CircularProgressIndicator(color: Colors.white)
                  : const Text('ثبت رزرو', style: TextStyle(fontSize: 18)),
            ),
            const SizedBox(height: 32),
          ],
        ),
      ),
    );
  }

  Widget _buildInfoCard() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.lightBlue,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.primary.withValues(alpha: 0.2)),
      ),
      child: Column(
        children: [
          _infoRow(Icons.person, 'هنرمند', widget.artistName),
          _infoRow(Icons.spa, 'خدمت', widget.serviceName),
          _infoRow(Icons.timer, 'مدت', '${widget.durationMinutes} دقیقه'),
          _infoRow(Icons.attach_money, 'قیمت', '${widget.price.toStringAsFixed(0)} تومان'),
        ],
      ),
    );
  }

  Widget _infoRow(IconData icon, String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        children: [
          Icon(icon, size: 18, color: AppColors.primary),
          const SizedBox(width: 8),
          Text('$label: ', style: const TextStyle(color: Colors.grey)),
          Text(value, style: const TextStyle(fontWeight: FontWeight.bold)),
        ],
      ),
    );
  }

  Widget _buildDateSelector() {
    return SizedBox(
      height: 90,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        itemCount: 14,
        itemBuilder: (_, i) {
          final date = DateTime.now().add(Duration(days: i + 1));
          final isSelected = date.day == _selectedDate.day && date.month == _selectedDate.month;
          final jalali = date.toJalali();

          return GestureDetector(
            onTap: () { setState(() => _selectedDate = date); _loadSlots(); },
            child: Container(
              width: 70,
              margin: const EdgeInsets.only(left: 8),
              decoration: BoxDecoration(
                color: isSelected ? AppColors.primary : Colors.white,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: isSelected ? AppColors.primary : Colors.grey.shade300),
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(_weekDayName(date),
                      style: TextStyle(fontSize: 10, color: isSelected ? Colors.white70 : Colors.grey)),
                  Text('${jalali.day}',
                      style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold,
                          color: isSelected ? Colors.white : AppColors.primary)),
                  Text(_jalaliMonthName(jalali.month),
                      style: TextStyle(fontSize: 10, color: isSelected ? Colors.white70 : Colors.grey)),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _buildSlots() {
    if (_loadingSlots) return const Center(child: CircularProgressIndicator());
    if (_slots.isEmpty) {
      return const Center(child: Text('تایم خالی وجود ندارد', style: TextStyle(color: Colors.grey)));
    }
    return Wrap(
      spacing: 8,
      runSpacing: 8,
      children: _slots.map((slot) {
        final isSelected = slot.start == _selectedSlot;
        return GestureDetector(
          onTap: () => setState(() => _selectedSlot = slot.start),
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
            decoration: BoxDecoration(
              color: isSelected ? AppColors.primary : Colors.white,
              borderRadius: BorderRadius.circular(10),
              border: Border.all(color: isSelected ? AppColors.primary : Colors.grey.shade300),
            ),
            child: Text(slot.start,
                style: TextStyle(color: isSelected ? Colors.white : AppColors.dark, fontWeight: FontWeight.bold)),
          ),
        );
      }).toList(),
    );
  }
}
