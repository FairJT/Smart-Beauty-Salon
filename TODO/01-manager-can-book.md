# Task 01 — SalonManager can book on behalf 🟢

Make ONLY this change. Do not edit anything else.

**File:** `src/SalonOS.Shared/Authorization/RolePermissions.cs`

**Find (exact):**
```csharp
            Permissions.AppointmentViewAll,
            Permissions.AppointmentConfirm,
```

**Replace with:**
```csharp
            Permissions.AppointmentViewAll,
            Permissions.AppointmentCreate,      // book-on-behalf (replaces Receptionist role)
            Permissions.AppointmentConfirm,
```

**Done when:** the `["SalonManager"]` array contains `Permissions.AppointmentCreate`.

**Verify (PowerShell):**
```powershell
Select-String -Path src\SalonOS.Shared\Authorization\RolePermissions.cs -Pattern "AppointmentCreate"
```
Expect 2 hits for now (SalonManager + the old Receptionist block — Task 02 removes the second).
