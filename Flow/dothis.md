# SalonOS — Implementation Roadmap (from the product spec)

**Source:** `salon.docx` (the product/requirements document) reconciled with the audited codebase and prior planning files.
**Generated:** 2026-06-13

This roadmap supersedes the feature ordering in `SALONOS_DESIGN_AND_IMPLEMENTATION_PLAN.md`. The *foundation* work in the earlier files still stands — this layers the real product features on top of it.

---

## 1. What the spec changes vs. what we'd planned

| Area | Earlier understanding | What the spec actually requires |
|---|---|---|
| Roles | SuperAdmin, SalonManager, Artist, Client | **+ JobSeeker (کارجو)** — a capability *inside* Client, not a new user type |
| Artist scheduling | One availability model | **Two contract models** (fixed-salary حقوق ثابت / line-room rental اجاره لاین) that change who owns the schedule |
| Services | Flat `SalonService` (price, duration) | **Configurable**: service → options (گزینه‌ها) + materials (متریال) that vary price *and* duration |
| Booking | One deposit flow | **Three entry points** + dynamic estimated price + deposit (بیعانه) + in-service upsell |
| Reviews | Client rates artist | **Bidirectional**: client→artist, client→salon, artist→client, + moderation question |
| Salon site | A profile | **Subdomain per salon** + per-salon theming (color/logo/font); shared or unique template |
| Platform | Package subscriptions | **+ CMS, ads, ladder/bump (نردبان), VIP listings, panel sales** |
| Job board | Not present | **Full two-sided job market** (manager postings ↔ jobseeker requests) |
| Accounting | Not present | **Per-role accounting** (financial flow by date/service/personnel/salon) + payroll |
| AI | Not present | **LLM search/booking + specialized consultation** (final version) |

---

## 2. Corrected role model

| Role | Scope | Notes |
|---|---|---|
| SuperAdmin (مدیر کل) | Platform | CMS, service-type definitions, monetization, accounting |
| SalonManager | Tenant | Salon, staff, catalog, scheduling, payroll, job postings |
| Artist (پرسنل) | Tenant | Own schedule/profile/reviews; behaviour depends on contract type |
| Client | Global | Discovery, booking, reviews, history |
| **JobSeeker** | **Capability on Client** | A Client with a `JobSeekerProfile` — resume, skills, location, job requests |

**Action for the base structure you're building now:** add JobSeeker as an optional `JobSeekerProfile` attached to a Client (1:1, opt-in), not as a `UserType`. Everything else in `SALONOS_ROLES_BASE_STRUCTURE_TODO.md` holds.

---

## 3. Two cross-cutting concepts that shape everything

### 3.1 Contract type (نوع قرارداد)
Every Artist belongs to one model, and it changes system behaviour:

| | Fixed-salary (حقوق ثابت) | Line/room rental (اجاره لاین) |
|---|---|---|
| Who sets hours | The salon (routine shifts) | The artist (own calendar) |
| Availability source | Salon working hours | Artist-declared calendar |
| Customer presence | Staff attend regardless | Artist attends per booking |
| Leave | Needs manager approval | Informational; just blocks slots |
| Pay | Salon pays salary (payroll) | Artist pays rent; keeps service revenue |
| Discounts | Manager can set | N/A |

Availability, leave, payroll, and discounts must all branch on this. Model it as a first-class enum on the salon/artist, not an afterthought.

### 3.2 Configurable services
`Service` → has many `ServiceOption` (e.g. color, design, length) and references `Material`. Each option/material contributes a price delta and a duration delta. The booking estimate is computed from base + selected options + material. Final price is confirmed after service (estimate vs final is why the schema needs both deposit and final amount).

---

## 4. Open product decisions (the spec itself raises these)

Resolve before building the affected phase:
1. **Review moderation** — are reviews public by default? Do they need manager/admin approval before showing?
2. **Artist service control** — can an Artist add/edit/remove their own services, or is it manager-only / manager-approved (since it affects pay)?
3. **Slot duration** — fixed (e.g. 30 min) for all, or variable per service+options? The spec leans toward variable with a fixed fallback.
4. **Subdomain strategy** — true per-salon subdomains (DNS/wildcard + routing) or path-based salon pages first? Big infra difference.
5. **JobSeeker monetization** — pay-per-application model (e.g. a fee per N requests) confirmed?

---

## 5. The roadmap

Complexity/Risk: 1 (low) → 5 (severe). Each phase assumes the prior one.

### Phase 0 — Foundation *(already specified; do first)*
Tenant isolation, payment provider, **base role structure (now incl. JobSeeker capability)**, money types, backups/CI. See `SALONOS_AGENT_TODO.md` and `SALONOS_ROLES_BASE_STRUCTURE_TODO.md`.
**Complexity 3 · Risk 4** — it's the security floor everything sits on.

### Phase 1 — Real catalog & salon identity
The data backbone the booking engine needs.
- Configurable services: `Service`, `ServiceOption`, `Material`, with price/duration deltas (§3.2). SuperAdmin defines allowed service *types*; salons instantiate them.
- Artist contract type (§3.1) on the artist/salon.
- Salon profile: name, contact, address, location, license, grade, meta; gallery (images/video); theming (color/logo/font).
- Subdomain or path-based salon pages (per decision #4).
- *Reuse:* existing Salon/Artist/SalonService scaffolding. *Change:* SalonService → configurable. *New:* options, materials, contract type, theming.
**Complexity 4 · Risk 3**

### Phase 2 — The booking engine *(the heart — "اصلی‌ترین بخش")*
- Availability service branching on contract type (salon hours vs artist calendar).
- Leave/holiday (مرخصی/تعطیلی) blocking: approved leave and salon closures make slots unselectable.
- Three booking entry points: (a) client dashboard search by date+service, optional salon/artist/material filters → suggested slots; (b) from salon page (salon fixed); (c) from artist page (salon+artist fixed).
- Service-option selection → dynamic estimated price + duration → deposit (بیعانه) → slot reserved.
- Variable vs fixed slot duration (per decision #3).
- In-service upsell / service change applied to the booking.
- *Reuse:* Appointment, deposit, overlap check. *Change:* dynamic duration/price, options. *New:* availability-by-contract, leave blocking, multi-entry booking.
**Complexity 5 · Risk 4** — concurrency on slot reservation + money estimate accuracy.

### Phase 3 — Profiles, reviews & dashboards
- Bidirectional reviews: client→artist (per service), client→salon (per visit, e.g. parking/ventilation claims), artist→client (punctuality/fussiness); moderation per decision #1.
- Artist profile: short + long resume, skills with proficiency % (chart), certificates per service with dates (drives "most up-to-date staff" ranking).
- Public surfaces: homepage (10 newest salons, top-rated, VIP, laddered), salon page (staff list, top staff, resume, location, reviews), artist page (resume, skills chart, certificates, booking).
- The five role dashboards: service history with filters (date/service/salon/staff) + reports + totals; manager customer/comment views; client history.
- *Reuse:* rating fields, loyalty. *New:* multi-target reviews, moderation, skills/certificates, public pages, dashboard rollups.
**Complexity 4 · Risk 3**

### Phase 4 — Job board (JobSeeker ↔ SalonManager)
- `JobSeekerProfile`: resume, work history, skills+proficiency, mandatory location.
- Manager job postings in three forms: internship (کارآموزی), rental (اجاره‌ای), fixed-salary.
- Work-seeking requests by proficiency/salary; discovery + filters (urgency, salary, location, space) on both sides.
- Interaction → hire → converts the jobseeker into salon personnel (back into the Artist flow).
- Pay-per-application monetization (per decision #5).
- *New:* entire job-market subsystem.
**Complexity 4 · Risk 3**

### Phase 5 — Accounting & platform monetization
- Per-role accounting: financial flow viewable by date/subject/service/personnel/salon; integer-minor-unit money, double-entry ledger.
- Payroll for fixed-salary staff (correctness-critical — money math).
- Platform monetization: panel/subdomain sales, advertising boxes, ladder/bump (نردبان) with time-based fees, VIP always-top listings, holiday/offer discounts for panel sales.
- SuperAdmin CMS: homepage sliders/gallery, dynamic header/footer menus, blog/news, about/contact, ad boxes.
- *New:* accounting, payroll, ad/ladder/VIP mechanics, CMS.
**Complexity 5 · Risk 5** — money + tax + payouts; sequence carefully.

### Phase 6 — AI features *(final version, per the spec)*
- LLM + recommender for natural-language service search → booking.
- Specialized AI consultation per service category (nails, hair, skin, beauty).
**Complexity 4 · Risk 3** — additive; depends on a rich catalog + history from earlier phases.

---

## 6. Recommended sequence & rationale

Phase 0 → 1 → 2 first and in order: nothing works without the foundation, the configurable catalog, and the booking engine — the spec is explicit that booking is the core. Phase 3 (reviews/profiles/dashboards) makes it usable and trustworthy. Phase 4 (job board) and Phase 5 (accounting/monetization) are parallelizable once 0–3 are solid, with accounting last because of money/tax risk. Phase 6 (AI) is explicitly the final version and rides on the data the rest produces.

---

## 7. How this maps to the existing files

- `SALONOS_AGENT_TODO.md` → Phase 0 foundation.
- `SALONOS_ROLES_BASE_STRUCTURE_TODO.md` → Phase 0 identity — **amend to add the JobSeeker capability** per §2.
- `SALONOS_USER_ROLES_SPEC.md` → still the role blueprint; add `JobSeekerProfile`.
- `SALONOS_UI_DASHBOARD_TODO.md` → folds into Phase 3 dashboards (now five roles, with the spec's filters/reports).
- This file → the master *feature* roadmap above them.

Each phase here can be decomposed into agent-ready cards (the 🟢/🟡/🔴 style) when you're ready to build it — Phase 1's configurable catalog is the natural next one to break down.
