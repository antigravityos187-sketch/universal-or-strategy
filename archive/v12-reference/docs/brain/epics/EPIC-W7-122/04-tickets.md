# EPIC-W7-122 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-122/02-architecture-plan.md, docs/brain/EPIC-W7-122/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-122 |
| **Method** | `RemoveFsmOrderIdMappings` |
| **Source File** | `src/V12_002.Symmetry.BracketFSM.cs` |
| **Original CYC** | 10 |
| **max_cyc_projected** | 3 |
| **DNA Verdict** | PASS |
| **Ticket Count** | 3 |
| **Extraction Count** | 3 helpers |

Generate 3 surgical tickets for the cyc reduction of `RemoveFsmOrderIdMappings` (CYC 10 → 2).
All tickets target the same partial class in a single file. Ticket execution order: T1 → T2 → T3.

---

## MCP Evidence

| Tool | Result |
|---|---|
| `resolve_repo` | `antigravityos187-sketch/universal-or-strategy` indexed, 5147 symbols |
| `get_symbol_complexity` | Symbol not in index (private method in partial class — expected; CYC=10 sourced from task brief + Phase 2 architecture plan) |
| `get_extraction_candidates` | `candidates=[]` — method has 1 caller, automated threshold not triggered; manual Jane Street pattern decomposition applied per Phase 2 |
| `sequentialthinking` | 4 thoughts completed — ticket structure validated (see Sequential Thinking section) |

---

## Sequential Thinking Validation

| Thought | Conclusion |
|---|---|
| 1 — Ticket Granularity | Single atomic extraction ticket preferred over 3 separate helper tickets — partial extraction leaves parent calling non-existent helpers, causing compilation errors |
| 2 — Ticket Structure | 3 tickets: T1=surgical extraction (all helpers + parent refactor), T2=xUnit tests, T3=build verification + deploy-sync |
| 3 — Acceptance Criteria | T1: 4 methods at correct CYC; T2: [Fact] tests per helper; T3: zero build errors + deploy-sync pass |
| 4 — Final Validation | MCP results consistent with Phase 2/3; CYC=10 from architecture plan is authoritative; 3-ticket plan is minimal and non-overlapping |

---

## Ticket Definitions

---

### TICKET-1 — Surgical Extraction: Extract 3 Helpers from `RemoveFsmOrderIdMappings`

| Field | Value |
|---|---|
| **ID** | EPIC-W7-122-T1 |
| **Type** | extraction |
| **Epic** | EPIC-W7-122 |
| **Lane** | P4-L8 |
| **File** | `src/V12_002.Symmetry.BracketFSM.cs` |
| **Method** | `RemoveFsmOrderIdMappings` |
| **Original CYC** | 10 |
| **CYC Target (parent)** | ≤2 |
| **CYC Target (helpers max)** | ≤3 |
| **max_cyc_projected** | 3 |
| **Agent** | v12-engineer (Bob CLI) |
| **Priority** | P0 — blocks T2 and T3 |

#### Description

Atomically extract 3 private helper methods from `RemoveFsmOrderIdMappings` in the partial class
`V12_002` within `src/V12_002.Symmetry.BracketFSM.cs`. All helpers must be added in the same
commit as the parent method refactor — no intermediate broken state is acceptable.

#### Helpers to Extract

| Helper | Signature | CYC Target | Jane Street Pattern |
|---|---|---|---|
| `RemoveSingleOrderMapping` | `private void RemoveSingleOrderMapping(Order order)` | **3** | `carl_cook` — [AggressiveInlining], zero-alloc hot-path leaf |
| `RemoveReplacingCancelMapping` | `private void RemoveReplacingCancelMapping(string cancelOrderId)` | **2** | `trading_billions` — single-responsibility, no Order coupling |
| `RemoveTargetOrderMappings` | `private void RemoveTargetOrderMappings(Order[] targets)` | **3** | `gjengset` — isolated iteration kernel; `trading_billions` — single-responsibility array processor |

#### Implementation

```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private void RemoveSingleOrderMapping(Order order)
{
    if (order != null && !string.IsNullOrEmpty(order.OrderId))
        _orderIdToFsmKey.TryRemove(order.OrderId, out _);
}

private void RemoveReplacingCancelMapping(string cancelOrderId)
{
    if (!string.IsNullOrEmpty(cancelOrderId))
        _orderIdToFsmKey.TryRemove(cancelOrderId, out _);
}

private void RemoveTargetOrderMappings(Order[] targets)
{
    if (targets == null)
        return;

    foreach (Order target in targets)
        RemoveSingleOrderMapping(target);
}

private void RemoveFsmOrderIdMappings(FollowerBracketFSM fsm)
{
    if (fsm == null)
        return;

    RemoveSingleOrderMapping(fsm.EntryOrder);
    RemoveReplacingCancelMapping(fsm.ReplacingCancelOrderId);
    RemoveSingleOrderMapping(fsm.StopOrder);
    RemoveTargetOrderMappings(fsm.Targets);
}
```

#### Acceptance Criteria

- [ ] `RemoveSingleOrderMapping` exists with `[AggressiveInlining]` attribute, CYC = **3**
- [ ] `RemoveReplacingCancelMapping` exists, CYC = **2**
- [ ] `RemoveTargetOrderMappings` exists, CYC = **3**
- [ ] `RemoveFsmOrderIdMappings` (parent) refactored to flat coordinator, CYC = **2**
- [ ] All 4 methods are `private` in partial class `V12_002` in `src/V12_002.Symmetry.BracketFSM.cs`
- [ ] max_cyc_projected = **3** (all helpers CYC ≤ 8, Jane Street threshold satisfied)
- [ ] Zero `lock()` blocks — `ConcurrentDictionary.TryRemove` used exclusively (lock-free)
- [ ] ASCII-only string literals — no Unicode, emoji, or curly quotes
- [ ] `TryTerminateFollowerBracket` call-site unchanged (1 caller, no signature modification)
- [ ] No cross-file changes — only `src/V12_002.Symmetry.BracketFSM.cs` modified
- [ ] `dotnet csharpier check src/` passes after modification
- [ ] CYC reduction: **10 → 2** (parent), net delta = -8

---

### TICKET-2 — xUnit Tests for Extracted Helpers

| Field | Value |
|---|---|
| **ID** | EPIC-W7-122-T2 |
| **Type** | test |
| **Epic** | EPIC-W7-122 |
| **Lane** | P4-L8 |
| **File** | `tests/V12_Performance.Tests/Core/BracketFSMRemoveMappingsTests.cs` (new) |
| **CYC Target** | ≤4 per test method |
| **Agent** | v12-engineer (Bob CLI) |
| **Depends On** | TICKET-1 must be complete |
| **Priority** | P1 — required for wave quality gate |

#### Description

Write xUnit [Fact] tests for the 3 extracted helpers: `RemoveSingleOrderMapping`,
`RemoveReplacingCancelMapping`, and `RemoveTargetOrderMappings`. Tests must use ONLY xUnit
([Fact], Assert.Equal, Assert.Null etc.) — NEVER NUnit or MSTest. Each extraction helper
must have tests covering the null-guard path and the happy path to validate cyc branch coverage.

#### Test Cases Required

| Test | Covers | CYC Branch |
|---|---|---|
| `RemoveSingleOrderMapping_NullOrder_DoesNotThrow` | null guard | branch 1 |
| `RemoveSingleOrderMapping_EmptyOrderId_DoesNotRemove` | empty string guard | branch 2 |
| `RemoveSingleOrderMapping_ValidOrder_RemovesFromDictionary` | happy path TryRemove | branch 3 |
| `RemoveReplacingCancelMapping_NullOrEmpty_DoesNotThrow` | null/empty guard | branch 1 |
| `RemoveReplacingCancelMapping_ValidId_RemovesFromDictionary` | happy path TryRemove | branch 2 |
| `RemoveTargetOrderMappings_NullTargets_DoesNotThrow` | null array guard | branch 1 |
| `RemoveTargetOrderMappings_ValidTargets_RemovesAll` | iteration + delegate | branches 2–3 |

#### Acceptance Criteria

- [ ] Test file exists at `tests/V12_Performance.Tests/Core/BracketFSMRemoveMappingsTests.cs`
- [ ] All test methods decorated with `[Fact]` (xUnit — NEVER NUnit `[Test]` or MSTest `[TestMethod]`)
- [ ] Assertions use `Assert.Equal`, `Assert.Null`, `Assert.False`, or equivalent xUnit assertions
- [ ] 7 minimum test cases covering all 3 helpers (see table above)
- [ ] Tests compile and `dotnet test` passes with 100% test success
- [ ] No NUnit or MSTest references anywhere in the test file
- [ ] Tests validate cyc branch coverage for each helper extraction

---

### TICKET-3 — Build Verification & Deploy-Sync

| Field | Value |
|---|---|
| **ID** | EPIC-W7-122-T3 |
| **Type** | verification |
| **Epic** | EPIC-W7-122 |
| **Lane** | P4-L8 |
| **File** | `src/V12_002.Symmetry.BracketFSM.cs` (verify only) |
| **CYC Target** | N/A (verification) |
| **Agent** | v12-engineer (Bob CLI) |
| **Depends On** | TICKET-1 and TICKET-2 must be complete |
| **Priority** | P1 — required for wave completion |

#### Description

Run full build pipeline and deploy-sync to confirm the extraction is stable. Verify the
pre-push validation script passes all 13 quality gates. Confirm NinjaTrader hard-link
synchronization succeeds via `deploy-sync.ps1`.

#### Commands

```bash
# 1. Format check
dotnet csharpier check src/

# 2. Full build
dotnet build

# 3. Unit tests
dotnet test

# 4. Pre-push validation (fast mode)
powershell -File ./scripts/pre_push_validation.ps1 -Fast

# 5. Deploy-sync (NinjaTrader hard links)
powershell -File ./deploy-sync.ps1
```

#### Acceptance Criteria

- [ ] `dotnet build` returns **0 errors, 0 warnings** (or only pre-existing warnings)
- [ ] `dotnet test` returns **100% pass** with no new failures
- [ ] `dotnet csharpier check src/` returns **0 formatting issues**
- [ ] `pre_push_validation.ps1 -Fast` passes all blocking gates (checks 1–5)
- [ ] `deploy-sync.ps1` executes without error — NinjaTrader hard links synchronized
- [ ] Complexity audit confirms `RemoveFsmOrderIdMappings` CYC = **2** post-extraction
- [ ] No new Codacy/Semgrep issues introduced

---

## CYC Projection Summary

| Symbol | Before | After | Delta | Passes ≤8? |
|---|---|---|---|---|
| `RemoveFsmOrderIdMappings` (parent) | 10 | **2** | -8 | PASS |
| `RemoveSingleOrderMapping` (new) | — | **3** | +3 | PASS |
| `RemoveReplacingCancelMapping` (new) | — | **2** | +2 | PASS |
| `RemoveTargetOrderMappings` (new) | — | **3** | +3 | PASS |
| **max_cyc_projected** | — | **3** | — | **PASS** |

**Cyc reduction: 10 → 2 (parent). Extraction count: 3. max_cyc_projected = 3.**

---

## Execution Order

```
TICKET-1 (extraction) → TICKET-2 (xUnit tests) → TICKET-3 (build + deploy-sync)
```

Tickets are sequential. TICKET-1 must complete before TICKET-2 (tests reference extracted
helpers). TICKET-3 validates the full pipeline after both code and tests are in place.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 2.0 |
| **Execution Time** | batch |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-122 |
| **Ticket Count** | 3 |
| **Original CYC** | 10 |
| **max_cyc_projected** | 3 |
| **DNA Verdict** | PASS |
| **MCP Tools Called** | resolve_repo, get_symbol_complexity, get_extraction_candidates, sequentialthinking (x4) |
