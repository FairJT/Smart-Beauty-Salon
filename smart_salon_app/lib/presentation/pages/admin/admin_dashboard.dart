import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/app_colors.dart';
import '../../providers/admin_provider.dart';
import '../../providers/auth_provider.dart';
import '../login_screen.dart';

class AdminDashboard extends ConsumerStatefulWidget {
  const AdminDashboard({super.key});

  @override
  ConsumerState<AdminDashboard> createState() => _AdminDashboardState();
}

class _AdminDashboardState extends ConsumerState<AdminDashboard>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(adminProvider.notifier).loadStats();
      ref.read(adminProvider.notifier).loadUsers();
      ref.read(adminProvider.notifier).loadSalons();
    });
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(adminProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('پنل مدیریت', style: TextStyle(fontWeight: FontWeight.bold)),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () async {
              await ref.read(authProvider.notifier).logout();
              if (!mounted) return;
              Navigator.pushReplacement(
                context,
                MaterialPageRoute(builder: (_) => const LoginScreen()),
              );
            },
          ),
        ],
        bottom: TabBar(
          controller: _tabController,
          labelColor: Colors.white,
          unselectedLabelColor: Colors.white70,
          indicatorColor: Colors.white,
          tabs: const [
            Tab(text: 'کاربران', icon: Icon(Icons.people_outline)),
            Tab(text: 'سالن‌ها', icon: Icon(Icons.store_outlined)),
          ],
        ),
      ),
      body: Column(
        children: [
          _buildStats(state),
          Expanded(
            child: TabBarView(
              controller: _tabController,
              children: [
                _buildUsersTab(state),
                _buildSalonsTab(state),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStats(AdminState state) {
    final stats = state.stats;
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: const BoxDecoration(
        gradient: AppColors.heroGradient,
        borderRadius: BorderRadius.only(
          bottomLeft: Radius.circular(20),
          bottomRight: Radius.circular(20),
        ),
      ),
      child: state.loading && stats == null
          ? const Center(
              child: Padding(
                padding: EdgeInsets.all(20),
                child: CircularProgressIndicator(color: Colors.white),
              ),
            )
          : Row(
              children: [
                _statItem(Icons.people, 'کاربران', '${stats?.totalUsers ?? 0}'),
                _statItem(Icons.store, 'سالن‌ها', '${stats?.totalSalons ?? 0}'),
                _statItem(Icons.calendar_month, 'نوبت‌ها', '${stats?.totalAppointments ?? 0}'),
                _statItem(Icons.attach_money, 'درآمد', _formatRevenue(stats?.totalRevenue ?? 0)),
              ],
            ),
    );
  }

  Widget _statItem(IconData icon, String label, String value) {
    return Expanded(
      child: Column(
        children: [
          Icon(icon, color: Colors.white70, size: 22),
          const SizedBox(height: 4),
          Text(value, style: const TextStyle(color: Colors.white, fontSize: 16, fontWeight: FontWeight.bold)),
          Text(label, style: const TextStyle(color: Colors.white70, fontSize: 11)),
        ],
      ),
    );
  }

  String _formatRevenue(double revenue) {
    if (revenue >= 1000000) {
      return '${(revenue / 1000000).toStringAsFixed(1)}M';
    } else if (revenue >= 1000) {
      return '${(revenue / 1000).toStringAsFixed(1)}K';
    }
    return revenue.toStringAsFixed(0);
  }

  Widget _buildUsersTab(AdminState state) {
    if (state.users.isEmpty && state.loading) {
      return const Center(child: CircularProgressIndicator());
    }

    return RefreshIndicator(
      onRefresh: () async {
        await ref.read(adminProvider.notifier).loadUsers();
      },
      child: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: state.users.length,
        itemBuilder: (_, i) => _UserCard(
          user: state.users[i],
          onToggleActive: () =>
              ref.read(adminProvider.notifier).toggleUserActive(state.users[i].id),
          onChangeType: (type) =>
              ref.read(adminProvider.notifier).changeUserType(state.users[i].id, type),
        ),
      ),
    );
  }

  Widget _buildSalonsTab(AdminState state) {
    if (state.salons.isEmpty && state.loading) {
      return const Center(child: CircularProgressIndicator());
    }

    return RefreshIndicator(
      onRefresh: () async {
        await ref.read(adminProvider.notifier).loadSalons();
      },
      child: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: state.salons.length,
        itemBuilder: (_, i) => _SalonCard(
          salon: state.salons[i],
          onToggleActive: () =>
              ref.read(adminProvider.notifier).toggleSalonActive(state.salons[i].id),
          onToggleVip: () =>
              ref.read(adminProvider.notifier).toggleSalonVip(state.salons[i].id),
        ),
      ),
    );
  }
}

class _UserCard extends StatelessWidget {
  final AdminUser user;
  final VoidCallback onToggleActive;
  final ValueChanged<int> onChangeType;

  const _UserCard({
    required this.user,
    required this.onToggleActive,
    required this.onChangeType,
  });

  @override
  Widget build(BuildContext context) {
    final typeColor = _userTypeColor(user.userType);

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
                  backgroundColor: typeColor.withValues(alpha: 0.1),
                  child: Text(
                    user.fullName.isNotEmpty ? user.fullName[0] : '?',
                    style: TextStyle(color: typeColor, fontWeight: FontWeight.bold),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        user.fullName,
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                      ),
                      const SizedBox(height: 2),
                      Text(
                        user.phoneNumber,
                        style: const TextStyle(color: Colors.grey, fontSize: 13),
                      ),
                    ],
                  ),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: typeColor.withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(color: typeColor),
                  ),
                  child: Text(
                    _userTypeLabel(user.userType),
                    style: TextStyle(color: typeColor, fontSize: 11, fontWeight: FontWeight.bold),
                  ),
                ),
              ],
            ),
            const Divider(height: 20),
            Row(
              children: [
                Expanded(
                  child: DropdownButton<int>(
                    value: _userTypeValue(user.userType),
                    isExpanded: true,
                    underline: const SizedBox(),
                    hint: const Text('تغییر نوع', style: TextStyle(fontSize: 13)),
                    items: const [
                      DropdownMenuItem(value: 1, child: Text('سوپر ادمین')),
                      DropdownMenuItem(value: 2, child: Text('مدیر سالن')),
                      DropdownMenuItem(value: 3, child: Text('هنرمند')),
                      DropdownMenuItem(value: 4, child: Text('مشتری')),
                    ],
                    onChanged: (v) {
                      if (v != null) onChangeType(v);
                    },
                  ),
                ),
                const SizedBox(width: 12),
                Switch(
                  value: user.isActive,
                  onChanged: (_) => onToggleActive(),
                  activeColor: AppColors.success,
                ),
                Text(
                  user.isActive ? 'فعال' : 'غیرفعال',
                  style: TextStyle(
                    fontSize: 12,
                    color: user.isActive ? AppColors.success : AppColors.danger,
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  static int _userTypeValue(String type) {
    switch (type) {
      case 'SuperAdmin':
        return 1;
      case 'SalonManager':
        return 2;
      case 'Artist':
        return 3;
      default:
        return 4;
    }
  }

  static String _userTypeLabel(String type) {
    switch (type) {
      case 'SuperAdmin':
        return 'سوپر ادمین';
      case 'SalonManager':
        return 'مدیر سالن';
      case 'Artist':
        return 'هنرمند';
      case 'Client':
        return 'مشتری';
      default:
        return type;
    }
  }

  static Color _userTypeColor(String type) {
    switch (type) {
      case 'SuperAdmin':
        return AppColors.primary;
      case 'SalonManager':
        return AppColors.info;
      case 'Artist':
        return AppColors.accent;
      case 'Client':
        return AppColors.gray;
      default:
        return AppColors.gray;
    }
  }
}

class _SalonCard extends StatelessWidget {
  final AdminSalon salon;
  final VoidCallback onToggleActive;
  final VoidCallback onToggleVip;

  const _SalonCard({
    required this.salon,
    required this.onToggleActive,
    required this.onToggleVip,
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
                  backgroundColor: salon.isVip
                      ? AppColors.warning.withValues(alpha: 0.1)
                      : AppColors.primary.withValues(alpha: 0.1),
                  child: Icon(
                    salon.isVip ? Icons.star : Icons.store,
                    color: salon.isVip ? AppColors.warning : AppColors.primary,
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Expanded(
                            child: Text(
                              salon.name,
                              style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                            ),
                          ),
                          if (salon.isVip)
                            Container(
                              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                              decoration: BoxDecoration(
                                color: AppColors.warning.withValues(alpha: 0.1),
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(color: AppColors.warning),
                              ),
                              child: const Text(
                                'VIP',
                                style: TextStyle(
                                  color: AppColors.warning,
                                  fontSize: 10,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                        ],
                      ),
                      const SizedBox(height: 4),
                      if (salon.managerName.isNotEmpty)
                        Text(
                          'مدیر: ${salon.managerName}',
                          style: const TextStyle(color: Colors.grey, fontSize: 13),
                        ),
                      if (salon.address != null && salon.address!.isNotEmpty)
                        Text(
                          salon.address!,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(color: Colors.grey, fontSize: 12),
                        ),
                    ],
                  ),
                ),
              ],
            ),
            const Divider(height: 20),
            Row(
              children: [
                _infoChip(Icons.people_outline, '${salon.artistCount} هنرمند'),
                const SizedBox(width: 12),
                _infoChip(Icons.spa_outlined, '${salon.serviceCount} سرویس'),
                const Spacer(),
                Row(
                  children: [
                    IconButton(
                      icon: Icon(
                        salon.isActive ? Icons.check_circle : Icons.cancel,
                        color: salon.isActive ? AppColors.success : AppColors.danger,
                        size: 22,
                      ),
                      onPressed: onToggleActive,
                      tooltip: salon.isActive ? 'غیرفعال کردن' : 'فعال کردن',
                    ),
                    IconButton(
                      icon: Icon(
                        salon.isVip ? Icons.star : Icons.star_border,
                        color: salon.isVip ? AppColors.warning : Colors.grey,
                        size: 22,
                      ),
                      onPressed: onToggleVip,
                      tooltip: salon.isVip ? 'حذف VIP' : 'فعال کردن VIP',
                    ),
                  ],
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _infoChip(IconData icon, String label) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, size: 16, color: AppColors.primary),
        const SizedBox(width: 4),
        Text(label, style: const TextStyle(fontSize: 12, color: Colors.grey)),
      ],
    );
  }
}
