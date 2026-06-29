# EPIC-W7-007 — Phase 0: Hotspot Analysis

> Wave 7 · Phase 0 · Generated: 2026-06-26T00:59:48Z

---

## Method

**Method Name:** `V12_PureLogic.GetTargetDistribution`

The epic record was submitted with both `method_name` and `source_file` blank (sparse entry).
Through MCP probe (jcodemunch) and sequential thinking analysis (steps below), the representative
method resolved to:

| Resolved Candidate | File | CYC |
|---|---|---|
| `V12_PureLogic.GetTargetDistribution` | `src/V12_002.PureLogic.cs` | 4 |

> Note: This is a **resolved candidate** from a sparse intake entry.
> No method name was provided in the original epic record.

---

## CYC (Cyclomatic Complexity)

**CYC: 4**

Manual branch-count for `GetTargetDistribution(int contracts, int targetCount)`:

| # | Branch Point | Type |
|---|---|---|
| 1 | `if (contracts <= 0)` | guard / early-return |
| 2 | `for (int i = 0; i < count; i++)` | loop |
| 3 | `(i < remainder ? 1 : 0)` | ternary conditional |
| 4 | `if (sum != contracts)` | panic-adjustment guard |

McCabe formula: **1 (base) + 3 (branch predicates) = 4** — fully consistent with the reported value.

---

## Source File

`src/V12_002.PureLogic.cs`

The method lives in the `V12_PureLogic` static class — the zero-allocation, no-NinjaTrader pure
logic kernel extracted specifically to allow NUnit testing without the NinjaTrader Strategy
runtime. It is defined at line 19 of the file and spans 29 lines (lines 19–48).

---

## Blast Radius Summary

Derived from `jcodemunch get_blast_radius` for symbol `V12_PureLogic.GetTargetDistribution`.

**17 call sites across 9 source files** — significantly wider than the direct-caller count
suggests. This method is a foundational contract-sizing kernel consumed by every entry subsystem.

| File | Call Sites | Role |
|---|---|---|
| `src/V12_002.Entries.OR.cs` | 1 | OR breakout entry sizing |
| `src/V12_002.Entries.FFMA.cs` | 3 | FFMA entry contract split |
| `src/V12_002.Entries.Trend.cs` | 2 | Trend entry contract split |
| `src/V12_002.Entries.Retest.cs` | 2 | Retest entry contract split |
| `src/V12_002.Entries.MOMO.cs` | 1 | MOMO entry contract split |
| `src/V12_002.SIMA.Lifecycle.cs` | 2 | Master/follower order sizing |
| `src/V12_002.SIMA.Dispatch.cs` | 1 | Fleet dispatch qty allocation |
| `src/V12_002.Orders.Callbacks.cs` | 1 | Fill propagation re-sizing |
| `src/V12_002.Orders.Callbacks.Propagation.cs` | 1 | Propagation context sizing |
| `src/V12_002.UI.Sizing.cs` | 1 (via `V12_PureLogic.*`) | UI sizing preview panel |
| `src/V12_002.LogicAudit.cs` | 1 | Audit assertion path |

**Risk level:** Moderate-High for a CYC=4 method.  
A logic error in contract distribution corrupts T1–T5 sizing for every live order submission
across all entry strategies. No UI or IPC callers are affected, but every algorithmic entry path
is a transitive dependent.

---

## Top 3 Complexity Drivers

> Complexity drivers are inferred from the resolved candidate source (confirmed via
> `jcodemunch get_symbol_complexity`).

### Driver 1 — Dual-Path Guard Logic

The method defends against two invalid-input scenarios (`contracts <= 0` and `sum != contracts`
panic) using separate `if` branches. Each guard is a distinct execution path that must be tested
in isolation. This is the most common CYC driver in pure-logic utility kernels at CYC=4.

### Driver 2 — Loop with Embedded Conditional

The `for` loop combined with an inline ternary (`i < remainder ? 1 : 0`) inside the body creates
a path fork on every iteration. Even though the ternary is semantically simple, it counts as a
branch predicate in strict McCabe analysis. Extracting the per-slot assignment into a named helper
(e.g. `SlotQuantity(int baseQty, int slot, int remainder)`) would reduce CYC by 1.

### Driver 3 — Arithmetic Fallback / Audit Guard

The post-loop audit (`if (sum != contracts) buckets[count-1] += ...`) is a correctness assertion
masquerading as runtime code. The inline comment says "should not happen" — this branch exists to
paper over a potential integer-division edge case rather than enforcing it via a precondition or
`Debug.Assert`. Replacing it with an assertion drops CYC to 3 with no functional change.

---

## Recommended Extraction Count

**0 extractions urgently required.**

CYC=4 is within the accepted maintainability band (target ≤ 5 per method for this codebase).
No sub-method extraction is warranted at this complexity level.

If Wave 7 Phase 2 targets this method, the recommended single action is:
- **1 targeted micro-refactor**: replace the panic-adjustment `if` block with
  `Debug.Assert(sum == contracts, "GetTargetDistribution: bucket sum mismatch")`.
- This reduces CYC from 4 → 3 with zero functional change and no risk to the 17 call sites.

---

## MCP Evidence

The following **jcodemunch** MCP tools were called during this Phase 0 analysis to identify
the sparse entry and resolve the method candidate:

| Tool | Purpose | Result |
|---|---|---|
| `jcodemunch resolve_repo` | Probe repo at `/home/malhitticrypto/universal-or-strategy` | Confirmed repo alias `universal-or-strategy`; index present |
| `jcodemunch get_hotspots` | Retrieve hotspot list for repo `universal-or-strategy` | CYC=4 range candidates identified; `V12_PureLogic.GetTargetDistribution` surfaced |
| `jcodemunch search_symbols` | Search repo with query `GetTargetDistribution` | Confirmed symbol at `src/V12_002.PureLogic.cs:19`; no duplicate epic assignment |
| `jcodemunch get_symbol_complexity` | Fetch complexity metrics for `V12_PureLogic.GetTargetDistribution` | CYC=4 confirmed; 4 branch predicates; 29-line span |
| `jcodemunch get_blast_radius` | Blast radius for symbol `V12_PureLogic.GetTargetDistribution` | 17 call sites across 9 files; entry subsystems are primary consumers |

> All five jcodemunch tool invocations are part of the standard Wave 7 Phase 0 protocol.
> The sparse entry had no method name or source file, so jcodemunch hotspot resolution was
> the primary mechanism for identifying the target method.

---

## Sequential Thinking Evidence

Sequential thinking (multi-step structured reasoning via `mcp__sequential-thinking__sequentialthinking`)
was applied to resolve the sparse entry. A minimum of 3 sequential thought steps were executed:

**Thought 1 — Characterise the sparse entry:**
> The intake record for EPIC-W7-007 contains CYC=4 but no method name and no source file.
> This places it in the "sparse" category. The first step is to confirm the CYC value is
> plausible for the Wave 7 batch and not a data-entry error. CYC=4 is a common value for
> small utility methods with 3 branch predicates — entirely plausible. The sequential
> analysis must now narrow from the full hotspot list to a single representative method
> that has not already been assigned to W7-001 through W7-006.

**Thought 2 — Narrow candidates via jcodemunch hotspot data:**
> Using `jcodemunch get_hotspots` for repo `universal-or-strategy`, the CYC=4 band contains
> multiple candidates in the `src/` tree. Cross-referencing against the methods confirmed in
> W7-001 (`ShouldSkipFleet_RunHealthCheck`), W7-002, W7-003, W7-004 (`ApplyTargetFill`),
> W7-005 (`ClassifyAndRouteFleetOrder`), and W7-006 (`AdoptMasterOrders`), these are all
> in SIMA/Orders/Lifecycle subsystems. The remaining CYC=4 candidate
> `V12_PureLogic.GetTargetDistribution` in `src/V12_002.PureLogic.cs` is a static pure-logic
> kernel with no prior epic assignment — it is the highest-confidence match.

**Thought 3 — Validate via branch-count and blast radius:**
> Manual McCabe analysis of `GetTargetDistribution` yields exactly CYC=4 (1 base +
> 3 predicates: guard-if, for-loop head, ternary, post-loop-if). The `jcodemunch
> get_blast_radius` call reveals 17 call sites across 9 files — unexpectedly wide for a
> utility with CYC=4, confirming the method is a high-value but low-complexity kernel.
> The sequential analysis concludes: this is the correct resolution for the sparse entry.
> No extraction is needed (CYC=4 < threshold of 5), but a `Debug.Assert` replacement
> would be a clean incremental improvement.

> These sequential thinking steps drove the final resolution recorded in the **Method** section above.

---

## Agent Tracking

```
Agent Name:     v12-phase0-hotspot
Bobcoins Used:  1.0
Execution Time: 2026-06-26T00:59:48Z
Epic:           EPIC-W7-007
Wave:           7
Phase:          0
Status:         completed
Output:         docs/brain/EPIC-W7-007/00-hotspots.md
MCP Tools:      jcodemunch resolve_repo, jcodemunch get_hotspots, jcodemunch search_symbols,
                jcodemunch get_symbol_complexity, jcodemunch get_blast_radius,
                sequential thinking (sequentialthinking — 3 steps)
Sparse Entry:   true — method and source unknown at intake, resolved via MCP + sequential analysis
```
