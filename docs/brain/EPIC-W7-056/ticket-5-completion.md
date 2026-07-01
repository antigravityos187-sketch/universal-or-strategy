# Ticket 5 Completion — Extract IsProtectedBracketOrder

**EPIC:** EPIC-W7-056
**Ticket:** T5
**Agent:** v12-p5-ticket
**File:** src/V12_002.SIMA.Lifecycle.cs
**Date:** 2026-06-29

## Summary
Extracted the `[FIX-FF]` bracket-exclusion logic from `SweepBrokerOrders` into `private static bool IsProtectedBracketOrder(string ordName)`, which delegates to T3 (IsStopSideProtectedPrefix) and T4 (IsTakeProfitProtectedPrefix).

## Change
- **Added:** `IsProtectedBracketOrder(string ordName)` helper
- **Replaced:** inline `bool isBracketOrder = ...` multi-clause OR block + `if(isBracketOrder)` guard
  with: delegation via `!force && IsProtectedBracketOrder(ordName)` inside TryCancelV12Order (T7)

## Rationale
Bracket-order classification is a distinct business rule — isolating it preserves the [FIX-FF] intent
(protect live positions on SIMA soft-disable) without entangling it in the iteration loop.

## Metrics
| Method | CYC |
|--------|-----|
| IsProtectedBracketOrder | 2 |

## DNA Compliance
- [x] No lock()
- [x] ASCII-only strings
- [x] [FIX-FF] comment preserved in method docstring
- [x] static pure predicate
- [x] Zero logic drift
