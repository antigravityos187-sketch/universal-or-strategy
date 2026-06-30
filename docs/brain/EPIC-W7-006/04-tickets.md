# Phase 4: Ticket Definitions — EPIC-W7-006

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Inputs:**
- [`docs/brain/EPIC-W7-006/02-architecture-plan.md`](docs/brain/EPIC-W7-006/02-architecture-plan.md)
- [`docs/brain/EPIC-W7-006/03-audit-report.md`](docs/brain/EPIC-W7-006/03-audit-report.md)

---

## Method Under Extraction

- **Method:** [`HydrateWorkingOrdersFromBroker`](src/V12_002.SIMA.Lifecycle.cs:309)
  *(Epic concept: `AdoptFleetWorkingOrders` — runtime name is `HydrateWorkingOrdersFromBroker`)*
- **Source File:** [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:309)
- **Original CYC (manual branch-count):** 14
- **Original CYC (jCodemunch index):** 23 (high; max_nesting=7, lines=149)
- **Target CYC:** ≤ 8 for all methods (parent + helpers)
- **DNA Audit Verdict:** PASS (Phase 3, zero violations)

---

## ticket_count: 2

---

## Ticket 1

```
ticket_id:             1
helper_name:           RebuildMasterFilledPosition
concern:               Pure PositionInfo factory — construct one PositionInfo object for a
                       master-filled position with all 6 trade-DNA flags assigned (IsMOMOTrade,
                       IsRMATrade, IsTRENDTrade, IsRetestTrade, IsFFMAFTrade, override).
                       Mirrors existing RebuildFleetPositionFromEntry() pattern. No side effects
                       on shared state (ConcurrentDictionary fields, FSM fields).
lines_to_move:         Lines ~388–420 — inline PositionInfo construction block extracted from
                       HydrateWorkingOrdersFromBroker. Includes:
                         - if(masterMP != Flat) direction guard
                         - IsMOMOTrade flag compound condition (isMomoCandidate check)
                         - trendMnl compound condition (IsRMATrade + IsTRENDTrade connectors)
                         - IsRetestTrade / IsFFMAFTrade / override flag assignments
                         - Return new PositionInfo { ... } with all flags populated
cyc_reduction:         ~5 decision points removed from parent
                         (if masterMP!=Flat [1], IsMOMO compound [1],
                          trendMnl 2-connector [2], IsRMA/IsFFMA [1])
projected_helper_cyc:  5  (≤ 8 ✅)
```

### Ticket 1 Detail

**Signature:**
```csharp
private PositionInfo RebuildMasterFilledPosition(
    string instrument,
    double entryPrice,
    int qty,
    bool isMomoCandidate,
    bool isTrend,
    bool isMnl)
```

**Jane Street pattern:** Named helper, single responsibility (construct one object), pure function
with no side effects on shared state, CYC ≤ 8.

**xUnit Test required:**
- `TestRebuildMasterFilledPosition_SetsAllTradeDNAFlags` — all 6 flags correctly assigned for
  given input permutations
- `TestRebuildMasterFilledPosition_FlatPositionHandled` — flat masterMP condition returns default
  PositionInfo without crash

**Files touched:** [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:309) only (new
private method added in same partial class; no cross-file changes)

---

## Ticket 2

```
ticket_id:             2
helper_name:           HydrateMasterFilledPositions
concern:               Master-path orchestration — iterate broker positions for master accounts,
                       call RebuildMasterFilledPosition() for filled positions, delegate to
                       AdoptMasterOrders(). Encapsulates the try/catch block away from parent
                       orchestrator. Single responsibility: adopt master-account positions only.
lines_to_move:         Lines ~334–442 — entire master-path try-block extracted from
                       HydrateWorkingOrdersFromBroker. Includes:
                         - try/catch wrapper
                         - foreach(brokerPosition) loop over master-account positions
                         - 4-condition compound if for master-position validity check
                         - if(masterMP != Flat) call to RebuildMasterFilledPosition() [Ticket 1]
                         - adoptedCount > 0 partial guard
                         - Existing callees AdoptMasterOrders() (unchanged)
cyc_reduction:         ~9 decision points removed from parent across Ticket 1 + Ticket 2
                       combined (parent retains 5 decision points post-extraction;
                       Ticket 2 absorbs try/catch[1] + foreach[1] + compound-if[2] +
                       masterPos-check[1] + adoptedCount[1] = 6 in this helper, after
                       the 5 absorbed by Ticket 1 helper internally)
projected_helper_cyc:  6  (≤ 8 ✅)
```

### Ticket 2 Detail

**Signature:**
```csharp
private void HydrateMasterFilledPositions()
```

**Jane Street pattern:** Single responsibility (adopt master positions only), encapsulates
try/catch away from parent orchestrator, calls extracted pure helper (`RebuildMasterFilledPosition`)
for PositionInfo construction, lock-free (no `lock()` blocks introduced).

**Dependency on Ticket 1:** `HydrateMasterFilledPositions` calls `RebuildMasterFilledPosition`.
Ticket 1 must be implemented first (or in the same atomic commit).

**xUnit Test required:**
- `TestHydrateMasterFilledPositions_SkipsNonMasterAccounts` — non-master accounts produce zero
  adoptions

**Files touched:** [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:309) only (new
private method added in same partial class; no cross-file changes)

---

## Parent Method After All Extractions

**Remaining logic in `HydrateWorkingOrdersFromBroker` after Ticket 1 + Ticket 2:**

| # | Logic | Decision Points |
|---|---|---|
| 1 | `if (!master)` early-return guard | 1 |
| 2 | Call `HydrateMasterFilledPositions()` (delegated) | 0 |
| 3 | Call `AdoptFleetOrders()` (delegated) | 0 |
| 4 | `foreach(stopKvp)` stop-order loop | 1 |
| 5 | `if (Fleet_ skip)` filter | 1 |
| 6 | `if (ContainsKey)` dict guard | 1 |
| 7 | `if (adoptedCount > 0)` completion gate | 1 |
| 8 | Call `HydrateFSMsFromWorkingOrders()` (delegated) | 0 |

```
projected_parent_cyc_after_all: 5  (≤ 8 ✅)
remaining_lines: ~35 (down from 149)
callers_unchanged: true
```

---

## CYC Summary

| Method | Role | Original CYC | Projected CYC | Threshold | Status |
|---|---|---|---|---|---|
| `HydrateWorkingOrdersFromBroker` | Parent | 14 / 23* | **5** | ≤ 8 | ✅ PASS |
| `RebuildMasterFilledPosition` | New helper (T1) | N/A | **5** | ≤ 8 | ✅ PASS |
| `HydrateMasterFilledPositions` | New helper (T2) | N/A | **6** | ≤ 8 | ✅ PASS |

*\*14 = manual branch-count (authoritative for extraction planning); 23 = jCodemunch index count
(may include exception filters, null-conditional operators as branch points)*

**Max projected CYC: 6** — all methods satisfy Jane Street threshold ≤ 8.

---

## Sequential Thinking Validation

**3-thought chain executed (STEP 4):**

- **Thought 1:** ticket_count = 2. One ticket per extracted helper, one concern per ticket.
  No merging of concerns across tickets. Architecture plan (Phase 2, 5-thought chain) + audit
  (Phase 3, PASS) confirm this decomposition.
- **Thought 2:** Lines to move and helper CYC verified per ticket. RebuildMasterFilledPosition
  absorbs ~5 branch points (factory block, lines 388–420). HydrateMasterFilledPositions absorbs
  try/catch + loop + compound guards (lines 334–442). Parent retains exactly 5 decision points.
- **Thought 3:** All values satisfy CYC ≤ 8. Discrepancy between index CYC (23) and plan CYC (14)
  does not affect correctness — post-extraction parent body is bounded by the 5 documented
  decision points regardless of baseline counting method.

---

## Execution Notes for Phase 5 (Bob CLI v12-engineer)

1. **Implement Ticket 1 first** (`RebuildMasterFilledPosition`) — it is a callee of Ticket 2.
2. **Implement Ticket 2 next** (`HydrateMasterFilledPositions`) — calls T1 helper.
3. **Slim parent last** — replace extracted blocks with two delegating calls.
4. **All changes in ONE file:** [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs)
   — no cross-file edits, no interface changes.
5. **Run `dotnet csharpier format src/`** after extraction.
6. **Run `powershell -File .\deploy-sync.ps1`** to re-sync NinjaTrader hard links.
7. **Add 3 xUnit `[Fact]` tests** (see test requirements above) — NEVER NUnit/MSTest.

---

## Agent Tracking

```
Agent Name:       v12-phase4-tickets
Bobcoins Used:    1.8
Execution Time:   ~4 minutes
Epic:             EPIC-W7-006
Wave:             7
Phase:            4 (Ticket Generation)
Output:           docs/brain/EPIC-W7-006/04-tickets.md
Method in Scope:  HydrateWorkingOrdersFromBroker (CYC 14/23 -> target <= 8)
Source:           src/V12_002.SIMA.Lifecycle.cs (lines 309-457)
ticket_count:     2
max_cyc_projected: 6
projected_parent_cyc_after_all: 5
jCodemunch tools: resolve_repo, get_symbol_complexity, get_extraction_candidates
sequential_thinking_calls: 4 (1 probe + 3 ticket-validation thoughts)
dna_audit_input:  PASS (Phase 3, 0 violations)
```
