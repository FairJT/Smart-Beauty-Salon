# C1 — Client permissions: browse catalog + file feedback 🟡

## Step 1 — add the feedback permission constant
**File:** `src/SalonOS.Shared/Authorization/Permissions.cs`
**Find (exact):**
```csharp
    // ─── Platform / Tenant ──────────────────────────────────
```
**Replace with:**
```csharp
    // ─── Client self-service ─────────────────────────────────
    public const string ClientFeedbackCreate = "clientfeedback.create";

    // ─── Platform / Tenant ──────────────────────────────────
```

## Step 2 — give Client `CatalogView` + `ClientFeedbackCreate`
**File:** `src/SalonOS.Shared/Authorization/RolePermissions.cs`
**Find (exact):**
```csharp
                Permissions.LoyaltyViewOwn,
                Permissions.NotificationViewOwn,
            },
        };
```
**Replace with:**
```csharp
                Permissions.LoyaltyViewOwn,
                Permissions.NotificationViewOwn,
                Permissions.CatalogView,
                Permissions.ClientFeedbackCreate,
            },
        };
```
(`LoyaltyViewOwn` is unique to the Client role, and the trailing `};` confirms it's the last block.)

**Done when:** build succeeds; the Client role array contains `CatalogView` and `ClientFeedbackCreate`.
