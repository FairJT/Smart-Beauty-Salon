---
name: payments
description: Use this skill whenever writing or reviewing any code that touches money, payments, payouts, pricing, refunds, payroll amounts, or currency in SalonOS. Use it any time a gateway, provider, Zarinpal, Stripe, webhook, or amount field appears, and whenever a monetary value is stored, displayed, or calculated. The platform must work in Iran now and globally later under sanctions, so payment code that binds to one provider or mishandles money is a serious defect. Apply this even for a small price field.
---

# Payments and money in SalonOS

Two hard constraints shape everything here. First, money bugs are expensive and
often silent. Second, the platform launches in Iran and goes global later, and
because of sanctions Iranian and global payment gateways never overlap. So
payment code must be provider-agnostic from the start.

## Money representation

Store money as an integer in the currency's minor unit, plus an explicit
currency code. Never use floating point. Never assume the currency.

```typescript
type Money = {
  amount: number;   // integer minor units (e.g. 150000 = 1500.00)
  currency: string; // ISO 4217, e.g. "IRR", "USD"
};
```

- All arithmetic on `Money` stays in integers. Round only at display time.
- Two `Money` values can be added or compared only if currencies match.
  Mismatched currency is a thrown error, never a silent coercion.
- Persist `amount` as a database integer (`BigInt` if values can be large, as
  IRR can) and `currency` as a separate column. Never one float column.

A shared `money` utility in `packages/shared` owns add, subtract, multiply by
quantity, format, and the currency-match guard. Use it everywhere. Do not
reinvent money math inside a module.

## The provider abstraction

Domain code never imports a gateway SDK. It depends on one interface.

```typescript
interface PaymentProvider {
  createPayment(input: CreatePaymentInput): Promise<PaymentSession>;
  verifyPayment(reference: string): Promise<PaymentResult>;
  verifyWebhook(payload: unknown, signature: string): WebhookEvent;
}
```

Each real gateway is an adapter implementing this interface:

- Iranian gateways (for example Zarinpal) behind one adapter.
- Global processors (for example Stripe) behind another, added at the global
  phase.

The active provider is chosen by configuration per deployment, injected through
Nest DI. A salon's region determines which adapter serves it. Adding a provider
is a new adapter and a config entry. It never touches booking, catalog, or
payroll code.

```typescript
// CORRECT: domain depends on the interface
constructor(private readonly payments: PaymentProvider) {}

// WRONG: domain bound to a specific gateway
import Zarinpal from "zarinpal-checkout"; // never in a module's service
```

## Idempotency and webhooks

- Every payment-creating operation carries an idempotency key so a retry never
  charges twice.
- Gateway state is authoritative. Treat your own DB as a cache of it. Reconcile
  on webhook and on explicit verify, never assume success from a redirect alone.
- Always verify the webhook signature before acting. An unverified webhook is
  ignored. Webhook handlers must be idempotent: the same event can arrive twice.

## Payouts and payroll

Per-employee payroll amounts are `Money` and follow every rule above. Payouts to
salons or staff also go through a provider interface, never a hardcoded gateway.
Payroll math (bonuses, deductions) is correctness-critical: keep it on Claude,
never offload to the local model, and cover it with tests.

## Review checklist

- Is every monetary value a `Money` (integer minor units + currency), never a float?
- Does all money math go through the shared `money` utility?
- Does domain code depend on `PaymentProvider`, never on a gateway SDK?
- Are payment operations idempotent and webhooks signature-verified?
- Is gateway state treated as authoritative over local state?
