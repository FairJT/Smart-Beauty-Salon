# Task 12 — Build everything 🟢 (final check)

No code change. Confirm Tasks 01–10 compile.

**Run (PowerShell) from repo root:**
```powershell
dotnet build SalonOS.slnx
```

**Done when:** the build succeeds with no NEW errors.

**If it fails:** copy the FIRST error (file + line + message) and report it. Most likely causes:
- Task 02: a dangling comma or brace after deleting the Receptionist block.
- Task 06/07: a comma or `GO`-split issue in the SQL / Program.cs.
Fix only the exact line the compiler points to; if unsure, hand the error to Claude.
