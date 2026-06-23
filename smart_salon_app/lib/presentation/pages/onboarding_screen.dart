import 'package:flutter/material.dart';
import 'package:smart_salon_app/core/fresha/fresha_ui.dart';

class OnboardingScreen extends StatelessWidget {
  const OnboardingScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: FPrimaryButton(
          'شروع',
          onTap: () => Navigator.pushReplacementNamed(context, '/home'),
        ),
      ),
    );
  }
}
