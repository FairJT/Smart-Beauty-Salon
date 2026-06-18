# Fix: Flutter Web Blank Page (v3) — Commit prior fixes + diagnose runtime blank — Agent TODO

**For:** a free / local AI coding agent (Continue.dev + deepseek-coder)
**Situation:**
  - The v2 fixes (single bootstrap loader, `--no-web-resources-cdn`, removed sed surgery, port 5080)
    were applied and verified LOCALLY but were **never committed/pushed** — the GitHub repo still has
    the broken versions. First we get those into git, then we diagnose the remaining blank page.
  - Even the corrected local build is still blank, so there is a **runtime error after the engine starts**.
    We add a temporary on-page error overlay so the silent error becomes visible.
**Generated:** 2026-06-18 (follows flutter-blank-page-fix-v2.md)

This is agent-doable for the file/git/docker steps (🟢/🟡). 🔴 = the agent CANNOT do it; a human must.

## Hard limits for this agent
- **You cannot see a web page or DevTools.** Never claim the page renders, shows an error, or that the
  console is clean. After a rebuild, STOP and ask the human to report what they see. (Flagged 🔴 below.)
- **Edit source files only.** Never edit files inside the running container or under `build-output/`.
- **One card at a time.** Apply it, report exactly what changed, then move on.
- If a file's real content does not match the "before" in a card, STOP and report — do not guess.

Flags: 🟢 safe · 🟡 agent does it, human reviews · 🔴 human-only (agent must stop and hand off).

---

## PHASE A — Get the prior fixes into git

### A1 — Inspect git state 🟢
**Steps:** Run and report the full output of:
```bash
git status
git log --oneline -5
```
**Done when:** you have reported whether `smart_salon_app/web/index.html`, `Dockerfile.FlutterWeb`, and
`docker-compose.yml` show up as modified/untracked, and whether the last commits mention the blank-page fix.
If `git status` is clean (nothing modified), the fixes are not on disk — they must be re-applied in A2–A4.

### A2 — Ensure index.html is the clean single-loader version 🟡
**File:** `smart_salon_app/web/index.html`
**Action:** Overwrite the ENTIRE file with exactly this (clean, no debug overlay yet):
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
**Done when:** the file has exactly one loader line (`flutter_bootstrap.js` in `<body>`) and no `main.dart.js` / `flutter.js` / `_flutter.loader.load(...)` lines.

### A3 — Ensure the Dockerfile build command is fixed 🟡
**File:** `Dockerfile.FlutterWeb`
**Find (broken "before"):**
```dockerfile
    flutter build web --release \
      --dart-define=API_BASE_URL=${API_BASE_URL} \
      --web-renderer=html
```
**Replace with (fixed "after"):**
```dockerfile
    flutter build web --release \
      --no-web-resources-cdn \
      --dart-define=API_BASE_URL=${API_BASE_URL}
```
**Done when:** build command has `--no-web-resources-cdn` and no `--web-renderer=html`. If it is already like the "after", report "already fixed" and move on.

### A4 — Ensure the Dockerfile sed surgery is removed 🟡
**File:** `Dockerfile.FlutterWeb`
**Find (broken "before"):**
```dockerfile
COPY --from=build /app/build/web /usr/share/nginx/html
       
RUN sed -i '/serviceWorkerSettings:/d' /usr/share/nginx/html/flutter_bootstrap.js && \
    sed -i 's/_flutter.loader.load({[^}]*});/_flutter.loader.load({});/' /usr/share/nginx/html/flutter_bootstrap.js
RUN rm -f /usr/share/nginx/html/flutter_service_worker.js || true
COPY nginx.conf /etc/nginx/conf.d/default.conf
```
**Replace with (fixed "after"):**
```dockerfile
COPY --from=build /app/build/web /usr/share/nginx/html

COPY nginx.conf /etc/nginx/conf.d/default.conf
```
**Done when:** the `RUN sed ...` and `RUN rm -f ... flutter_service_worker.js` lines are gone. If already gone, report "already fixed".

### A5 — Ensure the compose port is 5080 🟡
**File:** `docker-compose.yml`
**Find (broken "before"):** the `flutter-web` service `ports:` entry `- "8080:80"`
**Replace with (fixed "after"):** `- "5080:80"`
**Note:** only change the entry under the `flutter-web` service. Do not touch `1433`, `5015`, or `5016`.
**Done when:** the `flutter-web` service maps `5080:80`. If already `5080:80`, report "already fixed".

### A6 — Commit and push 🟡 (human reviews the diff before push)
**Steps:**
```bash
git add smart_salon_app/web/index.html Dockerfile.FlutterWeb docker-compose.yml
git diff --cached            # show the staged diff for human review — STOP here for approval
git commit -m "fix(web): single bootstrap loader, local canvaskit, drop sed surgery, port 5080"
git push
```
**Done when:** the three files are committed and pushed; report the new commit hash.

---

## PHASE B — Diagnose the remaining blank page (temporary, not committed)

### B1 — Add a temporary on-page error overlay 🟡
**File:** `smart_salon_app/web/index.html`
**Why:** the clean index.html shows nothing if a JS/engine error occurs. This overlay paints the error
onto the page so we can read it without DevTools. **This is temporary — do NOT commit it.**
**Action:** Overwrite the ENTIRE file with exactly this:
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

  <!-- TEMP DEBUG: paints any silent JS error onto the page. REMOVE once the blank page is fixed. -->
  <script>
    function __showErr(label, text) {
      document.body.innerHTML =
        '<pre style="color:#c00;white-space:pre-wrap;padding:16px;font:13px/1.5 monospace">'
        + label + ':\n' + text + '</pre>';
    }
    window.addEventListener('error', function (e) {
      __showErr('JS ERROR', (e.message || e.error)
        + (e.filename ? ('\n@ ' + e.filename + ':' + e.lineno + ':' + e.colno) : '')
        + (e.error && e.error.stack ? ('\n\n' + e.error.stack) : ''));
    });
    window.addEventListener('unhandledrejection', function (e) {
      var r = e.reason;
      __showErr('PROMISE REJECTION', (r && r.stack) ? r.stack : String(r));
    });
  </script>
</head>
<body>
  <script src="flutter_bootstrap.js" async></script>
</body>
</html>
```
**Done when:** the file contains the `__showErr` script and the single bootstrap loader. Do NOT `git add` this change.

### B2 — Rebuild and restart 🟢
**Steps:**
```bash
docker compose build --no-cache flutter-web
docker compose up -d
docker compose ps          # confirm FlutterWeb is Up
```
Also capture build logs in case the build itself errors:
```bash
docker compose build flutter-web 2>&1 | tail -40
```
**Done when:** the `flutter-web` container is Up and you have reported the last lines of the build log
(any Flutter build error or warning matters).

### B3 — HAND OFF: human checks the page 🔴
**The agent stops here.** Post this message to the human and wait:
> Open http://localhost:5080 and hard-reload (Ctrl+Shift+R). Tell me which one happens:
> (1) red error text on the page → copy it here;
> (2) a purple splash screen with a spinner → the engine is fine, the bug is in auth/navigation;
> (3) still fully white → open DevTools, and from the **Console** copy every red line, and from the
>     **Network** tab report the status code AND Content-Type of `canvaskit.wasm` and `canvaskit.js`.
**Done when:** the human has reported outcome (1), (2), or (3) with the requested details.
Do not proceed or guess a fix until then.

---

## After the root cause is known
- If the cause is fixed, **remove the B1 debug overlay** (restore the clean A2 version of index.html),
  rebuild, confirm, then commit the clean version.
- Record the actual root cause at the top of this file for next time.

## Order
A1 → A2 → A3 → A4 → A5 → A6 → B1 → B2 → **B3 (hand off, stop)**.
