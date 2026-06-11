import 'package:flutter/material.dart';
import '../../core/api_constants.dart';
import '../../core/api_service.dart';
import '../../core/app_colors.dart';
import '../booking/booking_screen.dart';

class SalonDetailScreen extends StatefulWidget {
  final int salonId;
  const SalonDetailScreen({super.key, required this.salonId});

  @override
  State<SalonDetailScreen> createState() => _SalonDetailScreenState();
}

class _SalonDetailScreenState extends State<SalonDetailScreen>
    with SingleTickerProviderStateMixin {
  dynamic _salon;
  bool _loading = true;
  String? _error;
  late TabController _tabController;
  dynamic _selectedService;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
    _loadSalon();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<void> _loadSalon() async {
    setState(() { _loading = true; _error = null; });
    try {
      final res = await ApiService.get('${ApiConstants.salons}/${widget.salonId}');
      setState(() => _salon = res);
    } catch (e) {
      setState(() => _error = e.toString().replaceAll('Exception: ', ''));
    } finally {
      setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? Center(child: Text(_error!))
              : _buildBody(),
    );
  }

  Widget _buildBody() {
    return CustomScrollView(
      slivers: [
        SliverAppBar(
          expandedHeight: 200,
          pinned: true,
          flexibleSpace: FlexibleSpaceBar(
            title: Text(
              _salon['name'] ?? '',
              style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
            ),
            background: Container(
              decoration: const BoxDecoration(
                gradient: LinearGradient(
                  colors: [AppColors.primary, Color(0xFF2C5F8A)],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                ),
              ),
              child: const Center(
                child: Icon(Icons.content_cut_rounded, size: 80, color: Colors.white24),
              ),
            ),
          ),
        ),

        SliverToBoxAdapter(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                if (_salon['address'] != null)
                  _buildInfoRow(Icons.location_on_outlined, _salon['address']),
                if (_salon['phone'] != null)
                  _buildInfoRow(Icons.phone_outlined, _salon['phone']),
                _buildInfoRow(
                  Icons.star_outline,
                  'امتیاز: ${(_salon['ratingAvg'] ?? 0.0).toStringAsFixed(1)}',
                ),
                if (_salon['description'] != null) ...[
                  const SizedBox(height: 12),
                  Text(
                    _salon['description'],
                    style: const TextStyle(color: Colors.grey, fontSize: 14, height: 1.6),
                  ),
                ],

                const SizedBox(height: 20),

                TabBar(
                  controller: _tabController,
                  labelColor: AppColors.primary,
                  unselectedLabelColor: Colors.grey,
                  indicatorColor: AppColors.primary,
                  tabs: const [Tab(text: 'خدمات'), Tab(text: 'پرسنل')],
                ),

                const SizedBox(height: 16),

                // راهنما
                if (_selectedService != null)
                  Container(
                    padding: const EdgeInsets.all(10),
                    decoration: BoxDecoration(
                      color: AppColors.primary.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Row(
                      children: [
                        const Icon(Icons.info_outline,
                            color: AppColors.primary, size: 18),
                        const SizedBox(width: 8),
                        Expanded(
                          child: Text(
                            'خدمت «${_selectedService['name']}» انتخاب شد. حالا پرسنل را انتخاب کنید.',
                            style: const TextStyle(
                                color: AppColors.primary, fontSize: 13),
                          ),
                        ),
                        GestureDetector(
                          onTap: () => setState(() => _selectedService = null),
                          child: const Icon(Icons.close,
                              color: AppColors.primary, size: 18),
                        ),
                      ],
                    ),
                  ),

                if (_selectedService != null) const SizedBox(height: 8),

                SizedBox(
                  height: 350,
                  child: TabBarView(
                    controller: _tabController,
                    children: [
                      _buildServices(),
                      _buildArtists(),
                    ],
                  ),
                ),

                const SizedBox(height: 80),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildInfoRow(IconData icon, String text) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        children: [
          Icon(icon, size: 18, color: AppColors.primary),
          const SizedBox(width: 8),
          Expanded(child: Text(text, style: const TextStyle(fontSize: 14))),
        ],
      ),
    );
  }

  // ─── لیست خدمات ───────────────────────────────────────
  Widget _buildServices() {
    final services = (_salon['services'] as List? ?? [])
        .where((s) => s['isActive'] == true)
        .toList();

    if (services.isEmpty) {
      return const Center(
        child: Text('خدماتی ثبت نشده', style: TextStyle(color: Colors.grey)),
      );
    }

    return ListView.builder(
      itemCount: services.length,
      itemBuilder: (_, i) {
        final s = services[i];
        final isSelected = _selectedService != null &&
            _selectedService['id'] == s['id'];

        return Card(
          margin: const EdgeInsets.only(bottom: 8),
          color: isSelected ? AppColors.primary.withOpacity(0.1) : null,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
            side: isSelected
                ? const BorderSide(color: AppColors.primary)
                : BorderSide.none,
          ),
          child: ListTile(
            leading: CircleAvatar(
              backgroundColor: isSelected
                  ? AppColors.primary
                  : AppColors.primary.withOpacity(0.1),
              child: Icon(
                Icons.spa_outlined,
                color: isSelected ? Colors.white : AppColors.primary,
                size: 20,
              ),
            ),
            title: Text(
              s['name'] ?? '',
              style: TextStyle(
                fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
              ),
            ),
            subtitle: Text('${s['baseDurationMinutes']} دقیقه'),
            trailing: isSelected
                ? const Icon(Icons.check_circle, color: AppColors.primary)
                : const Icon(Icons.arrow_forward_ios,
                    size: 14, color: Colors.grey),
            onTap: () {
              setState(() => _selectedService = s);
              // رفتن به تب پرسنل
              _tabController.animateTo(1);
            },
          ),
        );
      },
    );
  }

  // ─── لیست پرسنل ───────────────────────────────────────
  Widget _buildArtists() {
    final allArtists = (_salon['artists'] as List? ?? [])
        .where((a) => a['isActive'] == true)
        .toList();

    if (allArtists.isEmpty) {
      return const Center(
        child: Text('پرسنلی ثبت نشده', style: TextStyle(color: Colors.grey)),
      );
    }

    if (_selectedService == null) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.touch_app_outlined,
                size: 50, color: Colors.grey.shade400),
            const SizedBox(height: 12),
            const Text(
              'ابتدا از تب «خدمات» یک خدمت انتخاب کنید',
              style: TextStyle(color: Colors.grey),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      );
    }

    return ListView.builder(
      itemCount: allArtists.length,
      itemBuilder: (_, i) {
        final a = allArtists[i];
        final user = a['user'];
        final name = user != null
            ? '${user['firstName']} ${user['lastName']}'
            : 'نامشخص';

        return Card(
          margin: const EdgeInsets.only(bottom: 8),
          shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(12)),
          child: ListTile(
            leading: CircleAvatar(
              backgroundColor: AppColors.primary,
              backgroundImage: a['photoUrl'] != null
                  ? NetworkImage(
                      '${ApiConstants.baseUrl.replaceAll('/api', '')}${a['photoUrl']}')
                  : null,
              child: a['photoUrl'] == null
                  ? Text(
                      name.isNotEmpty ? name[0] : '؟',
                      style: const TextStyle(color: Colors.white),
                    )
                  : null,
            ),
            title: Text(name,
                style: const TextStyle(fontWeight: FontWeight.bold)),
            subtitle: Text(a['bioShort'] ?? ''),
            trailing: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(Icons.star_rounded,
                    color: Colors.amber, size: 16),
                Text((a['ratingAvg'] ?? 0.0).toStringAsFixed(1)),
                const SizedBox(width: 4),
                const Icon(Icons.arrow_forward_ios,
                    size: 14, color: Colors.grey),
              ],
            ),
            onTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (_) => BookingScreen(
                    salonId:         _salon['id'],
                    artistId:        a['id'],
                    artistName:      name,
                    serviceId:       _selectedService['id'],
                    serviceName:     _selectedService['name'],
                    durationMinutes: _selectedService['baseDurationMinutes'],
                    price: (_selectedService['basePrice'] as num).toDouble(),
                  ),
                ),
              );
            },
          ),
        );
      },
    );
  }
}