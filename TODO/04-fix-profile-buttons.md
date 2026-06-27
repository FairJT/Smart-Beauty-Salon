# ۰۴ — رفع دکمه‌های خالی پروفایل 🟡

سه آیتم منوی پروفایل `onTap: () {}` خالی دارند.

**فایل:** `smart_salon_app/lib/presentation/pages/profile_screen.dart`

**۱) «رزروهای من» — پیدا کن (دقیق):**
```dart
              icon: Icons.calendar_month, title: 'رزروهای من', onTap: () {}),
```
**جایگزین کن با:**
```dart
              icon: Icons.calendar_month,
              title: 'رزروهای من',
              onTap: () => Navigator.of(context).pushNamed('/my-appointments')),
```

**۲) «راهنما» — پیدا کن (دقیق):**
```dart
              icon: Icons.help_outline, title: 'راهنما', onTap: () {}),
```
**جایگزین کن با:**
```dart
              icon: Icons.help_outline,
              title: 'راهنما',
              onTap: () => showDialog(
                    context: context,
                    builder: (_) => const AlertDialog(
                      title: Text('راهنما'),
                      content: Text('برای پشتیبانی با ما در ارتباط باشید.'),
                    ),
                  )),
```

**۳) «درباره ما» — پیدا کن (دقیق):**
```dart
              icon: Icons.info_outline, title: 'درباره ما', onTap: () {}),
```
**جایگزین کن با:**
```dart
              icon: Icons.info_outline,
              title: 'درباره ما',
              onTap: () => showAboutDialog(
                    context: context,
                    applicationName: 'سالن هوشمند',
                    applicationVersion: '۱.۰.۰',
                  )),
```

> اگر مسیر `/my-appointments` در main.dart نبود، STOP و گزارش بده.

**تأیید (PowerShell):**
```powershell
Select-String -Path smart_salon_app/lib/presentation/pages/profile_screen.dart -Pattern "onTap: \(\) \{\}"
# انتظار: هیچ نتیجه‌ای برنگردد
cd smart_salon_app ; flutter analyze
```

**تمام وقتی که:** هیچ دکمه‌ی خالی در پروفایل نماند.
