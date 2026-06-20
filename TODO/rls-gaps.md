# RLS Gaps Audit

The following tenant-scoped entities were identified (have a `public Guid TenantId` property):

- TenantEntity
- TenantContext
- CurrentUser
- PackagePurchasedHandler
- TenantContextFromClaims
- BookingCompleted
- BookingCancelled
- ArtistProfile
- InventoryLow
- Membership
- SalonManagerProfile
- MembershipDtos (request/response DTOs)

The RLS policy (src/SalonOS.Infrastructure/Migrations/AddRLS.sql) currently includes the following tables:

- Bookings
- CatalogServices
- CatalogServiceOptions
- InventoryItems
- StockMovements
- SalonPackageLicenses
- ArtistSchedules
- Leaves

**Entities whose corresponding tables are NOT covered by the RLS policy** (manual verification required):

- TenantEntity (no direct table)
- TenantContext (no direct table)
- CurrentUser (no direct table)
- PackagePurchasedHandler (no direct table)
- TenantContextFromClaims (no direct table)
- BookingCompleted (corresponds to `Bookings` – already covered)
- BookingCancelled (corresponds to `Bookings` – already covered)
- ArtistProfile (may map to `Artists` – not in RLS)
- InventoryLow (may map to `InventoryItems` – already covered)
- Membership (maps to `Memberships` – not in RLS)
- SalonManagerProfile (maps to `SalonManagers` – not in RLS)
- MembershipDtos (DTOs, no table)

**Potential tables to add to RLS policy (subject to review):**

- Artists
- Memberships
- SalonManagers

Please review and confirm which of these should be added to the Row‑Level Security policy.