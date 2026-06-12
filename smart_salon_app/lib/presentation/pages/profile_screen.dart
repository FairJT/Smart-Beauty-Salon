import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/app_colors.dart';
import '../../domain/entities/user_entity.dart';
import '../providers/auth_provider.dart';
import '../providers/notification_provider.dart';
import 'login_screen.dart';
import 'notifications_screen.dart';

class ProfileScreen extends ConsumerStatefulWidget {
  const ProfileScreen({super.key});

  @override
  ConsumerState<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends ConsumerState<ProfileScreen> {
  UserEntity? _user;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _loadProfile();
  }

  Future<void> _loadProfile() async {
    try {
      final authState = ref.read(authProvider);
      setState(() { _user = authState.user; _loading = false; });
    } catch (_) {
      setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final notifState = ref.watch(notificationProvider);

    return RefreshIndicator(
      onRefresh: () async { await _loadProfile(); await ref.read(notificationProvider.notifier).load(); },
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          const SizedBox(height: 20),
          _buildProfileHeader(),
          const SizedBox(height: 16),
          if (!_loading && _user != null) _buildLoyaltyCard(),
          const SizedBox(height: 16),
          if (!_loading && _user != null) _buildLoyaltyLevel(_user!.loyaltyPoints ?? 0),
          const SizedBox(height: 16),
          _buildMenuItem(icon: Icons.calendar_month, title: 'رزروهای من', onTap: () {}),
          Card(
            margin: const EdgeInsets.only(bottom: 8),
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            child: ListTile(
              leading: Stack(
                clipBehavior: Clip.none,
                children: [
                  const Icon(Icons.notifications_outlined, color: AppColors.primary),
                  if (notifState.unreadCount > 0)
                    Positioned(
                      right: -4,
                      top: -4,
                      child: Container(
                        padding: const EdgeInsets.all(2),
                        decoration: const BoxDecoration(color: Colors.red, shape: BoxShape.circle),
                        constraints: const BoxConstraints(minWidth: 16, minHeight: 16),
                        child: Text('${notifState.unreadCount}',
                            style: const TextStyle(color: Colors.white, fontSize: 10),
                            textAlign: TextAlign.center),
                      ),
                    ),
                ],
              ),
              title: const Text('اعلان‌ها'),
              trailing: const Icon(Icons.arrow_back_ios, size: 16),
              onTap: () => Navigator.push(context,
                  MaterialPageRoute(builder: (_) => const NotificationsScreen())),
            ),
          ),
          _buildMenuItem(icon: Icons.help_outline, title: 'راهنما', onTap: () {}),
          _buildMenuItem(icon: Icons.info_outline, title: 'درباره ما', onTap: () {}),
          const SizedBox(height: 20),
          ElevatedButton.icon(
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.danger,
              foregroundColor: Colors.white,
              minimumSize: const Size(double.infinity, 52),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            ),
            icon: const Icon(Icons.logout),
            label: const Text('خروج از حساب', style: TextStyle(fontSize: 16)),
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
    );
  }

  Widget _buildProfileHeader() {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: AppColors.heroGradient,
        borderRadius: BorderRadius.circular(20),
      ),
      child: _loading
          ? const Center(child: CircularProgressIndicator(color: Colors.white))
          : Column(
              children: [
                Container(
                  width: 80,
                  height: 80,
                  decoration: BoxDecoration(
                    color: Colors.white.withValues(alpha: 0.2),
                    borderRadius: BorderRadius.circular(40),
                  ),
                  child: const Icon(Icons.person, size: 50, color: Colors.white),
                ),
                const SizedBox(height: 12),
                Text(
                  _user?.fullName ?? '',
                  style: const TextStyle(color: Colors.white, fontSize: 20, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 4),
                Text(_user?.phoneNumber ?? '', style: const TextStyle(color: Colors.white70, fontSize: 14)),
              ],
            ),
    );
  }

  Widget _buildLoyaltyCard() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: AppColors.primaryGradient,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Row(
        children: [
          const Icon(Icons.stars, color: Colors.white, size: 40),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text('امتیاز وفاداری شما', style: TextStyle(color: Colors.white70, fontSize: 13)),
                Text('${_user?.loyaltyPoints ?? 0} امتیاز',
                    style: const TextStyle(color: Colors.white, fontSize: 24, fontWeight: FontWeight.bold)),
              ],
            ),
          ),
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              const Text('تعداد مراجعات', style: TextStyle(color: Colors.white70, fontSize: 12)),
              Text('${_user?.totalVisits ?? 0} بار',
                  style: const TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.bold)),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildLoyaltyLevel(int points) {
    String level;
    Color color;
    String nextLevel;
    int pointsNeeded;
    double progress;

    if (points >= 500) {
      level = 'الماس'; color = Colors.blue; nextLevel = 'بالاترین سطح'; pointsNeeded = 0; progress = 1.0;
    } else if (points >= 200) {
      level = 'طلایی'; color = Colors.amber; nextLevel = 'الماس'; pointsNeeded = 500 - points; progress = (points - 200) / 300;
    } else if (points >= 50) {
      level = 'نقره‌ای'; color = Colors.grey; nextLevel = 'طلایی'; pointsNeeded = 200 - points; progress = (points - 50) / 150;
    } else {
      level = 'برنزی'; color = Colors.brown; nextLevel = 'نقره‌ای'; pointsNeeded = 50 - points; progress = points / 50;
    }

    return Card(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Text('سطح: $level', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: color)),
                const Spacer(),
                if (pointsNeeded > 0)
                  Text('$pointsNeeded امتیاز تا $nextLevel', style: const TextStyle(color: Colors.grey, fontSize: 12)),
              ],
            ),
            const SizedBox(height: 8),
            ClipRRect(
              borderRadius: BorderRadius.circular(10),
              child: LinearProgressIndicator(
                value: progress.clamp(0.0, 1.0),
                minHeight: 10,
                backgroundColor: Colors.grey.shade200,
                valueColor: const AlwaysStoppedAnimation<Color>(AppColors.primary),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildMenuItem({required IconData icon, required String title, required VoidCallback onTap}) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: ListTile(
        leading: Icon(icon, color: AppColors.primary),
        title: Text(title),
        trailing: const Icon(Icons.arrow_back_ios, size: 16),
        onTap: onTap,
      ),
    );
  }
}
