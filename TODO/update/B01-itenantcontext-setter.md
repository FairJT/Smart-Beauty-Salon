# Task B01 — Add SetPublicTenant to ITenantContext 🟡

Make ONLY this change.

**File:** `src/SalonOS.Shared/ITenantContext.cs`

**Find (exact):**
```csharp
public interface ITenantContext
{
    Guid TenantId { get; }
    bool IsPlatformOwner { get; }
}
```

**Replace with:**
```csharp
public interface ITenantContext
{
    Guid TenantId { get; }
    bool IsPlatformOwner { get; }

    /// <summary>
    /// Public read-only paths (anonymous salon/slots pages) resolve the tenant from a
    /// PUBLIC slug server-side and set it here. Implementations must only apply it when
    /// no tenant is present yet (anonymous); it never overrides an authenticated tenant.
    /// </summary>
    void SetPublicTenant(Guid tenantId);
}
```

**Done when:** the interface declares `SetPublicTenant`.

**Verify (PowerShell):**
```powershell
Select-String -Path src\SalonOS.Shared\ITenantContext.cs -Pattern "SetPublicTenant"
```
Expect 1 hit.
