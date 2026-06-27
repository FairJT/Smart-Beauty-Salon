# 14 — RTL-aware alignment 🟢

Make layouts direction-aware (the app is RTL/fa).

**Find candidates:**
```powershell
Select-String -Path smart_salon_app\lib -Pattern "TextAlign.left|TextAlign.right|Alignment.centerLeft|Alignment.centerRight|EdgeInsets.only\((left|right):" -Recurse
```
Replace:
- `TextAlign.left` → `TextAlign.start`, `TextAlign.right` → `TextAlign.end`
- `Alignment.centerLeft` → `AlignmentDirectional.centerStart`, `centerRight` → `centerEnd`
- `EdgeInsets.only(left: x)` → `EdgeInsetsDirectional.only(start: x)`, `right:` → `end:`

Leave symmetric/centered layouts unchanged.
**Done when:** no hardcoded left/right directional values remain for text/padding.
