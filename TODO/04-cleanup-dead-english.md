# ۰۴ — پاک‌سازی فایل‌های مرده‌ی انگلیسی 🟢

این فایل‌ها هیچ‌جا import یا استفاده نمی‌شن و فقط متن انگلیسی/placeholder دارن:
- `smart_salon_app/lib/presentation/pages/all_screens_skeletons.dart`
- `smart_salon_app/lib/presentation/pages/all_generated_screens.dart`
- `smart_salon_app/lib/presentation/pages/home_screen.dart` (نسخه‌ی روت — روی `/home` نسخه‌ی `generated/` استفاده می‌شه)

**قبل از حذف، مطمئن شو واقعاً استفاده نمی‌شن (PowerShell):**
```powershell
Select-String -Path smart_salon_app/lib -Recurse -Pattern "all_screens_skeletons|all_generated_screens" |
  Where-Object { $_.Path -notmatch "all_screens_skeletons|all_generated_screens" }
# اگه چیزی برنگشت، یعنی استفاده نمی‌شن
```

اگه خروجی خالی بود، حذفشون کن:
```powershell
Remove-Item smart_salon_app/lib/presentation/pages/all_screens_skeletons.dart
Remove-Item smart_salon_app/lib/presentation/pages/all_generated_screens.dart
```

> فایل روت `home_screen.dart` رو فقط وقتی حذف کن که مطمئن شدی هیچ import فعالی بهش اشاره نمی‌کنه
> (`Select-String ... -Pattern "pages/home_screen.dart'"`). در غیر این صورت دست نزن و گزارش بده.

**تمام وقتی که:** `flutter analyze` بعد از حذف بدون خطا باشه و دیگه متن انگلیسی توی کد زنده نباشه.
