# Phase 4: Ticket Definitions — EPIC-W7-140

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-140/02-architecture-plan.md + docs/brain/EPIC-W7-140/03-audit-report.md

---

## Method Under Extraction

| Field | Value |
|---|---|
| **Method** | `InitiateStopReplacement` |
| **Source File** | `src/V12_002.Trailing.StopUpdate.cs` |
| **Lines** | 307–369 |
| **Original CYC** | 10 (manual static; tool-reported 0 — not complexity-indexed) |
| **Target CYC** | <= 8 (Jane Street strict) |

---

## ticket_count: 3

---

## Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | T1 |
| **helper_name** | `TrySnapshotReplacementTargets` |
| **concern** | Iterate `_tB = 1..5`, call `GetTargetOrdersDictionary` for each level, apply 4-clause compound null+state guard (non-null, not terminal, OrderAction check, existing-key exclusion), accumulate matched `(Order, Dictionary<int,Order>)` tuples into output list; return `false` early if no targets found |
| **lines_to_move** | Lines 317–336 — the `for (_tB = 1; _tB <= 5; _tB++)` loop body including the compound-guard `if` block and the tuple accumulation into `snapshot` |
| **signature** | `private bool TrySnapshotReplacementTargets(string entryName, out List<(Order order, Dictionary<int, Order> targets)> snapshot)` |
| **cyc_reduction** | ~4 (loop iteration branch + 4-clause compound AND guard = 4 decision points removed from parent) |
| **projected_helper_cyc** | 5 |
| **cyc_constraint_met** | YES — 5 <= 8 ✅ |
| **jane_street_rule** | Extract Loop Body + Extract Guard Clauses; Single-Responsibility (snapshot-only concern) |
| **notes** | "Replacement" noun in name prevents name clash with any `UpdateStopOrder` helper extracted by EPIC-W7-051 in same file. Returns `bool` so caller can early-return when snapshot is empty without additional null/count check in parent. |

---

## Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | T2 |
| **helper_name** | `TryEnqueuePendingReplacement` |
| **concern** | Build `PendingReplacement` record from parameters, attempt `ConcurrentDictionary.TryAdd` to `pendingStopReplacements`, increment atomic counter via `Interlocked.Increment`, activate circuit-breaker if counter exceeds threshold; return `bool` surfacing duplicate-key path previously silently swallowed |
| **lines_to_move** | Lines 351–360 — `PendingReplacement` record construction, `pendingStopReplacements.TryAdd(entryName, ...)`, `Interlocked.Increment(ref _pendingCount)`, nested `if (_pendingCount >= CIRCUIT_THRESHOLD)` activation block |
| **signature** | `private bool TryEnqueuePendingReplacement(string entryName, PositionInfo pos, Order currentStop, double validatedStopPrice, int newTrailLevel)` |
| **cyc_reduction** | ~3 (TryAdd return-branch + circuit-breaker threshold comparison + Interlocked path branch = 3 decision points removed from parent) |
| **projected_helper_cyc** | 3 |
| **cyc_constraint_met** | YES — 3 <= 8 ✅ |
| **jane_street_rule** | Extract Named Helper Methods + FSM Decomposition (circuit-breaker as explicit state transition); Actor/Enqueue mandate satisfied — uses `ConcurrentDictionary.TryAdd` + `Interlocked.Increment`, zero `lock()` blocks |
| **notes** | "TryEnqueue" prefix aligns with Jane Street Actor/Enqueue pattern. Returning `bool` makes the duplicate-key path explicit; callers can log or handle the collision rather than silently losing the enqueue. "PendingReplacement" matches the `ConcurrentDictionary` key type in the class. |

---

## Ticket 3

| Field | Value |
|---|---|
| **ticket_id** | T3 |
| **helper_name** | `FormatTrailLevelName` |
| **concern** | Resolve integer trail level to human-readable display string: level <= 0 → `"Initial"`, level == 1 → `"BE"`, level >= 2 → `"T" + (level - 1)`; pure stateless mapping with zero side effects |
| **lines_to_move** | Line 367 — nested ternary expression `level <= 0 ? "Initial" : level == 1 ? "BE" : "T" + (level - 1)` currently inlined into `LogBuffer.Format(...)` call |
| **signature** | `private static string FormatTrailLevelName(int level)` |
| **cyc_reduction** | ~2 (2 ternary branch points removed from parent's inline expression) |
| **projected_helper_cyc** | 2 |
| **cyc_constraint_met** | YES — 2 <= 8 ✅ |
| **jane_street_rule** | Extract Named Helper Methods + Replace Nested Ternary with Named Function; `static` modifier enforces statelessness and zero allocation overhead |
| **notes** | `private static` makes statelessness explicit and prevents accidental field capture. Pattern also present in `CreateDirectStopOrder` (line 454) — that call site is OUT OF SCOPE for this epic but may call `FormatTrailLevelName` as a future deduplication; do NOT modify line 454 in this epic. All string literals (`"Initial"`, `"BE"`, `"T"`) are ASCII-only (DNA check PASS). |

---

## Projected Parent CYC After All Extractions

| Metric | Value |
|---|---|
| **projected_parent_cyc_after_all** | 5 (conservative upper bound; actual likely 3) |
| **constraint_met** | YES — 5 <= 8 ✅ |
| **remaining_branches_in_parent** | Early-return guard on `currentStop.OrderState` validity (1), empty-snapshot early-return (1), `foreach` loop over snapshot entries dispatching `CancelOrderForReplace` + `MarkStickyDirty` (1), `pos` state update (0), `TryEnqueuePendingReplacement` call with result check (1), `LogBuffer.Format` call (0) |

---

## Extraction Verification Matrix

| Helper | Projected CYC | <= 8? | DNA PASS? |
|---|---|---|---|
| `TrySnapshotReplacementTargets` | 5 | ✅ | ✅ |
| `TryEnqueuePendingReplacement` | 3 | ✅ | ✅ |
| `FormatTrailLevelName` | 2 | ✅ | ✅ |
| Parent `InitiateStopReplacement` (post-extraction) | 5 | ✅ | ✅ |
| **max_cyc_projected** | **5** | ✅ | ✅ |

---

## jcodemunch MCP Evidence

| Tool | Result |
|---|---|
| `resolve_repo` | `found=true`, `indexed=true`, `repo=antigravityos187-sketch/universal-or-strategy` |
| `get_symbol_complexity(InitiateStopReplacement)` | `error: Symbol not found in index` — consistent with Phase 2 note (complexity not indexed); manual CYC=10 from 00-hotspots.md is authoritative |
| `get_extraction_candidates(src/V12_002.Trailing.StopUpdate.cs)` | `candidates=[]` — consistent with tool-reported complexity=0 indexing gap; extraction plan from Phase 2 manual static analysis is authoritative |

---

## Sequential Thinking Summary

**Thought 1 — Ticket count determination:** Three distinct complexity clusters map to three concerns → `ticket_count = 3`. Each cluster maps to exactly one helper owning one responsibility.

**Thought 2 — Per-ticket detail:** Lines, helper names, signatures, and CYC contributions confirmed from 02-architecture-plan.md. T1 removes ~4 CYC (loop + 4-clause guard), T2 removes ~3 CYC (TryAdd path + circuit-breaker), T3 removes ~2 CYC (nested ternary). Total CYC removed from parent: ~9 → parent 10 - 9 + orchestration = 3–5.

**Thought 3 — CYC constraint verification:** All helpers (5, 3, 2) and parent (5) satisfy <= 8. DNA conformance: no lock(), ASCII-only, no scope creep, xUnit protocol, EPIC-W7-051 name conflict mitigated.

**Thought 4 — Final confirmation:** All phase 0-3 data consistent. ticket_count=3 is valid. Plan approved.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | ~12 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **Wave** | 7 |
| **Phase** | 4 |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 5 (1 probe + 4 analysis) |
| **Input: 02-architecture-plan.md** | extraction_count=3, max_cyc_projected=5, boundary_verdict=PASS |
| **Input: 03-audit-report.md** | dna_verdict=PASS, violations=[] |
| **Output** | docs/brain/EPIC-W7-140/04-tickets.md |
