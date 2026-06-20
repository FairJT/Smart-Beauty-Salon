# Task 05 — Legacy Receptionist logs in as SalonManager 🟡 (review after)

Make ONLY this change. Do not edit anything else.

**File:** `src/Modules/Identity/Infrastructure/AuthService.cs`

**Find (exact):**
```csharp
                // Receptionist has its own MembershipRole value (Task 7.2).
                if (membership.Role == MembershipRole.Receptionist)
                    roleName = "Receptionist";
```

**Replace with:**
```csharp
                // Receptionist folded into SalonManager (book-on-behalf). Any legacy
                // Receptionist membership now authenticates as SalonManager.
                if (membership.Role == MembershipRole.Receptionist)
                    roleName = "SalonManager";
```

**Done when:** no code sets `roleName = "Receptionist"`.

**Verify (PowerShell):**
```powershell
Select-String -Path src\Modules\Identity\Infrastructure\AuthService.cs -Pattern 'roleName = "Receptionist"'
```
Expect **0 hits**.

**⚠️ Human review:** any existing user with `Membership.Role == Receptionist` will now get the FULL
SalonManager permission set at login. If that is too much, tell Claude — the alternative is to map them
to `"Artist"` plus only the book-on-behalf permission.
