import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/app_colors.dart';
import '../../../core/format/money_formatter.dart';
import '../../../data/datasources/dio_client.dart';
import '../../../data/datasources/api_constants.dart';
import '../../widgets/dashboard_widgets.dart';

class FinanceScreen extends ConsumerStatefulWidget {
  const FinanceScreen({super.key});

  @override
  ConsumerState<FinanceScreen> createState() => _FinanceScreenState();
}

class _FinanceScreenState extends ConsumerState<FinanceScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  RevenueSummary? _revenue;
  List<ArtistPayout> _payouts = [];
  bool _loading = true;
  bool _closing = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
    _loadData();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<void> _loadData() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      await Future.wait([_loadRevenue(), _loadPayouts()]);
    } catch (e) {
      setState(() => _error = e.toString());
    } finally {
      setState(() => _loading = false);
    }
  }

  Future<void> _loadRevenue() async {
    final response =
        await DioClient.instance.get('${ApiConstants.baseUrl}/finance/revenue');
    setState(() {
      _revenue = RevenueSummary.fromJson(response.data);
    });
  }

  Future<void> _loadPayouts() async {
    final response =
        await DioClient.instance.get('${ApiConstants.baseUrl}/finance/payouts');
    final data =
        response.data is List ? response.data : (response.data['data'] ?? []);
    setState(() {
      _payouts = (data as List).map((j) => ArtistPayout.fromJson(j)).toList();
    });
  }

  Future<void> _closePeriod() async {
    setState(() => _closing = true);
    try {
      await DioClient.instance
          .post('${ApiConstants.baseUrl}/finance/close-period');
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('دوره مالی با موفقیت بسته شد'),
          backgroundColor: AppColors.success,
        ),
      );
      await _loadData();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('خطا: ${e.toString()}'),
          backgroundColor: AppColors.danger,
        ),
      );
    } finally {
      if (mounted) setState(() => _closing = false);
    }
  }

  void _showClosePeriodDialog() {
    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('بستن دوره مالی'),
        content: const Text(
          'آیا مطمئن هستید؟ این عملیات غیرقابل بازگشت است و دوره مالی جاری را می‌بندد.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('لغو'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.danger),
            onPressed: () {
              Navigator.pop(context);
              _closePeriod();
            },
            child: const Text('بستن دوره'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('امور مالی',
            style: TextStyle(fontWeight: FontWeight.bold)),
        bottom: TabBar(
          controller: _tabController,
          labelColor: Colors.white,
          unselectedLabelColor: Colors.white70,
          indicatorColor: Colors.white,
          tabs: const [
            Tab(text: 'خلاصه درآمد', icon: Icon(Icons.trending_up)),
            Tab(text: 'پرداخت هنرمندان', icon: Icon(Icons.people_outline)),
            Tab(text: 'بستن دوره', icon: Icon(Icons.lock_outline)),
          ],
        ),
      ),
      body: _loading
          ? const LoadingState()
          : _error != null
              ? ErrorState(
                  message: _error!,
                  onRetry: _loadData,
                )
              : TabBarView(
                  controller: _tabController,
                  children: [
                    _buildRevenueTab(),
                    _buildPayoutsTab(),
                    _buildClosePeriodTab(),
                  ],
                ),
    );
  }

  Widget _buildRevenueTab() {
    if (_revenue == null) {
      return const EmptyState(message: 'داده‌ای برای نمایش وجود ندارد');
    }

    return RefreshIndicator(
      onRefresh: _loadRevenue,
      child: ListView(
        padding: AppSpacing.pagePadding,
        children: [
          SummaryCard(
            title: 'درآمد دوره جاری',
            child: StatGrid(tiles: [
              StatTileConfig(
                icon: Icons.monetization_on,
                label: 'مجموع درآمد',
                value: MoneyFormatter.format(_revenue!.totalRevenue),
                iconColor: AppColors.success,
                valueColor: AppColors.success,
              ),
              StatTileConfig(
                icon: Icons.receipt_long,
                label: 'تعداد تراکنش‌ها',
                value: '${_revenue!.transactionCount}',
              ),
              StatTileConfig(
                icon: Icons.account_balance_wallet,
                label: 'مجموع واریزی‌ها',
                value: MoneyFormatter.format(_revenue!.totalDeposits),
                iconColor: AppColors.primary,
              ),
              StatTileConfig(
                icon: Icons.pending_actions,
                label: 'تراکنش‌های pending',
                value: '${_revenue!.pendingCount}',
                iconColor: AppColors.warning,
                valueColor: AppColors.warning,
              ),
            ]),
          ),
          if (_revenue!.periodStart != null || _revenue!.periodEnd != null)
            SummaryCard(
              title: 'دوره مالی',
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  if (_revenue!.periodStart != null)
                    StatTile(
                      icon: Icons.date_range,
                      label: 'از تاریخ',
                      value: _revenue!.periodStart!,
                    ),
                  if (_revenue!.periodEnd != null)
                    StatTile(
                      icon: Icons.date_range,
                      label: 'تا تاریخ',
                      value: _revenue!.periodEnd!,
                    ),
                ],
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildPayoutsTab() {
    if (_payouts.isEmpty) {
      return RefreshIndicator(
        onRefresh: _loadPayouts,
        child: ListView(
          children: [
            const EmptyState(message: 'هنوز هنرمندی برای پرداخت وجود ندارد'),
          ],
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _loadPayouts,
      child: ListView.builder(
        padding: AppSpacing.pagePadding,
        itemCount: _payouts.length,
        itemBuilder: (_, i) => _PayoutCard(payout: _payouts[i]),
      ),
    );
  }

  Widget _buildClosePeriodTab() {
    return Center(
      child: Padding(
        padding: AppSpacing.pagePadding,
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(
              Icons.lock_outline,
              size: 80,
              color: AppColors.textMuted,
            ),
            const SizedBox(height: 24),
            const Text(
              'بستن دوره مالی',
              style: TextStyle(
                fontSize: 20,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 12),
            const Text(
              'پس از بستن دوره مالی جاری، گزارش‌ها نهایی شده و\nدوره جدیدی آغاز خواهد شد.',
              textAlign: TextAlign.center,
              style: TextStyle(color: AppColors.textSecondary, fontSize: 14),
            ),
            const SizedBox(height: 32),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: _closing ? null : _showClosePeriodDialog,
                icon: _closing
                    ? const SizedBox(
                        width: 20,
                        height: 20,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: Colors.white,
                        ),
                      )
                    : const Icon(Icons.lock),
                label: Text(_closing ? 'در حال بستن...' : 'بستن دوره مالی'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.danger,
                  foregroundColor: Colors.white,
                  minimumSize: const Size(double.infinity, 52),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

// ─── Data Models ──────────────────────────────────────────────────

class RevenueSummary {
  final int totalRevenue;
  final int totalDeposits;
  final int transactionCount;
  final int pendingCount;
  final String? periodStart;
  final String? periodEnd;

  RevenueSummary({
    this.totalRevenue = 0,
    this.totalDeposits = 0,
    this.transactionCount = 0,
    this.pendingCount = 0,
    this.periodStart,
    this.periodEnd,
  });

  factory RevenueSummary.fromJson(Map<String, dynamic> json) => RevenueSummary(
        totalRevenue: json['totalRevenue'] ?? 0,
        totalDeposits: json['totalDeposits'] ?? 0,
        transactionCount: json['transactionCount'] ?? 0,
        pendingCount: json['pendingCount'] ?? 0,
        periodStart: json['periodStart'],
        periodEnd: json['periodEnd'],
      );
}

class ArtistPayout {
  final dynamic id;
  final String artistName;
  final int completedAppointments;
  final double rating;
  final String contractType;
  final int? revenue;
  final int? commissionAmount;
  final int? fixedAmount;

  ArtistPayout({
    required this.id,
    required this.artistName,
    this.completedAppointments = 0,
    this.rating = 0.0,
    this.contractType = 'Salaried',
    this.revenue,
    this.commissionAmount,
    this.fixedAmount,
  });

  factory ArtistPayout.fromJson(Map<String, dynamic> json) => ArtistPayout(
        id: json['id'] ?? 0,
        artistName: json['artistName'] ?? '',
        completedAppointments: json['completedAppointments'] ?? 0,
        rating: (json['rating'] ?? 0).toDouble(),
        contractType: json['contractType'] ?? 'Salaried',
        revenue: json['revenue'],
        commissionAmount: json['commissionAmount'],
        fixedAmount: json['fixedAmount'],
      );
}

// ─── Payout Card Widget ───────────────────────────────────────────

class _PayoutCard extends StatelessWidget {
  final ArtistPayout payout;

  const _PayoutCard({required this.payout});

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
                  backgroundColor: AppColors.primary.withValues(alpha: 0.1),
                  child: Text(
                    payout.artistName.isNotEmpty ? payout.artistName[0] : '?',
                    style: const TextStyle(
                      color: AppColors.primary,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    payout.artistName,
                    style: const TextStyle(
                        fontWeight: FontWeight.bold, fontSize: 15),
                  ),
                ),
                Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: payout.contractType == 'Salaried'
                        ? AppColors.info.withValues(alpha: 0.1)
                        : AppColors.accent.withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(
                      color: payout.contractType == 'Salaried'
                          ? AppColors.info
                          : AppColors.accent,
                    ),
                  ),
                  child: Text(
                    payout.contractType == 'Salaried' ? 'حقوقی' : 'مشارکتی',
                    style: TextStyle(
                      fontSize: 11,
                      fontWeight: FontWeight.bold,
                      color: payout.contractType == 'Salaried'
                          ? AppColors.info
                          : AppColors.accent,
                    ),
                  ),
                ),
              ],
            ),
            const Divider(height: 20),
            Row(
              children: [
                _infoChip(Icons.calendar_today,
                    '${payout.completedAppointments} نوبت'),
                const SizedBox(width: 12),
                _infoChip(Icons.star, payout.rating.toStringAsFixed(1)),
              ],
            ),
            const SizedBox(height: 12),
            // Contract-aware display
            if (payout.contractType == 'Salaried')
              const Text(
                'هنرمند حقوقی — درآمد از طریق حقوق ثابت',
                style: TextStyle(
                  color: AppColors.textSecondary,
                  fontSize: 12,
                  fontStyle: FontStyle.italic,
                ),
              )
            else ...[
              if (payout.revenue != null)
                StatTile(
                  icon: Icons.monetization_on,
                  label: 'درآمد',
                  value: MoneyFormatter.format(payout.revenue!),
                  iconColor: AppColors.success,
                  valueColor: AppColors.success,
                ),
              if (payout.commissionAmount != null)
                StatTile(
                  icon: Icons.percent,
                  label: 'کمیسیون',
                  value: MoneyFormatter.format(payout.commissionAmount!),
                  iconColor: AppColors.primary,
                ),
            ],
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
