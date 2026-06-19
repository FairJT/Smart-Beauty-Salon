# Fix: dashboard 401 (token storage) + appointment crashes (field/response) — Agent TODO

**For:** a free / local AI coding agent (Continue.dev + deepseek-coder) on Windows / PowerShell
**Prereq:** apply connections-and-auth-fix.md first (nginx single-/api + platform-owner). Routing is now
single `/api/...` and reaches the backend.

## Bugs
1. **401 on every dashboard + favorites.** Token is stored in TWO different places:
   - login + `DioClient` use **SharedPreferences** (key `auth_token`).
   - `ApiService` (used by the dashboards + favorites) uses **FlutterSecureStorage** — which is empty,
     so it sends no `Authorization` header → 401. Fix: make `ApiService` use SharedPreferences too.
2. **Appointment list crashes.** `/api/appointments/mine` and `/{id}` return `BookingDto`
   (`startsAt`, `endsAt`, `estimatedPriceAmount`, `depositAmountValue`), but the Flutter parser reads
   `startTime`/`endTime`/`estimatedPrice`/`depositAmount` → `DateTime.parse(null)` → crash.
3. **Create / cancel appointment crash.** `/simple` returns `{message, id, deposit}` and `/cancel` returns
   `{message}` — neither contains booking dates, so building an `AppointmentEntity` from them crashes.
   Fix: after create/cancel, refetch the booking by id.
**Important:** the SLOT parser (`getAvailableSlots`) reads `startTime`/`endTime`/`isAvailable` and that is
CORRECT (backend `SlotDto` uses those names). **Do not touch the slot block.**
**Generated:** 2026-06-18

Flags: 🟢 safe · 🟡 agent does it, human reviews · 🔴 human-only / hand-off.

---

## PHASE A — Fix dashboard 401 (unify token storage)  ← current blocker, do first

### A1 — Make ApiService read/write the token from SharedPreferences 🟡
**File:** `smart_salon_app/lib/core/api_service.dart`

**Edit 1 — the import. Find:**
```dart
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
```
**Replace with:**
```dart
import 'package:shared_preferences/shared_preferences.dart';
```

**Edit 2 — the token methods. Find:**
```dart
  static const _storage = FlutterSecureStorage();
  static const _tokenKey = 'auth_token';

  static Future<void> saveToken(String token) async {
    await _storage.write(key: _tokenKey, value: token);
  }

  static Future<String?> getToken() async {
    return await _storage.read(key: _tokenKey);
  }

  static Future<void> clearToken() async {
    await _storage.delete(key: _tokenKey);
  }
```
**Replace with:**
```dart
  static const _tokenKey = 'auth_token';

  static Future<void> saveToken(String token) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_tokenKey, token);
  }

  static Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_tokenKey);
  }

  static Future<void> clearToken() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_tokenKey);
  }
```
**Done when:** `ApiService` no longer references `FlutterSecureStorage`; all three token methods use
`SharedPreferences`. Verify: `Select-String -Path smart_salon_app\lib\core\api_service.dart -Pattern "FlutterSecureStorage|SharedPreferences"` shows only SharedPreferences.

---

## PHASE B — Fix appointment crashes

### B1 — Fix the booking parser field names 🟡
**File:** `smart_salon_app/lib/data/repositories/appointment_repository_impl.dart`
**Find:**
```dart
  AppointmentEntity _parseAppointment(Map<String, dynamic> json) {
    return AppointmentEntity(
      id: json['id']?.toString() ?? '',
      startTime: DateTime.parse(json['startTime']),
      endTime: DateTime.parse(json['endTime']),
      status: json['status'] ?? 1,
      estimatedPrice: (json['estimatedPrice'] ?? 0).toDouble(),
      depositAmount: (json['depositAmount'] ?? 0).toDouble(),
```
**Replace with:**
```dart
  AppointmentEntity _parseAppointment(Map<String, dynamic> json) {
    return AppointmentEntity(
      id: json['id']?.toString() ?? '',
      startTime: DateTime.parse(json['startsAt']),
      endTime: DateTime.parse(json['endsAt']),
      status: json['status'] ?? 1,
      estimatedPrice: (json['estimatedPriceAmount'] ?? 0).toDouble(),
      depositAmount: (json['depositAmountValue'] ?? 0).toDouble(),
```
**Done when:** `_parseAppointment` reads `startsAt`/`endsAt`/`estimatedPriceAmount`/`depositAmountValue`.
(Leave the rest of the method — isRated/rating/comment/salonName/artistName/serviceName — unchanged.)

### B2 — Create: refetch the booking instead of parsing the confirmation 🟡
**File:** same file, in `createAppointment`.
**Find:**
```dart
    final json = response.data;
    return AppointmentEntity(
      id: json['id']?.toString() ?? '',
      startTime: DateTime.parse(json['startTime']),
      endTime: DateTime.parse(json['endTime']),
      status: json['status'] ?? 1,
      estimatedPrice: (json['estimatedPrice'] ?? 0).toDouble(),
      depositAmount: (json['depositAmount'] ?? 0).toDouble(),
    );
  }
```
**Replace with:**
```dart
    final json = response.data;
    return await getAppointmentById(json['id'].toString());
  }
```
**Done when:** `createAppointment` returns `getAppointmentById(...)` using the new booking's id.

### B3 — Cancel: refetch the booking instead of parsing the confirmation 🟡
**File:** same file, in `cancelAppointment`.
**Find:**
```dart
    final response = await DioClient.instance.put(
      '${ApiConstants.appointments}/$id/cancel',
      data: {},
    );

    final json = response.data;
    return AppointmentEntity(
      id: id,
      startTime: DateTime.parse(json['startTime']),
      endTime: DateTime.parse(json['endTime']),
      status: 5,
      estimatedPrice: (json['estimatedPrice'] ?? 0).toDouble(),
      depositAmount: (json['depositAmount'] ?? 0).toDouble(),
    );
  }
```
**Replace with:**
```dart
    await DioClient.instance.put(
      '${ApiConstants.appointments}/$id/cancel',
      data: {},
    );

    return await getAppointmentById(id);
  }
```
**Done when:** `cancelAppointment` does the PUT then returns `getAppointmentById(id)`.

### B-NOTE — do NOT touch `getAvailableSlots` 🟢
Its `json['startTime']` / `json['endTime']` / `json['isAvailable']` are correct (backend `SlotDto`). Leave it.

---

## PHASE C — Build & verify

### C1 — Rebuild flutter-web 🟢
Only the Flutter app changed (no backend changes in this file).
```powershell
cd D:\PR\Smart-Beauty-Salon-Claude-v1
docker compose build --no-cache flutter-web
docker compose up -d --force-recreate flutter-web
```
**Done when:** flutter-web is Up; report build errors. (If `flutter analyze` is available, run it on the two
edited files first; a missing `getAppointmentById` reference means a typo in B2/B3.)

### C2 — HAND OFF: human verifies 🔴
**Agent stops.** Post to the human and wait:
> Hard-reload (Ctrl+Shift+R).
> 1. Log in as SalonManager (09110000002 / Test@1234) → does the manager dashboard load (counts, not 401)?
>    In Network, `dashboard/manager` = 200?
> 2. Log in as Client (09110000004 / Test@1234) → client dashboard + favorites load?
> 3. Open the appointments list for an account that has one → does it render without a crash/blank?
> Report any remaining 401 / 500 / crash with the URL + status.
**Done when:** human confirms dashboards load and appointments render, or reports the failure.

### C3 — Commit (after C2) 🟡
```powershell
git add smart_salon_app/lib/core/api_service.dart smart_salon_app/lib/data/repositories/appointment_repository_impl.dart
git commit -m "fix: ApiService uses SharedPreferences token (dashboards 401); map booking fields + refetch on create/cancel"
git push
```

## Order
A1 → B1 → B2 → B3 → C1 → **C2 (hand off, stop)** → C3.

## Known follow-ups (not in this file — report after, fix later)
- `createAppointment` request body omits price fields (`estimatedPriceAmount`, `currency`,
  `depositAmountValue`), so new bookings get a 0 price. Wire these from the input when the booking UI sends them.
- `AppointmentEntity.status` is set from `json['status']`, but `BookingDto.Status` is a string
  ("Pending"/"Confirmed"/...). If the status chip/colors look wrong, map the string → your int codes.
- salon / service / artist / notification parsers weren't fully verified against their DTOs — watch the
  Network tab for 500s or blank lists on those screens.
