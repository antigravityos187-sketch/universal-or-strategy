# EPIC-W7-097 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Epic ID:** EPIC-W7-097
**Method:** `ExecuteRMAEntryV2`
**Source File:** `src/V12_002.SIMA.Execution.cs`
**Generated:** 2026-06-29T01:05:00Z
**Input:** docs/brain/EPIC-W7-097/01-scope-boundary.md

---

## Summary

`ExecuteRMAEntryV2` (lines 686–844, ~158 lines) already delegates its primary concerns to 4 extracted helpers:
- `ValidateRMAEntryGuards` — entry guard validation
- `CalculateRMABracketPrices` — bracket price calculation
- `SubmitLocalRMAEntry` — local account order submission
- `ProcessSingleFleetRMAAccount` — per-fleet-account order dispatch

The method's orchestrator body retains **8 decision points** (CYC ~9 pre-extraction, borderline over the Jane Street threshold of 8). Two targeted extractions reduce the orchestrator to CYC = 8:

1. **`BuildRmaForensicPulseReport`** — extracts the ~17-line cold-path StringBuilder forensic report block per carl_cook `[NoInlining]` cold-path logging rule (0 branches, pure LOC reduction).
2. **`IsEligibleFleetAccount`** — merges two inline fleet-account guard checks (`IsFleetAccount` + `acct == this.Account`) into a single predicate, reducing orchestrator decision points by 1 (CYC −1).

**max_cyc_projected = 8** (orchestrator after both extractions).
**precomputed.json estimated_extractions = 1** (plan adds 1 additional predicate helper for full CYC compliance).

---

## Complexity Drivers

| Driver | Type | Branch Count | Notes |
|---|---|---|---|
| `if (!ValidateRMAEntryGuards(...))` | guard return | +1 | Existing delegated helper |
| Outer `catch (Exception ex)` | exception handler | +1 | Wraps entire execution body |
| Inner `catch (Exception localEx)` | exception handler | +1 | Local submission failure rollback |
| `if (!localSubmitted)` | null-guard | +1 | Abort on null local order |
| `foreach (Account acct in Account.All)` | iteration | +1 | Fleet account iteration |
| `if (!IsFleetAccount(acct)) continue` | fleet guard | +1 | **Extracted to IsEligibleFleetAccount** |
| `if (acct == this.Account) continue` | self-guard | +1 | **Extracted to IsEligibleFleetAccount** |
| `if (ProcessSingleFleetRMAAccount(...))` | result check | +1 | fleetOk++ / fleetSkip++ |
| **Total pre-extraction** | | **CYC ~9** | 1 over threshold |
| **Total post-extraction** | | **CYC 8** | IsEligibleFleetAccount merges 2→1 |

---

## Extraction Plan

| Helper Name | Signature | CYC | Jane Street Attributes | Rationale |
|---|---|---|---|---|
| `BuildRmaForensicPulseReport` | `private void BuildRmaForensicPulseReport(StringBuilder dispatchLog, int fleetOk, int fleetSkip, double setupMs, double localMs, double loopMs, double totalMs)` | 1 | `[MethodImpl(MethodImplOptions.NoInlining)]` | carl_cook: cold-path logging extracted out-of-line; ~17 AppendLine calls, 0 branches; reduces orchestrator LOC by ~20 lines |
| `IsEligibleFleetAccount` | `private bool IsEligibleFleetAccount(Account acct)` | 2 | _(none — hot predicate, trivially inlineable)_ | trading_billions: single responsibility predicate; merges `IsFleetAccount(acct) && acct != this.Account` into one guard, reducing orchestrator CYC by 1 |

---

## max_cyc_projected: 8

| Method | CYC Post-Extraction |
|---|---|
| `ExecuteRMAEntryV2` (orchestrator) | 8 |
| `BuildRmaForensicPulseReport` | 1 |
| `IsEligibleFleetAccount` | 2 |

---

## MCP Evidence

### get_context_bundle (symbol_id: `src/V12_002.SIMA.Execution.cs::V12_002.ExecuteRMAEntryV2#method`)

- **Lines:** 686–844 (~158 lines)
- **Signature:** `private void ExecuteRMAEntryV2(double price, MarketPosition direction, int contracts)`
- **Body confirmed:** Delegates to 4 helpers (ValidateRMAEntryGuards, CalculateRMABracketPrices, SubmitLocalRMAEntry, ProcessSingleFleetRMAAccount). Contains Stopwatch latency instrumentation, inline `StringBuilder(1024)` forensic report block (~17 AppendLine calls), and fleet loop with 2 inline guard checks.
- **No LINQ** — zero-alloc hot path compliant (carl_cook).
- **No lock() blocks** — gjengset compliance confirmed.
- **StringBuilder pre-allocated** with capacity 512/1024 — zero-realloc on hot path.

### get_call_hierarchy (depth=2, direction=both)

- **Callers:** 0 indexed (callers exist upstream but are not in the jcodemunch call graph for this symbol — confirmed 4 callers per Phase 1 analysis).
- **Direct callees (depth=1):** ValidateRMAEntryGuards, CalculateRMABracketPrices, SymmetryGuardBeginDispatch, LogBuffer, SubmitLocalRMAEntry, SymmetryGuardRollbackDispatch, IsFleetAccount, ProcessSingleFleetRMAAccount
- **Depth-2 callees:** MetadataGuardDuplicate, CalculateATRStopDistance, CalculateTargetPrice, GetTargetDistribution, SymmetryNormalizeTradeType, LogBuffer.Format, SymmetryGuardRegisterMasterEntry, SymmetryGuardRegisterFollower, GetStableHash — all in separate files, no cross-concern coupling introduced by the 2 new helpers.

### get_dependency_graph (file: `src/V12_002.SIMA.Execution.cs`, direction=both, depth=1)

- **node_count:** 1, **edge_count:** 0
- File has no indexed import/export edges — partial class pattern (all C# partials compile as one unit). No cross-file dependency impact from the 2 new private helpers.
- Blast radius confirmed: `src/V12_002.SIMA.Execution.cs` only.

---

## Sequential Thinking Evidence

### Thought 1 — Complexity Drivers
Counted 8 actual decision points in the orchestrator body by reading the full source from `get_context_bundle`. The index-reported CYC=0 is a pre-extraction artifact. Actual post-delegation CYC is ~9 (base 1 + 8 branches). The ~17-line forensic StringBuilder block contributes 0 branches — it is a cold-path logging artifact meeting carl_cook's extraction mandate.

### Thought 2 — Extraction Strategy
Defined `BuildRmaForensicPulseReport` as the primary extraction target (cold logging, `[NoInlining]`, CYC=1). Confirmed all Jane Street rules: no lock(), no LINQ, single responsibility per helper. Confirmed alignment with precomputed.json `estimated_extractions=1`. Orchestrator CYC after extraction: still ~9 (branches unchanged), requiring a second extraction.

### Thought 3 — CYC Validation
Identified that extracting `BuildRmaForensicPulseReport` alone does NOT reduce CYC (0 branches removed). To reach CYC=8, a second extraction `IsEligibleFleetAccount(Account acct)` merges the 2 foreach-level guards into 1 predicate call, reducing orchestrator CYC by 1 (9→8). Final plan: 2 extractions, max_cyc_projected=8. Full Jane Street compliance confirmed across carl_cook, gjengset, and trading_billions KB rules.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **MCP Tools Used** | resolve_repo, get_context_bundle, get_call_hierarchy, get_dependency_graph, sequentialthinking (3 thoughts) |
| **Extractions Planned** | 2 |
| **max_cyc_projected** | 8 |
| **Jane Street KB Applied** | carl_cook (NoInlining cold logging), gjengset (no locks), trading_billions (single responsibility, CYC<=8) |
