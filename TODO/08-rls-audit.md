# Task 08 — Audit: which tenant tables are NOT under RLS 🟢 (output only)

This task writes a list. It does NOT change any code file.

**Step 1 — list every entity that has a TenantId:**
```powershell
Select-String -Path src\ -Pattern "public Guid TenantId" -Recurse |
  ForEach-Object { Split-Path $_.Path -Leaf } | Sort-Object -Unique
```

**Step 2 — list tables already in the RLS policy:**
```powershell
Select-String -Path src\SalonOS.Infrastructure\Migrations\AddRLS.sql -Pattern "ON \[dbo\]\.\[(\w+)\]" |
  ForEach-Object { $_.Matches.Groups[1].Value } | Sort-Object -Unique
```

**Step 3 — write the difference into a new file** `TODO\rls-gaps.md`:
list every entity from Step 1 whose table is NOT in Step 2.

**Done when:** `TODO\rls-gaps.md` exists with the candidate list.

**⚠️ Do NOT add these tables to the policy yourself.** Some entities are global (no tenant scoping).
Hand `TODO\rls-gaps.md` back to Claude to decide which are truly tenant-owned.
