# Fix: Persian font invisible + auth issues — Agent TODO

**For:** a free / local AI coding agent (Continue.dev + deepseek-coder) on Windows / PowerShell
**Bugs:**
  1. **No Persian text renders.** The theme sets `fontFamily: 'Vazirmatn'` (main.dart + app_colors.dart)
     but pubspec has NO `fonts:` section and no font file ships. On Flutter web, CanvasKit does not use
     browser fonts — a Persian-capable font MUST be bundled, or all Farsi glyphs render blank.
  2. **Auth weak spots.** Role mapping itself now works (`_parseUserType` maps the string userType to the
     int the getters expect). Remaining real issues: `isClient` compares an int to a String (always false);
     there are two duplicate `authProvider`s (one is dead code); and token persistence on web via
     `flutter_secure_storage` is fragile and may log the user out on reload.
**Generated:** 2026-06-18

## Hard limits for this agent
- **You cannot see the running app or a browser.** After the rebuild, STOP and hand off (cards 🔴).
- **A font file is binary — never fabricate it.** If the download in A1 fails, STOP and ask the human.
- **YAML indent is exact** (2 spaces). Do A2 exactly as written.
- Edit source files only. If a "before" block doesn't match, STOP and report.

Flags: 🟢 safe · 🟡 agent does it, human reviews · 🔴 human-only / hand-off.

---

## PHASE A — Bundle the Persian font (fixes "no Farsi string")

### A1 — Download Vazirmatn into assets/fonts 🟡
**Run (PowerShell, from `smart_salon_app/`):**
```powershell
New-Item -ItemType Directory -Force -Path assets\fonts
Invoke-WebRequest -Uri "https://github.com/rastikerdar/vazirmatn/raw/master/fonts/ttf/Vazirmatn-Regular.ttf" -OutFile "assets\fonts\Vazirmatn-Regular.ttf"
Invoke-WebRequest -Uri "https://github.com/rastikerdar/vazirmatn/raw/master/fonts/ttf/Vazirmatn-Bold.ttf"    -OutFile "assets\fonts\Vazirmatn-Bold.ttf"
Get-Item assets\fonts\*.ttf | Select-Object Name, Length
```
**Done when:** both `.ttf` files exist and each `Length` is greater than ~50000 bytes.
**If a download fails or a file is tiny/HTML (404):** STOP. Ask the human to manually download Vazirmatn
from https://fonts.google.com/specimen/Vazirmatn and place `Vazirmatn-Regular.ttf` and
`Vazirmatn-Bold.ttf` into `smart_salon_app/assets/fonts/`. Do NOT continue with a fake/empty file.

### A2 — Declare the font in pubspec 🟡
**File:** `smart_salon_app/pubspec.yaml`
**Find (before):**
```yaml
flutter:
  generate: true
  uses-material-design: true
```
**Replace with (after):**
```yaml
flutter:
  generate: true
  uses-material-design: true
  fonts:
    - family: Vazirmatn
      fonts:
        - asset: assets/fonts/Vazirmatn-Regular.ttf
        - asset: assets/fonts/Vazirmatn-Bold.ttf
          weight: 700
```
**Done when:** the `flutter:` block contains the `fonts:` entry with family `Vazirmatn`. Indent is 2 spaces
per level — do not use tabs.

### A3 — Sync packages 🟢
**Run:** `flutter pub get`
**Done when:** it completes with no error. (Assets declared in pubspec are bundled automatically by
`flutter build web`; no Dockerfile change is needed.)

---

## PHASE B — Auth fixes

### B1 — Fix the `isClient` getter (int vs String bug) 🟡
**File:** `smart_salon_app/lib/presentation/providers/auth_provider.dart`
**Find (before):**
```dart
  bool get isClient => user?.userType == 'Client';
```
**Replace with (after):**
```dart
  bool get isClient => user?.userType == 4;
```
**Why:** `UserEntity.userType` is an `int` (1=SuperAdmin, 2=SalonManager, 3=Artist, 4=Client). Comparing
it to the String `'Client'` is always false. The other getters in this file already use the int form.
**Done when:** `isClient` compares to `4`.

### B2 — Remove the dead duplicate auth provider (guarded) 🟡
**Goal:** the screens import `presentation/providers/auth_provider.dart`. The file
`lib/providers/auth_provider.dart` (JWT/permissions based) is a separate, likely-unused `authProvider`.
**Steps:**
1. Check whether anything imports it:
   ```bash
   grep -rn "providers/auth_provider.dart" lib | grep -v "presentation/providers/auth_provider.dart"
   ```
2. If the command prints NOTHING (no importers), delete the file `lib/providers/auth_provider.dart`
   and also `lib/core/jwt_decoder.dart`, `lib/core/permissions.dart`, `lib/models/user_profile.dart`
   ONLY if `grep -rn "<filename>" lib` shows they are imported by nothing else.
3. If ANY file imports them, do NOT delete — just report which files do.
**Done when:** either the unused file(s) are deleted, or you report that they are still imported and left in place.
**This is cleanup only — if unsure, skip and report.**

---

## PHASE C — Build & verify

### C1 — Rebuild and restart 🟢
```bash
docker compose build --no-cache flutter-web
docker compose up -d
docker compose ps
```
**Done when:** `flutter-web` is Up; report build errors if any.

### C2 — HAND OFF: human verifies font + roles 🔴
**Agent stops.** Post to the human and wait:
> Open http://localhost:5080 and hard-reload (Ctrl+Shift+R). Check:
> 1. Is the Persian text now visible (login labels like «شماره موبایل»)?
> 2. Log in as a **SalonManager** (and an **Artist** if possible). Do you land on the Manager / Artist
>    dashboard — not the client home?
> 3. After logging in, **refresh the page**. Are you still logged in, or kicked back to login?
**Done when:** the human reports answers to 1, 2, and 3.

---

## PHASE D — Conditional: fix web token persistence (only if C2.3 = "kicked back to login")

### D1 — Switch token storage from secure_storage to shared_preferences 🟡 (human reviews)
`shared_preferences` is already in pubspec and persists reliably on web (localStorage); secure_storage on
web is the likely culprit if refresh logs the user out.
**File 1:** `lib/data/datasources/dio_client.dart` — replace the `FlutterSecureStorage` token read with
shared_preferences:
- change the import `package:flutter_secure_storage/flutter_secure_storage.dart`
  → `package:shared_preferences/shared_preferences.dart`
- replace `static const _storage = FlutterSecureStorage();` and the
  `_storage.read(key: _tokenKey)` / `_storage.delete(key: _tokenKey)` calls with:
  `final prefs = await SharedPreferences.getInstance();` then `prefs.getString(_tokenKey)` /
  `prefs.remove(_tokenKey)`.
**File 2:** `lib/data/repositories/auth_repository_impl.dart` — same swap for `_saveToken` /
read / clear: use `SharedPreferences.getInstance()` and `setString` / `getString` / `remove`.
**Constraint:** keep the key name `'auth_token'` identical in both files so they stay in sync.
**Done when:** neither file references `FlutterSecureStorage` for the token, both use `shared_preferences`,
and `flutter analyze` is clean. Then rebuild (C1) and ask the human to re-test refresh.

---

## PHASE E — Commit (after C2, and D if it was needed)

### E1 — Commit and push 🟡 (human reviews diff)
```bash
git add -A
git diff --cached     # show staged diff for human review — STOP for approval
git commit -m "fix: bundle Vazirmatn font (Persian rendering); fix isClient role check; auth cleanup"
git push
```
**Done when:** changes are pushed; report the commit hash.

---

## Order
A1 → A2 → A3 → B1 → B2 → C1 → **C2 (hand off, stop)** → (D1 only if needed) → E1.

## Notes
- The screens hard-code Persian strings and barely use `AppLocalizations` (the `app_fa.arb` data is
  unused). That is NOT why text is invisible — the font is — but converting screens to `AppLocalizations`
  later is worth a separate task.
