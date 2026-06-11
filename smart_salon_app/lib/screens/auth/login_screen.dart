import 'package:flutter/material.dart';
import '../../core/api_constants.dart';
import '../../core/api_service.dart';
import '../../core/app_colors.dart';
import '../home/home_screen.dart';
import 'register_screen.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _mobileController   = TextEditingController();
  final _passwordController = TextEditingController();
  bool _loading  = false;
  bool _hidePass = true;
  String? _error;

  // ─── ورود ─────────────────────────────────────────────
  Future<void> _login() async {
    if (_mobileController.text.isEmpty || _passwordController.text.isEmpty) {
      setState(() => _error = 'لطفاً همه فیلدها را پر کنید');
      return;
    }

    setState(() { _loading = true; _error = null; });

    try {
      final res = await ApiService.post(ApiConstants.login, {
        'mobile':   _mobileController.text.trim(),
        'password': _passwordController.text,
      });

      // ذخیره توکن
      await ApiService.saveToken(res['token'] as String);

      if (!mounted) return;

      // رفتن به صفحه خانه
      Navigator.pushReplacement(
        context,
        MaterialPageRoute(builder: (_) => const HomeScreen()),
      );

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
              const SizedBox(height: 40),

              // ─── آیکون ──────────────────────────────────
              const Icon(
                Icons.content_cut_rounded,
                size: 80,
                color: Colors.amber,
              ),

              const SizedBox(height: 16),

              const Text(
                'سالن هوشمند ابری',
                textAlign: TextAlign.center,
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 24,
                  fontWeight: FontWeight.bold,
                ),
              ),

              const SizedBox(height: 8),

              const Text(
                'وارد حساب کاربری خود شوید',
                textAlign: TextAlign.center,
                style: TextStyle(color: Colors.white60, fontSize: 14),
              ),

              const SizedBox(height: 48),

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

              const SizedBox(height: 16),

              // ─── فیلد رمز عبور ──────────────────────────
              TextField(
                controller: _passwordController,
                obscureText: _hidePass,
                textAlign: TextAlign.right,
                decoration: InputDecoration(
                  hintText: 'رمز عبور',
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

              // ─── دکمه ورود ──────────────────────────────
              ElevatedButton(
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.amber,
                  foregroundColor: Colors.white,
                  minimumSize: const Size(double.infinity, 52),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                onPressed: _loading ? null : _login,
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
                        'ورود',
                        style: TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
              ),

              const SizedBox(height: 16),

              // ─── لینک ثبت‌نام ────────────────────────────
              TextButton(
                onPressed: () => Navigator.push(
                  context,
                  MaterialPageRoute(builder: (_) => const RegisterScreen()),
                ),
                child: const Text(
                  'حساب ندارید؟  ثبت‌نام کنید',
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