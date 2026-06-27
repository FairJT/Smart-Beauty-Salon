# ۰۲ — ست‌کردن فونت روی کل تم 🟢

با یه خط، کل اپ ایران‌سانس می‌شه (textTheme و همه‌ی ویجت‌ها از این ارث می‌برن).

**فایل:** `smart_salon_app/lib/main.dart`

**پیدا کن (دقیق):**
```dart
        useMaterial3: true,
        scaffoldBackgroundColor: AppColors.background,
```
**جایگزین کن با:**
```dart
        useMaterial3: true,
        fontFamily: 'IRANSans',
        scaffoldBackgroundColor: AppColors.background,
```

**تأیید (PowerShell):**
```powershell
Select-String -Path smart_salon_app/lib/main.dart -Pattern "fontFamily: 'IRANSans'"
cd smart_salon_app ; flutter analyze
```

**تمام وقتی که:** اپ اجرا بشه و همه‌ی متن‌ها (لاگین، داشبوردها، کارت‌ها) با ایران‌سانس دیده بشن.
