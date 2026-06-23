# Agent Tasks — UI implementation (Fresha-style, all screens)

Implements the approved Fresha-style design (white + warm gray + olive accent, rounded cards, big
search, 4-item bottom nav, Persian/RTL/Jalali/Toman) across the WHOLE app.

## How this is organized
1. **UI-0 — design system** (`UI-0-design-system.md`): one Flutter file `fresha_ui.dart` with the
   palette + reusable widgets (`FCard`, `FChip`, `FServiceRow`, `FSlot`, `FPrimaryButton`,
   `FMoneyText`, `FStatusChip`, `FBottomNav`, `FAvatar`, `FStat`, `FEmpty/FLoading/FError`).
   **Build this FIRST** — every screen composes these, so the design stays consistent and screens stay small.
2. **Screen checklist** (`UI-screens-all.md`): EVERY screen across all 5 roles + public, as tasks
   (new / restyle, components used, endpoint, which mock it derives from). This is the full "all screens" map.
3. **Per-screen code batches**: delivered role-by-role using UI-0, starting with the 4 mocked client
   screens. Each screen = one task with near-complete Flutter (a weak agent can't freehand Flutter).

## Build order (most-used → least)
UI-0 → Public/Client core (the 4 mocked screens: onboarding, home, salon, booking) → rest of Client
(invoice, offers, feedback, my-appointments) → Artist → Manager → SuperAdmin.

## Conventions (every screen)
- Wrap in `Directionality(textDirection: TextDirection.rtl)` is already global — just build RTL-aware.
- Dates via `core/format/jalaali_helper`; money via `FMoneyText` (Toman, Persian digits).
- Every list screen handles loading / empty / error with `FLoading` / `FEmpty` / `FError`.
- New screens import `core/fresha/fresha_ui.dart` and use `FCol.*` colors (NOT `AppColors`).
  Existing screens keep `AppColors` until their restyle task swaps them.

## Non-destructive migration
UI-0 adds a NEW palette/components file; it does NOT touch `AppColors` or the 19 existing screens.
Each existing screen is restyled in its own task. Once all screens are migrated, a final task swaps the
global `MaterialApp` theme to the warm palette.
