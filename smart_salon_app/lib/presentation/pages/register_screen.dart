import 'package:flutter/material.dart';
import '../../core/fresha/fresha_ui.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _nameController = TextEditingController();
  String? _selectedCity;
  bool _loading = false;

  final List<String> _cities = [
    'تهران',
    'اصفهان',
    'شیراز',
    'مشهد',
    'تبریز',
    'کرج',
    'قم',
    'اهواز',
  ];

  @override
  void dispose() {
    _nameController.dispose();
    super.dispose();
  }

  void _register() async {
    if (_nameController.text.trim().isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('نام خود را وارد کنید')),
      );
      return;
    }
    setState(() => _loading = true);
    await Future.delayed(const Duration(seconds: 1));
    if (!mounted) return;
    setState(() => _loading = false);
    Navigator.of(context).pushReplacementNamed('/home');
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: FCol.surface,
      appBar: AppBar(
        backgroundColor: FCol.surface,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: FCol.ink),
          onPressed: () => Navigator.of(context).pop(),
        ),
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const SizedBox(height: 20),
              const Center(
                child: Text(
                  'تکمیل پروفایل',
                  style: TextStyle(
                    fontSize: 20,
                    fontWeight: FontWeight.w700,
                    color: FCol.ink,
                  ),
                ),
              ),
              const SizedBox(height: 8),
              const Center(
                child: Text(
                  'اطلاعات خود را تکمیل کنید',
                  style: TextStyle(fontSize: 13, color: FCol.muted),
                ),
              ),
              const SizedBox(height: 36),
              Center(
                child: Stack(
                  children: [
                    const CircleAvatar(
                      radius: 44,
                      backgroundColor: FCol.olive,
                      child: Icon(Icons.person, size: 44, color: Colors.white),
                    ),
                    Positioned(
                      bottom: 0,
                      right: 0,
                      child: Container(
                        width: 30,
                        height: 30,
                        decoration: BoxDecoration(
                          color: FCol.ink,
                          shape: BoxShape.circle,
                          border: Border.all(color: FCol.surface, width: 2),
                        ),
                        child: const Icon(Icons.camera_alt,
                            size: 14, color: Colors.white),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 32),
              const Text('نام و نام خانوادگی',
                  style: TextStyle(
                      fontSize: 13,
                      fontWeight: FontWeight.w600,
                      color: FCol.ink)),
              const SizedBox(height: 8),
              FCard(
                child: TextField(
                  controller: _nameController,
                  decoration: const InputDecoration(
                    hintText: 'مثال: علی محمدی',
                    hintStyle: TextStyle(color: FCol.muted),
                    border: InputBorder.none,
                    prefixIcon:
                        Icon(Icons.person_outline, color: FCol.olive, size: 20),
                  ),
                ),
              ),
              const SizedBox(height: 20),
              const Text('شهر (اختیاری)',
                  style: TextStyle(
                      fontSize: 13,
                      fontWeight: FontWeight.w600,
                      color: FCol.ink)),
              const SizedBox(height: 8),
              FCard(
                child: DropdownButtonFormField<String>(
                  value: _selectedCity,
                  decoration: const InputDecoration(
                    border: InputBorder.none,
                    prefixIcon: Icon(Icons.location_on_outlined,
                        color: FCol.olive, size: 20),
                  ),
                  hint: const Text('انتخاب شهر',
                      style: TextStyle(color: FCol.muted)),
                  items: _cities
                      .map((c) => DropdownMenuItem(value: c, child: Text(c)))
                      .toList(),
                  onChanged: (v) => setState(() => _selectedCity = v),
                ),
              ),
              const SizedBox(height: 32),
              FPrimaryButton(
                'ثبت و ادامه',
                onTap: _loading ? null : _register,
              ),
              if (_loading)
                const Padding(
                  padding: EdgeInsets.only(top: 16),
                  child: FLoading(),
                ),
            ],
          ),
        ),
      ),
    );
  }
}
