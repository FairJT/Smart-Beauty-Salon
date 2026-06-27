# ۰۲ — وصل‌کردن کلاینت به پوسته (نوار پایین فعال) 🟡

کلاینت الان به `/client-dashboard` می‌رود که یک صفحه‌ی تکی بدون نوار پایین است.
آن را به `MainShell` تغییر بده تا کلاینت ناوبری کامل داشته باشد.

**فایل:** `smart_salon_app/lib/main.dart`

**۱) افزودن import** کنار بقیه‌ی importها:
```dart
import 'presentation/pages/main_shell.dart';
```

**۲) پیدا کن (دقیق):**
```dart
          case '/client-dashboard':
            return MaterialPageRoute(
                builder: (_) => const ClientDashboardScreen());
```
**جایگزین کن با:**
```dart
          case '/client-dashboard':
            return MaterialPageRoute(
                builder: (_) => const MainShell());
```

> import قبلی `ClientDashboardScreen` را پاک نکن مگر اینکه analyze بگوید استفاده‌نشده است؛
> چون داخل `MainShell` استفاده می‌شود مشکلی نیست اگر بماند.

**تأیید (PowerShell):**
```powershell
Select-String -Path smart_salon_app/lib/main.dart -Pattern "MainShell"
cd smart_salon_app ; flutter analyze
```

**تمام وقتی که:** لاگین با کاربر کلاینت → پوسته با نوار پایین باز شود و چهار تب جابه‌جا شوند.
