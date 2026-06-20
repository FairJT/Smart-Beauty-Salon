# Task B3 — Delete the dead legacy screens 🟢

`lib/screens/booking/booking_screen.dart` and `lib/screens/home/home_screen.dart` are old
duplicates of the files under `lib/presentation/pages/`. Nothing imports them.

**Step 1 — confirm they're truly unused (PowerShell):**
```powershell
Select-String -Path smart_salon_app\lib -Pattern "screens/booking|screens/home" -Recurse |
  Where-Object { $_.Path -notmatch "lib\\screens\\" }
```
Expect **0 hits**. If there ARE hits, STOP — they're still referenced, don't delete.

**Step 2 — delete the files (only if Step 1 was empty):**
```powershell
Remove-Item smart_salon_app\lib\screens\booking\booking_screen.dart
Remove-Item smart_salon_app\lib\screens\home\home_screen.dart
```
Then delete the now-empty `screens\booking` and `screens\home` folders if nothing else is in them.

**Done when:** the two files are gone and `flutter analyze` still passes.
