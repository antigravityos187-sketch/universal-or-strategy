# EPIC-W7-144 — Phase 4: Ticket Definitions

**agent_name:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T03:00:00Z
**Input:** docs/brain/EPIC-W7-144/02-architecture-plan.md + docs/brain/EPIC-W7-144/03-audit-report.md
**Lane:** P4-L9
**DNA Verdict:** PASS
**Ticket Count:** 4

---

## Target Method

| Field | Value |
|---|---|
| Method | `IsOrderAllowed` |
| File | `src/V12_002.UI.Compliance.cs` |
| Lines | 323–389 |
| CYC (baseline) | 21 (jCodemunch confirmed: cyclomatic=21, max_nesting=5, lines=67) |
| CYC (target) | ≤ 8 |

---

## Sequential Thinking Evidence

**ST-thought-1 (Complexity Analysis):**
IsOrderAllowed CYC=21 driven by: feature flag preamble (2 branches), trailing drawdown compound guard (4 branches including null guard), balance retrieval try/catch (2 branches), buffer check + cold Print logging (1 branch), daily profit cap compound guard (5 branches). jCodemunch confirms cyclomatic=21, max_nesting=5, assessment="high". Three extraction targets identified: trailing drawdown concern, daily profit cap concern, and cold logging.

**ST-thought-2 (Helper Design):**
Four tickets designed: T1 extracts LogComplianceBlock [NoInlining] cold logger (CYC=1); T2 extracts CheckTrailingDrawdown covering the drawdown compound guard + try/catch + buffer check (CYC=8); T3 extracts CheckDailyProfitCap covering the SIMA+ConsistencyLock gate + profit TryGetValue + cap comparison (CYC=6); T4 verifies all reductions via complexity audit + xUnit [Fact] tests.

**ST-thought-3 (CYC Validation):**
Post-extraction CYC projected: IsOrderAllowed parent=5, CheckTrailingDrawdown=8 (at threshold), CheckDailyProfitCap=6, LogComplianceBlock=1. Max=8 satisfies ≤8 mandate. dna_verdict=PASS confirmed from Phase 3 with violations=[]. All constraints satisfied. Ticket plan is valid and complete.

---

## Extraction Tickets

---

### T1 — Extract LogComplianceBlock Cold Logger

**ID:** T1
**Type:** extraction
**File:** `src/V12_002.UI.Compliance.cs`
**CYC Target:** 1

**Description:**
Extract all inline `Print`/`string.Format` cold logging calls from `IsOrderAllowed` into a new private helper method `LogComplianceBlock(string blockType, string acctName, double value)`.
The method must be decorated with `[MethodImpl(MethodImplOptions.NoInlining)]` to prevent inlining of the cold string-allocation path into the hot compliance gate.
Replace each logging site in `IsOrderAllowed` with a single call to `LogComplianceBlock(...)`.

This implements the Jane Street `carl_cook` rule: extract cold logging out-of-line, removing `string.Format` allocation from the hot path.

**Acceptance Criteria:**
- [ ] New `private` method `LogComplianceBlock(string blockType, string acctName, double value)` exists in `src/V12_002.UI.Compliance.cs`
- [ ] Method decorated with `[MethodImpl(MethodImplOptions.NoInlining)]`
- [ ] All `Print`/`string.Format` calls originating from `IsOrderAllowed` logging sites are replaced with `LogComplianceBlock(...)` calls
- [ ] `IsOrderAllowed` body contains zero inline `string.Format` allocations post-extraction
- [ ] `dotnet build` passes with zero errors
- [ ] `CYC(LogComplianceBlock) = 1` (no branches in the new helper)

---

### T2 — Extract CheckTrailingDrawdown Helper

**ID:** T2
**Type:** extraction
**File:** `src/V12_002.UI.Compliance.cs`
**CYC Target:** ≤ 8
**Depends On:** T1 (logging sites must be extracted first)

**Description:**
Extract the trailing drawdown compliance block from `IsOrderAllowed` into a new `private bool CheckTrailingDrawdown(string acctName)` method.

The extracted block encompasses:
- `trailingDrawdownPeak.TryGetValue(acctName, out double peak)` call and guard
- `peak > 0 && TrailingDrawdownLimit > 0` compound guard
- `currentAccount != null` null guard
- Balance retrieval `try/catch` block (try entry +1, catch handler +1)
- `buffer <= 0` check and block

Replace the extracted block in `IsOrderAllowed` with a single call: `if (!CheckTrailingDrawdown(acctName)) return false;`

This implements the Jane Street `trading_billions` single-responsibility principle: trailing drawdown enforcement is an independent concern from daily profit cap enforcement.

**Acceptance Criteria:**
- [ ] New `private bool CheckTrailingDrawdown(string acctName)` method exists in `src/V12_002.UI.Compliance.cs`
- [ ] Method returns `false` when drawdown limit is breached, `true` when order is allowed
- [ ] `IsOrderAllowed` replaces the drawdown block with `if (!CheckTrailingDrawdown(acctName)) return false;`
- [ ] `try/catch` for balance retrieval is fully inside `CheckTrailingDrawdown`
- [ ] `dotnet build` passes with zero errors
- [ ] `CYC(CheckTrailingDrawdown) ≤ 8`

---

### T3 — Extract CheckDailyProfitCap Helper

**ID:** T3
**Type:** extraction
**File:** `src/V12_002.UI.Compliance.cs`
**CYC Target:** ≤ 6
**Depends On:** T1 (logging sites extracted), T2 (trailing drawdown extracted)

**Description:**
Extract the daily profit cap compliance block from `IsOrderAllowed` into a new `private bool CheckDailyProfitCap(string acctName)` method.

The extracted block encompasses:
- `EnableSIMA && EnableConsistencyLock` compound gate
- `dailyProfitTracker.TryGetValue(acctName, out double dp)` call and guard
- `MaxDailyProfitCap > 0 && dp >= MaxDailyProfitCap` compound comparison
- Cold logging call (already delegated to `LogComplianceBlock` after T1)

Replace the extracted block in `IsOrderAllowed` with a single call: `if (!CheckDailyProfitCap(acctName)) return false;`

This implements the Jane Street `trading_billions` single-responsibility principle: daily profit cap enforcement is an independent concern from trailing drawdown enforcement.

**Acceptance Criteria:**
- [ ] New `private bool CheckDailyProfitCap(string acctName)` method exists in `src/V12_002.UI.Compliance.cs`
- [ ] Method returns `false` when daily profit cap is exceeded, `true` when order is allowed
- [ ] `IsOrderAllowed` replaces the profit cap block with `if (!CheckDailyProfitCap(acctName)) return false;`
- [ ] No inline `string.Format` inside `CheckDailyProfitCap` (uses `LogComplianceBlock` for logging)
- [ ] `dotnet build` passes with zero errors
- [ ] `CYC(CheckDailyProfitCap) ≤ 6`

---

### T4 — Verify CYC Reduction and Write xUnit Tests

**ID:** T4
**Type:** verification + testing
**File:** `src/V12_002.UI.Compliance.cs` (read) + test file (write)
**CYC Target:** Confirm all symbols ≤ 8
**Depends On:** T1, T2, T3

**Description:**
Validate the full extraction by running `python scripts/complexity_audit.py` and confirming all four resulting symbols meet their CYC targets. Write xUnit [Fact] tests (never NUnit/MSTest) for the three extracted helpers covering:
- `CheckTrailingDrawdown`: happy path (drawdown within limit), boundary (buffer==0), breach (buffer<0)
- `CheckDailyProfitCap`: happy path (dp < cap), boundary (dp == cap), breach (dp > cap), disabled gate (EnableSIMA=false)
- `LogComplianceBlock`: smoke test (no exception thrown, Print called once)

**Acceptance Criteria:**
- [ ] `complexity_audit.py` reports `CYC(IsOrderAllowed) ≤ 5`
- [ ] `complexity_audit.py` reports `CYC(CheckTrailingDrawdown) ≤ 8`
- [ ] `complexity_audit.py` reports `CYC(CheckDailyProfitCap) ≤ 6`
- [ ] `complexity_audit.py` reports `CYC(LogComplianceBlock) = 1`
- [ ] xUnit test file exists with `[Fact]` attributes (no `[Test]`, no `[TestMethod]`)
- [ ] All tests use `Assert.Equal`, `Assert.True`, `Assert.False` — never NUnit/MSTest assertions
- [ ] `dotnet test` passes with 100% test suite pass rate
- [ ] `dotnet build` passes with zero errors

---

## CYC Summary

| Symbol | CYC Baseline | CYC Projected | ≤ 8? |
|---|---|---|---|
| `IsOrderAllowed` (parent) | 21 | 5 | ✅ |
| `CheckTrailingDrawdown` | — (new) | 8 | ✅ (at threshold) |
| `CheckDailyProfitCap` | — (new) | 6 | ✅ |
| `LogComplianceBlock` | — (new) | 1 | ✅ |
| **Max** | **21** | **8** | ✅ |

---

## Jane Street KB Alignment

| Rule | Ticket | Status |
|---|---|---|
| `carl_cook`: extract cold logging out-of-line | T1 (`LogComplianceBlock` → `[NoInlining]`) | ✓ |
| `carl_cook`: zero-alloc hot path | T1 removes all `string.Format` from hot gate | ✓ |
| `gjengset`: no new `lock()` blocks | No `lock()` in any extracted helper | ✓ |
| `trading_billions`: single responsibility | T2+T3 separate drawdown vs profit cap concerns | ✓ |
| `trading_billions`: CYC ≤ 8 | T4 validates max=8 via complexity_audit.py | ✓ |
| `trading_billions`: defense in depth | Each helper returns `bool`; parent chains defensively | ✓ |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic ID** | EPIC-W7-144 |
| **Bobcoins Used** | 0.8 |
| **MCP Tools Called** | resolve_repo, sequentialthinking (×4: 1 probe + 3 analysis), search_symbols, get_symbol_complexity, get_extraction_candidates |
| **jCodemunch Complexity** | cyclomatic=21, max_nesting=5, param_count=1, lines=67, assessment=high |
| **DNA Verdict** | PASS (from Phase 3) |
| **Ticket Count** | 4 (T1–T4) |
