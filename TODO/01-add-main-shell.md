# ۰۱ — افزودن پوسته‌ی ناوبری 🟢

**ایجاد فایل:** `smart_salon_app/lib/presentation/pages/main_shell.dart`
**محتوا:** دقیقاً از فایل `main_shell.dart` که همراه این تسک‌هاست استفاده کن.

این پوسته چهار تب واقعی دارد: خانه (داشبورد کلاینت)، نوبت‌ها، اعلان‌ها، پروفایل —
و نوار پایینش با `setState` کار می‌کند.

**تأیید (PowerShell):**
```powershell
Test-Path smart_salon_app/lib/presentation/pages/main_shell.dart
cd smart_salon_app ; flutter analyze lib/presentation/pages/main_shell.dart
```

> اگر هرکدام از چهار صفحه (ClientDashboardScreen / AppointmentList / NotificationsScreen /
> ProfileScreen) پیدا نشد، STOP و گزارش بده — مسیر import را حدس نزن.

**تمام وقتی که:** فایل بدون خطا analyze شود.
