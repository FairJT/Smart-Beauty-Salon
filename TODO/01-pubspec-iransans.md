# ۰۱ — معرفی فونت ایران‌سانس در pubspec 🟡

**فایل:** `smart_salon_app/pubspec.yaml`

فایل‌های فونت همین‌جان: `smart_salon_app/fonts/IRANSansX-Regular.ttf` و
`IRANSansX-Bold.ttf`. فقط باید معرفی بشن.

**پیدا کن (دقیق):**
```yaml
flutter:
  generate: true
  uses-material-design: true
```
**جایگزین کن با:**
```yaml
flutter:
  generate: true
  uses-material-design: true
  fonts:
    - family: IRANSans
      fonts:
        - asset: fonts/IRANSansX-Regular.ttf
          weight: 400
        - asset: fonts/IRANSansX-Bold.ttf
          weight: 700
```

> دقت کن تورفتگی (indentation) با بقیه‌ی فایل yaml یکی باشه — دو فاصله.

**تأیید (PowerShell):**
```powershell
Select-String -Path smart_salon_app/pubspec.yaml -Pattern "IRANSans"
cd smart_salon_app ; flutter pub get
```

**تمام وقتی که:** `flutter pub get` بدون خطا اجرا بشه و خانواده‌ی فونت `IRANSans` شناخته بشه.
