# Task 02 — Remove the Receptionist permission block 🟢

Make ONLY this change. Do not edit anything else.

**File:** `src/SalonOS.Shared/Authorization/RolePermissions.cs`

**Find (exact — the whole block) and DELETE it:**
```csharp
        ["Receptionist"] = new[]
        {
            Permissions.SalonView,
            Permissions.StaffView,
            Permissions.CatalogView,
            Permissions.AppointmentViewAll,
            Permissions.AppointmentCreate,
            Permissions.AppointmentConfirm,
            Permissions.AppointmentComplete,
            Permissions.AppointmentCancelAll,
            Permissions.InventoryView,
            Permissions.FinanceDepositTake,
            Permissions.NotificationSend,
            Permissions.NotificationViewOwn,
        },
```

**Replace with:** *(nothing — remove it entirely, including the trailing comma)*

**Done when:** there is no `"Receptionist"` key in the map.

**Verify (PowerShell):**
```powershell
Select-String -Path src\SalonOS.Shared\Authorization\RolePermissions.cs -Pattern "Receptionist"
```
Expect **0 hits**.
