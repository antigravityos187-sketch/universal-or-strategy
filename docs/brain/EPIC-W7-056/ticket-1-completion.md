# Ticket 1 Completion — Extract BuildSweepPrefixes

**EPIC:** EPIC-W7-056
**Ticket:** T1
**Agent:** v12-p5-ticket
**File:** src/V12_002.SIMA.Lifecycle.cs
**Date:** 2026-06-29

## Summary
Extracted the ternary prefix-array literal from `SweepBrokerOrders` into a new `private static string[] BuildSweepPrefixes(bool force)` helper.

## Change
- **Added:** `BuildSweepPrefixes(bool force)` before `SweepBrokerOrders`
- **Replaced:** `var v12Prefixes = force ? new[] { ... } : new[] { ... };` (lines 1361-1379)
  with: `string[] prefixes = BuildSweepPrefixes(force);`

## Rationale
Single-responsibility: prefix-list construction is a distinct concern from the sweep loop logic.
Removes a data-initialization branch from the parent method, reducing CYC.

## Metrics
| Method | CYC Before | CYC After |
|--------|-----------|-----------|
| BuildSweepPrefixes | NEW | 1 |
| SweepBrokerOrders | 24 | 23 (partial, further reduced by T2-T7) |

## DNA Compliance
- [x] No lock()
- [x] ASCII-only strings
- [x] static pure helper (zero allocation overhead)
- [x] Zero logic drift
