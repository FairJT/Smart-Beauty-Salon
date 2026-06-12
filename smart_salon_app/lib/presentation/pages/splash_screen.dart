import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/app_colors.dart';
import '../providers/auth_provider.dart';
import 'login_screen.dart';
import 'home_screen.dart';
import 'admin/admin_dashboard.dart';
import 'artist/artist_schedule_screen.dart';

class SplashScreen extends ConsumerStatefulWidget {
  const SplashScreen({super.key});

  @override
  ConsumerState<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends ConsumerState<SplashScreen> {
  @override
  void initState() {
    super.initState();
    _navigate();
  }

  Future<void> _navigate() async {
    await Future.delayed(const Duration(seconds: 2));
    if (!mounted) return;

    final auth = ref.read(authProvider);

    if (!auth.isLoggedIn) {
      Navigator.pushReplacement(context, MaterialPageRoute(builder: (_) => const LoginScreen()));
      return;
    }

    Widget destination;
    if (auth.isSuperAdmin) {
      destination = const AdminDashboard();
    } else if (auth.isSalonManager) {
      destination = const HomeScreen();
    } else if (auth.isArtist) {
      destination = const ArtistScheduleScreen();
    } else {
      destination = const HomeScreen();
    }

    Navigator.pushReplacement(context, MaterialPageRoute(builder: (_) => destination));
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        width: double.infinity,
        height: double.infinity,
        decoration: const BoxDecoration(gradient: AppColors.darkGradient),
        child: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Container(
                width: 120,
                height: 120,
                decoration: BoxDecoration(
                  gradient: AppColors.primaryGradient,
                  borderRadius: BorderRadius.circular(30),
                  boxShadow: [
                    BoxShadow(
                      color: AppColors.primary.withValues(alpha: 0.4),
                      blurRadius: 24,
                      offset: const Offset(0, 8),
                    ),
                  ],
                ),
                child: const Icon(Icons.content_cut_rounded, size: 70, color: Colors.white),
              ),
              const SizedBox(height: 24),
              const Text(
                'سالن هوشمند',
                style: TextStyle(color: Colors.white, fontSize: 28, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 8),
              const Text('SMART SALON', style: TextStyle(color: Colors.white60, fontSize: 16)),
              const SizedBox(height: 60),
              const CircularProgressIndicator(color: Colors.white, strokeWidth: 3),
            ],
          ),
        ),
      ),
    );
  }
}
