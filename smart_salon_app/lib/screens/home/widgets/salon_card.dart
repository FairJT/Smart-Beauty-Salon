import 'package:flutter/material.dart';
import '../../../core/app_colors.dart';
import '../../../models/salon.dart';

class SalonCard extends StatelessWidget {
  final SalonListItem salon;
  final VoidCallback onTap;

  const SalonCard({super.key, required this.salon, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: ListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        leading: CircleAvatar(
          radius: 28,
          backgroundColor: AppColors.primary,
          child: Text(
            salon.initial,
            style: const TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.bold),
          ),
        ),
        title: Row(
          children: [
            Expanded(
              child: Text(salon.name, style: const TextStyle(fontWeight: FontWeight.bold)),
            ),
            if (salon.isVip) const Icon(Icons.verified, color: Colors.amber, size: 18),
          ],
        ),
        subtitle: Text(
          salon.address ?? '',
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
          style: const TextStyle(color: Colors.grey),
        ),
        trailing: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.star_rounded, color: Colors.amber, size: 18),
            Text(
              salon.ratingAvg.toStringAsFixed(1),
              style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
            ),
          ],
        ),
        onTap: onTap,
      ),
    );
  }
}
