# Fix: CSP "eval" error blocks the app (login) — Agent TODO

**For:** a free / local AI coding agent (Continue.dev + deepseek-coder)
**Bug:** the page shows `Content Security Policy ... blocks the use of 'eval'`. Flutter web renders
with CanvasKit, which compiles WebAssembly; under a CSP, wasm compilation counts as "eval" and needs
`'wasm-unsafe-eval'` in `script-src`. The current CSP lacks it, so the app is blocked at startup.
**Note:** this CSP is NOT in the committed repo — it exists only in the local working copy. You are
running on that local copy, so you can find and fix it. Edit source files only.
**Generated:** 2026-06-18

Flags: 🟢 safe · 🟡 agent does it, human reviews · 🔴 human-only (agent stops and hands off).

---

### T1 — Find where the CSP is set 🟢
**Steps:** run and report the output:
```bash
grep -rni "content-security-policy" . --include=*.html --include=*.conf | grep -v "/build/"
```
**Done when:** you report exactly one of:
- (A) it is a `<meta http-equiv="Content-Security-Policy" ...>` line in `smart_salon_app/web/index.html` → do **T2**
- (B) it is an `add_header Content-Security-Policy ...` line in `nginx.conf` → do **T3**
- (C) no match → do **T4**

### T2 — Fix the CSP meta tag (only if T1 = A) 🟡
**File:** `smart_salon_app/web/index.html`
**Action:** replace the entire existing `<meta http-equiv="Content-Security-Policy" ...>` line with exactly:
```html
  <meta http-equiv="Content-Security-Policy" content="default-src 'self'; script-src 'self' 'wasm-unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self'; object-src 'none'; base-uri 'self'">
```
**Done when:** the meta line's `script-src` contains `'wasm-unsafe-eval'`. (Skip T3/T4.)

### T3 — Fix the CSP in nginx (only if T1 = B) 🟡
**File:** `nginx.conf`
**Step 3a:** replace the existing `add_header Content-Security-Policy ...` line (inside the `server` block) with exactly:
```nginx
    add_header Content-Security-Policy "default-src 'self'; script-src 'self' 'wasm-unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self'; object-src 'none'; base-uri 'self'; frame-ancestors 'none'" always;
```
**Step 3b (important — nginx does not inherit add_header):** the three `location =` blocks that already
have `add_header Cache-Control` will NOT receive the server CSP. Add the same CSP line to each of them.
Change these three lines:
```nginx
    location = /index.html               { add_header Cache-Control 'no-cache'; }
    location = /flutter_service_worker.js { add_header Cache-Control 'no-cache'; }
    location = /flutter_bootstrap.js      { add_header Cache-Control 'no-cache'; }
```
into:
```nginx
    location = /index.html               { add_header Cache-Control 'no-cache'; add_header Content-Security-Policy "default-src 'self'; script-src 'self' 'wasm-unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self'; object-src 'none'; base-uri 'self'; frame-ancestors 'none'" always; }
    location = /flutter_service_worker.js { add_header Cache-Control 'no-cache'; add_header Content-Security-Policy "default-src 'self'; script-src 'self' 'wasm-unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self'; object-src 'none'; base-uri 'self'; frame-ancestors 'none'" always; }
    location = /flutter_bootstrap.js      { add_header Cache-Control 'no-cache'; add_header Content-Security-Policy "default-src 'self'; script-src 'self' 'wasm-unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self'; object-src 'none'; base-uri 'self'; frame-ancestors 'none'" always; }
```
**Done when:** every CSP line in `nginx.conf` has `'wasm-unsafe-eval'` in `script-src`. (Skip T2/T4.)

### T4 — No CSP found in repo (only if T1 = C) 🔴
**The agent stops.** Report to the human:
> No CSP is set in the project files. The `eval` error is coming from outside the app — most likely a
> browser extension or a proxy. Try the page in an incognito window with extensions disabled.

### T5 — Guard HttpsRedirection for Docker 🟡
**File:** `src/SalonOS.Api/Program.cs`
**Find (before):**
```csharp
app.UseHttpsRedirection();
```
**Replace with (after):**
```csharp
if (!app.Environment.IsDevelopment() && app.Environment.EnvironmentName != "Docker")
{
    app.UseHttpsRedirection();
}
```
**Why:** nginx proxies to the API over plain HTTP; unconditional HTTPS redirection can turn the login
POST into a 307 redirect and break it.
**Done when:** `app.UseHttpsRedirection();` is wrapped in the environment check.

### T6 — Rebuild and restart 🟢
**Steps:**
```bash
docker compose build --no-cache flutter-web salonos-api
docker compose up -d
docker compose ps
```
**Done when:** both containers are Up. Report the last lines if the build errors.

### T7 — HAND OFF: human verifies 🔴
**The agent stops here.** Post to the human and wait:
> Open http://localhost:5080, hard-reload (Ctrl+Shift+R), and try to log in. Tell me:
> (1) does the app load now (no more eval error)?
> (2) on login, what does the Network tab show for `POST /api/auth/login` (status code + response)?
**Done when:** the human reports the result.

### T8 — Commit and push (only after T7 confirms it works) 🟡
**Steps:**
```bash
git add -A
git diff --cached     # show staged diff for human review — STOP for approval
git commit -m "fix(web): allow wasm-unsafe-eval in CSP; guard HttpsRedirection for Docker"
git push
```
**Done when:** changes are pushed; report the commit hash.

---

## Order
T1 → (T2 **or** T3 **or** T4) → T5 → T6 → **T7 (hand off, stop)** → T8.

## Notes
- `'wasm-unsafe-eval'` only permits WebAssembly compilation, not general JS `eval` — safer than `'unsafe-eval'`.
- `frame-ancestors` is ignored inside a `<meta>` CSP (header-only), which is why the T2 meta version omits it.
