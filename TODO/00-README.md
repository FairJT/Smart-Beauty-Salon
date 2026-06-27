# Agent Tasks — Role Pages & Dashboards (atomic)

Each file = ONE small change. Do them **in order**. If a "Find (exact)" block is not
found character-for-character, **STOP and report** — do not guess.

## Delegation legend
- 🟢 trivial / safe — agent does it unattended
- 🟡 mechanical but exact — agent does it, you skim the diff
- 🔴 needs judgment / new screen — agent scaffolds, **you review before commit**

---

## Why the app looked like a disaster (root cause)

After login, **every role lands on a placeholder stub with hard-coded fake numbers**.
`main.dart` imports the dashboards from `presentation/pages/generated/…`, which are
23-line `StatelessWidget`s full of `FStat('5','مجموع کاربران')` literals — no provider,
no API, no actions. The **real** dashboards already exist and are wired to providers,
but nothing imports them:

| Role | Lands on now (stub, fake) | Real screen that exists, unused |
|---|---|---|
| SuperAdmin (1) | `generated/admin_dashboard_screen.dart` | `admin/admin_dashboard.dart` (`AdminDashboard`, 459 lines, real actions) |
| SalonManager (2) | `generated/manager_dashboard_screen.dart` | `manager/manager_dashboard_screen.dart` (provider-wired) |
| Artist (3) | `generated/artist_dashboard_screen.dart` | `artist/artist_dashboard_screen.dart` (provider-wired) |
| Client (4) | `/home` (generic browse) | **no client dashboard screen exists** — but provider + model + API do |

Second problem: even the real dashboards are **read-only stat views**. The screens that
let a role *do their job* exist but are **orphaned** (nothing navigates to them):
- `manager/artist_management_screen.dart` — manager adds/edits staff (365 lines, unreachable)
- `artist/artist_schedule_screen.dart` — artist confirm/complete appointments (has the buttons, unreachable)
- `client_home_screen.dart` — client favorites + loyalty (unreachable)

Good news: **the backend already supports nearly everything** (~42 controllers:
artists, contracts, catalog, finance/payouts, leave, inventory, client offers, platform
admin…). So almost all of this is **frontend wiring**, which is exactly what the local
agent is good at.

---

## Order of work

### PHASE A — Wire the real dashboards (the actual "disaster" fix) 🟡
- `01-wire-manager-dashboard.md` — point main.dart at the real manager dashboard
- `02-wire-artist-dashboard.md` — point main.dart at the real artist dashboard
- `03-wire-admin-dashboard.md` — point main.dart at the real admin dashboard
> After Phase A, log in as manager/artist/admin → you see **real data**, not `5 / 120 / 200k`.

### PHASE B — Give the Client a real dashboard 🟡
- `04-create-client-dashboard-screen.md` — create `ClientDashboardScreen` (full file provided)
- `05-route-client-dashboard.md` — add route + send Client there after login

### PHASE C — Surface each role's duties (un-orphan the action screens) 🟡
- `06-manager-quick-actions.md` — manager dashboard → buttons to staff/catalog/finance
- `07-artist-schedule-nav.md` — artist dashboard → button to "my schedule" (confirm/complete)
- `08-client-quick-actions.md` — client dashboard → my bookings / favorites

### PHASE D — Build the genuinely missing screens 🔴 (review each)
- `09-manager-catalog-screen.md` — services/packages CRUD screen (spec + scaffold)
- `10-manager-finance-screen.md` — revenue / payouts / period close (spec + scaffold)
- `11-admin-platform-tabs.md` — tenants / billing / platform config tabs (spec + scaffold)

---

## After each phase
```powershell
cd smart_salon_app
flutter pub get
flutter analyze
```
Fix analyzer errors before moving on. After Phase A, also run the app and log in as
each of the 4 roles to confirm the right dashboard loads.
