# Ticket 4 Completion — Extract IsTakeProfitProtectedPrefix

**EPIC:** EPIC-W7-056
**Ticket:** T4
**Agent:** v12-p5-ticket
**File:** src/V12_002.SIMA.Lifecycle.cs
**Date:** 2026-06-29

## Summary
Extracted take-profit bracket prefix detection into `private static bool IsTakeProfitProtectedPrefix(string ordName)`.

## Change
- **Added:** `IsTakeProfitProtectedPrefix(string ordName)` helper
  Covers: `T1_`, `T2_`, `T3_`, `T4_`, `T5_` prefixes

## Rationale
Separates take-profit bracket classification from stop-side classification for independent testability.
Used by `IsProtectedBracketOrder` (T5).

## Metrics
| Method | CYC |
|--------|-----|
| IsTakeProfitProtectedPrefix | 5 |

## DNA Compliance
- [x] No lock()
- [x] ASCII-only strings
- [x] static pure predicate
- [x] Zero logic drift
