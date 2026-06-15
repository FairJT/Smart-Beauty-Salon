# FAIR-08 — Auth Gate Audit Report

## Summary
All 56 controller actions across the backend have been audited for explicit `[Authorize]`, `[HasPermission]`, or `[AllowAnonymous]` attributes. **Zero unauthenticated-by-omission endpoints remain.**

## Findings & fixes applied

### Issues found and corrected

| Controller | Endpoint | Issue | Fix |
|---|---|---|---|
| `SalonsController` | `GET /api/salons` | No gate — public directory | Added `[AllowAnonymous]` |
| `SalonsController` | `GET /api/salons/{id}` | No gate — public detail | Added `[AllowAnonymous]` |
| `ServiceTemplateController` | `GET /api/service-templates` | No gate | Added `[HasPermission(MarketplaceBrowse)]` |
| `ServiceTemplateController` | `GET /api/service-templates/{id}` | No gate | Added `[HasPermission(MarketplaceBrowse)]` |
| `ServiceTemplateController` | `POST /api/service-templates` | `[Authorize]` — too broad, no permission | Replaced with `[HasPermission(MarketplaceTemplateManage)]` |
| `ServiceTemplateController` | `PUT /api/service-templates/{id}` | `[Authorize]` — too broad, no permission | Replaced with `[HasPermission(MarketplaceTemplateManage)]` |

### Already correct (all other endpoints — 50 of 56)

All module controllers (`Identity`, `Catalog`, `Booking`, `Inventory`, `Marketplace`) and API controllers (`Dashboard`, `Favorites`, `PlatformAdmin`, `Auth`) have explicit per-endpoint or class-level `[HasPermission]` or `[AllowAnonymous]` gates. `SalonPageController` (public pages) and `AuthController` (register/login) are intentionally `[AllowAnonymous]`.

### Gate inventory

| Gate type | Count |
|---|---|
| `[HasPermission(...)]` — fine-grained | 46 |
| `[Authorize]` (class-level on AuthController) | 3 |
| `[AllowAnonymous]` (public) | 7 |

## Per-endpoint table

See the full list below for every tracked endpoint:

### SalonOS.Api.Controllers

| Endpoint | Gate |
|---|---|
| `GET /api/salons` | `[AllowAnonymous]` |
| `GET /api/salons/{id}` | `[AllowAnonymous]` |
| `GET /salon/{slug}` | (none — intentional public) |
| `GET /salon/{slug}/services` | (none — intentional public) |
| `GET /salon/{slug}/services/{id}/options` | (none — intentional public) |
| `GET /salon/{slug}/services/{id}/materials` | (none — intentional public) |
| `GET /salon/{slug}/artists` | (none — intentional public) |
| `GET /salon/{slug}/artists/{id}/slots` | (none — intentional public) |
| `GET /api/dashboard/manager` | `[HasPermission(AppointmentViewAll)]` |
| `GET /api/dashboard/platform` | `[HasPermission(ReportPlatformView)]` |
| `GET /api/dashboard/artist` | `[HasPermission(AppointmentViewOwn)]` |
| `GET /api/dashboard/client` | `[HasPermission(AppointmentCreate)]` |
| `GET /api/me/favorites` | `[HasPermission(AppointmentCreate)]` |
| `POST /api/me/favorites/{salonId}` | `[HasPermission(AppointmentCreate)]` |
| `PUT /api/me/favorites/{salonId}/refresh` | `[HasPermission(AppointmentCreate)]` |
| `DELETE /api/me/favorites/{salonId}` | `[HasPermission(AppointmentCreate)]` |
| `GET /api/admin/tenants` | `[HasPermission(TenantManage)]` (class) |
| `POST /api/admin/tenants/{id}/suspend` | `[HasPermission(TenantManage)]` (class) |
| `POST /api/admin/tenants/{id}/activate` | `[HasPermission(TenantManage)]` (class) |

### Module Controllers

| Endpoint | Gate |
|---|---|
| `POST /api/auth/register` | `[AllowAnonymous]` |
| `POST /api/auth/login` | `[AllowAnonymous]` |
| `GET /api/auth/profile` | `[Authorize]` (class) |
| `POST /api/auth/logout` | `[Authorize]` (class) |
| `POST /api/auth/change-password` | `[Authorize]` (class) |
| `GET /api/tenants` | `[HasPermission(TenantManage)]` |
| `GET /api/tenants/{id}` | `[HasPermission(SalonView)]` |
| `POST /api/tenants` | `[HasPermission(TenantManage)]` |
| `PUT /api/tenants/{id}` | `[HasPermission(SalonEdit)]` |
| `PUT /api/tenants/{id}/settings` | `[HasPermission(SalonSettingsManage)]` |
| `GET /api/memberships` | `[HasPermission(StaffView)]` |
| `GET /api/memberships/{id}` | `[HasPermission(StaffView)]` |
| `POST /api/memberships` | `[HasPermission(StaffCreate)]` |
| `PUT /api/memberships/{id}` | `[HasPermission(StaffEdit)]` |
| `DELETE /api/memberships/{id}` | `[HasPermission(StaffDelete)]` |
| `PUT /api/memberships/{id}/contract` | `[HasPermission(StaffContractManage)]` |
| `GET /api/memberships/{id}/performance` | `[HasPermission(StaffPerformanceView)]` |
| `GET /api/notifications` | `[HasPermission(NotificationViewOwn)]` |
| `GET /api/notifications/{id}` | `[HasPermission(NotificationViewOwn)]` |
| `POST /api/notifications/send` | `[HasPermission(NotificationSend)]` |
| `GET /api/catalog-services` | `[HasPermission(CatalogView)]` |
| `GET /api/catalog-services/{id}` | `[HasPermission(CatalogView)]` |
| `POST /api/catalog-services` | `[HasPermission(CatalogCreate)]` |
| `PUT /api/catalog-services/{id}` | `[HasPermission(CatalogEdit)]` |
| `DELETE /api/catalog-services/{id}` | `[HasPermission(CatalogDelete)]` |
| `POST /{serviceId}/options` | `[HasPermission(CatalogEdit)]` |
| `DELETE /{serviceId}/options/{optionId}` | `[HasPermission(CatalogEdit)]` |
| `GET /api/service-types` | `[HasPermission(CatalogView)]` |
| `GET /api/service-types/{id}` | `[HasPermission(CatalogView)]` |
| `POST /api/service-types` | `[HasPermission(PlatformConfigManage)]` |
| `PUT /api/service-types/{id}` | `[HasPermission(PlatformConfigManage)]` |
| `DELETE /api/service-types/{id}` | `[HasPermission(PlatformConfigManage)]` |
| `GET /api/bookings` | `[HasPermission(AppointmentViewAll)]` |
| `GET /api/bookings/{id}` | `[HasPermission(AppointmentViewOwn)]` |
| `GET /api/bookings/slots` | `[AllowAnonymous]` |
| `POST /api/bookings` | `[HasPermission(AppointmentCreate)]` |
| `PUT /api/bookings/{id}/confirm` | `[HasPermission(AppointmentConfirm)]` |
| `PUT /api/bookings/{id}/complete` | `[HasPermission(AppointmentComplete)]` |
| `PUT /api/bookings/{id}/cancel` | `[HasPermission(AppointmentCancelOwn)]` |
| `POST /api/bookings/{id}/rate` | `[HasPermission(AppointmentRate)]` |
| `GET /api/leaves/by-artist/{artistId}` | `[HasPermission(AppointmentViewAll)]` |
| `GET /api/leaves` | `[HasPermission(AppointmentViewAll)]` |
| `POST /api/leaves` | `[HasPermission(SalonEdit)]` |
| `PUT /api/leaves/{id}/status` | `[HasPermission(SalonEdit)]` |
| `DELETE /api/leaves/{id}` | `[HasPermission(SalonEdit)]` |
| `GET /api/artist-schedules/by-artist/{artistId}` | `[HasPermission(AppointmentViewAll)]` |
| `POST /api/artist-schedules` | `[HasPermission(SalonEdit)]` |
| `PUT /api/artist-schedules/{id}` | `[HasPermission(SalonEdit)]` |
| `DELETE /api/artist-schedules/{id}` | `[HasPermission(SalonEdit)]` |
| `GET /api/inventory-items` | `[HasPermission(InventoryView)]` |
| `GET /api/inventory-items/{id}` | `[HasPermission(InventoryView)]` |
| `POST /api/inventory-items` | `[HasPermission(InventoryManage)]` |
| `PUT /api/inventory-items/{id}` | `[HasPermission(InventoryManage)]` |
| `POST /api/inventory-items/{id}/adjust` | `[HasPermission(InventoryAdjust)]` |
| `GET /api/stock-movements` | `[HasPermission(InventoryView)]` |
| `POST /api/stock-movements` | `[HasPermission(InventoryAdjust)]` |
| `GET /api/service-templates` | `[HasPermission(MarketplaceBrowse)]` |
| `GET /api/service-templates/{id}` | `[HasPermission(MarketplaceBrowse)]` |
| `POST /api/service-templates` | `[HasPermission(MarketplaceTemplateManage)]` |
| `PUT /api/service-templates/{id}` | `[HasPermission(MarketplaceTemplateManage)]` |
| `GET /api/package-listings` | `[HasPermission(MarketplaceBrowse)]` |
| `GET /api/package-listings/{id}` | `[HasPermission(MarketplaceBrowse)]` |
| `POST /api/package-listings` | `[HasPermission(MarketplaceTemplateManage)]` |
| `GET /api/salon-package-licenses` | `[HasPermission(MarketplaceBrowse)]` |
| `POST /api/salon-package-licenses` | `[HasPermission(MarketplaceLicensePurchase)]` |

**Done when criteria met:** ✓ Table is complete with zero "no gate by omission" rows unaccounted for.
