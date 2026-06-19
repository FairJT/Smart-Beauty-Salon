# Fix: Authentication broken (login crash + register) — Agent TODO

**For:** a free / local AI coding agent (Continue.dev + deepseek-coder) on Windows / PowerShell
**Bugs:**
  1. **PRIMARY — login/profile crash.** The backend returns the phone number in a field called
     `mobile`, but the Flutter code reads `phoneNumber`. That key is missing → value is `null`.
     `UserEntity.phoneNumber` is a **non-nullable** `String`, so `UserEntity(phoneNumber: null)` throws
     `type 'Null' is not a subtype of type 'String'` at runtime — inside `login()`, `register()`, AND
     `getProfile()`. So login fails even with correct credentials, and session-restore on reload fails too.
  2. **SECONDARY — register.** `register()` only sends `{ mobile, password }`, but the backend
     `RegisterDto` also requires `firstName`, `lastName`, `nationalCode` → 400 Bad Request. Also
     `AuthNotifier.register` receives `nationalCode` but never passes it to the repository.
**Generated:** 2026-06-18

## Hard limits
- **You cannot see the running app.** After rebuild, STOP and hand off (card C3, 🔴).
- Edit source files only. If a "before" block doesn't match, STOP and report.
- Backend field names in JSON are camelCase: `mobile`, `password`, `firstName`, `lastName`, `nationalCode`, `userType`.

Flags: 🟢 safe · 🟡 agent does it, human reviews · 🔴 human-only / hand-off.

---

## PHASE A — Fix the login/profile crash (phoneNumber → mobile)  ← the critical one

### A1 — Read the wrong keys to the correct `mobile` key 🟡
**File:** `smart_salon_app/lib/data/repositories/auth_repository_impl.dart`
**What's wrong:** three reads use `phoneNumber` but the JSON key is `mobile`:
- in `login()`   → `phoneNumber: data['user']['phoneNumber'],`   (this exact line appears in `register()` too)
- in `register()`→ `phoneNumber: data['user']['phoneNumber'],`
- in `getProfile()` → `phoneNumber: data['phoneNumber'],`

**Reliable way (run this in PowerShell — fixes all three at once):**
```powershell
cd D:\PR\Smart-Beauty-Salon-Claude-v1\smart_salon_app
$f = "lib\data\repositories\auth_repository_impl.dart"
(Get-Content $f -Raw) `
  -replace "data\['user'\]\['phoneNumber'\]", "data['user']['mobile']" `
  -replace "data\['phoneNumber'\]", "data['mobile']" `
  | Set-Content $f -NoNewline
```
**Verify:**
```powershell
Select-String -Path $f -Pattern "phoneNumber'\]"
```
**Done when:** the `Select-String` returns **nothing** (no more `['phoneNumber']` reads). The line
`phoneNumber: data['user']['mobile'],` now appears in `login()` and `register()`, and
`phoneNumber: data['mobile'],` appears in `getProfile()`.
(If you edit by hand instead: there are TWO identical `phoneNumber: data['user']['phoneNumber'],`
lines — change BOTH — and one `phoneNumber: data['phoneNumber'],` line.)

---

## PHASE B — Fix register (send all required fields + thread nationalCode)

### B1 — Add `nationalCode` to the repository interface 🟡
**File:** `smart_salon_app/lib/domain/repositories/auth_repository.dart`
**Find:**
```dart
  Future<UserEntity> register(String phoneNumber, String password, String firstName, String lastName);
```
**Replace with:**
```dart
  Future<UserEntity> register(String phoneNumber, String password, String firstName, String lastName, String nationalCode);
```
**Done when:** the interface `register` has a 5th parameter `String nationalCode`.

### B2 — Send the required fields in the register request 🟡
**File:** `smart_salon_app/lib/data/repositories/auth_repository_impl.dart`
**Find:**
```dart
  Future<UserEntity> register(String phoneNumber, String password,
      String firstName, String lastName) async {
    final response = await DioClient.instance.post(
      ApiConstants.register,
      data: {
        'mobile': phoneNumber,
        'password': password,
      },
    );
```
**Replace with:**
```dart
  Future<UserEntity> register(String phoneNumber, String password,
      String firstName, String lastName, String nationalCode) async {
    final response = await DioClient.instance.post(
      ApiConstants.register,
      data: {
        'mobile': phoneNumber,
        'password': password,
        'firstName': firstName,
        'lastName': lastName,
        'nationalCode': nationalCode,
      },
    );
```
**Done when:** `register()` takes `nationalCode` and the POST body includes `firstName`, `lastName`, `nationalCode`.

### B3 — Pass `nationalCode` from the notifier to the repository 🟡
**File:** `smart_salon_app/lib/presentation/providers/auth_provider.dart`
**Find:**
```dart
    final user = await _authRepository.register(
        mobile ?? '', password, firstName, lastName);
```
**Replace with:**
```dart
    final user = await _authRepository.register(
        mobile ?? '', password, firstName, lastName, nationalCode ?? '');
```
**Done when:** the call passes `nationalCode ?? ''` as the 5th argument.

---

## PHASE C — Build & verify

### C1 — Analyze 🟢
```powershell
cd D:\PR\Smart-Beauty-Salon-Claude-v1\smart_salon_app
flutter analyze lib\data\repositories\auth_repository_impl.dart lib\presentation\providers\auth_provider.dart lib\domain\repositories\auth_repository.dart
```
**Done when:** no errors. (If it complains about a missing argument to `register`, a call site wasn't updated — find it with `grep -rn ".register(" lib` and fix it.)

### C2 — Rebuild & restart 🟢
```powershell
cd ..
docker compose build --no-cache flutter-web
docker compose up -d --force-recreate flutter-web
```
**Done when:** `flutter-web` is Up; report build errors if any.

### C3 — HAND OFF: human tests 🔴
**Agent stops.** Post to the human and wait:
> Open http://localhost:5080, hard-reload (Ctrl+Shift+R):
> 1. Log in with a real account → does it now succeed and land on the right screen (not an error)?
> 2. Refresh the page after login → are you still logged in?
> 3. Try registering a new account (fill all fields) → does it succeed?
> If any fails, open DevTools → Network and copy the status + response of `POST /api/auth/login`
> (or `/register`).
**Done when:** the human reports results.

### C4 — Commit (after C3 confirms) 🟡
```powershell
cd D:\PR\Smart-Beauty-Salon-Claude-v1
git add smart_salon_app/lib
git diff --cached
git commit -m "fix(auth): read 'mobile' field from API (was crashing on null); send required fields on register"
git push
```
**Done when:** changes are pushed.

---

## Order
A1 → B1 → B2 → B3 → C1 → C2 → **C3 (hand off, stop)** → C4.

## Note
A1 alone fixes login (the crash). B1–B3 fix registration. Do A1 first — it's the one that makes
login work at all.
