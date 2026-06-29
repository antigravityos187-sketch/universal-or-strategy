# EPIC-W7-003 — Phase 1: Scope Definition

## Overview

This document defines the **single method** in scope for EPIC-W7-003 and
establishes the **scope boundary** that governs all subsequent phases (planning,
implementation, validation). No other method, class, or file may be modified
unless it is a direct mechanical consequence of extracting helpers from the
target method.

---

## Method in Scope

| Field | Value |
|---|---|
| **Method** | `IsOrderAllowed` |
| **File** | `src/V12_002.UI.Compliance.cs` |
| **Line range** | 323 – 389 (66 LOC) |
| **Visibility** | `private bool` |
| **Signature** | `private bool IsOrderAllowed(string? accountName = null)` |
| **Class** | `V12_002` (partial) |
| **Namespace** | `NinjaTrader.NinjaScript.Strategies` |
| **Current CYC** | **21** |
| **Target CYC** | **≤ 8** (Wave 7 policy; projected post-refactor: 4) |

This is the **single method** targeted by EPIC-W7-003. Exactly one method
definition site exists in the codebase (confirmed by grep against all `*.cs`
files under `src/`).

---

## Scope Boundary Statement

The **scope boundary** for EPIC-W7-003 is: the body of `IsOrderAllowed` at
[`src/V12_002.UI.Compliance.cs:323`](../../src/V12_002.UI.Compliance.cs:323)
and any new **private** helper methods extracted from it into the same partial
class file. Nothing outside this boundary — including caller files, public
APIs, test harnesses, configuration, or other methods in the same file — may
be added, deleted, or structurally altered. This scope boundary is enforced
by the V12.23 No Scope Creep Protocol (see below).

---

## Callers (Blast Radius)

Grep against `src/**/*.cs` yielded **11 call sites** across **5 entry files**.
The method signature (`private bool`, optional `string?` param) must remain
byte-for-byte identical after refactoring — zero caller changes are permitted.

| Caller file | Call sites | Lines |
|---|---|---|
| [`src/V12_002.Entries.OR.cs`](../../src/V12_002.Entries.OR.cs) | 3 | 40, 84, 128 |
| [`src/V12_002.Entries.FFMA.cs`](../../src/V12_002.Entries.FFMA.cs) | 3 | 117, 310, 505 |
| [`src/V12_002.Entries.Trend.cs`](../../src/V12_002.Entries.Trend.cs) | 2 | 208, 848 |
| [`src/V12_002.Entries.Retest.cs`](../../src/V12_002.Entries.Retest.cs) | 2 | 53, 332 |
| [`src/V12_002.Entries.MOMO.cs`](../../src/V12_002.Entries.MOMO.cs) | 1 | 47 |
| **Total** | **11** | — |

All 11 call sites invoke `IsOrderAllowed()` with no arguments (using the
default `null` parameter). The pattern is uniformly `if (!IsOrderAllowed()) return;`
— a hard gate at the top of each entry method. No caller passes an explicit
`accountName`, so the method's optional-parameter default path is always taken
in production.

---

## Why Other Methods Are NOT in Scope

### V12.23 No Scope Creep Protocol

Wave 7 engineering policy V12.23 prohibits any work item from expanding its
scope beyond the single declared hotspot method once Phase 1 is ratified.
The following are explicitly **out of scope**:

| Method / Entity | Reason excluded |
|---|---|
| `ProcessAccountExecutionQueue` (same file, CYC ~14) | Different hotspot; addressed by a separate EPIC if/when it crosses the Wave 7 CYC threshold. Not part of this work item. |
| `LogApexPerformance` (same file, CYC ~8) | Below the CYC ≥ 15 mandatory-extraction threshold; no action required in this wave. |
| `CheckTrailingDrawdown` (new helper) | **In scope only as an extraction target** — it does not exist yet and will be created as a direct product of refactoring `IsOrderAllowed`. |
| `TryGetAccountBalance` (new helper) | Same — in scope only as an extraction target, not a pre-existing method to change. |
| `CheckDailyProfitCap` (new helper) | Same — in scope only as an extraction target. |
| All 5 caller entry files | Callers must not change. Signature preservation ensures zero caller-side impact. |
| Any UI, configuration, or test files | No UI or test changes are required; the method is `private` with no external surface. |

The rationale is explicit: modifying any caller file, any other method body,
or any public API to "clean things up while we're here" constitutes scope creep
and is forbidden under V12.23. The complexity reduction must be achieved purely
through internal extraction within the compliance module.

---

## CYC Reduction Plan (Summary — Detail in Phase 2)

Three private helper extractions are planned, each reducing CYC monotonically:

1. **`TryGetAccountBalance(Account acct, out double balance) → bool`**
   Removes the inline `try/catch` broker API call from `IsOrderAllowed`.
   CYC contribution removed: ~4.

2. **`CheckTrailingDrawdown(string acctName) → bool`**
   Encapsulates the `TryGetValue / peak / limit / buffer` decision cluster.
   CYC contribution removed: ~9. Calls helper #1 internally.

3. **`CheckDailyProfitCap(string acctName) → bool`**
   Encapsulates the `EnableSIMA / EnableConsistencyLock / dp` decision cluster.
   CYC contribution removed: ~6.

Post-refactor `IsOrderAllowed` becomes a ~10-line orchestrator with projected
**CYC = 4**, well within the ≤ 8 target.

---

## Source Evidence

Grep results from `src/` directory (all `*.cs` files):

- **Definition:** `src/V12_002.UI.Compliance.cs:323` — 1 occurrence
- **Call sites:** 11 occurrences across 5 files (confirmed above)
- **Total grep hits:** 12 (1 definition + 11 calls)

Source lines 323–389 of [`src/V12_002.UI.Compliance.cs`](../../src/V12_002.UI.Compliance.cs:323)
read directly and verified: the method body contains the drawdown block
(lines 333–366) and the SIMA daily-profit-cap block (lines 369–386), matching
all branch-count data from Phase 0.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase1-scope |
| **Bobcoins Used** | 2.5 |
| **Execution Time** | ~90s |
| **Tools used** | `read_file`, `grep` (native file tools; source-of-truth verification against live `.cs` files) |
| **Phase 0 input** | `docs/brain/EPIC-W7-003/00-hotspots.md` |
| **Output** | `docs/brain/EPIC-W7-003/00-scope.md` (this file) |
| **Scope confirmed** | ✅ Single method `IsOrderAllowed`, CYC 21 → ≤ 8, zero caller changes |
