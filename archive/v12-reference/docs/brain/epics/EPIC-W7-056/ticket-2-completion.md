# Ticket 2 Completion — Extract IsCancellableOrderState

**EPIC:** EPIC-W7-056
**Ticket:** T2
**Agent:** v12-p5-ticket
**File:** src/V12_002.SIMA.Lifecycle.cs
**Date:** 2026-06-29

## Summary
Extracted the 5-clause `OrderState` guard from `SweepBrokerOrders` into `private static bool IsCancellableOrderState(Order ord)`.

## Change
- **Added:** `IsCancellableOrderState(Order ord)` helper
- **Replaced:** 5-clause `if(ord.OrderState != ... && ...)` continue block
  with: `if (!IsCancellableOrderState(ord)) continue;` (inside TryCancelV12Order via T7)

## Rationale
The order-state check is a pure state predicate — extracting it makes it independently testable and removes 5 conditions from the parent method's CYC.

## Metrics
| Method | CYC |
|--------|-----|
| IsCancellableOrderState | 5 |

## DNA Compliance
- [x] No lock()
- [x] ASCII-only strings
- [x] static pure predicate
- [x] Zero logic drift
