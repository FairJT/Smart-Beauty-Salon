# Task 04 — Clean Receptionist out of 3 comments 🟢

Comment-only edits. No behavior change. Do them one by one.

### 4a
**File:** `src/SalonOS.Shared/Identity/ICurrentUser.cs`
**Find:**
```csharp
    /// The role name as stored in the token: SalonManager, Receptionist, Artist,
```
**Replace with:**
```csharp
    /// The role name as stored in the token: SalonManager, Artist,
```

### 4b
**File:** `src/Modules/Identity/API/Controllers/NotificationsController.cs`
**Find:** `(SalonManager, Receptionist, Artist)`
**Replace with:** `(SalonManager, Artist)`
(Apply to every line where that exact text appears in this file.)

### 4c
**File:** `src/Modules/Inventory/API/Controllers/InventoryItemController.cs`
**Find:** `SalonManager, Receptionist.`
**Replace with:** `SalonManager.`

**Done when (Verify, PowerShell):**
```powershell
Select-String -Path src\ -Pattern "Receptionist" -Recurse
```
Expect only ONE hit left: the deprecated enum line in `Membership.cs` (from Task 03).
