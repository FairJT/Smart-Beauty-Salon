# 09 — Manager: services / catalog management screen 🔴 (review before commit)

This is a **new screen with judgment calls** — let the agent scaffold it, then you review.
Model it on the existing `manager/artist_management_screen.dart` (same list + add/edit
dialog pattern, same provider style).

**Backend already exists** — no API work needed:
- `GET/POST/PUT/DELETE` services: `ServicesController` (`ApiConstants.services` → `/services`)
  and `CatalogServiceController` (`/catalog-services`, 7 endpoints).
- Service types: `ServiceTypesController` (`/service-types`).
- Packages: `Marketplace/.../ServiceTemplateController` (`/service-templates`).

**Create:** `smart_salon_app/lib/presentation/pages/manager/catalog_management_screen.dart`
- `ConsumerStatefulWidget`, class `CatalogManagementScreen`, `const CatalogManagementScreen({super.key})`.
- On init, load services from the API. Show them in a list (name, duration, price using
  `MoneyFormatter.format` — **price is integer Rial minor units**, never a float).
- Add / Edit via a dialog → POST/PUT. Delete with a confirm dialog → DELETE.
- Reuse `LoadingState` / `ErrorState` / `EmptyState` / `SummaryCard` from `dashboard_widgets.dart`.

**Then wire it** into the manager dashboard's "مدیریت سالن" card (the one from task 06):
add a second `ListTile` ("مدیریت خدمات", icon `Icons.content_cut`) that navigates to
`const CatalogManagementScreen()`.

**Guardrails (do not skip):**
- All money stays in integer minor units end to end (see the `payments` rules).
- Never send a `tenantId` from the client — the salon is derived from the token
  (see the `multi-tenancy` rules). If any catalog DTO asks for a tenant id, STOP and report.

**Done when:** a SalonManager can list, add, edit, and delete their own services, and the
prices display correctly in Toman/Rial.
