import 'package:flutter/material.dart';
import '../../core/app_colors.dart';
import '../../core/format/jalaali_helper.dart';

// ─── Shared state widgets ─────────────────────────────────────

class LoadingState extends StatelessWidget {
  const LoadingState({super.key});

  @override
  Widget build(BuildContext context) {
    return const Center(
      child: Padding(
        padding: EdgeInsets.all(AppSpacing.xxl),
        child: CircularProgressIndicator(),
      ),
    );
  }
}

class EmptyState extends StatelessWidget {
  final String message;
  final IconData icon;

  const EmptyState({
    super.key,
    this.message = 'داده‌ای یافت نشد',
    this.icon = Icons.inbox_outlined,
  });

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.xxl),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 48, color: AppColors.textMuted),
            const SizedBox(height: AppSpacing.sm),
            Text(
              message,
              style: const TextStyle(color: AppColors.textMuted, fontSize: 14),
            ),
          ],
        ),
      ),
    );
  }
}

class ErrorState extends StatelessWidget {
  final String message;
  final VoidCallback? onRetry;

  const ErrorState({super.key, required this.message, this.onRetry});

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.xxl),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.wifi_off, size: 48, color: AppColors.danger),
            const SizedBox(height: AppSpacing.sm),
            Text(
              message,
              textAlign: TextAlign.center,
              style: const TextStyle(color: AppColors.danger, fontSize: 14),
            ),
            if (onRetry != null) ...[
              const SizedBox(height: AppSpacing.md),
              ElevatedButton.icon(
                onPressed: onRetry,
                icon: const Icon(Icons.refresh, size: 18),
                label: const Text('تلاش مجدد'),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

// ─── Dashboard card widgets ───────────────────────────────────

class SummaryCard extends StatelessWidget {
  final String title;
  final Widget child;
  final EdgeInsetsGeometry? padding;

  const SummaryCard({
    super.key,
    required this.title,
    required this.child,
    this.padding,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: AppSpacing.sm),
      child: Padding(
        padding: padding ?? AppSpacing.cardPadding,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              title,
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    color: AppColors.textPrimary,
                  ),
            ),
            const SizedBox(height: AppSpacing.sm),
            child,
          ],
        ),
      ),
    );
  }
}

class StatTile extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;
  final Color? iconColor;
  final Color? valueColor;

  const StatTile({
    super.key,
    required this.icon,
    required this.label,
    required this.value,
    this.iconColor,
    this.valueColor,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Icon(icon, size: 20, color: iconColor ?? AppColors.primary),
        const SizedBox(width: AppSpacing.xs),
        Expanded(
          child: Text(label,
              style: const TextStyle(
                  color: AppColors.textSecondary, fontSize: 13)),
        ),
        Text(
          value,
          style: TextStyle(
            fontWeight: FontWeight.bold,
            fontSize: 14,
            color: valueColor ?? AppColors.textPrimary,
          ),
        ),
      ],
    );
  }
}

class StatGrid extends StatelessWidget {
  final List<StatTileConfig> tiles;

  const StatGrid({super.key, required this.tiles});

  @override
  Widget build(BuildContext context) {
    return Column(
      children: tiles
          .map((t) => Padding(
                padding: const EdgeInsets.only(bottom: AppSpacing.xxs),
                child: StatTile(
                  icon: t.icon,
                  label: t.label,
                  value: t.value,
                  iconColor: t.iconColor,
                  valueColor: t.valueColor,
                ),
              ))
          .toList(),
    );
  }
}

class StatTileConfig {
  final IconData icon;
  final String label;
  final String value;
  final Color? iconColor;
  final Color? valueColor;

  const StatTileConfig({
    required this.icon,
    required this.label,
    required this.value,
    this.iconColor,
    this.valueColor,
  });
}

class QuickLink extends StatelessWidget {
  final IconData icon;
  final String label;
  final VoidCallback onTap;

  const QuickLink({
    super.key,
    required this.icon,
    required this.label,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: AppSpacing.borderRadiusMd,
      child: Container(
        padding: const EdgeInsets.symmetric(
          horizontal: AppSpacing.md,
          vertical: AppSpacing.sm,
        ),
        decoration: BoxDecoration(
          color: AppColors.primary50,
          borderRadius: AppSpacing.borderRadiusMd,
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 18, color: AppColors.primary),
            const SizedBox(width: AppSpacing.xxs),
            Text(label,
                style: const TextStyle(color: AppColors.primary, fontSize: 13)),
          ],
        ),
      ),
    );
  }
}

// ─── Appointment card ─────────────────────────────────────────

class AppointmentCard extends StatelessWidget {
  final String id;
  final DateTime startTime;
  final String title;
  final String subtitle;
  final int status;
  final VoidCallback? onTap;

  const AppointmentCard({
    super.key,
    required this.id,
    required this.startTime,
    required this.title,
    this.subtitle = '',
    required this.status,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final statusStr = AppColors.statusText(status);
    final statusClr = AppColors.statusColor(status);

    return Card(
      margin: const EdgeInsets.only(bottom: AppSpacing.xs),
      child: InkWell(
        onTap: onTap,
        borderRadius: AppSpacing.borderRadiusLg,
        child: Padding(
          padding: AppSpacing.cardPadding,
          child: Row(
            children: [
              Container(
                width: 4,
                height: 48,
                decoration: BoxDecoration(
                  color: statusClr,
                  borderRadius: AppSpacing.borderRadiusSm,
                ),
              ),
              const SizedBox(width: AppSpacing.sm),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(title,
                        style: const TextStyle(
                            fontWeight: FontWeight.w600, fontSize: 14)),
                    if (subtitle.isNotEmpty)
                      Text(subtitle,
                          style: const TextStyle(
                              color: AppColors.textSecondary, fontSize: 12)),
                    const SizedBox(height: 4),
                    Text(
                      JalaaliHelper.formatDateTime(startTime),
                      style: const TextStyle(
                          color: AppColors.textMuted, fontSize: 11),
                    ),
                  ],
                ),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: BoxDecoration(
                  color: statusClr.withValues(alpha: 0.1),
                  borderRadius: AppSpacing.borderRadiusSm,
                ),
                child: Text(
                  statusStr,
                  style: TextStyle(
                      color: statusClr,
                      fontSize: 11,
                      fontWeight: FontWeight.w600),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
