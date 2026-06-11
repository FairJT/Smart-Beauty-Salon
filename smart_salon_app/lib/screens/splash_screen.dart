import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/app_colors.dart';
import 'auth/login_screen.dart';
import 'home/home_screen.dart';

class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen> {
  @override
  void initState() {
    super.initState();
    _checkLogin();
  }

  Future<void> _checkLogin() async {
    // ۲ ثانیه صبر می‌کنیم
    await Future.delayed(const Duration(seconds: 2));

    // بررسی می‌کنیم توکن داریم یا نه
    final token = await ApiService.getToken();

    if (!mounted) return;

    // اگر توکن داریم → خانه، اگر نه → ورود
    Navigator.pushReplacement(
      context,
      MaterialPageRoute(
        builder: (_) =>
            token != null ? const HomeScreen() : const LoginScreen(),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.primary,
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            // آیکون
            Container(
              width: 120,
              height: 120,
              decoration: BoxDecoration(
                color: Colors.white.withOpacity(0.15),
                borderRadius: BorderRadius.circular(30),
              ),
              child: const Icon(
                Icons.content_cut_rounded,
                size: 70,
                color: Colors.amber,
              ),
            ),

            const SizedBox(height: 24),

            // اسم اپ
            const Text(
              'سالن هوشمند ابری',
              style: TextStyle(
                color: Colors.white,
                fontSize: 28,
                fontWeight: FontWeight.bold,
              ),
            ),

            const SizedBox(height: 8),

            const Text(
              'Smart Cloud Salon',
              style: TextStyle(
                color: Colors.white60,
                fontSize: 16,
              ),
            ),

            const SizedBox(height: 60),

            // لودینگ
            const CircularProgressIndicator(
              color: Colors.amber,
              strokeWidth: 3,
            ),
          ],
        ),
      ),
    );
  }
}