import 'package:flutter/material.dart';

class AppColors {
  // Primary palette — purple accent
  static const Color primary = Color(0xFF7C3AED);
  static const Color primaryLight = Color(0xFFA78BFA);
  static const Color primaryDark = Color(0xFF5B21B6);
  static const Color primary50 = Color(0x0D7C3AED);
  static const Color primary100 = Color(0x1A7C3AED);
  static const Color primary200 = Color(0x337C3AED);

  // Accent
  static const Color accent = Color(0xFFEC4899);
  static const Color accentLight = Color(0xFFF9A8D4);

  // Status colors
  static const Color success = Color(0xFF10B981);
  static const Color success50 = Color(0x1410B981);
  static const Color warning = Color(0xFFF59E0B);
  static const Color warning50 = Color(0x14F59E0B);
  static const Color danger = Color(0xFFEF4444);
  static const Color danger50 = Color(0x14EF4444);
  static const Color info = Color(0xFF3B82F6);
  static const Color info50 = Color(0x143B82F6);

  // Neutral
  static const Color white = Colors.white;
  static const Color background = Color(0xFFF5F3FF);
  static const Color surface = Colors.white;
  static const Color textPrimary = Color(0xFF1E1B4B);
  static const Color textSecondary = Color(0xFF6B7280);
  static const Color textMuted = Color(0xFF9CA3AF);
  static const Color border = Color(0x1A000000);

  // Gradient presets
  static const LinearGradient primaryGradient = LinearGradient(
    colors: [primary, Color(0xFFA78BFA)],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static const LinearGradient bgGradient = LinearGradient(
    colors: [Color(0xFFEDE9FE), Color(0xFFECFDF5), Color(0xFFFDF2F8)],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static const LinearGradient darkGradient = LinearGradient(
    colors: [primaryDark, primary],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static const LinearGradient heroGradient = LinearGradient(
    colors: [Color(0xFF7C3AED), Color(0xFFA78BFA), Color(0xFFC4B5FD)],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static Color statusColor(int status) {
    switch (status) {
      case 1: return const Color(0xFFF59E0B); // Pending
      case 2: return const Color(0xFF10B981); // Confirmed
      case 3: return const Color(0xFF3B82F6); // Completed
      case 4: return const Color(0xFF6B7280); // Cancelled
      case 5: return const Color(0xFFEF4444); // No show
      case 6: return const Color(0xFFEF4444); // Cancelled by artist
      default: return const Color(0xFF6B7280);
    }
  }

  static String statusText(int status) {
    switch (status) {
      case 1: return 'در انتظار';
      case 2: return 'تایید شده';
      case 3: return 'انجام شده';
      case 4: return 'لغو شده';
      case 5: return 'غیبت';
      case 6: return 'لغو توسط هنرمند';
      default: return 'نامشخص';
    }
  }
}
