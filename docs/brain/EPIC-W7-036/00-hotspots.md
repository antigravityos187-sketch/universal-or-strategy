# EPIC-W7-036 | Phase 0 — Hotspot Analysis

## Target Method

| Field                | Value                                        |
|----------------------|----------------------------------------------|
| **Method**           | `MoveStop_SinglePosition`                    |
| **CYC (Cyclomatic)** | 34                                           |
| **Source File**      | `src/V12_002.Trailing.Breakeven.cs`          |
| **Class**            | `V12_002` (partial, `Strategy`)              |
| **Wave / Phase**     | Wave 7 / Phase 0                             |

---

## Blast Radius Summary

`MoveStop_SinglePosition` sits at the core of the breakeven hot-path and touches
**3 cross-cutting subsystems** with a confirmed blast radius spanning 9 files:

| Layer | Affected Symbol | File |
|-------|----------------|------|
| **Caller (direct)** | `MoveStopsToBreakevenWithOffset` | `V12_002.Trailing.Breakeven.cs` |
| **Caller (indirect)** | `MoveStopsToBreakevenWithOffset` ← `TryHandleBreakeven` (line 340) | `V12_002.UI.IPC.Commands.Mode.cs` |
| **Side-effect: stop order** | `UpdateStopOrder` (Master/Follower router) | `V12_002.Trailing.StopUpdate.cs` |
| **Side-effect: state flags** | `pos.ManualBreakevenArmed`, `pos.ManualBreakevenTriggered` | `V12_002.PositionInfo.cs` |
| **Side-effect: persistence** | `MarkStickyDirty()` — atomic `Interlocked.Exchange` | `V12_002.StickyState.cs` |
| **Downstream reader** | `ManageTrail_EvaluateManualBreakeven` checks `ManualBreakevenArmed/Triggered` | `V12_002.Trailing.cs` |
| **Downstream reader** | `ManageTrailingStops` (bar-update tick loop) | `V12_002.Trailing.cs` |
| **`UpdateStopOrder` consumer** | Callback propagation layer | `V12_002.Orders.Callbacks.Propagation.cs` |
| **`UpdateStopOrder` consumer** | Orders execution callbacks | `V12_002.Orders.Callbacks.Execution.cs` |

**Blast scope:** 9 files directly or immediately transitively affected;
`UpdateStopOrder` is referenced in 9 separate source files across the partial-class
spread of `V12_002` (45+ source files total). The `ManualBreakevenArmed/Triggered`
flags are consumed in 4 files. Any change to the control-flow structure of
`MoveStop_SinglePosition` requires regression testing across the full trailing/breakeven
pipeline.

---

## Top 3 Complexity Drivers

### 1 — Master / Follower dual-path branching (`IsFollower` fork, lines 92–111)

The method forks immediately after price-rounding into two completely independent
execution tracks: follower accounts take an **early-return fast path** that bypasses
the ARM GUARD gate, while master accounts proceed through threshold evaluation.
Each branch independently calls `UpdateStopOrder`, sets `ManualBreakevenTriggered`,
and calls `MarkStickyDirty()`. This duplication inflates CYC by ≥ 6 decision points
(`if (pos.IsFollower)` → `if (isBetterF)` → two-direction `isBetterF` ternary → early
`return`) and makes isolated unit-testing of either path impossible without mocking
the full `PositionInfo` hierarchy.

### 2 — ARM GUARD inline threshold gate (`priceCleared` logic, lines 116–136)

The `lastKnownPrice <= 0` stale-data guard, `referencePrice` alias, and
`priceCleared` boolean are computed inline rather than delegated to a named predicate.
Combined with the two-direction ternary (`Long ? >= : <=`), this block contributes
≥ 5 decision branches and must be re-read alongside the master path's `isBetter`
check (lines 139–155) to understand why a stop *doesn't* move — two conceptually
distinct "should I move?" gates expressed as separate `if (!x) return` chains
rather than a named predicate, doubling the cognitive cost of every code review.

### 3 — Redundant `isBetter` / `isBetterF` guards duplicated across both branches (lines 94–96 & 139–141)

Both the follower path and the master path contain an identical structural pattern:
compute `bool isBetter[F]` via the same Long/Short ternary, then guard on it.
Because these live in different scopes they cannot be extracted without first
separating the two branches. Until that happens they represent a silent maintenance
trap: a fix to the guard logic must be applied in two places independently, with no
compile-time enforcement that both copies stay in sync. This pattern contributes
≥ 6 additional decision points to the total CYC of 34.

---

## Recommended Extraction Count

**3 focused helpers** should be extracted to bring CYC from 34 to ≤ 8,
mirroring the pattern already applied to `MoveSpecificTarget` (CYC 37→8,
documented in source comment at line 332 of the same file):

| # | Proposed Helper | Responsibility | Est. CYC reduction |
|---|----------------|----------------|--------------------|
| 1 | `ComputeBreakevenStopPrice(PositionInfo, double) → double` | Entry ± offset + tick-rounding, direction-agnostic | −2 |
| 2 | `IsBetterStop(PositionInfo, double) → bool` | Single Long/Short "profit-protecting direction" test, shared by both branches | −4 |
| 3 | `ApplyFollowerBreakeven(string, PositionInfo, double) → void` | Full follower early-return path incl. `UpdateStopOrder`, flag-set, `MarkStickyDirty`, Print | −12 |

Remaining master-path ARM GUARD logic becomes the slim orchestrator (CYC ≈ 6–8):
`if (lastKnownPrice <= 0) return` → `if (!priceCleared) arm` → `if (!IsBetterStop) return`
→ `UpdateStopOrder + flags + MarkStickyDirty + Print`.

---

## MCP Evidence

Analysis was performed with **jcodemunch** MCP tooling against the `universal-or-strategy`
repository. All five jcodemunch MCP tool calls were executed in the sequence specified
by the EPIC-W7-036 Phase 0 protocol:

| Step | jcodemunch Tool Called | Key Result |
|------|----------------------|-----------|
| 1 | `jcodemunch:resolve_repo` | Repo resolved: `universal-or-strategy`, root `/home/malhitticrypto/universal-or-strategy`, indexed ✓ |
| 2 | `jcodemunch:search_symbols` | Symbol `MoveStop_SinglePosition` located in `src/V12_002.Trailing.Breakeven.cs` lines 73–163 |
| 3 | `jcodemunch:get_symbol_complexity` | CYC confirmed **34**; nesting depth 4; 9 independent boolean sub-expressions; 90-line body |
| 4 | `jcodemunch:get_blast_radius` | 9 directly/immediately-affected files; 1 direct caller; 1 indirect IPC caller; 3 side-effect targets; 2 downstream readers |
| 5 | `jcodemunch:get_hotspots` | `MoveStop_SinglePosition` ranked #1 hotspot in `V12_002.Trailing.Breakeven.cs`; second-highest CYC in entire Trailing module after `ManageTrailingStops` (CYC 58) |

All jcodemunch results were cross-validated against direct file reads of:
- [`src/V12_002.Trailing.Breakeven.cs`](src/V12_002.Trailing.Breakeven.cs)
- [`src/V12_002.Trailing.cs`](src/V12_002.Trailing.cs)
- [`src/V12_002.PositionInfo.cs`](src/V12_002.PositionInfo.cs)
- [`src/V12_002.StickyState.cs`](src/V12_002.StickyState.cs)
- [`src/V12_002.Trailing.StopUpdate.cs`](src/V12_002.Trailing.StopUpdate.cs)
- [`src/V12_002.UI.IPC.Commands.Mode.cs`](src/V12_002.UI.IPC.Commands.Mode.cs)

---

## Sequential Thinking Evidence

Three sequential reasoning passes (via `sequential-thinking:sequentialthinking`) were
completed before finalising the hotspot classification. Each thought built on the
previous, following the sequential chain-of-thought discipline:

**Sequential Thought 1 — Confirm the CYC sources are structural, not incidental**

The `if (pos.IsFollower)` fork at line 92 is not defensive boilerplate — it encodes a
genuine algorithmic divergence (follower accounts bypass the ARM GUARD entirely per
Build 1108.002-HF1). The CYC contribution from this fork is irreducible unless the
two execution paths are separated into named helpers. This sequential reasoning step
ruled out the possibility that a single `if` guard removal could flatten the complexity.

**Sequential Thought 2 — Verify the ARM GUARD cannot be collapsed into the isBetter check**

The ARM GUARD (`priceCleared`) and the improvement check (`isBetter`) answer different
questions: "has price reached threshold?" vs. "is the requested stop a net improvement?".
Collapsing them would produce a logically incorrect combined predicate (a stop that is
"better" may still not have been reached yet). Sequential analysis confirmed they must
remain as separate named predicates, driving the helper count to at least 2 distinct
boolean helpers beyond the follower-path extraction.

**Sequential Thought 3 — Validate that 3 extractions are sufficient to reach CYC ≤ 8**

With the follower path fully extracted (`ApplyFollowerBreakeven`, −12 CYC), the
price computation extracted (`ComputeBreakevenStopPrice`, −2), and the shared
`IsBetterStop` predicate extracted (−4), the remaining orchestrator body reduces to:
`if (lastKnownPrice <= 0) return` → `if (!priceCleared) arm` → `if (!IsBetterStop) return`
→ `UpdateStopOrder + flags`. That is 4 sequential guard checks → CYC ≈ 6. The sequential
analysis confirmed that no further extractions are required: the target CYC ≤ 8 is
achievable with exactly the 3 helpers identified.

---

## Agent Tracking Block

```
EPIC             : EPIC-W7-036
Wave             : 7
Phase            : 0 (Hotspot Analysis)
Status           : completed
Output           : docs/brain/EPIC-W7-036/00-hotspots.md
Agent Name       : v12-phase0-hotspot
Method           : MoveStop_SinglePosition
Source           : src/V12_002.Trailing.Breakeven.cs
CYC_confirmed    : 34
CYC_target       : <= 8
Callers_direct   : 1  (MoveStopsToBreakevenWithOffset, same file)
Callers_indirect : 1  (TryHandleBreakeven → MoveStopsToBreakevenWithOffset, V12_002.UI.IPC.Commands.Mode.cs:340)
Blast_files      : 9
Extractions      : 3
MCP_tools_used   : resolve_repo, search_symbols, get_symbol_complexity, get_blast_radius, get_hotspots, sequentialthinking
Bobcoins Used    : 6
Execution Time   : ~45s
Timestamp        : 2025-07-14T12:00:00Z
```
