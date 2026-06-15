import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/app_colors.dart';
import '../../providers/auth_provider.dart';
import '../../providers/salon_provider.dart';
import '../../providers/notification_provider.dart';
import '../auth/login_screen.dart';
import '../salon/salon_detail_screen.dart';
import '../profile/profile_screen.dart';
import 'widgets/salon_card.dart';
import 'widgets/appointment_list.dart';
import 'widgets/search_dialog.dart';

class HomeScreen extends ConsumerStatefulWidget {
  const HomeScreen({super.key});

  @override
  ConsumerState<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends ConsumerState<HomeScreen> {
  int _currentTab = 0;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(salonListProvider.notifier).load();
      ref.read(notificationProvider.notifier).load();
    });
  }

  Future<void> _logout() async {
    await ref.read(authProvider.notifier).logout();
    if (!mounted) return;
    Navigator.pushReplacement(
      context,
      MaterialPageRoute(builder: (_) => const LoginScreen()),
    );
  }

  @override
  Widget build(BuildContext context) {
    final salonState = ref.watch(salonListProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('سالن هوشمند ابری',
            style: TextStyle(fontWeight: FontWeight.bold)),
        actions: [
          IconButton(
            icon: const Icon(Icons.search),
            onPressed: () => showSearchDialog(context, ref),
          ),
          IconButton(icon: const Icon(Icons.logout), onPressed: _logout),
        ],
      ),
      body: _currentTab == 0
          ? _buildHome(salonState)
          : _currentTab == 1
              ? const AppointmentList()
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

  Widget _buildHome(SalonListState salonState) {
    return RefreshIndicator(
      onRefresh: () => ref.read(salonListProvider.notifier).load(),
      child: ListView(
        children: [
          _buildBanner(),
          _buildFilters(salonState),
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
            child: Row(
              children: [
                const Text('سالن‌های برتر',
                    style:
                        TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                if (salonState.searchQuery.isNotEmpty) ...[
                  const SizedBox(width: 8),
                  Chip(
                    label: Text(salonState.searchQuery),
                    deleteIcon: const Icon(Icons.close, size: 16),
                    onDeleted: () {
                      ref.read(salonListProvider.notifier).setSearch('');
                    },
                  ),
                ],
              ],
            ),
          ),
          if (salonState.loading)
            const Padding(
              padding: EdgeInsets.all(40),
              child: Center(child: CircularProgressIndicator()),
            )
          else if (salonState.error != null)
            _buildError(salonState.error!)
          else if (salonState.salons.isEmpty)
            const Padding(
              padding: EdgeInsets.all(40),
              child: Center(
                child: Text('سالنی یافت نشد',
                    style: TextStyle(color: Colors.grey)),
              ),
            )
          else
            ...salonState.salons.map((s) => SalonCard(
                  salon: s,
                  onTap: () => Navigator.push(
                    context,
                    MaterialPageRoute(
                        builder: (_) => SalonDetailScreen(salonId: s.id)),
                  ),
                )),
        ],
      ),
    );
  }

  Widget _buildFilters(SalonListState salonState) {
    return SingleChildScrollView(
      scrollDirection: Axis.horizontal,
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
      child: Row(
        children: [
          FilterChip(
            label: const Text('فقط VIP'),
            selected: salonState.vipOnly,
            onSelected: (_) => ref.read(salonListProvider.notifier).toggleVip(),
          ),
          const SizedBox(width: 8),
          ...['رنگ مو', 'کوتاهی', 'مانیکور', 'پدیکور', 'ابро']
              .map((s) => Padding(
                    padding: const EdgeInsets.only(left: 8),
                    child: FilterChip(
                      label: Text(s),
                      selected: salonState.serviceFilter == s,
                      onSelected: (_) {
                        final notifier = ref.read(salonListProvider.notifier);
                        notifier.setServiceFilter(
                            salonState.serviceFilter == s ? '' : s);
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
            colors: [AppColors.primary, Color(0xFF2C5F8A)]),
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('خوش آمدید',
              style: TextStyle(color: Colors.white70, fontSize: 14)),
          const SizedBox(height: 6),
          const Text(
            'سالن مناسب خود را پیدا کنید',
            style: TextStyle(
                color: Colors.white, fontSize: 20, fontWeight: FontWeight.bold),
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
            onPressed: () => showSearchDialog(context, ref),
          ),
        ],
      ),
    );
  }

  Widget _buildError(String error) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(40),
        child: Column(
          children: [
            const Icon(Icons.wifi_off, size: 60, color: Colors.grey),
            const SizedBox(height: 12),
            Text(error,
                textAlign: TextAlign.center,
                style: const TextStyle(color: Colors.grey)),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: () => ref.read(salonListProvider.notifier).load(),
              child: const Text('تلاش مجدد'),
            ),
          ],
        ),
      ),
    );
  }
}
