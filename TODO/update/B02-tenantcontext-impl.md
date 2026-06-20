# Task B02 — Implement SetPublicTenant 🟡

Make ONLY this change. (Do B01 first — it adds the interface method.)

**File:** `src/SalonOS.Infrastructure/MultiTenancy/TenantContextFromClaims.cs`

**Find (exact):**
```csharp
    public Guid TenantId { get; }
    public bool IsPlatformOwner { get; }

    public TenantContextFromClaims(ICurrentUser currentUser)
    {
        TenantId        = currentUser.TenantId;
        IsPlatformOwner = currentUser.IsPlatformOwner;
    }
}
```

**Replace with:**
```csharp
    public Guid TenantId { get; private set; }
    public bool IsPlatformOwner { get; }

    public TenantContextFromClaims(ICurrentUser currentUser)
    {
        TenantId        = currentUser.TenantId;
        IsPlatformOwner = currentUser.IsPlatformOwner;
    }

    public void SetPublicTenant(Guid tenantId)
    {
        // Only fill an empty (anonymous) tenant; never override an authenticated one,
        // and never for the platform owner.
        if (TenantId == Guid.Empty && !IsPlatformOwner)
            TenantId = tenantId;
    }
}
```

**Done when:** `TenantId` has a `private set` and `SetPublicTenant` is implemented.

**Verify (PowerShell):**
```powershell
Select-String -Path src\SalonOS.Infrastructure\MultiTenancy\TenantContextFromClaims.cs -Pattern "SetPublicTenant"
```
Expect 1 hit.
