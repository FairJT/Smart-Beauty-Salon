import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/app_colors.dart';
import '../../../data/datasources/dio_client.dart';
import '../../../data/datasources/api_constants.dart';

class ArtistManagementScreen extends ConsumerStatefulWidget {
  const ArtistManagementScreen({super.key});

  @override
  ConsumerState<ArtistManagementScreen> createState() =>
      _ArtistManagementScreenState();
}

class _ArtistManagementScreenState
    extends ConsumerState<ArtistManagementScreen> {
  List<_ArtistInfo> _artists = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadArtists();
  }

  Future<void> _loadArtists() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final response = await DioClient.instance.get(ApiConstants.artists);
      final data = response.data as List;
      setState(() {
        _artists = data.map((j) => _ArtistInfo.fromJson(j)).toList();
        _loading = false;
      });
    } catch (e) {
      setState(() {
        _error = e.toString();
        _loading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('مدیریت هنرمندان',
            style: TextStyle(fontWeight: FontWeight.bold)),
        actions: [
          IconButton(
            icon: const Icon(Icons.person_add_outlined),
            onPressed: _showAddArtistDialog,
          ),
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? _buildError()
              : _artists.isEmpty
                  ? _buildEmpty()
                  : RefreshIndicator(
                      onRefresh: _loadArtists,
                      child: ListView.builder(
                        padding: const EdgeInsets.all(16),
                        itemCount: _artists.length,
                        itemBuilder: (_, i) => _ArtistCard(
                          artist: _artists[i],
                          onToggleActive: () => _toggleActive(_artists[i]),
                        ),
                      ),
                    ),
    );
  }

  Widget _buildEmpty() {
    return const Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.people_outline, size: 60, color: Colors.grey),
          SizedBox(height: 12),
          Text('هنوز هنرمندی اضافه نشده',
              style: TextStyle(color: Colors.grey, fontSize: 16)),
        ],
      ),
    );
  }

  Widget _buildError() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(40),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.wifi_off, size: 60, color: Colors.grey),
            const SizedBox(height: 12),
            Text(_error!,
                textAlign: TextAlign.center,
                style: const TextStyle(color: Colors.grey)),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: _loadArtists,
              child: const Text('تلاش مجدد'),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _toggleActive(_ArtistInfo artist) async {
    try {
      await DioClient.instance.put(
        '${ApiConstants.artists}/${artist.id}/toggle-active',
      );
      await _loadArtists();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
            content: Text(e.toString()), backgroundColor: AppColors.danger),
      );
    }
  }

  void _showAddArtistDialog() {
    final phoneController = TextEditingController();
    final firstNameController = TextEditingController();
    final lastNameController = TextEditingController();

    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('افزودن هنرمند'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(
              controller: phoneController,
              decoration: const InputDecoration(
                labelText: 'شماره موبایل',
                border: OutlineInputBorder(),
              ),
              keyboardType: TextInputType.phone,
            ),
            const SizedBox(height: 12),
            TextField(
              controller: firstNameController,
              decoration: const InputDecoration(
                labelText: 'نام',
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: lastNameController,
              decoration: const InputDecoration(
                labelText: 'نام خانوادگی',
                border: OutlineInputBorder(),
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('انصراف'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary),
            onPressed: () async {
              Navigator.pop(context);
              await _addArtist(
                phoneController.text,
                firstNameController.text,
                lastNameController.text,
              );
            },
            child: const Text('افزودن'),
          ),
        ],
      ),
    );
  }

  Future<void> _addArtist(
      String phone, String firstName, String lastName) async {
    try {
      await DioClient.instance.post(
        ApiConstants.artists,
        data: {
          'phoneNumber': phone,
          'firstName': firstName,
          'lastName': lastName,
        },
      );
      await _loadArtists();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
            content: Text(e.toString()), backgroundColor: AppColors.danger),
      );
    }
  }
}

class _ArtistInfo {
  final dynamic id;
  final String firstName;
  final String lastName;
  final String phoneNumber;
  final bool isActive;
  final double rating;
  final int totalAppointments;

  _ArtistInfo({
    required this.id,
    required this.firstName,
    required this.lastName,
    required this.phoneNumber,
    required this.isActive,
    required this.rating,
    required this.totalAppointments,
  });

  String get fullName => '$firstName $lastName'.trim();

  factory _ArtistInfo.fromJson(Map<String, dynamic> json) => _ArtistInfo(
        id: json['id'] ?? 0,
        firstName: json['firstName'] ?? '',
        lastName: json['lastName'] ?? '',
        phoneNumber: json['phoneNumber'] ?? '',
        isActive: json['isActive'] ?? true,
        rating: (json['rating'] ?? 0).toDouble(),
        totalAppointments: json['totalAppointments'] ?? 0,
      );
}

class _ArtistCard extends StatelessWidget {
  final _ArtistInfo artist;
  final VoidCallback onToggleActive;

  const _ArtistCard({
    required this.artist,
    required this.onToggleActive,
  });

  @override
  Widget build(BuildContext context) {
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
                CircleAvatar(
                  backgroundColor: AppColors.accent.withValues(alpha: 0.1),
                  child: Text(
                    artist.fullName.isNotEmpty ? artist.fullName[0] : '?',
                    style: const TextStyle(
                      color: AppColors.accent,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        artist.fullName,
                        style: const TextStyle(
                            fontWeight: FontWeight.bold, fontSize: 15),
                      ),
                      const SizedBox(height: 2),
                      Text(
                        artist.phoneNumber,
                        style:
                            const TextStyle(color: Colors.grey, fontSize: 13),
                      ),
                    ],
                  ),
                ),
                Column(
                  children: [
                    if (artist.rating > 0)
                      Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          const Icon(Icons.star,
                              color: AppColors.warning, size: 16),
                          const SizedBox(width: 4),
                          Text(
                            artist.rating.toStringAsFixed(1),
                            style: const TextStyle(
                                fontWeight: FontWeight.bold, fontSize: 13),
                          ),
                        ],
                      ),
                    const SizedBox(height: 4),
                    Switch(
                      value: artist.isActive,
                      onChanged: (_) => onToggleActive(),
                      thumbColor: WidgetStateProperty.resolveWith((states) => states.contains(WidgetState.selected) ? AppColors.success : null),
                    ),
                  ],
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
                  '${artist.totalAppointments} نوبت انجام شده',
                  style: const TextStyle(fontSize: 13, color: Colors.grey),
                ),
                const Spacer(),
                Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: artist.isActive
                        ? AppColors.success.withValues(alpha: 0.1)
                        : AppColors.danger.withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(
                      color: artist.isActive
                          ? AppColors.success
                          : AppColors.danger,
                    ),
                  ),
                  child: Text(
                    artist.isActive ? 'فعال' : 'غیرفعال',
                    style: TextStyle(
                      fontSize: 12,
                      fontWeight: FontWeight.bold,
                      color: artist.isActive
                          ? AppColors.success
                          : AppColors.danger,
                    ),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
