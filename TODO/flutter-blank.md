# Fix: Flutter Web Blank Page (v4 — ROOT CAUSE) — Agent TODO

**For:** a free / local AI coding agent (Continue.dev + deepseek-coder)
**Root cause (confirmed, deterministic):**
  In `lib/main.dart`, `ErrorBoundary` is the parent of `MaterialApp`. But `ErrorBoundary.build()`
  calls `AppLocalizations.of(context)!` on its first line, unconditionally. `Localizations` is
  provided by `MaterialApp` — which is a CHILD of `ErrorBoundary` — so at `ErrorBoundary`'s level
  the lookup returns `null`, and `!` throws `Null check operator used on a null value` on EVERY
  build, at the root, before anything paints. The custom `ErrorWidget.builder` can't recover
  because it returns a `Material` with no `Directionality` ancestor (which also throws). Net result:
  a fully white page, no splash. This is why fixing the loader / CanvasKit earlier did nothing —
  the bug was never in the loader.
**Fix:** move `ErrorBoundary` INSIDE `MaterialApp` (into its `builder`, where `Localizations` and
  `Directionality` exist), and wrap the root `ErrorWidget.builder` in `Directionality`.
**Generated:** 2026-06-18 (follows flutter-blank-page-fix-v3.md)

## Hard limits for this agent
- **You cannot see a web page or DevTools.** Never claim the page renders. After the rebuild, STOP
  and hand off to the human (card C2, flagged 🔴).
- **Edit source files only.** Never edit files inside the running container or under `build-output/`.
- **Dart is paren-sensitive.** Do cards A2, A3, A4 exactly, then run the balance check (A5) before
  building. If the check fails, STOP — do not try to build.
- If a card's "before" block does not match the real file, STOP and report — do not guess.

Flags: 🟢 safe · 🟡 agent does it, human reviews · 🔴 human-only (agent stops and hands off).

---

## PHASE A — Fix main.dart

### A1 — Read and confirm 🟢
**File:** `lib/main.dart` (read only)
**Steps:** Confirm both of these are true in the current file:
- the `build` method starts with `return ErrorBoundary(` then `child: MaterialApp(`
- `ErrorWidget.builder` returns `Material(` directly (NOT wrapped in `Directionality`)
**Done when:** both confirmed and reported. If either is already fixed, note which, and skip its card below.

### A2 — Wrap the root ErrorWidget in Directionality 🟡
**File:** `lib/main.dart`
**Find (before):**
```dart
  ErrorWidget.builder = (details) {
    return Material(
      child: Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.error_outline, size: 64, color: Colors.red),
              const SizedBox(height: 16),
              Text(
                'خطایی رخ داده است',
                style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 8),
              Text(
                details.exception.toString(),
                style: const TextStyle(fontSize: 14, color: Colors.grey),
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
      ),
    );
  };
```
**Replace with (after):**
```dart
  ErrorWidget.builder = (details) {
    return Directionality(
      textDirection: TextDirection.rtl,
      child: Material(
        child: Center(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(Icons.error_outline, size: 64, color: Colors.red),
                const SizedBox(height: 16),
                Text(
                  'خطایی رخ داده است',
                  style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 8),
                Text(
                  details.exception.toString(),
                  style: const TextStyle(fontSize: 14, color: Colors.grey),
                  textAlign: TextAlign.center,
                ),
              ],
            ),
          ),
        ),
      ),
    );
  };
```
**Done when:** the `ErrorWidget.builder` returns a `Directionality` that wraps the `Material`.

### A3 — Make MaterialApp the root + put ErrorBoundary in the builder 🟡
**File:** `lib/main.dart`
**Find (before):**
```dart
    return ErrorBoundary(
      child: MaterialApp(
        title: 'سالن هوشمند',
        debugShowCheckedModeBanner: false,
        localizationsDelegates: AppLocalizations.localizationsDelegates,
        supportedLocales: AppLocalizations.supportedLocales,
        locale: const Locale('fa'),
        builder: (context, child) => Directionality(
          textDirection: TextDirection.rtl,
          child: child!,
        ),
```
**Replace with (after):**
```dart
    return MaterialApp(
        title: 'سالن هوشمند',
        debugShowCheckedModeBanner: false,
        localizationsDelegates: AppLocalizations.localizationsDelegates,
        supportedLocales: AppLocalizations.supportedLocales,
        locale: const Locale('fa'),
        builder: (context, child) => Directionality(
          textDirection: TextDirection.rtl,
          child: ErrorBoundary(child: child!),
        ),
```
**Note:** this removes the `ErrorBoundary(` + `child: MaterialApp(` wrapper (MaterialApp becomes the
return value) and moves `ErrorBoundary` into the `builder`. A4 removes the now-extra closing paren.
**Done when:** `build` returns `MaterialApp(` directly and the builder's child is `ErrorBoundary(child: child!)`.

### A4 — Remove the now-extra closing paren 🟡
**File:** `lib/main.dart` (near the end of the `build` method)
**Find (before):**
```dart
        home: const SplashScreen(),
      ),
    );
  }
```
**Replace with (after):**
```dart
        home: const SplashScreen(),
    );
  }
```
**Why:** A3 dropped the `ErrorBoundary(` wrapper, so one closing `),` (the one that used to close
`MaterialApp` inside `ErrorBoundary`) is now extra. After this, `MaterialApp(` is closed by `    );`.
**Done when:** there is exactly one `);` closing the `return MaterialApp(...)`.

### A5 — Paren / brace balance check (catches a half-done A3/A4) 🟢
**Steps:** Run:
```bash
python3 - <<'PY'
s=open('lib/main.dart').read()
ok=True
for o,c in [('(',')'),('{','}'),('[',']')]:
    a,b=s.count(o),s.count(c)
    print(f"{o}{c}: open={a} close={b} {'OK' if a==b else 'MISMATCH'}")
    ok = ok and a==b
print('RESULT:', 'OK' if ok else 'FIX A3/A4')
PY
```
If `flutter` is on PATH, also run `flutter analyze lib/main.dart` and report any errors.
**Done when:** all three pairs are OK (and `flutter analyze` shows no errors if available).
If MISMATCH, re-check A3 and A4 — do NOT continue to Phase B.

---

## PHASE B — Make sure the loader is clean (remove any temp debug overlay)

### B1 — Restore the clean index.html 🟡
**File:** `smart_salon_app/web/index.html`
**Why:** if the v3 debug overlay (`__showErr` script) is still in the file, remove it now so it
isn't shipped. Overwrite the ENTIRE file with exactly this:
```html
<!DOCTYPE html>
<html>
<head>
  <base href="/">
  <meta charset="UTF-8">
  <meta content="IE=Edge" http-equiv="X-UA-Compatible">
  <meta name="description" content="Smart Salon App">
  <meta name="mobile-web-app-capable" content="yes">
  <meta name="apple-mobile-web-app-status-bar-style" content="black">
  <meta name="apple-mobile-web-app-title" content="smart_salon_app">
  <link rel="apple-touch-icon" href="icons/Icon-192.png">
  <link rel="icon" type="image/png" href="favicon.png"/>
  <title>smart_salon_app</title>
  <link rel="manifest" href="manifest.json">
</head>
<body>
  <script src="flutter_bootstrap.js" async></script>
</body>
</html>
```
**Done when:** the file has only the single `flutter_bootstrap.js` loader and no `__showErr` script.

---

## PHASE C — Build, verify, commit

### C1 — Rebuild and restart 🟢
**Steps:**
```bash
docker compose build --no-cache flutter-web
docker compose up -d
docker compose ps
```
Capture build errors if any: `docker compose build flutter-web 2>&1 | tail -40`
**Done when:** `flutter-web` is Up and the build log shows no Dart compile error. Report the last log lines.

### C2 — HAND OFF: human checks the page 🔴
**The agent stops here.** Post to the human and wait:
> Open http://localhost:5080 and hard-reload (Ctrl+Shift+R). Expected: a purple splash with a
> spinner, then the login screen after ~2 seconds. Tell me what you see — splash+login (fixed),
> a red error message (paste it), or still white (then paste the DevTools Console output).
**Done when:** the human confirms the splash + login appear, or reports the error text.

### C3 — Commit and push (only after C2 confirms it works) 🟡
**Steps:**
```bash
git add smart_salon_app/web/index.html lib/main.dart
git diff --cached      # show staged diff for human review — STOP for approval
git commit -m "fix(web): move ErrorBoundary inside MaterialApp; guard root ErrorWidget (fixes blank page)"
git push
```
Also confirm the earlier v2/v3 fixes are committed (run `git status`; if `Dockerfile.FlutterWeb`
or `docker-compose.yml` are still modified, add and commit them too).
**Done when:** `lib/main.dart` and `index.html` (plus any leftover Dockerfile/compose fixes) are pushed.

---

## Order
A1 → A2 → A3 → A4 → **A5 (balance check)** → B1 → C1 → **C2 (hand off, stop)** → C3.

## Root cause note (for next time)
A widget that calls `*.of(context)` for an inherited widget (Localizations, Theme, MediaQuery,
Navigator, etc.) must sit BELOW the widget that provides it. `MaterialApp` provides all of these,
so app-wide wrappers like `ErrorBoundary` belong in `MaterialApp.builder`, never above `MaterialApp`.
