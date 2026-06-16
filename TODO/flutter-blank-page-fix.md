# Fix: Flutter Web Blank Page — Agent TODO

**For:** a free / local AI coding agent
**Bug:** `index.html` mixes `flutter_bootstrap.js` with a hand-written `_flutter.loader.load({serviceWorkerSettings: null})` that never calls `runApp()` → entrypoint loads, app never starts, blank page with no console errors.
**Generated:** 2026-06-14

This whole fix is agent-doable (🟢/🟡 — no 🔴). The golden rule: **fix the SOURCE `web/index.html`, never the built file inside the container.** Editing the container artifact is what created this bug.

## Rules (every card)
1. **Verify before changing.** Look at the real file first.
2. **Edit `web/index.html` in the Flutter project — not `/usr/share/nginx/html/index.html` in the container.**
3. **One change, then reload and observe.** Don't batch fixes.
4. **Do not re-add a manual `_flutter.loader.load(...)` call.**
5. Report and stop if a step doesn't behave as described.

Flags: 🟢 safe · 🟡 agent drafts, human reviews.

---

### T1 — Confirm the diagnosis 🟢
**Steps:**
- Dump the served file: `docker exec <flutter-container> cat /usr/share/nginx/html/index.html`. Confirm it loads `flutter_bootstrap.js` **and** also calls `_flutter.loader.load(...)`.
- Open the page → DevTools → Elements. Check whether a `<flutter-view>` / `<flt-glass-pane>` element exists.
**Done when:** you've confirmed both the mixed loader in the HTML and that `<flutter-view>` is **absent** (engine never initialized). If `<flutter-view>` is present, stop and report — the cause is different.

### T2 — Find the SOURCE index.html 🟢
**Steps:** Locate `smart_salon_app/web/index.html` in the Flutter project. Confirm whether the container's blank-page HTML matches it or whether someone edited the built output directly.
**Done when:** you know which file is the real source of truth and report any drift between source and container.

### T3 — Fix the loader (the core fix) 🟡
**File:** `smart_salon_app/web/index.html`
**Steps:** Make the `<body>` use a single bootstrap script and **remove** the manual loader call and the service-worker-disable script:
```html
<body>
  <script src="flutter_bootstrap.js" async></script>
</body>
```
Do **not** keep both `flutter_bootstrap.js` and a `_flutter.loader.load(...)` call.
**Done when:** `web/index.html` has exactly one loader (the bootstrap script) and no hand-written `_flutter.loader.load(...)`.

### T4 — Confirm the base href 🟢
**File:** `smart_salon_app/web/index.html`
**Steps:** Ensure the head has `<base href="$FLUTTER_BASE_HREF">` (Flutter replaces it at build). Since the app is served at the root (`/`), the built result should be `<base href="/">`.
**Done when:** the base href token is present in source and resolves to `/` in the build.

### T5 — Nginx cache headers (so the SW can't serve a stale blank page) 🟡
**File:** `nginx.conf`
**Why:** the service worker was disabled by hand because a stale cache caused a blank page. Fix that with headers instead, so you don't need the loader hack.
**Steps:** Add `no-cache` for the shell/SW; leave hashed assets cacheable:
```nginx
location = /index.html               { add_header Cache-Control "no-cache"; }
location = /flutter_service_worker.js { add_header Cache-Control "no-cache"; }
location = /flutter_bootstrap.js      { add_header Cache-Control "no-cache"; }
```
**Done when:** `nginx.conf` revalidates the shell + SW + bootstrap; hashed assets (`main.dart.js`, `canvaskit/`, `assets/`) are untouched.

### T6 — Build Flutter inside the image 🟡
**File:** `Dockerfile.FlutterWeb`
**Steps:** Confirm the Dockerfile runs `flutter build web --release` inside the build and copies `build/web` to Nginx — not a pre-built or hand-edited directory. If it copies a pre-built dir, change it to build in-image.
**Done when:** a clean build produces the served `index.html` from source `web/index.html`, with no manual post-build editing.

### T7 — Rebuild, restart, verify 🟢
**Steps:**
- `docker build -f Dockerfile.FlutterWeb -t smartsalon-flutter .`
- Restart the container.
- Reload the page; confirm the UI renders and `<flutter-view>` now exists in the DOM.
- DevTools → Network: confirm `main.dart.js`, `flutter_bootstrap.js`, `flutter_service_worker.js`, and assets are all 200 (no 404 / MIME errors).
**Done when:** the app renders; `<flutter-view>` is present; no 404s.

---

## Order
T1 → T2 → **T3 (fix)** → T4 → T5 → T6 → **T7 (rebuild & verify)**.
T3 is the fix that makes the page render; T5/T6 stop it from regressing. If after T7 the page is still blank with `<flutter-view>` absent, stop and report the served `index.html` + the Network tab — the cause is then elsewhere (likely base href or a build error).
