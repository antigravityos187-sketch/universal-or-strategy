# EPIC-W7-143 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T02:00:00Z
**Input:** docs/brain/EPIC-W7-143/01-scope-boundary.md

---

## Target Method

| Field | Value |
|---|---|
| Method | `OnKeyDown` |
| File | `src/V12_002.UI.Callbacks.cs` |
| Line | 391 |
| CYC (baseline) | 3 (precomputed list: 0, jcodemunch confirmed: CYC 3) |
| CYC (target) | ≤ 8 |
| Status | **ALREADY COMPLIANT** |

---

## Complexity Drivers

**None requiring extraction.** `OnKeyDown` is annotated `[Phase7-UI T-A]` indicating prior Wave 7 Phase work has already refactored this method into a Command Pattern dispatcher.

- `_keyCommands` dictionary (pre-allocated, zero-allocation hot path) performs O(1) key lookup
- Method delegates to `HandleRunnerAction` (CYC 6) and `HandleTargetAction` (CYC 6) helpers
- Residual CYC=3 in parent dispatcher is well within the ≤ 8 threshold

---

## Extraction Plan

**No extraction required.** Method already complies with Jane Street CYC ≤ 8 standard.

| # | New Helper | Reason | CYC Projected |
|---|---|---|---|
| — | None | CYC=3 already at threshold | N/A |

---

## Max CYC Projected

| Symbol | CYC |
|---|---|
| `OnKeyDown` (dispatcher) | 3 ✓ |
| `HandleRunnerAction` (existing) | 6 ✓ |
| `HandleTargetAction` (existing) | 6 ✓ |
| **Max** | **6** ✓ ≤ 8 |

---

## Jane Street KB Compliance

| Rule | Application | Status |
|---|---|---|
| `carl_cook`: zero-alloc hot path | `_keyCommands` pre-allocated dictionary, O(1) lookup | ✓ Already applied |
| `gjengset`: no new lock() blocks | No locks present | ✓ |
| `trading_billions`: single responsibility per helper | HandleRunnerAction / HandleTargetAction single-purpose | ✓ Already applied |
| `trading_billions`: CYC ≤ 8 | Max CYC = 6 across all helpers | ✓ |

---

## MCP Evidence

- **resolve_repo**: `universal-or-strategy` indexed, 5147 symbols, fresh
- **search_symbols (full)**: `OnKeyDown` confirmed at line 391, `[Phase7-UI T-A]` annotation, CYC 3 residual dispatcher
- **Symbol summary**: "Command Pattern with O(1) lookup" — zero-alloc architecture already in place

---

## Sequential Thinking Evidence

- **Thought 1**: CYC=0/3 (epiclist sparse vs jcodemunch confirmed 3). Method already has Command Pattern applied from prior Phase 7 work. No extraction needed. Architecture passes as-is.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Phase** | 2 |
| **Bobcoins Used** | 0.5 |
