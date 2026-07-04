# Ticket 7 Completion — Extract TryCancelV12Order

**EPIC:** EPIC-W7-056
**Ticket:** T7
**Agent:** v12-p5-ticket
**File:** src/V12_002.SIMA.Lifecycle.cs
**Date:** 2026-06-29

## Summary
Extracted all per-order validation and cancellation logic into `private static bool TryCancelV12Order(...)`, then refactored `SweepBrokerOrders` to a clean 3-clause delegation loop.

## Change
- **Added:** `TryCancelV12Order(Account acct, Order ord, bool force, string[] prefixes, string instrumentFullName)`
  - Delegates to: `IsCancellableOrderState`, `HasMatchingV12Prefix`, `IsProtectedBracketOrder`
  - Performs the `acct.Cancel(new[] { ord })` call
  - Returns `true` if cancel was successfully issued
- **Refactored:** `SweepBrokerOrders` inner body to:
  ```
  if (TryCancelV12Order(acct, ord, force, prefixes, instrumentFullName))
      brokerCancels++;
  ```

## Rationale
TryCancelV12Order unifies the per-order decision chain into a single testable unit. SweepBrokerOrders becomes a pure iteration coordinator with no embedded business logic.

## Metrics (Final)
| Method | CYC |
|--------|-----|
| SweepBrokerOrders | 6 |
| TryCancelV12Order | 7 |

## DNA Compliance
- [x] No lock()
- [x] ASCII-only strings
- [x] static helper (no closure over instance state except instrumentFullName param)
- [x] Zero logic drift
- [x] [FIX-FF] guard preserved verbatim
