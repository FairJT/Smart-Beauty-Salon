// lib/presentation/widgets/ui_kit.dart
//
// Classic / tidy component kit for SalonOS. Built on the EXISTING tokens
// (AppColors, AppSpacing) and the existing TextTheme — it does not introduce a
// parallel design system. Use these instead of hand-writing TextStyle /
// BoxDecoration / Row on each page so every screen reads the same.
//
// RTL-correct: disclosure chevrons point left, paddings use Directional insets.

import 'package:flutter/material.dart';
import '../../core/app_colors.dart';

/// Tiny spacing helpers — `const Gap.h(12)` reads cleaner than SizedBox everywhere.
class Gap extends StatelessWidget {
  final double size;
  final bool horizontal;
  const Gap.v(this.size, {super.key}) : horizontal = false;
  const Gap.h(this.size, {super.key}) : horizontal = true;
  @override
  Widget build(BuildContext context) =>
      SizedBox(width: horizontal ? size : 0, height: horizontal ? 0 : size);
}

/// A section heading above a group of content. Optional trailing action.
/// Use for "نوبت‌های امروز", "مدیریت سالن", etc.
class SectionHeader extends StatelessWidget {
  final String title;
  final String? actionLabel;
  final VoidCallback? onAction;
  const SectionHeader(this.title, {super.key, this.actionLabel, this.onAction});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.sm, top: AppSpacing.xs),
      child: Row(
        children: [
          Expanded(
            child: Text(
              title,
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    color: AppColors.textPrimary,
                  ),
            ),
          ),
          if (actionLabel != null)
            TextButton(
              onPressed: onAction,
              style: TextButton.styleFrom(
                padding: const EdgeInsets.symmetric(horizontal: AppSpacing.xs),
                minimumSize: Size.zero,
                tapTargetSize: MaterialTapTargetSize.shrinkWrap,
              ),
              child: Text(actionLabel!, style: const TextStyle(fontSize: 13)),
            ),
        ],
      ),
    );
  }
}

/// A bordered surface card. Thin wrapper so spacing/radius/border stay uniform.
/// (Card is already themed; this just gives consistent padding + optional title.)
class AppCard extends StatelessWidget {
  final Widget child;
  final String? title;
  final EdgeInsetsGeometry? padding;
  final VoidCallback? onTap;
  const AppCard({super.key, required this.child, this.title, this.padding, this.onTap});

  @override
  Widget build(BuildContext context) {
    final content = Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        if (title != null) ...[
          Text(title!,
              style: Theme.of(context).textTheme.titleMedium?.copyWith(color: AppColors.textPrimary)),
          const Gap.v(AppSpacing.sm),
        ],
        child,
      ],
    );
    return Card(
      margin: const EdgeInsets.only(bottom: AppSpacing.sm),
      clipBehavior: Clip.antiAlias,
      child: onTap == null
          ? Padding(padding: padding ?? AppSpacing.cardPadding, child: content)
          : InkWell(
              onTap: onTap,
              child: Padding(padding: padding ?? AppSpacing.cardPadding, child: content),
            ),
    );
  }
}

/// A tidy tappable row: soft tinted icon, title, optional subtitle, RTL chevron.
/// Use for quick actions, settings, navigation lists, management entry points.
class AppListRow extends StatelessWidget {
  final IconData icon;
  final String title;
  final String? subtitle;
  final Widget? trailing; // defaults to a chevron when onTap != null
  final Color? tint;
  final VoidCallback? onTap;
  const AppListRow({
    super.key,
    required this.icon,
    required this.title,
    this.subtitle,
    this.trailing,
    this.tint,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final c = tint ?? AppColors.primary;
    return InkWell(
      onTap: onTap,
      borderRadius: AppSpacing.borderRadiusMd,
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: AppSpacing.sm),
        child: Row(
          children: [
            Container(
              width: 38,
              height: 38,
              decoration: BoxDecoration(
                color: c.withValues(alpha: 0.10),
                borderRadius: AppSpacing.borderRadiusSm,
              ),
              child: Icon(icon, size: 20, color: c),
            ),
            const Gap.h(AppSpacing.sm),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(title, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: AppColors.textPrimary)),
                  if (subtitle != null) ...[
                    const Gap.v(2),
                    Text(subtitle!, style: const TextStyle(fontSize: 12, color: AppColors.textSecondary)),
                  ],
                ],
              ),
            ),
            trailing ??
                (onTap != null
                    ? const Icon(Icons.chevron_left, color: AppColors.textMuted)
                    : const SizedBox.shrink()),
          ],
        ),
      ),
    );
  }
}

/// A soft status pill, driven by the app's status codes (uses AppColors.statusColor/Text).
class StatusPill extends StatelessWidget {
  final int status;
  const StatusPill(this.status, {super.key});
  @override
  Widget build(BuildContext context) {
    final c = AppColors.statusColor(status);
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: AppSpacing.xs, vertical: 3),
      decoration: BoxDecoration(
        color: c.withValues(alpha: 0.10),
        borderRadius: AppSpacing.borderRadiusSm,
        border: Border.all(color: c.withValues(alpha: 0.30)),
      ),
      child: Text(
        AppColors.statusText(status),
        style: TextStyle(color: c, fontSize: 11, fontWeight: FontWeight.w600),
      ),
    );
  }
}

/// A small outline chip for tags / filters / metadata.
class InfoChip extends StatelessWidget {
  final String label;
  final IconData? icon;
  final Color? color;
  const InfoChip(this.label, {super.key, this.icon, this.color});
  @override
  Widget build(BuildContext context) {
    final c = color ?? AppColors.textSecondary;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: AppSpacing.xs, vertical: 4),
      decoration: BoxDecoration(
        color: AppColors.background,
        borderRadius: AppSpacing.borderRadiusSm,
        border: Border.all(color: AppColors.border),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          if (icon != null) ...[Icon(icon, size: 13, color: c), const Gap.h(4)],
          Text(label, style: TextStyle(fontSize: 12, color: c)),
        ],
      ),
    );
  }
}

/// A hero metric: big tabular number + label. For dashboard headline stats.
class MetricCard extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;
  final Color? color;
  const MetricCard({super.key, required this.icon, required this.label, required this.value, this.color});
  @override
  Widget build(BuildContext context) {
    final c = color ?? AppColors.primary;
    return Card(
      margin: EdgeInsets.zero,
      child: Padding(
        padding: AppSpacing.cardPadding,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Icon(icon, size: 20, color: c),
            const Gap.v(AppSpacing.xs),
            Text(
              value,
              style: const TextStyle(
                fontSize: 20,
                fontWeight: FontWeight.bold,
                color: AppColors.textPrimary,
                fontFeatures: [FontFeature.tabularFigures()],
              ),
            ),
            const Gap.v(2),
            Text(label, style: const TextStyle(fontSize: 12, color: AppColors.textSecondary)),
          ],
        ),
      ),
    );
  }
}

/// A thin RTL-aware divider for inside cards/lists.
class AppDivider extends StatelessWidget {
  const AppDivider({super.key});
  @override
  Widget build(BuildContext context) =>
      const Divider(height: 1, thickness: 1, color: AppColors.border);
}
