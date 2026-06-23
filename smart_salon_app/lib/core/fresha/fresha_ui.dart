import 'package:flutter/material.dart';

/// ── Fresha-style palette (warm white + olive accent) ──
class FCol {
  static const ink = Color(0xFF16140F);
  static const muted = Color(0xFF8A857C);
  static const line = Color(0xFFECE9E3);
  static const surface = Color(0xFFF7F5F1);
  static const card = Colors.white;
  static const olive = Color(0xFF59663A);
  static const oliveSoft = Color(0xFFEEF0E6);
  static const star = Color(0xFFE9A93A);
  static const offer = Color(0xFFD85A30);
  static const verified = Color(0xFF378ADD);
}

/// Toman money text (Rials ÷ 10), Persian digits.
class FMoneyText extends StatelessWidget {
  final int rials;
  final double size;
  final FontWeight weight;
  const FMoneyText(this.rials,
      {super.key, this.size = 14, this.weight = FontWeight.w700});
  @override
  Widget build(BuildContext context) {
    final toman = (rials / 10).round();
    final s = toman
        .toString()
        .replaceAllMapped(RegExp(r'(\d)(?=(\d{3})+(?!\d))'), (m) => '${m[1]},');
    return Text('\${_fa(s)} تومان',
        style: TextStyle(fontSize: size, fontWeight: weight, color: FCol.ink));
  }

  static String _fa(String s) {
    const en = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
    const fa = ['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹'];
    for (var i = 0; i < 10; i++) {
      s = s.replaceAll(en[i], fa[i]);
    }
    return s;
  }

  static String fa(String s) => _fa(s);
}

/// White rounded card with hairline border.
class FCard extends StatelessWidget {
  final Widget child;
  final EdgeInsets padding;
  final VoidCallback? onTap;
  const FCard(
      {super.key,
      required this.child,
      this.padding = const EdgeInsets.all(14),
      this.onTap});
  @override
  Widget build(BuildContext context) {
    final box = Container(
      padding: padding,
      decoration: BoxDecoration(
        color: FCol.card,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: FCol.line),
      ),
      child: child,
    );
    return onTap == null
        ? box
        : InkWell(
            borderRadius: BorderRadius.circular(18), onTap: onTap, child: box);
  }
}

/// Horizontal category / filter chip.
class FChip extends StatelessWidget {
  final String label;
  final IconData? icon;
  final bool selected;
  final VoidCallback? onTap;
  const FChip(this.label,
      {super.key, this.icon, this.selected = false, this.onTap});
  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 13, vertical: 9),
        decoration: BoxDecoration(
          color: selected ? FCol.olive : FCol.surface,
          borderRadius: BorderRadius.circular(14),
        ),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          if (icon != null) ...[
            Icon(icon,
                size: 15,
                color: selected ? Colors.white : const Color(0xFF3B372F)),
            const SizedBox(width: 6),
          ],
          Text(label,
              style: TextStyle(
                  fontSize: 12.5,
                  color: selected ? Colors.white : const Color(0xFF3B372F))),
        ]),
      ),
    );
  }
}

/// Surface stat tile (value + label).
class FStat extends StatelessWidget {
  final String value;
  final String label;
  const FStat(this.value, this.label, {super.key});
  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(11),
      decoration: BoxDecoration(
          color: FCol.surface, borderRadius: BorderRadius.circular(13)),
      child: Column(children: [
        Text(value,
            style: const TextStyle(
                fontWeight: FontWeight.w700, fontSize: 16, color: FCol.ink)),
        const SizedBox(height: 3),
        Text(label, style: const TextStyle(fontSize: 11, color: FCol.muted)),
      ]),
    );
  }
}

/// Service row: name + duration·price + add button.
class FServiceRow extends StatelessWidget {
  final String name;
  final int durationMin;
  final int priceRials;
  final bool added;
  final VoidCallback? onAdd;
  const FServiceRow(
      {super.key,
      required this.name,
      required this.durationMin,
      required this.priceRials,
      this.added = false,
      this.onAdd});
  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 13),
      child: Row(children: [
        Expanded(
            child:
                Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Text(name,
              style: const TextStyle(
                  fontWeight: FontWeight.w600, fontSize: 14, color: FCol.ink)),
          const SizedBox(height: 4),
          Row(children: [
            const Icon(Icons.schedule, size: 13, color: FCol.muted),
            const SizedBox(width: 4),
            const Text('\${FMoneyText.fa(durationMin.toString())} دقیقه',
                style: TextStyle(fontSize: 11.5, color: FCol.muted)),
            const Text(' • ', style: TextStyle(color: FCol.muted)),
            FMoneyText(priceRials, size: 11.5, weight: FontWeight.w500),
          ]),
        ])),
        GestureDetector(
          onTap: onAdd,
          child: Container(
            width: 30,
            height: 30,
            decoration: BoxDecoration(
              color: added ? FCol.olive : Colors.transparent,
              borderRadius: BorderRadius.circular(10),
              border:
                  Border.all(color: added ? FCol.olive : FCol.ink, width: 1.4),
            ),
            child: Icon(added ? Icons.check : Icons.add,
                size: 16, color: added ? Colors.white : FCol.ink),
          ),
        ),
      ]),
    );
  }
}

/// Time slot with state.
enum FSlotState { free, busy, selected }

class FSlot extends StatelessWidget {
  final String label;
  final FSlotState state;
  final VoidCallback? onTap;
  const FSlot(this.label,
      {super.key, this.state = FSlotState.free, this.onTap});
  @override
  Widget build(BuildContext context) {
    final sel = state == FSlotState.selected, busy = state == FSlotState.busy;
    return GestureDetector(
      onTap: busy ? null : onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 10),
        alignment: Alignment.center,
        decoration: BoxDecoration(
          color: sel ? FCol.olive : (busy ? FCol.surface : Colors.white),
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: sel ? FCol.olive : FCol.line),
        ),
        child: Text(label,
            style: TextStyle(
              fontSize: 13,
              decoration: busy ? TextDecoration.lineThrough : null,
              color: sel
                  ? Colors.white
                  : (busy ? const Color(0xFFC3BDB1) : const Color(0xFF3B372F)),
            )),
      ),
    );
  }
}

/// Primary CTA (ink button).
class FPrimaryButton extends StatelessWidget {
  final String label;
  final VoidCallback? onTap;
  final bool expand;
  const FPrimaryButton(this.label, {super.key, this.onTap, this.expand = true});
  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: expand ? double.infinity : null,
      child: ElevatedButton(
        style: ElevatedButton.styleFrom(
          backgroundColor: FCol.ink,
          foregroundColor: Colors.white,
          elevation: 0,
          padding: const EdgeInsets.symmetric(vertical: 15, horizontal: 30),
          shape:
              RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        ),
        onPressed: onTap,
        child: Text(label,
            style:
                const TextStyle(fontSize: 14.5, fontWeight: FontWeight.w600)),
      ),
    );
  }
}

/// Status pill (booking/request status). Pass color + text.
class FStatusChip extends StatelessWidget {
  final String text;
  final Color color;
  const FStatusChip(this.text, this.color, {super.key});
  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
          color: color.withValues(alpha: .12),
          borderRadius: BorderRadius.circular(11)),
      child: Text(text,
          style: TextStyle(
              fontSize: 11.5, fontWeight: FontWeight.w600, color: color)),
    );
  }
}

/// Initials avatar.
class FAvatar extends StatelessWidget {
  final String initial;
  final bool active;
  const FAvatar(this.initial, {super.key, this.active = false});
  @override
  Widget build(BuildContext context) {
    return Container(
      width: 38,
      height: 38,
      alignment: Alignment.center,
      decoration: BoxDecoration(
        color: FCol.oliveSoft,
        shape: BoxShape.circle,
        border: active ? Border.all(color: FCol.olive, width: 2) : null,
      ),
      child: Text(initial,
          style: const TextStyle(
              color: FCol.olive, fontWeight: FontWeight.w600, fontSize: 13)),
    );
  }
}

/// 4-item bottom nav.
class FBottomNav extends StatelessWidget {
  final int index;
  final ValueChanged<int> onTap;
  final List<({IconData icon, String label})> items;
  const FBottomNav(
      {super.key,
      required this.index,
      required this.onTap,
      required this.items});
  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.only(top: 11, bottom: 14),
      decoration: const BoxDecoration(
          color: Colors.white,
          border: Border(top: BorderSide(color: Color(0xFFF0EDE7)))),
      child: Row(mainAxisAlignment: MainAxisAlignment.spaceAround, children: [
        for (var i = 0; i < items.length; i++)
          GestureDetector(
            onTap: () => onTap(i),
            child: Column(mainAxisSize: MainAxisSize.min, children: [
              Icon(items[i].icon,
                  size: 21,
                  color: i == index ? FCol.ink : const Color(0xFFAAA49A)),
              const SizedBox(height: 3),
              Text(items[i].label,
                  style: TextStyle(
                      fontSize: 10.5,
                      color: i == index ? FCol.ink : const Color(0xFFAAA49A))),
            ]),
          ),
      ]),
    );
  }
}

/// Empty / loading / error states.
class FLoading extends StatelessWidget {
  const FLoading({super.key});
  @override
  Widget build(BuildContext context) => const Center(
      child: Padding(
          padding: EdgeInsets.all(40),
          child: CircularProgressIndicator(color: FCol.olive)));
}

class FEmpty extends StatelessWidget {
  final String message;
  final IconData icon;
  const FEmpty(this.message, {super.key, this.icon = Icons.inbox_outlined});
  @override
  Widget build(BuildContext context) => Center(
          child: Padding(
        padding: const EdgeInsets.all(40),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Icon(icon, size: 48, color: const Color(0xFFCFCABF)),
          const SizedBox(height: 12),
          Text(message,
              style: const TextStyle(color: FCol.muted, fontSize: 13.5)),
        ]),
      ));
}

class FError extends StatelessWidget {
  final String message;
  final VoidCallback? onRetry;
  const FError(this.message, {super.key, this.onRetry});
  @override
  Widget build(BuildContext context) => Center(
          child: Padding(
        padding: const EdgeInsets.all(40),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          const Icon(Icons.error_outline, size: 44, color: FCol.offer),
          const SizedBox(height: 12),
          Text(message,
              textAlign: TextAlign.center,
              style: const TextStyle(color: FCol.muted, fontSize: 13.5)),
          if (onRetry != null) ...[
            const SizedBox(height: 14),
            OutlinedButton(
                onPressed: onRetry, child: const Text('تلاش دوباره')),
          ],
        ]),
      ));
}
