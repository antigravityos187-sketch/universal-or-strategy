# EPIC-W7-144 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T02:00:00Z
**Input:** docs/brain/EPIC-W7-144/01-scope-boundary.md

---

## Target Method

| Field | Value |
|---|---|
| Method | `IsOrderAllowed` |
| File | `src/V12_002.UI.Compliance.cs` |
| Line | 323–389 |
| CYC (baseline) | 20 |
| CYC (target) | ≤ 8 |
| Direct Callers | 0 jcodemunch-resolved (11 per Phase 1 scope — called from entry methods) |

---

## Complexity Drivers

1. **Feature flag + account resolution preamble** — `!EnableComplianceHub` + `IsNullOrEmpty(acctName)` = 2 branches
2. **Trailing drawdown compound guard** — `TryGetValue && peak>0 && TrailingDrawdownLimit>0` = 3 short-circuit branches, plus `currentAccount!=null` null guard = +1
3. **Balance retrieval try/catch** — try block entry (+1) + catch exception handler (+1) = 2 branches
4. **Buffer check + inline Print logging** — `buffer<=0` + string.Format Print = +1 + cold-logging allocation
5. **Daily profit cap compound guard** — `EnableSIMA && EnableConsistencyLock` = 2 branches, `TryGetValue && MaxDailyProfitCap>0 && dp>=MaxDailyProfitCap` = 3 short-circuit branches + cold Print log

Total CYC driver sum: ~14–20 (audit tool counts each `&&` operand independently, yielding CYC=20)

---

## Extraction Plan

| # | New Helper | Responsibility | CYC Projected | Modifier |
|---|---|---|---|---|
| 1 | `CheckTrailingDrawdown(string acctName)` | TryGetValue peak check + account balance retrieval + try/catch + buffer evaluation | 8 | `private` |
| 2 | `CheckDailyProfitCap(string acctName)` | SIMA+ConsistencyLock gate + TryGetValue daily profit + cap comparison | 6 | `private` |
| 3 | `LogComplianceBlock(string blockType, string acctName, double value)` | Cold Print/string.Format diagnostic logging | 1 | `[MethodImpl(NoInlining)]` |

**Parent after extraction:** CYC ≤ 5

---

## Max CYC Projected

| Symbol | CYC Projected |
|---|---|
| `IsOrderAllowed` (parent) | 5 |
| `CheckTrailingDrawdown` | 8 |
| `CheckDailyProfitCap` | 6 |
| `LogComplianceBlock` | 1 |
| **Max** | **8** ✓ ≤ 8 |

---

## Jane Street KB Compliance

| Rule | Application | Status |
|---|---|---|
| `carl_cook`: extract cold logging out-of-line | `LogComplianceBlock` → `[NoInlining]`, removes string.Format allocation from hot gate | ✓ |
| `carl_cook`: zero-alloc hot path | Compliance check hot path has no alloc after log extraction | ✓ |
| `gjengset`: no new lock() blocks | `Interlocked.Increment` retained (correct), no new lock() | ✓ |
| `trading_billions`: single responsibility | DrawdownCheck and ProfitCapCheck are independent concerns | ✓ |
| `trading_billions`: CYC ≤ 8 | Max projected = 8 (CheckTrailingDrawdown) | ✓ |
| `trading_billions`: defense in depth | Each helper returns bool; parent chains results defensively | ✓ |

---

## MCP Evidence

- **resolve_repo**: `universal-or-strategy` indexed, fresh
- **get_symbol_source**: `IsOrderAllowed` lines 323–389, 67 LOC, full source confirmed
- **get_call_hierarchy (callers)**: 0 resolved by AST (11 per Phase 1 scope — callers use string-based dispatch patterns)
- **Docstring**: `V12.Phase7 [C-09]: Compliance enforcement gate. Call at START of every entry method.`

---

## Sequential Thinking Evidence

- **Thought 1**: CYC=20 drivers: 3 compound && chains (3 branches each) + try/catch (2) + null guards (2) + flag checks (2) + buffer check (1) = ~14–20 per audit counting
- **Thought 2**: Extraction: 3 helpers (CheckTrailingDrawdown CYC=8, CheckDailyProfitCap CYC=6, LogComplianceBlock CYC=1); parent reduces to CYC=5
- **Thought 3**: CYC validation: max=8 (CheckTrailingDrawdown at threshold), all others ≤ 6 ✓

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Phase** | 2 |
| **Bobcoins Used** | 1.0 |
