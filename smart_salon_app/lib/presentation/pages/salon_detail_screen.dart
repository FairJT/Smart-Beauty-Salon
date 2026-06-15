import 'package:flutter/material.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../core/app_colors.dart';
import '../../domain/entities/salon_entity.dart';
import '../../domain/entities/service_entity.dart';
import '../../domain/entities/artist_entity.dart';
import '../../data/repositories/salon_repository_impl.dart';
import '../../data/repositories/service_repository_impl.dart';
import '../../data/repositories/artist_repository_impl.dart';
import 'booking_screen.dart';
import 'guest_booking_screen.dart';

class SalonDetailScreen extends StatefulWidget {
  final String slug;
  const SalonDetailScreen({super.key, required this.slug});

  @override
  State<SalonDetailScreen> createState() => _SalonDetailScreenState();
}

class _SalonDetailScreenState extends State<SalonDetailScreen>
    with SingleTickerProviderStateMixin {
  SalonEntity? _salon;
  List<ServiceEntity> _services = [];
  List<ArtistEntity> _artists = [];
  bool _loading = true;
  String? _error;
  late TabController _tabController;
  ServiceEntity? _selectedService;

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
    final salon = _salon!;
    final activeServices = _services.where((s) => s.price > 0).toList();
    final activeArtists = _artists;

    return CustomScrollView(
      slivers: [
        SliverAppBar(
          expandedHeight: 200,
          pinned: true,
          flexibleSpace: FlexibleSpaceBar(
            title: Text(
              salon.name,
              style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
            ),
            background: Container(
              decoration: BoxDecoration(
                gradient: AppColors.darkGradient,
              ),
              child: salon.imageUrl != null
                  ? CachedNetworkImage(
                      imageUrl: salon.imageUrl!,
                      fit: BoxFit.cover,
                      color: Colors.black26,
                      colorBlendMode: BlendMode.darken,
                    )
                  : const Center(
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
                if (salon.address != null) _infoRow(Icons.location_on_outlined, salon.address!),
                if (salon.phoneNumber != null) _infoRow(Icons.phone_outlined, salon.phoneNumber!),
                _infoRow(Icons.star_outline, 'امتیاز: ${salon.rating.toStringAsFixed(1)}'),
                if (salon.description != null) ...[
                  const SizedBox(height: 12),
                  Text(salon.description!, style: const TextStyle(color: Colors.grey, fontSize: 14, height: 1.6)),
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
                if (_selectedService != null)
                  Container(
                    padding: const EdgeInsets.all(10),
                    decoration: BoxDecoration(
                      color: AppColors.primary.withValues(alpha: 0.1),
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Row(
                      children: [
                        const Icon(Icons.info_outline, color: AppColors.primary, size: 18),
                        const SizedBox(width: 8),
                        Expanded(
                          child: Text(
                            'خدمت «${_selectedService!.name}» انتخاب شد. حالا پرسنل را انتخاب کنید.',
                            style: const TextStyle(color: AppColors.primary, fontSize: 13),
                          ),
                        ),
                        GestureDetector(
                          onTap: () => setState(() => _selectedService = null),
                          child: const Icon(Icons.close, color: AppColors.primary, size: 18),
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
                      _buildServices(activeServices),
                      _buildArtists(activeArtists),
                    ],
                  ),
                ),
                const SizedBox(height: 20),
                SizedBox(
                  width: double.infinity,
                  child: OutlinedButton.icon(
                    style: OutlinedButton.styleFrom(
                      foregroundColor: AppColors.primary,
                      side: const BorderSide(color: AppColors.primary),
                      minimumSize: const Size(double.infinity, 52),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    ),
                    icon: const Icon(Icons.person_outline),
                    label: const Text('رزرو مهمان', style: TextStyle(fontSize: 16)),
                    onPressed: () {
                      Navigator.push(
                        context,
                        MaterialPageRoute(
                          builder: (_) => GuestBookingScreen(slug: widget.slug),
                        ),
                      );
                    },
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

  Widget _infoRow(IconData icon, String text) {
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

  Widget _buildServices(List<ServiceEntity> services) {
    if (services.isEmpty) {
      return const Center(child: Text('خدماتی ثبت نشده', style: TextStyle(color: Colors.grey)));
    }

    return ListView.builder(
      itemCount: services.length,
      itemBuilder: (_, i) {
        final s = services[i];
        final isSelected = _selectedService?.id == s.id;

        return Card(
          margin: const EdgeInsets.only(bottom: 8),
          color: isSelected ? AppColors.primary.withValues(alpha: 0.1) : null,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
            side: isSelected ? const BorderSide(color: AppColors.primary) : BorderSide.none,
          ),
          child: ListTile(
            leading: CircleAvatar(
              backgroundColor: isSelected ? AppColors.primary : AppColors.primary.withValues(alpha: 0.1),
              child: Icon(Icons.spa_outlined, color: isSelected ? Colors.white : AppColors.primary, size: 20),
            ),
            title: Text(s.name, style: TextStyle(fontWeight: isSelected ? FontWeight.bold : FontWeight.normal)),
            subtitle: Text('${s.durationMinutes} دقیقه - ${s.price.toStringAsFixed(0)} تومان'),
            trailing: isSelected
                ? const Icon(Icons.check_circle, color: AppColors.primary)
                : const Icon(Icons.arrow_back_ios, size: 14, color: Colors.grey),
            onTap: () {
              setState(() => _selectedService = s);
              _tabController.animateTo(1);
            },
          ),
        );
      },
    );
  }

  Widget _buildArtists(List<ArtistEntity> allArtists) {
    if (allArtists.isEmpty) {
      return const Center(child: Text('پرسنلی ثبت نشده', style: TextStyle(color: Colors.grey)));
    }

    if (_selectedService == null) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.touch_app_outlined, size: 50, color: Colors.grey.shade400),
            const SizedBox(height: 12),
            const Text('ابتدا از تب «خدمات» یک خدمت انتخاب کنید',
                style: TextStyle(color: Colors.grey), textAlign: TextAlign.center),
          ],
        ),
      );
    }

    return ListView.builder(
      itemCount: allArtists.length,
      itemBuilder: (_, i) {
        final a = allArtists[i];

        return Card(
          margin: const EdgeInsets.only(bottom: 8),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
          child: ListTile(
            leading: CircleAvatar(
              backgroundColor: AppColors.primary,
              backgroundImage: a.profileImageUrl != null ? CachedNetworkImageProvider(a.profileImageUrl!) : null,
              child: a.profileImageUrl == null
                  ? Text(a.name[0], style: const TextStyle(color: Colors.white))
                  : null,
            ),
            title: Text(a.name, style: const TextStyle(fontWeight: FontWeight.bold)),
            subtitle: Text(a.specialization ?? ''),
            trailing: const Icon(Icons.arrow_back_ios, size: 14, color: Colors.grey),
            onTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (_) => BookingScreen(
                    slug: widget.slug,
                    artistId: a.id,
                    artistName: a.name,
                    serviceId: _selectedService!.id,
                    serviceName: _selectedService!.name,
                    durationMinutes: _selectedService!.durationMinutes,
                    price: _selectedService!.price,
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
