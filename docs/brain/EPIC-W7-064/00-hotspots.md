# EPIC-W7-064 Hotspot Analysis

**Method:** ResolveFsm_ByScan
**CYC:** 11
**File:** src/V12_002.Symmetry.BracketFSM.cs (lines 209–246)

---

## Overview

`ResolveFsm_ByScan` is the Tier 3 last-resort path in the 3-tier FSM resolution chain orchestrated
by `ResolveFsmFromEvent`. It performs an O(N) linear scan across all entries in the
`_followerBrackets` `ConcurrentDictionary`, filtering by `AccountName` and then matching a given
`orderId` against three order slots per FSM: `StopOrder`, `Targets[0-4]`, and `EntryOrder`.
When a match is found it back-fills `_orderIdToFsmKey` so that the same order resolves in O(1)
on future events, self-healing the lookup map. CYC = 11 confirmed against the source; the count
derives from 2 loop constructs (outer `foreach`, inner `for`), 5 conditional guards, and 3 compound
short-circuit conditions collapsed to single branches by the static-analysis tool.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `ResolveFsmFromEvent` (line 264, same file) — sole call site |
| **Caller chain** | `ProcessBracketEvent` → `ValidateFsmEventPreconditions` → `ResolveFsmFromEvent` → `ResolveFsm_ByScan` |
| **Tier peers** | `ResolveFsm_ByOrderId` (Tier 1, O(1)), `ResolveFsm_BySignalName` (Tier 2, O(1) after parse) |
| **Shared state read** | `_followerBrackets` (ConcurrentDictionary — live, lock-free enumeration) |
| **Shared state written** | `_orderIdToFsmKey` (ConcurrentDictionary — backfill write on every match) |
| **Threading constraint** | Strategy thread only (drained via `DrainAccountMailbox` from `OnBarUpdate`/`TriggerCustomEvent`) |
| **Order slots scanned per FSM** | 7 (1 StopOrder + 5 Targets + 1 EntryOrder) |
| **Performance exposure** | O(N × 7) per unresolved event; degrades with fleet size and `_orderIdToFsmKey` map misses |
| **Risk on change** | Medium — backfill side-effect at lines 221, 230, 240 is load-bearing for map convergence |

**Affected symbol count (blast radius):** 4 symbols directly coupled; 2 shared concurrent state bags.

---

## Top 3 Complexity Drivers

1. **Nested two-level loop with per-slot null-guard chains**
   The outer `foreach` over `_followerBrackets.Values` (CYC +1) contains an inner `for (int i = 0;
   i < 5; i++)` targets loop (CYC +1). Each of the three order-slot checks uses a compound
   null-guard + equality condition (`f.StopOrder != null && f.StopOrder.OrderId == orderId`,
   `f.Targets[i] != null && f.Targets[i].OrderId == orderId`,
   `f.EntryOrder != null && f.EntryOrder.OrderId == orderId`), each contributing +1 CYC per
   compound. The two-loop nesting structure is the single largest contributor (~5 CYC points)
   and is the primary extraction candidate.

2. **AccountName equality filter as outer-loop guard**
   The `if (f.AccountName != accountAlias) continue` guard on line 216 (CYC +1) appears before
   any order-slot checks. Its placement is correct (account-scoped isolation) but forces a full
   dictionary enumeration even when the fleet has many accounts, as there is no per-account index.
   An account-keyed sub-dictionary would eliminate this guard entirely and reduce the scan to
   O(K × 7) where K = brackets for that account only.

3. **Dead-code `foundT` flag with unreachable `break`**
   Lines 225–235 introduce a boolean `foundT` set to `true` inside the targets `for` loop, but the
   only assignment is preceded by `return f` (line 232), making `foundT` permanently `false` and
   the `if (foundT) break` on line 235 unreachable. This adds a spurious branch that inflates CYC
   by +1 and misleads readers about loop exit intent. Removing it is zero-risk and is the minimal
   correctness improvement for Phase 1.

---

## Recommended Extraction Count

**2 helpers recommended; 1 dead-code removal recommended.**

| Action | Detail | CYC reduction |
|---|---|---|
| Extract `MatchOrderInFsm(FollowerBracketFSM f, string orderId)` | Encapsulates the 3-slot scan (Stop → Targets → Entry) with backfill; removes inner loop from parent | −5 CYC from parent |
| Remove `foundT` dead-code flag | Lines 225, 234–235 are unreachable; deletion eliminates 1 spurious branch | −1 CYC |
| (Optional) Extract account filter | Move account-equality guard into a pre-filtered enumerable helper | −1 CYC |

**Post-refactor target CYC for parent:** ≤5 (null guard + foreach + account filter + delegating match call).

---

## Agent Tracking

Agent Name: bob-hotspot-phase0 | Bobcoins Used: 1.0 | Execution Time: ~60s
