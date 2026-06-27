# 15 — No English in the UI 🟡

Translate every visible English UI string to Persian.

**Find candidates:**
```powershell
Select-String -Path smart_salon_app\lib\presentation -Pattern "Text\('[A-Za-z]|hintText: '[A-Za-z]|label(Text)?: '[A-Za-z]" -Recurse
```
For each hit, translate the visible text to Persian (buttons, titles, hints, snackbars, tab labels).
Likely offenders: `all_screens_skeletons.dart`, `generated/*`, placeholder screens.

Rules:
- Translate ONLY user-facing text. Do NOT change route names (`/home`), code identifiers, JSON keys, or API fields.
- Show numbers with Persian digits (reuse `FMoneyText` / the jalaali helper).

**Done when:** the scan shows no English user-facing `Text('...')` left.
