# 04 — Delete the OTP screen file 🟡

Only after tasks 01 & 02 (so nothing imports it).

```powershell
Remove-Item smart_salon_app\lib\presentation\pages\otp_screen.dart
```

**Verify first:** `Select-String -Path smart_salon_app\lib -Pattern "otp_screen.dart" -Recurse` → 0 hits.
If any hit remains, fix that import first, then delete.
