import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/fresha/fresha_ui.dart';
import '../../presentation/providers/auth_provider.dart';

class HomeScreen extends ConsumerWidget {
  const HomeScreen({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final auth = ref.watch(authProvider);
    if (auth.loading) {
      return const Scaffold(
        body: Center(child: CircularProgressIndicator()),
      );
    }

    // Guest users see limited UI
    if (auth.isGuest) {
      return Scaffold(
        appBar: AppBar(
          title: const Text('Salon زیبایی'),
        ),
        body: const Center(
          child: Text(
            'به حالت مهمان وارد شدید. برخی امکانات محدود هستند.',
            style: TextStyle(fontSize: 16),
          ),
        ),
        bottomNavigationBar: FBottomNav(
          index: 0,
          onTap: (i) {},
          items: const [],
        ),
      );
    }

    // Regular logged‑in user UI (placeholder)
    return Scaffold(
      appBar: AppBar(
        title: const Text('خانه'),
      ),
      body: const Center(child: Text('Home Screen Placeholder')),
      bottomNavigationBar: FBottomNav(
        index: 0,
        onTap: (i) {},
        items: const [],
      ),
    );
  }
}
