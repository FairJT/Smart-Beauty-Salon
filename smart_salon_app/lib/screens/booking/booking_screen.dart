import 'package:flutter/material.dart';
import '../../core/api_constants.dart';
import '../../core/api_service.dart';
import '../../core/app_colors.dart';

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
  List<dynamic> _slots = [];
  bool _loadingSlots = false;
  bool _booking = false;
  String? _error;

  // تبدیل میلادی به شمسی
  Map<String, dynamic> _toJalali(DateTime date) {
    int gy = date.year;
    int gm = date.month;
    int gd = date.day;

    int jy, jm, jd;
    int g_d_no, j_d_no;
    List<int> g_days_in_month = [31,28,31,30,31,30,31,31,30,31,30,31];
    List<int> j_days_in_month = [31,31,31,31,31,31,30,30,30,30,30,29];

    gy -= 1600;
    gm -= 1;
    gd -= 1;

    g_d_no = 365 * gy + (_div(gy + 3, 4)) - (_div(gy + 99, 100)) + (_div(gy + 399, 400));
    for (int i = 0; i < gm; i++) g_d_no += g_days_in_month[i];
    if (gm > 1 && ((gy % 4 == 0 && gy % 100 != 0) || (gy % 400 == 0))) g_d_no++;
    g_d_no += gd;

    j_d_no = g_d_no - 79;
    int j_np = _div(j_d_no, 12053);
    j_d_no %= 12053;
    jy = 979 + 33 * j_np + 4 * _div(j_d_no, 1461);
    j_d_no %= 1461;

    if (j_d_no >= 366) {
      jy += _div(j_d_no - 1, 365);
      j_d_no = (j_d_no - 1) % 365;
    }

    for (jm = 0; jm < 11 && j_d_no >= j_days_in_month[jm]; jm++) {
      j_d_no -= j_days_in_month[jm];
    }
    jd = j_d_no + 1;

    return {'year': jy, 'month': jm + 1, 'day': jd};
  }

  int _div(int a, int b) => (a / b).floor();

  String _jalaliMonthName(int month) {
    const months = [
      'فروردین','اردیبهشت','خرداد','تیر','مرداد','شهریور',
      'مهر','آبان','آذر','دی','بهمن','اسفند'
    ];
    return months[month - 1];
  }

  String _weekDayName(DateTime date) {
    const days = ['دوشنبه','سه‌شنبه','چهارشنبه','پنجشنبه','جمعه','شنبه','یکشنبه'];
    return days[date.weekday - 1];
  }

  @override
  void initState() {
    super.initState();
    _loadSlots();
  }

  Future<void> _loadSlots() async {
    setState(() { _loadingSlots = true; _slots = []; _selectedSlot = null; });
    try {
      final dateStr =
          '${_selectedDate.year}-${_selectedDate.month.toString().padLeft(2,'0')}-${_selectedDate.day.toString().padLeft(2,'0')}';
      final res = await ApiService.get(
        '${ApiConstants.slots}?artistId=${widget.artistId}&date=$dateStr&duration=${widget.durationMinutes}',
      );
      setState(() => _slots = res['slots'] ?? []);
    } catch (e) {
      setState(() => _error = e.toString().replaceAll('Exception: ', ''));
    } finally {
      setState(() => _loadingSlots = false);
    }
  }

  Future<void> _book() async {
    if (_selectedSlot == null) {
      setState(() => _error = 'لطفاً یک تایم انتخاب کنید');
      return;
    }
    setState(() { _booking = true; _error = null; });
    try {
      final slot = _slots.firstWhere((s) => s['start'] == _selectedSlot);
      final startTime = slot['startFull'];
      final res = await ApiService.post(ApiConstants.appointments, {
        'artistId':        widget.artistId,
        'salonId':         widget.salonId,
        'serviceId':       widget.serviceId,
        'startTime':       startTime,
        'durationMinutes': widget.durationMinutes,
        'estimatedPrice':  widget.price,
        'notes':           '',
      });

      if (!mounted) return;

      final jalali = _toJalali(_selectedDate);
      final monthName = _jalaliMonthName(jalali['month']!);

      showDialog(
        context: context,
        barrierDismissible: false,
        builder: (_) => AlertDialog(
          title: const Text('رزرو موفق! ✅'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text('تاریخ: ${jalali['day']} $monthName ${jalali['year']}'),
              Text('ساعت: $_selectedSlot'),
              Text('خدمت: ${widget.serviceName}'),
              Text('هنرمند: ${widget.artistName}'),
              const SizedBox(height: 12),
              Text(
                'مبلغ بیعانه: ${res['deposit']} تومان',
                style: const TextStyle(
                  color: AppColors.green,
                  fontWeight: FontWeight.bold,
                  fontSize: 16,
                ),
              ),
            ],
          ),
          actions: [
            ElevatedButton(
              onPressed: () {
                Navigator.pop(context);
                Navigator.pop(context);
              },
              child: const Text('باشه'),
            ),
          ],
        ),
      );
    } catch (e) {
      setState(() => _error = e.toString().replaceAll('Exception: ', ''));
    } finally {
      setState(() => _booking = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('رزرو نوبت')),
      body: SingleChildScrollView(
  padding: EdgeInsets.fromLTRB(16, 16, 16, 
      MediaQuery.of(context).padding.bottom + 16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _buildInfoCard(),
            const SizedBox(height: 20),
            const Text(
              'انتخاب تاریخ:',
              style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
            ),
            const SizedBox(height: 8),
            _buildDateSelector(),
            const SizedBox(height: 20),
            const Text(
              'انتخاب تایم:',
              style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
            ),
            const SizedBox(height: 8),
            _buildSlots(),
            const SizedBox(height: 20),
            if (_error != null)
              Container(
                padding: const EdgeInsets.all(12),
                margin: const EdgeInsets.only(bottom: 16),
                decoration: BoxDecoration(
                  color: Colors.red.shade100,
                  borderRadius: BorderRadius.circular(10),
                ),
                child: Text(
                  _error!,
                  textAlign: TextAlign.center,
                  style: const TextStyle(color: Colors.red),
                ),
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
        border: Border.all(color: AppColors.primary.withOpacity(0.2)),
      ),
      child: Column(
        children: [
          _buildInfoRow(Icons.person, 'هنرمند', widget.artistName),
          _buildInfoRow(Icons.spa, 'خدمت', widget.serviceName),
          _buildInfoRow(Icons.timer, 'مدت', '${widget.durationMinutes} دقیقه'),
          _buildInfoRow(Icons.attach_money, 'قیمت',
              '${widget.price.toStringAsFixed(0)} تومان'),
        ],
      ),
    );
  }

  Widget _buildInfoRow(IconData icon, String label, String value) {
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
          final isSelected = date.day == _selectedDate.day &&
              date.month == _selectedDate.month;

          final jalali = _toJalali(date);
          final dayName = _weekDayName(date);
          final monthName = _jalaliMonthName(jalali['month']!);

          return GestureDetector(
            onTap: () {
              setState(() => _selectedDate = date);
              _loadSlots();
            },
            child: Container(
              width: 70,
              margin: const EdgeInsets.only(left: 8),
              decoration: BoxDecoration(
                color: isSelected ? AppColors.primary : Colors.white,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: isSelected
                      ? AppColors.primary
                      : Colors.grey.shade300,
                ),
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    dayName,
                    style: TextStyle(
                      fontSize: 10,
                      color: isSelected ? Colors.white70 : Colors.grey,
                    ),
                  ),
                  Text(
                    '${jalali['day']}',
                    style: TextStyle(
                      fontSize: 22,
                      fontWeight: FontWeight.bold,
                      color: isSelected ? Colors.white : AppColors.primary,
                    ),
                  ),
                  Text(
                    monthName,
                    style: TextStyle(
                      fontSize: 10,
                      color: isSelected ? Colors.white70 : Colors.grey,
                    ),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _buildSlots() {
    if (_loadingSlots) {
      return const Center(child: CircularProgressIndicator());
    }
    if (_slots.isEmpty) {
      return const Center(
        child: Text('تایم خالی وجود ندارد',
            style: TextStyle(color: Colors.grey)),
      );
    }
    return Wrap(
      spacing: 8,
      runSpacing: 8,
      children: _slots.map((slot) {
        final isSelected = slot['start'] == _selectedSlot;
        return GestureDetector(
          onTap: () => setState(() => _selectedSlot = slot['start']),
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
            decoration: BoxDecoration(
              color: isSelected ? AppColors.primary : Colors.white,
              borderRadius: BorderRadius.circular(10),
              border: Border.all(
                color: isSelected
                    ? AppColors.primary
                    : Colors.grey.shade300,
              ),
            ),
            child: Text(
              slot['start'],
              style: TextStyle(
                color: isSelected ? Colors.white : AppColors.dark,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        );
      }).toList(),
    );
  }
}