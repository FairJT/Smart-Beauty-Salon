import 'package:flutter/material.dart';
import 'package:smart_salon_app/core/fresha/fresha_ui.dart';

class HomeScreen extends StatelessWidget {
  const HomeScreen({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
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
