# UI — complete screen checklist (all parts)

Every screen, every role. `🆕 new` · `♻️ restyle existing`. Each becomes one code task (delivered
role-by-role using UI-0). "Mock" = which of the 4 approved mockups it derives from.

Status legend for build: `[ ]` not started.

---

## A0) Entry / Auth — the FIRST screens (accent: olive/ink)
> These are the app's main entry / startup / login flow. They were missing/under-specified before.
- [ ] **Splash / Startup** ♻️ (`splash_screen`) — auth mock A — brand + 3-dot loading; then auto-routes
  by auth state + role (this is the "main / startup" page).
- [ ] **Onboarding** 🆕 — client mock 1 — shown once to first-time users right after splash.
- [ ] **Login (phone → OTP request)** ♻️ (`login_screen`) — auth mock B — phone field, "دریافت کد تأیید",
  and a "مرور بدون ورود" guest link. OTP-based, no password. `POST /api/auth/login` (or request-otp endpoint).
- [ ] **OTP verify** ♻️ (`otp_screen`) — auth mock C — 5-box code + resend timer + "تغییر شماره".
- [ ] **Complete profile / Register** ♻️ (`register_screen`) — auth mock D — name + optional city/avatar
  (new users only, after OTP). `POST /api/auth/register`.

## A) Public / Guest  (accent: olive/ink)
- [ ] **Home / discovery** ♻️ (`home_screen`) — mock 2 — search bar, `FChip` categories, featured `FCard`,
  nearby salon `FCard` list, `FBottomNav`. Endpoints: `/api/salons`, `/api/homepage/slides`, `/api/placements/active`.
- [ ] **Salon profile** ♻️ (`salon_detail_screen`) — mock 3 — hero, `FStat` row, tabs (services/artists/reviews),
  `FServiceRow` grouped by parent line, sticky booking bar. `/api/salons/{slug}`, `/api/catalog-services`.
- [ ] **Artist public page** 🆕 — services + skill bars + reviews + book. `/api/artists/{id}`.
- [ ] **Blog list** 🆕 + **Blog post** 🆕 — `/api/blog`.
- [ ] **Join-salon form** 🆕 — "سالن خود را ثبت کنید". `POST /api/join-requests`.

## B) Client  (accent: ink) — bottom nav: خانه · رزرو · نوبت‌ها · پروفایل
- [ ] **Booking flow** ♻️ (`booking_screen`/`guest_booking_screen`) — mock 4 — 4-step progress,
  `FAvatar` artist chips, Jalali strip, `FSlot` grid, summary card, deposit CTA. `/api/appointments`, `/api/salons/{slug}/slots`.
- [ ] **My appointments** ♻️ (`appointment_list`) — `FCard` + `FStatusChip`, cancel / change. `/api/appointments/mine`.
- [ ] **Service history** ♻️ — past visits + previous artist + total paid.
- [ ] **Invoice** 🆕 — `FCard` summary. `GET /api/invoices/{id}`.
- [ ] **Offers / discounts** 🆕 — list + code field (`validate`). `/api/offers/discounts(/validate)`.
- [ ] **Feedback / complaint** 🆕 — form. `POST /api/client-feedback`.
- [ ] **Profile** ♻️ (`profile_screen`) + **Notifications** ♻️ (`notifications_screen`).

## C) Artist  (accent: teal) — tabs: امروز · برنامه · مشتری‌ها · بیشتر
- [ ] **Today / dashboard** ♻️ (`artist_dashboard_screen`) — `FStat` count + next appts. `/api/artist-schedule/my/stats`.
- [ ] **Schedule / shifts** ♻️ (`artist_schedule_screen`). `/api/artist-schedule/my`.
- [ ] **Appointments + check-in** 🆕 — `FStatusChip`, check-in / complete / reschedule-request. `/api/artist-visit/*`.
- [ ] **Leave request** 🆕 — `POST /api/leaves/my`.
- [ ] **My clients + notes** 🆕 — `/api/client-notes`.
- [ ] **Product usage** 🆕 — `/api/product-usage`.
- [ ] **Staff requests** 🆕 (issue/equipment) — `/api/staff-requests`.
- [ ] **My contracts** 🆕 — `GET /api/staff-contracts/my`.
- [ ] **Notices / instructions** 🆕 — `GET /api/salon/notices`.

## D) SalonManager  (accent: indigo) — tabs: داشبورد · سالن · پرسنل · مالی · بیشتر
- [ ] **Dashboard** ♻️ (`manager_dashboard_screen`) — `FStat` overview.
- [ ] **Salon profile + location + amenities + notices** 🆕 — `/api/salon/amenities`, `/api/salon/notices`, salon edit.
- [ ] **Working hours + closures** 🆕 — `/api/salon/working-hours`, `/api/salon/closures`.
- [ ] **Services (parent-child)** 🆕 — manage lines + sub-services. `/api/catalog-services`.
- [ ] **Staff + contracts** ♻️ (`artist_management_screen`) + `/api/staff-contracts`.
- [ ] **Appointments (view/confirm)** ♻️.
- [ ] **Customers + reviews** 🆕 — `/api/salon/insights/*`.
- [ ] **Discounts** 🆕 — `/api/salon/discounts`.
- [ ] **Finance ledger** 🆕 — `/api/salon/finance`.
- [ ] **Hiring (postings + applications)** 🆕 — `/api/salon/hiring/*`.
- [ ] **Inbox** 🆕 — staff-requests + reschedule-requests + client-feedback + leave approvals (tabs).

## E) SuperAdmin  (accent: gold) — tabs: داشبورد · پلتفرم · محتوا · مالی
- [ ] **Dashboard / stats** ♻️ (`admin_dashboard`). `/api/admin/stats`.
- [ ] **Tenants / salons** ♻️ + **Users** ♻️. `/api/admin/*`.
- [ ] **Service templates** 🆕 + **Package listings** 🆕. `/api/service-templates`, `/api/package-listings`.
- [ ] **Homepage CMS** 🆕 (slides + menus). `/api/homepage/*`.
- [ ] **Blog / news editor** 🆕. `/api/blog`.
- [ ] **Placements (VIP/ladder)** 🆕. `/api/placements`.
- [ ] **Join requests** 🆕. `/api/join-requests`.
- [ ] **Platform accounting** 🆕. `/api/admin/accounting/overview`.

---

## Totals & order
~54 screens across **A0 entry/auth (5)** + public + 4 roles. Build order: **UI-0 → A0 (splash/login/otp/
register — the entry flow) → A (public/client core, the 4 mocks) → rest of B (client) → C (artist) →
D (manager) → E (superadmin)**. Each role is a code batch; each screen a task with near-complete Flutter built on UI-0.
