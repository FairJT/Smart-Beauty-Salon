# Task C3 — Document the production API base URL 🟢

The default base URL is `http://localhost:5016`, which bypasses nginx. In the docker/nginx
deployment the web build must use a same-origin / nginx URL, or every request fails (or hits CORS).
This task only adds a clear note so it isn't forgotten — no behavior change.

**File:** `smart_salon_app/lib/data/datasources/api_constants.dart`

**Find (exact):**
```dart
  static const String baseUrl = String.fromEnvironment(
```

**Replace with:**
```dart
  // PROD: pass --dart-define=API_BASE_URL=https://<your-domain> (behind nginx) at build time.
  // The localhost default below is for LOCAL dev only and bypasses nginx.
  static const String baseUrl = String.fromEnvironment(
```

**Done when:** the comment is present above the `baseUrl` definition.

**Note (not a code change):** confirm the web build command passes
`--dart-define=API_BASE_URL=...` for production, and that nginx proxies `/api` to the API container.
