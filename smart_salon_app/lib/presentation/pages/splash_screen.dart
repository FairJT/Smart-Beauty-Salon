import 'package:flutter/material.dart';
import '../../core/fresha/fresha_ui.dart';

class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen> {
  @override
  void initState() {
    super.initState();
    _initApp();
  }

  Future<void> _initApp() async {
    await Future.delayed(const Duration(seconds: 2));
    if (!mounted) return;
    Navigator.of(context).pushReplacementNamed('/login');
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: FCol.surface,
      body: Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 100,
              height: 100,
              decoration: BoxDecoration(
                color: FCol.olive,
                borderRadius: BorderRadius.circular(24),
              ),
              child:
                  const Icon(Icons.spa_rounded, size: 52, color: Colors.white),
            ),
            const SizedBox(height: 20),
            const Text('سالن زیبایی',
                style: TextStyle(
                    fontSize: 22,
                    fontWeight: FontWeight.w700,
                    color: FCol.ink)),
            const SizedBox(height: 6),
            const Text('رزرو آنلاین خدمات زیبایی',
                style: TextStyle(fontSize: 13, color: FCol.muted)),
            const SizedBox(height: 40),
            Row(
              mainAxisSize: MainAxisSize.min,
              children: List.generate(3, (i) {
                return AnimatedContainer(
                  duration: Duration(milliseconds: 400 + (i * 150)),
                  margin: const EdgeInsets.symmetric(horizontal: 4),
                  width: 8,
                  height: 8,
                  decoration: BoxDecoration(
                    color: FCol.olive.withValues(alpha: 0.5 + (i * 0.17)),
                    shape: BoxShape.circle,
                  ),
                );
              }),
            ),
          ],
        ),
      ),
    );
  }
}
