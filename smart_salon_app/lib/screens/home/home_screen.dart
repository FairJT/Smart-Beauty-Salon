import 'package:flutter/material.dart';
import '../../core/api_constants.dart';
import '../../core/api_service.dart';
import '../../core/app_colors.dart';
import '../auth/login_screen.dart';
import '../salon/salon_detail_screen.dart';
import '../profile/profile_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  List<dynamic> _salons = [];
  bool _loading = true;
  String? _error;
  int _currentTab = 0;
  final TextEditingController _searchController = TextEditingController();
  String _searchQuery = '';
  bool _vipOnly = false;
String _serviceFilter = '';

  @override
  void initState() {
    super.initState();
    _loadSalons();
  }

  Future<void> _loadSalons({String? search}) async {
  setState(() { _loading = true; _error = null; });
  try {
    var params = <String>[];
    if (search != null && search.isNotEmpty)
      params.add('search=$search');
    if (_serviceFilter.isNotEmpty)
      params.add('service=$_serviceFilter');
    if (_vipOnly)
      params.add('vipOnly=true');

    final url = params.isEmpty
        ? ApiConstants.salons
        : '${ApiConstants.salons}?${params.join('&')}';

    final res = await ApiService.get(url);
    setState(() => _salons = res['data'] ?? []);
  } catch (e) {
    setState(() => _error = e.toString().replaceAll('Exception: ', ''));
  } finally {
    setState(() => _loading = false);
  }
}

  Future<void> _logout() async {
    await ApiService.clearToken();
    if (!mounted) return;
    Navigator.pushReplacement(
      context,
      MaterialPageRoute(builder: (_) => const LoginScreen()),
    );
  }

  void _showSearchDialog() {
    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('جستجوی سالن'),
        content: TextField(
          controller: _searchController,
          autofocus: true,
          decoration: const InputDecoration(
            hintText: 'نام سالن...',
            prefixIcon: Icon(Icons.search),
            border: OutlineInputBorder(),
          ),
          onSubmitted: (val) {
            Navigator.pop(context);
            setState(() => _searchQuery = val);
            _loadSalons(search: val);
          },
        ),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              _searchController.clear();
              setState(() => _searchQuery = '');
              _loadSalons();
            },
            child: const Text('پاک کردن'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(context);
              setState(() => _searchQuery = _searchController.text);
              _loadSalons(search: _searchController.text);
            },
            child: const Text('جستجو'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text(
          'سالن هوشمند ابری',
          style: TextStyle(fontWeight: FontWeight.bold),
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.search),
            onPressed: _showSearchDialog,
          ),
          IconButton(icon: const Icon(Icons.logout), onPressed: _logout),
        ],
      ),
      body: _currentTab == 0
          ? _buildHome()
          : _currentTab == 1
              ? _buildAppointments()
              : const ProfileScreen(),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _currentTab,
        onDestinationSelected: (i) => setState(() => _currentTab = i),
        destinations: const [
          NavigationDestination(
            icon: Icon(Icons.home_outlined),
            selectedIcon: Icon(Icons.home),
            label: 'خانه',
          ),
          NavigationDestination(
            icon: Icon(Icons.calendar_month_outlined),
            selectedIcon: Icon(Icons.calendar_month),
            label: 'رزروهای من',
          ),
          NavigationDestination(
            icon: Icon(Icons.person_outline),
            selectedIcon: Icon(Icons.person),
            label: 'پروفایل',
          ),
        ],
      ),
    );
  }

  Widget _buildHome() {
    return RefreshIndicator(
      onRefresh: _loadSalons,
      child: ListView(
        children: [
          _buildBanner(),
          _buildFilters(),
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
            child: Row(
              children: [
                const Text(
                  'سالن‌های برتر',
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
                if (_searchQuery.isNotEmpty) ...[
                  const SizedBox(width: 8),
                  Chip(
                    label: Text(_searchQuery),
                    deleteIcon: const Icon(Icons.close, size: 16),
                    onDeleted: () {
                      _searchController.clear();
                      setState(() => _searchQuery = '');
                      _loadSalons();
                    },
                  ),
                ]
              ],
            ),
          ),
          if (_loading)
            const Center(
              child: Padding(
                padding: EdgeInsets.all(40),
                child: CircularProgressIndicator(),
              ),
            )
          else if (_error != null)
            _buildError()
          else if (_salons.isEmpty)
            const Center(
              child: Padding(
                padding: EdgeInsets.all(40),
                child: Text(
                  'سالنی یافت نشد',
                  style: TextStyle(color: Colors.grey),
                ),
              ),
            )
          else
            ...(_salons.map((s) => _buildSalonCard(s))),
        ],
      ),
    );
  }

  Widget _buildFilters() {
  return SingleChildScrollView(
    scrollDirection: Axis.horizontal,
    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
    child: Row(
      children: [
        FilterChip(
          label: const Text('⭐ فقط VIP'),
          selected: _vipOnly,
          onSelected: (val) {
            setState(() => _vipOnly = val);
            _loadSalons(search: _searchQuery.isNotEmpty ? _searchQuery : null);
          },
        ),
        const SizedBox(width: 8),
        ...[
          'رنگ مو', 'کوتاهی', 'مانیکور', 'پدیکور', 'ابرو'
        ].map((s) => Padding(
          padding: const EdgeInsets.only(left: 8),
          child: FilterChip(
            label: Text(s),
            selected: _serviceFilter == s,
            onSelected: (val) {
              setState(() => _serviceFilter = val ? s : '');
              _loadSalons(search: _searchQuery.isNotEmpty ? _searchQuery : null);
            },
          ),
        )),
      ],
    ),
  );
}


  Widget _buildBanner() {
    return Container(
      margin: const EdgeInsets.all(16),
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [AppColors.primary, Color(0xFF2C5F8A)],
        ),
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('خوش آمدید 👋',
              style: TextStyle(color: Colors.white70, fontSize: 14)),
          const SizedBox(height: 6),
          const Text(
            'سالن مناسب خود را پیدا کنید',
            style: TextStyle(
                color: Colors.white,
                fontSize: 20,
                fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 16),
          ElevatedButton.icon(
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.amber,
              foregroundColor: Colors.white,
              minimumSize: const Size(0, 42),
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10)),
            ),
            icon: const Icon(Icons.calendar_today, size: 18),
            label: const Text('رزرو نوبت'),
            onPressed: _showSearchDialog,
          ),
        ],
      ),
    );
  }

  Widget _buildSalonCard(dynamic salon) {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: ListTile(
        contentPadding:
            const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        leading: CircleAvatar(
          radius: 28,
          backgroundColor: AppColors.primary,
          child: Text(
            (salon['name'] as String).isNotEmpty
                ? (salon['name'] as String)[0]
                : '؟',
            style: const TextStyle(
                color: Colors.white,
                fontSize: 22,
                fontWeight: FontWeight.bold),
          ),
        ),
        title: Row(
          children: [
            Expanded(
              child: Text(salon['name'] ?? '',
                  style: const TextStyle(fontWeight: FontWeight.bold)),
            ),
            if (salon['isVip'] == true)
              const Icon(Icons.verified, color: Colors.amber, size: 18),
          ],
        ),
        subtitle: Text(
          salon['address'] ?? '',
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
          style: const TextStyle(color: Colors.grey),
        ),
        trailing: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.star_rounded, color: Colors.amber, size: 18),
            Text(
              (salon['ratingAvg'] ?? 0.0).toStringAsFixed(1),
              style: const TextStyle(
                  fontWeight: FontWeight.bold, fontSize: 13),
            ),
          ],
        ),
        onTap: () => Navigator.push(
          context,
          MaterialPageRoute(
              builder: (_) => SalonDetailScreen(salonId: salon['id'])),
        ),
      ),
    );
  }

  Widget _buildError() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(40),
        child: Column(
          children: [
            const Icon(Icons.wifi_off, size: 60, color: Colors.grey),
            const SizedBox(height: 12),
            Text(_error!,
                textAlign: TextAlign.center,
                style: const TextStyle(color: Colors.grey)),
            const SizedBox(height: 16),
            ElevatedButton(
                onPressed: _loadSalons, child: const Text('تلاش مجدد')),
          ],
        ),
      ),
    );
  }

  Widget _buildAppointments() {
    return FutureBuilder(
      future: ApiService.get(ApiConstants.myAppointments),
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const Center(child: CircularProgressIndicator());
        }

        if (snapshot.hasError) {
          return Center(
            child: Text(
              snapshot.error.toString().replaceAll('Exception: ', ''),
              style: const TextStyle(color: Colors.grey),
            ),
          );
        }

        final list = snapshot.data as List? ?? [];

        if (list.isEmpty) {
          return const Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(Icons.calendar_today_outlined,
                    size: 60, color: Colors.grey),
                SizedBox(height: 12),
                Text('هنوز رزروی ندارید',
                    style: TextStyle(color: Colors.grey, fontSize: 16)),
              ],
            ),
          );
        }

        return ListView.builder(
          padding: const EdgeInsets.all(16),
          itemCount: list.length,
          itemBuilder: (_, i) {
            final a = list[i];
            final status = a['status'] as int;
            final statusColor = _statusColor(status);
            final start = DateTime.parse(a['startTime']);
            final isRated = a['isRated'] == true || a['isRated'] == 1;

            return Card(
              margin: const EdgeInsets.only(bottom: 12),
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12)),
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            a['salonName'] ?? '',
                            style: const TextStyle(
                                fontWeight: FontWeight.bold, fontSize: 16),
                          ),
                        ),
                        Container(
                          padding: const EdgeInsets.symmetric(
                              horizontal: 10, vertical: 4),
                          decoration: BoxDecoration(
                            color: statusColor.withOpacity(0.1),
                            borderRadius: BorderRadius.circular(20),
                            border: Border.all(color: statusColor),
                          ),
                          child: Text(
                            _statusText(status),
                            style: TextStyle(
                                color: statusColor,
                                fontSize: 12,
                                fontWeight: FontWeight.bold),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 8),
                    const Divider(),
                    const SizedBox(height: 8),
                    _appointmentRow(
                        Icons.spa_outlined, a['serviceName'] ?? ''),
                    _appointmentRow(
                        Icons.person_outline, a['artistName'] ?? ''),
                    _appointmentRow(
                      Icons.calendar_today_outlined,
                      '${start.year}/${start.month}/${start.day}  ساعت  ${start.hour}:${start.minute.toString().padLeft(2, '0')}',
                    ),
                    _appointmentRow(
                      Icons.attach_money,
                      '${a['estimatedPrice']} تومان  |  بیعانه: ${a['depositAmount']} تومان',
                    ),
                    if (status == 1 || status == 2)
                      Padding(
                        padding: const EdgeInsets.only(top: 12),
                        child: SizedBox(
                          width: double.infinity,
                          child: OutlinedButton.icon(
                            style: OutlinedButton.styleFrom(
                              foregroundColor: Colors.red,
                              side: const BorderSide(color: Colors.red),
                              shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(8)),
                            ),
                            icon: const Icon(Icons.cancel_outlined, size: 18),
                            label: const Text('لغو نوبت'),
                            onPressed: () =>
                                _cancelAppointment(a['id'], context),
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
                              backgroundColor: Colors.amber,
                              foregroundColor: Colors.white,
                              shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(8)),
                            ),
                            icon: const Icon(Icons.star, size: 18),
                            label: const Text('ثبت امتیاز'),
                            onPressed: () =>
                                _showRateDialog(a['id'], context),
                          ),
                        ),
                      ),
                    if (status == 4 && isRated)
                      Padding(
                        padding: const EdgeInsets.only(top: 8),
                        child: Row(
                          children: [
                            const Icon(Icons.star,
                                color: Colors.amber, size: 18),
                            const SizedBox(width: 4),
                            Text(
                              'امتیاز شما: ${a['rating']}',
                              style: const TextStyle(color: Colors.amber),
                            ),
                          ],
                        ),
                      ),
                  ],
                ),
              ),
            );
          },
        );
      },
    );
  }

  Future<void> _cancelAppointment(int id, BuildContext context) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('لغو نوبت'),
        content: const Text('آیا مطمئنید می‌خواهید این نوبت را لغو کنید؟'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('خیر'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () => Navigator.pop(context, true),
            child: const Text('بله، لغو کن'),
          ),
        ],
      ),
    );

    if (confirm != true) return;

    try {
      await ApiService.put(
          '${ApiConstants.appointments}/$id/cancel', {});
      if (!context.mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('نوبت با موفقیت لغو شد'),
          backgroundColor: Colors.green,
        ),
      );
      setState(() {});
    } catch (e) {
      if (!context.mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(e.toString().replaceAll('Exception: ', '')),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  Future<void> _showRateDialog(int id, BuildContext context) async {
    int selectedRating = 5;
    final commentController = TextEditingController();

    await showDialog(
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
                    onTap: () =>
                        setStateDialog(() => selectedRating = i + 1),
                    child: Icon(
                      i < selectedRating
                          ? Icons.star
                          : Icons.star_border,
                      color: Colors.amber,
                      size: 36,
                    ),
                  );
                }),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: commentController,
                decoration: const InputDecoration(
                  labelText: 'نظر شما (اختیاری)',
                  border: OutlineInputBorder(),
                ),
                maxLines: 2,
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('انصراف'),
            ),
            ElevatedButton(
              style: ElevatedButton.styleFrom(backgroundColor: Colors.amber),
              onPressed: () async {
                Navigator.pop(context);
                try {
                  await ApiService.post(
                    '${ApiConstants.appointments}/$id/rate',
                    {
                      'rating': selectedRating,
                      'comment': commentController.text,
                    },
                  );
                  if (!context.mounted) return;
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(
                      content: Text('امتیاز با موفقیت ثبت شد ⭐'),
                      backgroundColor: Colors.amber,
                    ),
                  );
                  setState(() {});
                } catch (e) {
                  if (!context.mounted) return;
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(content: Text(e.toString())),
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

  Widget _appointmentRow(IconData icon, String text) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 3),
      child: Row(
        children: [
          Icon(icon, size: 16, color: AppColors.primary),
          const SizedBox(width: 8),
          Expanded(
              child: Text(text, style: const TextStyle(fontSize: 14))),
        ],
      ),
    );
  }

  String _statusText(int status) {
    switch (status) {
      case 1: return 'در انتظار';
      case 2: return 'تایید شده';
      case 3: return 'در حال انجام';
      case 4: return 'تمام شده';
      case 5: return 'لغو شده';
      case 6: return 'حضور نیافت';
      default: return 'نامشخص';
    }
  }

  Color _statusColor(int status) {
    switch (status) {
      case 1: return Colors.orange;
      case 2: return Colors.green;
      case 3: return Colors.blue;
      case 4: return Colors.grey;
      case 5: return Colors.red;
      case 6: return Colors.red;
      default: return Colors.grey;
    }
  }
}
