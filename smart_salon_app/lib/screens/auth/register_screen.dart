import 'package:flutter/material.dart';
import '../../core/api_constants.dart';
import '../../core/api_service.dart';
import '../../core/app_colors.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _firstNameController   = TextEditingController();
  final _lastNameController    = TextEditingController();
  final _mobileController      = TextEditingController();
  final _nationalCodeController = TextEditingController();
  final _passwordController    = TextEditingController();
  bool _loading  = false;
  bool _hidePass = true;
  String? _error;

  // ─── ثبت‌نام ───────────────────────────────────────────
  Future<void> _register() async {
    if (_firstNameController.text.isEmpty ||
        _lastNameController.text.isEmpty ||
        _mobileController.text.isEmpty ||
        _nationalCodeController.text.isEmpty ||
        _passwordController.text.isEmpty) {
      setState(() => _error = 'لطفاً همه فیلدها را پر کنید');
      return;
    }

    setState(() { _loading = true; _error = null; });

    try {
      await ApiService.post(ApiConstants.register, {
        'firstName':   _firstNameController.text.trim(),
        'lastName':    _lastNameController.text.trim(),
        'mobile':      _mobileController.text.trim(),
        'nationalCode': _nationalCodeController.text.trim(),
        'password':    _passwordController.text,
      });

      if (!mounted) return;

      // موفق شد — برگشت به صفحه ورود
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('ثبت‌نام موفق بود! وارد شوید.'),
          backgroundColor: Colors.green,
        ),
      );

      Navigator.pop(context);

    } catch (e) {
      setState(() => _error = e.toString().replaceAll('Exception: ', ''));
    } finally {
      setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.primary,
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 28, vertical: 40),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const SizedBox(height: 20),

              // ─── عنوان ──────────────────────────────────
              const Icon(
                Icons.person_add_rounded,
                size: 70,
                color: Colors.amber,
              ),

              const SizedBox(height: 16),

              const Text(
                'ثبت‌نام در سالن هوشمند',
                textAlign: TextAlign.center,
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 22,
                  fontWeight: FontWeight.bold,
                ),
              ),

              const SizedBox(height: 32),

              // ─── فیلد نام ───────────────────────────────
              TextField(
                controller: _firstNameController,
                textAlign: TextAlign.right,
                decoration: const InputDecoration(
                  hintText: 'نام',
                  prefixIcon: Icon(Icons.person_outline),
                ),
              ),

              const SizedBox(height: 12),

              // ─── فیلد نام خانوادگی ───────────────────────
              TextField(
                controller: _lastNameController,
                textAlign: TextAlign.right,
                decoration: const InputDecoration(
                  hintText: 'نام خانوادگی',
                  prefixIcon: Icon(Icons.person_outline),
                ),
              ),

              const SizedBox(height: 12),

              // ─── فیلد موبایل ────────────────────────────
              TextField(
                controller: _mobileController,
                keyboardType: TextInputType.phone,
                textAlign: TextAlign.right,
                decoration: const InputDecoration(
                  hintText: 'شماره موبایل',
                  prefixIcon: Icon(Icons.phone_android),
                ),
              ),

              const SizedBox(height: 12),

              // ─── فیلد کد ملی ────────────────────────────
              TextField(
                controller: _nationalCodeController,
                keyboardType: TextInputType.number,
                textAlign: TextAlign.right,
                decoration: const InputDecoration(
                  hintText: 'کد ملی',
                  prefixIcon: Icon(Icons.badge_outlined),
                ),
              ),

              const SizedBox(height: 12),

              // ─── فیلد رمز عبور ──────────────────────────
              TextField(
                controller: _passwordController,
                obscureText: _hidePass,
                textAlign: TextAlign.right,
                decoration: InputDecoration(
                  hintText: 'رمز عبور (حداقل ۸ کاراکتر)',
                  prefixIcon: const Icon(Icons.lock_outline),
                  suffixIcon: IconButton(
                    icon: Icon(
                      _hidePass ? Icons.visibility_off : Icons.visibility,
                      color: AppColors.gray,
                    ),
                    onPressed: () =>
                        setState(() => _hidePass = !_hidePass),
                  ),
                ),
              ),

              const SizedBox(height: 24),

              // ─── پیام خطا ───────────────────────────────
              if (_error != null)
                Container(
                  padding: const EdgeInsets.all(12),
                  margin: const EdgeInsets.only(bottom: 16),
                  decoration: BoxDecoration(
                    color: Colors.red.shade100,
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Text(
                    _error!,
                    textAlign: TextAlign.center,
                    style: const TextStyle(color: Colors.red),
                  ),
                ),

              // ─── دکمه ثبت‌نام ────────────────────────────
              ElevatedButton(
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.amber,
                  foregroundColor: Colors.white,
                  minimumSize: const Size(double.infinity, 52),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                onPressed: _loading ? null : _register,
                child: _loading
                    ? const SizedBox(
                        width: 24,
                        height: 24,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: Colors.white,
                        ),
                      )
                    : const Text(
                        'ثبت‌نام',
                        style: TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
              ),

              const SizedBox(height: 16),

              // ─── لینک ورود ──────────────────────────────
              TextButton(
                onPressed: () => Navigator.pop(context),
                child: const Text(
                  'قبلاً ثبت‌نام کردید؟  وارد شوید',
                  style: TextStyle(color: Colors.white70),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}