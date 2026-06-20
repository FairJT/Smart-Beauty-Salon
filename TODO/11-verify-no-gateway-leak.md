# Task 11 — Verify no gateway SDK leaks outside payments 🟢 (check only)

No code change. Just run the check and report.

**Run (PowerShell):**
```powershell
Select-String -Path src\ -Pattern "Zarinpal|Stripe" -Recurse |
  Where-Object { $_.Path -notmatch "Infrastructure\\Payments" }
```

**Done when:** the command returns **0 results** — meaning domain/module code depends only on
`IPaymentProvider`, never on a concrete gateway.

**If there ARE hits:** do NOT fix them yourself. Copy the file/line list and hand it to Claude —
moving gateway code behind the interface needs care.
