---
name: multi-tenancy
description: Use this skill whenever writing or reviewing any code that reads or writes tenant-owned data in SalonOS, including new database models, Prisma schemas, repository or service methods, controllers, queries, or migrations. Use it any time the words tenant, salon, isolation, RLS, scope, or cross-tenant come up, and any time a new entity is added. Tenant isolation is the backbone of the whole platform, so apply this skill even when the change looks small. A single unscoped query is a data breach.
---

# Multi-tenancy in SalonOS

SalonOS is a shared-schema, single-database multi-tenant system. Every salon is
a tenant. The single worst bug class in this codebase is one tenant reading or
writing another tenant's data. Three layers of defense exist and all are
mandatory. Never rely on just one.

## Layer 1: every tenant-owned row carries tenant_id

When adding any entity, decide first: is it tenant-owned or global?

- Tenant-owned (the default): bookings, staff, payslips, inventory, reviews,
  catalog instances. These get a non-nullable `tenantId` column and an index on it.
- Global (rare): the service-template definitions the platform owner sells,
  system config, the user/account table itself. These have no `tenantId`.

If unsure, it is tenant-owned.

Prisma model pattern:

```prisma
model Booking {
  id        String   @id @default(uuid())
  tenantId  String
  // ... fields
  @@index([tenantId])
}
```

## Layer 2: every query is scoped by request tenant context

A Nest middleware resolves the tenant from the authenticated token into a
request-scoped `TenantContext`. Service and repository code reads `tenantId`
from that context and includes it in every query. Never trust a `tenantId` that
arrives in a request body or query param.

```typescript
// CORRECT: scoped by the resolved context
async findBookings() {
  return this.prisma.booking.findMany({
    where: { tenantId: this.tenantContext.tenantId },
  });
}

// WRONG: trusts client input, leaks across tenants
async findBookings(tenantId: string) {
  return this.prisma.booking.findMany({ where: { tenantId } });
}
```

Writes must set `tenantId` from the context, never from the payload:

```typescript
await this.prisma.booking.create({
  data: { ...dto, tenantId: this.tenantContext.tenantId },
});
```

## Layer 3: Postgres Row-Level Security

Application code can have bugs. RLS is the floor that holds even when it does.
Every tenant table gets an RLS policy keyed on a session variable that the app
sets per request (`SET app.current_tenant = ...`).

```sql
ALTER TABLE "Booking" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON "Booking"
  USING ("tenantId" = current_setting('app.current_tenant', true));
```

The platform-owner role bypasses RLS deliberately for cross-tenant admin views.
That bypass is the only sanctioned cross-tenant path and lives in one clearly
named place.

## The mandatory test

No module ships without a test proving isolation. The shape:

1. Create tenant A and tenant B with data in each.
2. Authenticate as tenant A.
3. Attempt to read and to mutate tenant B's records by id.
4. Assert both fail (empty result or forbidden, never B's data).

If this test does not exist, the module is not done.

## Cross-module rule

Tenant scoping does not excuse reaching into another module's tables. Even
within the same tenant, a module reads another module's data only through that
module's service interface. See `ARCHITECTURE.md` section 3.

## Review checklist

- Does every new tenant-owned model have `tenantId` and an index on it?
- Is every query scoped by `tenantContext`, not by client input?
- Do writes set `tenantId` from context?
- Is RLS enabled on the new table with a policy?
- Does the cross-tenant access test exist and pass?
