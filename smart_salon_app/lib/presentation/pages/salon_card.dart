import 'package:flutter/material.dart';
import '../../core/app_colors.dart';
import '../../domain/entities/salon_entity.dart';

class SalonCard extends StatelessWidget {
  final SalonEntity salon;
  final VoidCallback onTap;
  final bool isFavorited;
  final VoidCallback? onToggleFavorite;

  const SalonCard({
    super.key,
    required this.salon,
    required this.onTap,
    this.isFavorited = false,
    this.onToggleFavorite,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
        side: BorderSide(color: AppColors.border),
      ),
      child: ListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        leading: CircleAvatar(
          radius: 28,
          backgroundColor: AppColors.primary,
          child: Text(
            salon.name[0],
            style: const TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.bold),
          ),
        ),
        title: Row(
          children: [
            Expanded(
              child: Text(salon.name, style: const TextStyle(fontWeight: FontWeight.bold)),
            ),
            if (onToggleFavorite != null)
              GestureDetector(
                onTap: onToggleFavorite,
                child: Icon(
                  isFavorited ? Icons.favorite : Icons.favorite_border,
                  color: isFavorited ? AppColors.danger : AppColors.textMuted,
                  size: 20,
                ),
              ),
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
            const Icon(Icons.star_rounded, color: AppColors.warning, size: 18),
            Text(
              salon.rating.toStringAsFixed(1),
              style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
            ),
          ],
        ),
        onTap: onTap,
      ),
    );
  }
}
