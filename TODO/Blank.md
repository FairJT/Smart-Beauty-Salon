# Fix: Flutter Web Blank Page (v2) — Agent TODO

**For:** a free / local AI coding agent (Continue.dev + deepseek-coder)
**Bug:** `localhost:8080` shows a blank white page. Two root causes:
  1. `smart_salon_app/web/index.html` loads BOTH `flutter_bootstrap.js` AND a manual
     `<script src="main.dart.js">` → `main.dart.js` loads twice, the Dart entrypoint
     double-initializes, the engine never starts → blank page, usually with no console error.
  2. `Dockerfile.FlutterWeb` builds WITHOUT `--no-web-resources-cdn`, so CanvasKit is fetched
     from `gstatic.com` at runtime. That host can be blocked (esp. for the Iran market) → blank page.
     It also uses the deprecated `--web-renderer=html` flag and does fragile `sed` surgery on the
     bootstrap file.
**Generated:** 2026-06-18 (supersedes flutter-blank-page-fix.md)

This whole fix is agent-doable (🟢/🟡 — no 🔴).
**Golden rule: edit the SOURCE files in the repo, never the built file inside the container.**

## Rules (every card)
1. **Verify before changing.** Open and read the real file first; confirm the "before" block matches.
2. **Edit source files only** — `smart_salon_app/web/index.html` and `Dockerfile.FlutterWeb`.
   Never edit `/usr/share/nginx/html/...` inside the container, and never edit `build-output/`.
3. **One change, then stop and report** what you changed. Do not batch multiple cards.
4. **Do not re-add** a manual `<script src="main.dart.js">` or a hand-written `_flutter.loader.load(...)`.
5. If a file's real content does not match the "before" block in a card, **stop and report** — do not guess.

Flags: 🟢 safe · 🟡 agent drafts, human reviews.

---

### T1 — Confirm the diagnosis 🟢
**File:** `smart_salon_app/web/index.html` (read only)
**Steps:**
- Open the file. Confirm the `<head>` contains BOTH of these lines:
  `<script src="flutter_bootstrap.js" async></script>` and `<script src="main.dart.js"></script>`.
- Confirm the `<body>` is empty (`<body>\n</body>`).
**Done when:** you have confirmed both script lines are present in `<head>` and `<body>` is empty.
If the file does not look like this, stop and report its actual `<head>`/`<body>`.

---

### T2 — Fix index.html: single bootstrap loader (THE CORE FIX) 🟡
**File:** `smart_salon_app/web/index.html`
**Find this block (the broken "before"):**
```html
  <link rel="manifest" href="manifest.json">
  <script src="flutter_bootstrap.js" async></script>
  <script src="main.dart.js"></script>
</head>
<body>
</body>
```
**Replace it with (the fixed "after"):**
```html
  <link rel="manifest" href="manifest.json">
</head>
<body>
  <script src="flutter_bootstrap.js" async></script>
</body>
```
**Constraints:** exactly ONE loader line, and it lives in `<body>`. No `main.dart.js` line. No `flutter.js` line. No `_flutter.loader.load(...)` line.
**Done when:** the file's only Flutter loader reference is `<script src="flutter_bootstrap.js" async></script>` inside `<body>`. Report the new `<head>`+`<body>`.

---

### T3 — Confirm the base href 🟢
**File:** `smart_salon_app/web/index.html` (read only)
**Steps:** Confirm `<head>` still contains `<base href="/">`. The app is served at root, so this must stay `/`.
**Done when:** `<base href="/">` is present. If it is missing or different, stop and report.

---

### T4 — Fix the Dockerfile build command 🟡
**File:** `Dockerfile.FlutterWeb`
**Find this block (the broken "before"):**
```dockerfile
RUN --mount=type=cache,target=/root/.pub-cache \
    flutter build web --release \
      --dart-define=API_BASE_URL=${API_BASE_URL} \
      --web-renderer=html
```
**Replace it with (the fixed "after"):**
```dockerfile
RUN --mount=type=cache,target=/root/.pub-cache \
    flutter build web --release \
      --no-web-resources-cdn \
      --dart-define=API_BASE_URL=${API_BASE_URL}
```
**Why:** `--no-web-resources-cdn` bundles CanvasKit into the image (no gstatic fetch at runtime).
`--web-renderer=html` is deprecated in Flutter 3.27.1 and is removed — drop it; default CanvasKit is fine.
**Done when:** the build command has `--no-web-resources-cdn` and no longer has `--web-renderer=html`. Report the new command.

---

### T5 — Remove the fragile bootstrap surgery from the Dockerfile 🟡
**File:** `Dockerfile.FlutterWeb`
**Find this block (the broken "before"):**
```dockerfile
COPY --from=build /app/build/web /usr/share/nginx/html
       
RUN sed -i '/serviceWorkerSettings:/d' /usr/share/nginx/html/flutter_bootstrap.js && \
    sed -i 's/_flutter.loader.load({[^}]*});/_flutter.loader.load({});/' /usr/share/nginx/html/flutter_bootstrap.js
RUN rm -f /usr/share/nginx/html/flutter_service_worker.js || true
COPY nginx.conf /etc/nginx/conf.d/default.conf
```
**Replace it with (the fixed "after"):**
```dockerfile
COPY --from=build /app/build/web /usr/share/nginx/html

COPY nginx.conf /etc/nginx/conf.d/default.conf
```
**Why:** the `sed` regex `_flutter.loader.load({[^}]*});` stops at the first `}` and can mangle the loader. It is unnecessary — staleness is already handled by the `no-cache` headers in `nginx.conf` (see T6).
**Done when:** the two `RUN sed ...` lines and the `RUN rm -f ... flutter_service_worker.js` line are gone; the `COPY build/web` and `COPY nginx.conf` lines remain. Report the surrounding lines.

---

### T6 — Verify nginx cache headers (no change expected) 🟢
**File:** `nginx.conf` (read only)
**Steps:** Confirm these three lines exist (they replace the old service-worker hack):
```nginx
location = /index.html               { add_header Cache-Control 'no-cache'; }
location = /flutter_service_worker.js { add_header Cache-Control 'no-cache'; }
location = /flutter_bootstrap.js      { add_header Cache-Control 'no-cache'; }
```
Also confirm the `location /api/ { proxy_pass http://salonos-api:5016/api/; ... }` block is present.
**Done when:** both confirmed. If a line is missing, report it but do NOT add anything yet — flag for human review.

---

### T7 — Rebuild, restart, verify 🟢
**Steps:**
- `docker compose build --no-cache flutter-web`
- `docker compose up -d`
- Open `http://localhost:8080`, hard-reload (Ctrl+Shift+R).
- DevTools → Elements: confirm a `<flutter-view>` element now exists in the DOM (it was absent before).
- DevTools → Network: confirm `flutter_bootstrap.js`, `main.dart.js`, and `canvaskit/*` are all **200**, and there is **no request to gstatic.com** and **no 404**.
**Done when:** the UI renders, `<flutter-view>` is present, and no 404 / no gstatic request appears.

---

## Order
T1 → **T2 (core fix)** → T3 → T4 → T5 → T6 → **T7 (rebuild & verify)**.

## If still blank after T7
Stop and report:
1. The served HTML: `docker exec FlutterWeb cat /usr/share/nginx/html/index.html`
2. The browser Console output (any red errors).
3. The Network tab (any non-200 / any gstatic request).
The cause is then elsewhere (likely a build error in the logs, or base href), not the loader.
