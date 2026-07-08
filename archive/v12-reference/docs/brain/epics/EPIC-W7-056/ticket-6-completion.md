# Ticket 6 Completion — Extract HasMatchingV12Prefix

**EPIC:** EPIC-W7-056
**Ticket:** T6
**Agent:** v12-p5-ticket
**File:** src/V12_002.SIMA.Lifecycle.cs
**Date:** 2026-06-29

## Summary
Extracted the `bool isV12 / for-loop / if(!isV12) continue` pattern from `SweepBrokerOrders` into `private static bool HasMatchingV12Prefix(string ordName, string[] prefixes)`.

## Change
- **Added:** `HasMatchingV12Prefix(string ordName, string[] prefixes)` helper
- **Replaced:** `bool isV12 = false; for(...) { ... } if (!isV12) continue;`
  with: `if (!HasMatchingV12Prefix(ordName, prefixes)) return false;` inside TryCancelV12Order (T7)

## Rationale
Prefix matching is a reusable pure operation — extracting it removes the for-loop branch from the parent CYC and enables isolated unit testing of the prefix matching logic.

## Metrics
| Method | CYC |
|--------|-----|
| HasMatchingV12Prefix | 3 |

## DNA Compliance
- [x] No lock()
- [x] ASCII-only strings
- [x] static pure helper
- [x] Zero logic drift
