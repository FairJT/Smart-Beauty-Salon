import 'package:flutter/material.dart';

class AppSpacing {
  static const double xxs = 4;
  static const double xs = 8;
  static const double sm = 12;
  static const double md = 16;
  static const double lg = 24;
  static const double xl = 32;
  static const double xxl = 48;

  static const EdgeInsets allXs = EdgeInsets.all(xs);
  static const EdgeInsets allSm = EdgeInsets.all(sm);
  static const EdgeInsets allMd = EdgeInsets.all(md);
  static const EdgeInsets allLg = EdgeInsets.all(lg);

  static const EdgeInsets hMd = EdgeInsets.symmetric(horizontal: md);
  static const EdgeInsets vMd = EdgeInsets.symmetric(vertical: md);

  static const Radius radiusSm = Radius.circular(8);
  static const Radius radiusMd = Radius.circular(12);
  static const Radius radiusLg = Radius.circular(16);
  static const BorderRadius borderRadiusSm = BorderRadius.all(radiusSm);
  static const BorderRadius borderRadiusMd = BorderRadius.all(radiusMd);
  static const BorderRadius borderRadiusLg = BorderRadius.all(radiusLg);

  static const EdgeInsets cardPadding = EdgeInsets.all(md);
  static const EdgeInsets pagePadding = EdgeInsets.all(md);
}

class AppColors {
  // Primary — navy (#1B3A5C from Salon entity)
  static const Color primary = Color(0xFF1B3A5C);
  static const Color primaryLight = Color(0xFF3B5A7C);
  static const Color primaryDark = Color(0xFF0F2440);
  static const Color primary50 = Color(0x0D1B3A5C);
  static const Color primary100 = Color(0x1A1B3A5C);

  // Admin gold accent
  static const Color adminGold = Color(0xFFD4A843);
  static const Color adminGoldLight = Color(0xFFE8C97A);
  static const Color adminGoldDark = Color(0xFFB8922E);

  // Accent — rose
  static const Color accent = Color(0xFFEC4899);
  static const Color accentLight = Color(0xFFF9A8D4);

  // Status
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
  static const Color background = Color(0xFFF8F9FC);
  static const Color surface = Colors.white;
  static const Color textPrimary = Color(0xFF1E1B4B);
  static const Color textSecondary = Color(0xFF6B7280);
  static const Color textMuted = Color(0xFF9CA3AF);
  static const Color border = Color(0xFFECEDEF); // hairline
  static const Color borderStrong = Color(0xFFDDE0E4); // dividers under headers

  // Extra colors used in screens
  static const Color gray = Color(0xFF9E9E9E);
  static const Color green = Color(0xFF4CAF50);
  static const Color dark = Color(0xFF1F2937);
  static const Color lightBlue = Color(0xFFE3F2FD);
  static const Color primary200 = Color(0xFF5A7C9C);

  static const LinearGradient bgGradient = LinearGradient(
    colors: [Color(0xFFF8F9FC), Color(0xFFE8ECF1)],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  // Gradients
  static const LinearGradient primaryGradient = LinearGradient(
    colors: [primary, primaryLight],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static const LinearGradient goldGradient = LinearGradient(
    colors: [adminGold, adminGoldLight],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static const LinearGradient darkGradient = LinearGradient(
    colors: [primaryDark, primary],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static const LinearGradient heroGradient = LinearGradient(
    colors: [primary, primaryLight, Color(0xFF5A7C9C)],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static Color statusColor(int status) {
    switch (status) {
      case 1:
        return warning;
      case 2:
        return success;
      case 3:
        return info;
      case 4:
        return textMuted;
      case 5:
        return danger;
      case 6:
        return danger;
      default:
        return textMuted;
    }
  }

  static String statusText(int status) {
    switch (status) {
      case 1:
        return 'در انتظار';
      case 2:
        return 'تایید شده';
      case 3:
        return 'انجام شده';
      case 4:
        return 'لغو شده';
      case 5:
        return 'غیبت';
      case 6:
        return 'لغو توسط هنرمند';
      default:
        return 'نامشخص';
    }
  }
}

class AppTextTheme {
  static TextTheme farsi({required ColorScheme colorScheme}) {
    return const TextTheme(
      displayLarge:
          TextStyle(fontSize: 28, fontWeight: FontWeight.bold, height: 1.3),
      displayMedium:
          TextStyle(fontSize: 24, fontWeight: FontWeight.bold, height: 1.3),
      headlineLarge:
          TextStyle(fontSize: 22, fontWeight: FontWeight.bold, height: 1.4),
      headlineMedium:
          TextStyle(fontSize: 18, fontWeight: FontWeight.w600, height: 1.4),
      headlineSmall:
          TextStyle(fontSize: 16, fontWeight: FontWeight.w600, height: 1.4),
      titleLarge:
          TextStyle(fontSize: 16, fontWeight: FontWeight.w600, height: 1.4),
      titleMedium:
          TextStyle(fontSize: 14, fontWeight: FontWeight.w600, height: 1.4),
      bodyLarge:
          TextStyle(fontSize: 16, fontWeight: FontWeight.normal, height: 1.6),
      bodyMedium:
          TextStyle(fontSize: 14, fontWeight: FontWeight.normal, height: 1.6),
      bodySmall:
          TextStyle(fontSize: 12, fontWeight: FontWeight.normal, height: 1.5),
      labelLarge:
          TextStyle(fontSize: 14, fontWeight: FontWeight.w600, height: 1.4),
      labelMedium:
          TextStyle(fontSize: 12, fontWeight: FontWeight.w500, height: 1.4),
      labelSmall:
          TextStyle(fontSize: 10, fontWeight: FontWeight.w500, height: 1.4),
    );
  }

  static ButtonThemeData button() {
    return const ButtonThemeData(
      shape: RoundedRectangleBorder(borderRadius: AppSpacing.borderRadiusSm),
    );
  }
}
