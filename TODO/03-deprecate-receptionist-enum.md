# Task 03 — Deprecate the Receptionist enum member 🟢

Make ONLY this change. Do not edit anything else.
Do NOT delete the member or change its number — that would shift `Staff = 5` / `Member = 6`
and corrupt existing rows.

**File:** `src/Modules/Identity/Domain/Membership.cs`

**Find (exact):**
```csharp
    Receptionist = 4,   // Task 7.2 — front-desk role (booking + deposit, no admin)
```

**Replace with:**
```csharp
    Receptionist = 4,   // DEPRECATED — folded into SalonManager (book-on-behalf). Do not assign.
```

**Done when:** the comment says DEPRECATED and the value is still `4`.

**Verify (PowerShell):**
```powershell
Select-String -Path src\Modules\Identity\Domain\Membership.cs -Pattern "Receptionist = 4"
```
Expect 1 hit with the new comment.
