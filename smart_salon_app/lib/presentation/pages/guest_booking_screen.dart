import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shamsi_date/shamsi_date.dart';
import '../../core/app_colors.dart';
import '../../domain/entities/salon_entity.dart';
import '../../domain/entities/service_entity.dart';
import '../../domain/entities/artist_entity.dart';
import '../../data/repositories/salon_repository_impl.dart';
import '../../data/repositories/service_repository_impl.dart';
import '../../data/repositories/artist_repository_impl.dart';
import 'otp_screen.dart';

class GuestBookingScreen extends ConsumerStatefulWidget {
  final String slug;

  const GuestBookingScreen({super.key, required this.slug});

  @override
  ConsumerState<GuestBookingScreen> createState() => _GuestBookingScreenState();
}

class _GuestBookingScreenState extends ConsumerState<GuestBookingScreen> {
  SalonEntity? _salon;
  List<ServiceEntity> _services = [];
  List<ArtistEntity> _artists = [];
  bool _loading = true;
  String? _error;

  ServiceEntity? _selectedService;
  ArtistEntity? _selectedArtist;
  DateTime _selectedDate = DateTime.now().add(const Duration(days: 1));
  String? _selectedTime;
  final _phoneController = TextEditingController();
  final _nameController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _loadSalonData();
  }

  @override
  void dispose() {
    _phoneController.dispose();
    _nameController.dispose();
    super.dispose();
  }

  Future<void> _loadSalonData() async {
    setState(() { _loading = true; _error = null; });
    try {
      final salonRepo = SalonRepositoryImpl();
      final serviceRepo = ServiceRepositoryImpl();
      final artistRepo = ArtistRepositoryImpl();

      _salon = await salonRepo.getSalonBySlug(widget.slug);
      _services = await serviceRepo.getServicesBySalon(_salon!.id);
      _artists = await artistRepo.getArtistsBySalon(_salon!.id);
      setState(() { _loading = false; });
    } catch (e) {
      setState(() { _error = e.toString().replaceAll('Exception: ', ''); _loading = false; });
    }
  }

  Future<void> _bookAsGuest() async {
    if (_selectedService == null || _selectedArtist == null || _selectedTime == null) {
      setState(() => _error = 'لطفاً تمام موارد را انتخاب کنید');
      return;
    }

    if (_phoneController.text.isEmpty || _nameController.text.isEmpty) {
      setState(() => _error = 'لطفاً شماره موبایل و نام را وارد کنید');
      return;
    }

    setState(() { _loading = true; _error = null; });
    try {
      // TODO: Implement guest booking with the backend
      // For now, navigate to OTP verification
      if (!mounted) return;
      Navigator.push(
        context,
        MaterialPageRoute(
          builder: (_) => OtpScreen(phoneNumber: _phoneController.text.trim()),
        ),
      );
    } catch (e) {
      setState(() => _error = e.toString().replaceAll('Exception: ', ''));
    } finally {
      if (mounted) setState(() => _loading = false);
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
      appBar: AppBar(
        title: const Text('رزرو مهمان'),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.pushReplacement(
                context,
                MaterialPageRoute(builder: (_) => OtpScreen(phoneNumber: _phoneController.text.isNotEmpty ? _phoneController.text.trim() : '')),
              );
            },
            child: const Text('ورود', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? Center(child: Text(_error!))
              : _buildBody(),
    );
  }

  Widget _buildBody() {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _buildSalonInfo(),
          const SizedBox(height: 20),
          const Text('انتخاب خدمت:', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
          const SizedBox(height: 8),
          _buildServiceSelector(),
          const SizedBox(height: 20),
          const Text('انتخاب هنرمند:', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
          const SizedBox(height: 8),
          _buildArtistSelector(),
          const SizedBox(height: 20),
          const Text('انتخاب تاریخ:', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
          const SizedBox(height: 8),
          _buildDateSelector(),
          const SizedBox(height: 20),
          const Text('انتخاب ساعت:', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
          const SizedBox(height: 8),
          _buildTimeSelector(),
          const SizedBox(height: 20),
          const Text('اطلاعات تماس:', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
          const SizedBox(height: 8),
          _buildContactInfo(),
          const SizedBox(height: 20),
          if (_error != null)
            Container(
              padding: const EdgeInsets.all(12),
              margin: const EdgeInsets.only(bottom: 16),
              decoration: BoxDecoration(color: Colors.red.shade100, borderRadius: BorderRadius.circular(10)),
              child: Text(_error!, textAlign: TextAlign.center, style: const TextStyle(color: Colors.red)),
            ),
          ElevatedButton(
            onPressed: _loading ? null : _bookAsGuest,
            child: _loading
                ? const CircularProgressIndicator(color: Colors.white)
                : const Text('رزرو مهمان', style: TextStyle(fontSize: 18)),
          ),
          const SizedBox(height: 32),
        ],
      ),
    );
  }

  Widget _buildSalonInfo() {
    if (_salon == null) return const SizedBox();
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.lightBlue,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.primary.withValues(alpha: 0.2)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(_salon!.name, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
          if (_salon!.address != null) ...[
            const SizedBox(height: 8),
            Row(
              children: [
                const Icon(Icons.location_on_outlined, size: 16, color: AppColors.primary),
                const SizedBox(width: 8),
                Expanded(child: Text(_salon!.address!, style: const TextStyle(color: Colors.grey))),
              ],
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildServiceSelector() {
    return SizedBox(
      height: 120,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        itemCount: _services.length,
        itemBuilder: (_, i) {
          final service = _services[i];
          final isSelected = _selectedService?.id == service.id;
          return GestureDetector(
            onTap: () => setState(() => _selectedService = service),
            child: Container(
              width: 150,
              margin: const EdgeInsets.only(left: 8),
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: isSelected ? AppColors.primary : Colors.white,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: isSelected ? AppColors.primary : Colors.grey.shade300),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(service.name,
                      style: TextStyle(fontWeight: FontWeight.bold, color: isSelected ? Colors.white : Colors.black)),
                  const Spacer(),
                  Text('${service.durationMinutes} دقیقه',
                      style: TextStyle(color: isSelected ? Colors.white70 : Colors.grey, fontSize: 12)),
                  Text('${service.price.toStringAsFixed(0)} تومان',
                      style: TextStyle(color: isSelected ? Colors.white : AppColors.primary, fontWeight: FontWeight.bold)),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _buildArtistSelector() {
    return SizedBox(
      height: 100,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        itemCount: _artists.length,
        itemBuilder: (_, i) {
          final artist = _artists[i];
          final isSelected = _selectedArtist?.id == artist.id;
          return GestureDetector(
            onTap: () => setState(() => _selectedArtist = artist),
            child: Container(
              width: 100,
              margin: const EdgeInsets.only(left: 8),
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: isSelected ? AppColors.primary : Colors.white,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: isSelected ? AppColors.primary : Colors.grey.shade300),
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  CircleAvatar(
                    backgroundColor: isSelected ? Colors.white : AppColors.primary,
                    child: Text(artist.name[0],
                        style: TextStyle(color: isSelected ? AppColors.primary : Colors.white)),
                  ),
                  const SizedBox(height: 8),
                  Text(artist.name,
                      style: TextStyle(color: isSelected ? Colors.white : Colors.black, fontSize: 12),
                      textAlign: TextAlign.center),
                ],
              ),
            ),
          );
        },
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
            onTap: () => setState(() => _selectedDate = date),
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

  Widget _buildTimeSelector() {
    final times = ['09:00', '10:00', '11:00', '12:00', '13:00', '14:00', '15:00', '16:00', '17:00', '18:00'];
    return Wrap(
      spacing: 8,
      runSpacing: 8,
      children: times.map((time) {
        final isSelected = _selectedTime == time;
        return GestureDetector(
          onTap: () => setState(() => _selectedTime = time),
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
            decoration: BoxDecoration(
              color: isSelected ? AppColors.primary : Colors.white,
              borderRadius: BorderRadius.circular(10),
              border: Border.all(color: isSelected ? AppColors.primary : Colors.grey.shade300),
            ),
            child: Text(time,
                style: TextStyle(color: isSelected ? Colors.white : AppColors.dark, fontWeight: FontWeight.bold)),
          ),
        );
      }).toList(),
    );
  }

  Widget _buildContactInfo() {
    return Column(
      children: [
        TextField(
          controller: _nameController,
          decoration: const InputDecoration(
            hintText: 'نام و نام خانوادگی',
            prefixIcon: Icon(Icons.person_outline),
          ),
        ),
        const SizedBox(height: 12),
        TextField(
          controller: _phoneController,
          keyboardType: TextInputType.phone,
          decoration: const InputDecoration(
            hintText: 'شماره موبایل',
            prefixIcon: Icon(Icons.phone_android),
          ),
        ),
      ],
    );
  }
}