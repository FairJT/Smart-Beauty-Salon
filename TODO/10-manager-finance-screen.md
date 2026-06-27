# 10 — Manager: finance (revenue / payouts / period close) 🔴 (review before commit)

New screen, **money-sensitive — review carefully**. Read the `payments` skill rules first.

**Backend already exists:**
- `FinanceController` (`/finance`, 3 endpoints) — revenue, deposits, period close.
- `StaffServiceContractController` / `ArtistContractController` — payout figures are
  contract-dependent (Salaried artists see NO revenue; chair/room renters see their own).

**Create:** `smart_salon_app/lib/presentation/pages/manager/finance_screen.dart`
- Class `FinanceScreen`, `ConsumerStatefulWidget`.
- Tabs or sections: (a) revenue summary, (b) artist payouts, (c) close period button.
- Display every amount with `MoneyFormatter.format` over **integer minor units**.
- "بستن دوره مالی" (period close) must use a confirm dialog — it is irreversible.

**Then wire it** into the manager dashboard "مدیریت سالن" card: a `ListTile`
("امور مالی", icon `Icons.account_balance_wallet_outlined`) → `const FinanceScreen()`.

**Guardrails (do not skip):**
- Respect contract-aware visibility: a Salaried artist's payout row shows ratings /
  completed count only, never revenue (§R5 of `SalonOS_Access_Control_Design.md`).
- Authorize on the server, not the client — the screen just calls the API and renders
  what it returns. Do not hide/show data purely client-side as a security measure.
- No `tenantId` from the client. No float money. If a response can't be parsed as integer
  minor units, STOP and report rather than guessing a divisor.

**Done when:** a SalonManager sees their own revenue and per-artist payouts (contract-aware)
and can close a finance period with confirmation.
