import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'core/app_colors.dart';
import 'l10n/app_localizations.dart';
import 'presentation/pages/splash_screen.dart';
import 'presentation/pages/generated/onboarding_screen.dart';
import 'presentation/pages/login_screen.dart';
import 'presentation/pages/register_screen.dart';
import 'presentation/pages/generated/home_screen.dart';
import 'presentation/pages/generated/salon_detail_screen.dart';
import 'presentation/pages/booking_screen.dart';
import 'presentation/pages/profile_screen.dart';
import 'presentation/pages/generated/artist_public_screen.dart';
import 'presentation/pages/artist/artist_dashboard_screen.dart';
import 'presentation/pages/manager/manager_dashboard_screen.dart';
import 'presentation/pages/admin/admin_dashboard.dart';
import 'presentation/pages/client/client_dashboard_screen.dart';
import 'presentation/pages/generated/my_appointments_screen.dart';
import 'presentation/pages/generated/booking_flow_screen.dart';
import 'widgets/error_boundary.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await SystemChrome.setPreferredOrientations([DeviceOrientation.portraitUp]);
  SystemChrome.setSystemUIOverlayStyle(const SystemUiOverlayStyle(
    statusBarColor: Colors.transparent,
    statusBarIconBrightness: Brightness.dark,
  ));
  FlutterError.onError = (details) {
    FlutterError.presentError(details);
  };
  ErrorWidget.builder = (details) {
    return Directionality(
      textDirection: TextDirection.rtl,
      child: Material(
        child: Center(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(Icons.error_outline, size: 64, color: Colors.red),
                const SizedBox(height: 16),
                const Text(
                  'خطایی رخ داده است',
                  style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 8),
                Text(
                  details.exception.toString(),
                  style: const TextStyle(fontSize: 14, color: Colors.grey),
                  textAlign: TextAlign.center,
                ),
              ],
            ),
          ),
        ),
      ),
    );
  };
  runZonedGuarded(
    () => runApp(const ProviderScope(child: SmartSalonApp())),
    (error, stack) {
      FlutterError.presentError(FlutterErrorDetails(
        exception: error,
        stack: stack,
        library: 'SmartSalon',
        context: ErrorDescription('Uncaught async error in root zone'),
      ));
    },
  );
}

class SmartSalonApp extends StatelessWidget {
  const SmartSalonApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'سالن هوشمند',
      debugShowCheckedModeBanner: false,
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      locale: const Locale('fa'),
      builder: (context, child) => Directionality(
        textDirection: TextDirection.rtl,
        child: ErrorBoundary(child: child!),
      ),
      onGenerateRoute: (settings) {
        switch (settings.name) {
          case '/onboarding':
            return MaterialPageRoute(builder: (_) => const OnboardingScreen());
          case '/login':
            return MaterialPageRoute(builder: (_) => const LoginScreen());
          case '/register':
            return MaterialPageRoute(builder: (_) => const RegisterScreen());
          case '/home':
            return MaterialPageRoute(builder: (_) => const HomeScreen());
          case '/salon-detail':
            return MaterialPageRoute(builder: (_) => const SalonDetailScreen());
          case '/booking':
            return MaterialPageRoute(
              builder: (_) => const BookingScreen(
                slug: 'default',
                artistId: '',
                artistName: '',
                serviceId: '',
                serviceName: '',
                durationMinutes: 0,
                price: 0,
              ),
            );
          case '/profile':
            return MaterialPageRoute(builder: (_) => const ProfileScreen());
          case '/artist-public':
            return MaterialPageRoute(
                builder: (_) => const ArtistPublicScreen());
          case '/artist-dashboard':
            return MaterialPageRoute(
                builder: (_) => const ArtistDashboardScreen());
          case '/manager-dashboard':
            return MaterialPageRoute(
                builder: (_) => const ManagerDashboardScreen());
          case '/admin-dashboard':
            return MaterialPageRoute(builder: (_) => const AdminDashboard());
          case '/client-dashboard':
            return MaterialPageRoute(
                builder: (_) => const ClientDashboardScreen());
          case '/my-appointments':
            return MaterialPageRoute(
                builder: (_) => const MyAppointmentsScreen());
          case '/generated-salon-detail':
            return MaterialPageRoute(builder: (_) => const SalonDetailScreen());
          case '/generated-home':
            return MaterialPageRoute(builder: (_) => const HomeScreen());
          case '/generated-booking':
            return MaterialPageRoute(builder: (_) => const BookingFlowScreen());
          case '/generated-onboarding':
            return MaterialPageRoute(builder: (_) => const OnboardingScreen());
          default:
            return MaterialPageRoute(builder: (_) => const SplashScreen());
        }
      },
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(
          seedColor: AppColors.primary,
          brightness: Brightness.light,
        ),
        useMaterial3: true,
        fontFamily: 'IRANSans',
        scaffoldBackgroundColor: AppColors.background,
        textTheme: AppTextTheme.farsi(
          colorScheme: ColorScheme.fromSeed(seedColor: AppColors.primary),
        ),
        appBarTheme: const AppBarTheme(
          backgroundColor: AppColors.primary,
          foregroundColor: Colors.white,
          centerTitle: true,
          elevation: 0,
          scrolledUnderElevation: 0,
        ),
        elevatedButtonTheme: ElevatedButtonThemeData(
          style: ElevatedButton.styleFrom(
            backgroundColor: AppColors.primary,
            foregroundColor: Colors.white,
            minimumSize: const Size(double.infinity, 52),
            elevation: 0,
            shape: const RoundedRectangleBorder(
              borderRadius: AppSpacing.borderRadiusMd,
            ),
            textStyle: const TextStyle(
              fontWeight: FontWeight.w600,
              fontSize: 16,
            ),
          ),
        ),
        inputDecorationTheme: const InputDecorationTheme(
          filled: true,
          fillColor: Colors.white,
          border: OutlineInputBorder(
            borderRadius: AppSpacing.borderRadiusMd,
            borderSide: BorderSide(color: AppColors.border),
          ),
          enabledBorder: OutlineInputBorder(
            borderRadius: AppSpacing.borderRadiusMd,
            borderSide: BorderSide(color: AppColors.border),
          ),
          focusedBorder: OutlineInputBorder(
            borderRadius: AppSpacing.borderRadiusMd,
            borderSide: BorderSide(color: AppColors.primaryLight, width: 2),
          ),
          contentPadding: EdgeInsets.symmetric(
            horizontal: AppSpacing.md,
            vertical: 14,
          ),
          hintStyle: TextStyle(color: AppColors.textMuted),
        ),
        cardTheme: const CardThemeData(
          color: AppColors.surface,
          elevation: 0,
          shape: RoundedRectangleBorder(
            borderRadius: AppSpacing.borderRadiusLg,
            side: BorderSide(color: AppColors.border),
          ),
        ),
        navigationBarTheme: NavigationBarThemeData(
          backgroundColor: Colors.white,
          indicatorColor: AppColors.primary100,
          elevation: 0,
          labelTextStyle: WidgetStateProperty.resolveWith((states) {
            if (states.contains(WidgetState.selected)) {
              return const TextStyle(
                color: AppColors.primary,
                fontWeight: FontWeight.w600,
                fontSize: 12,
              );
            }
            return const TextStyle(
              color: AppColors.textMuted,
              fontSize: 12,
            );
          }),
        ),
        snackBarTheme: const SnackBarThemeData(
          behavior: SnackBarBehavior.floating,
          shape: RoundedRectangleBorder(
            borderRadius: AppSpacing.borderRadiusMd,
          ),
        ),
        dividerTheme: const DividerThemeData(
          color: AppColors.border,
          thickness: 1,
          space: 1,
        ),
        listTileTheme: const ListTileThemeData(
          iconColor: AppColors.textSecondary,
          contentPadding: EdgeInsets.symmetric(horizontal: AppSpacing.xs),
        ),
        tabBarTheme: const TabBarThemeData(
          labelColor: AppColors.primary,
          unselectedLabelColor: AppColors.textMuted,
          indicatorColor: AppColors.primary,
          dividerColor: AppColors.border,
          labelStyle: TextStyle(fontWeight: FontWeight.w600, fontSize: 14),
        ),
        chipTheme: ChipThemeData(
          backgroundColor: AppColors.background,
          side: const BorderSide(color: AppColors.border),
          labelStyle:
              const TextStyle(fontSize: 12, color: AppColors.textSecondary),
          shape:
              RoundedRectangleBorder(borderRadius: AppSpacing.borderRadiusSm),
          padding: const EdgeInsets.symmetric(
              horizontal: AppSpacing.xs, vertical: 4),
        ),
      ),
      home: const SplashScreen(),
    );
  }
}
