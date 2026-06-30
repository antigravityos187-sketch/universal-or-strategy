# EPIC-W7-126 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-126/02-architecture-plan.md + docs/brain/EPIC-W7-126/03-audit-report.md

---

## Summary

Method `SymmetryGuardSubmitFollowerBracket` in [`src/V12_002.Symmetry.Follower.cs`](src/V12_002.Symmetry.Follower.cs) has **CYC=16** (MCP confirmed: `get_symbol_complexity` → cyclomatic=16, max_nesting=5, 141 lines, assessment=high). This epic reduces complexity to **CYC≤6** via **3 surgical extraction tickets** + **1 verification ticket**.

**DNA Verdict (Phase 3):** PASS — 0 violations. Proceed to Phase 5 execution.

| Ticket | Type | Helper | CYC Target | CYC Removed |
|--------|------|--------|-----------|-------------|
| W7-126-T1 | extraction | `ResolveOcoGroupId` | 2 | -1 |
| W7-126-T2 | extraction | `TryBuildTargetOrder` + `LogTargetSkip` | 5 | -3 |
| W7-126-T3 | extraction | `CommitFsmAndDictionaries` | 6 | -4 |
| W7-126-T4 | verification | parent after extraction | ≤6 | —  |

**Total cyc reduction from extraction: -8 (16 → 6)**

---

## Ticket W7-126-T1 — Extract `ResolveOcoGroupId`

| Field | Value |
|-------|-------|
| **ID** | W7-126-T1 |
| **Type** | extraction |
| **File** | [`src/V12_002.Symmetry.Follower.cs`](src/V12_002.Symmetry.Follower.cs) |
| **CYC Baseline (parent)** | 16 |
| **CYC Target (this helper)** | 2 |
| **CYC Removed From Parent** | -1 |
| **Execution Order** | 1 (first — trivial, no side effects, validates refactoring approach) |
| **Jane Street Rule** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — hot-path trivial helper (carl_cook) |

### Description

Extract the OCO group ID resolution ternary into a dedicated private helper. This is the simplest extraction: a single conditional expression with no side effects. Executing it first validates that the refactoring toolchain is working correctly before tackling the heavier extractions.

### Target Signature

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private string ResolveOcoGroupId(PositionInfo pos)
{
    return !string.IsNullOrEmpty(pos.OcoGroupId)
        ? pos.OcoGroupId
        : ("SG_" + DateTime.UtcNow.Ticks.ToString());
}
```

### Call Site in Parent (replace extracted code with)

```csharp
string ocoId = ResolveOcoGroupId(pos);
```

### Acceptance Criteria

- [ ] `ResolveOcoGroupId` private method added to same partial class in [`src/V12_002.Symmetry.Follower.cs`](src/V12_002.Symmetry.Follower.cs)
- [ ] Method decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] Parent call site replaced with `string ocoId = ResolveOcoGroupId(pos);`
- [ ] **cyc** of `ResolveOcoGroupId` = **2** (base +1 IsNullOrEmpty conditional)
- [ ] Build passes: `dotnet build` → 0 errors
- [ ] Callers `SymmetryGuardOnFollowerFill` (ln 17) and `SymmetryGuardTryResolveFollower` (ln 230) compile unchanged
- [ ] `lock(` count in file remains 0
- [ ] All string literals remain 7-bit ASCII (`"SG_"` only addition)
- [ ] xUnit `[Fact]` test stub added: `ResolveOcoGroupId_ReturnsExisting_WhenOcoGroupIdSet` and `ResolveOcoGroupId_GeneratesSgPrefix_WhenOcoGroupIdEmpty`

---

## Ticket W7-126-T2 — Extract `TryBuildTargetOrder` + `LogTargetSkip`

| Field | Value |
|-------|-------|
| **ID** | W7-126-T2 |
| **Type** | extraction |
| **File** | [`src/V12_002.Symmetry.Follower.cs`](src/V12_002.Symmetry.Follower.cs) |
| **CYC Baseline (parent after T1)** | 15 |
| **CYC Target (this helper)** | 5 |
| **CYC Removed From Parent** | -3 |
| **Execution Order** | 2 (after T1 — heaviest cyc driver in inner loop) |
| **Jane Street Rules** | `[AggressiveInlining]` on hot path; `[NoInlining]` on cold `LogTargetSkip`; `ref int runnerQty` + `out (int,Order) staged` avoids allocation (carl_cook) |

### Description

Extract the inner loop body (target number 1–5 iteration) into `TryBuildTargetOrder`. This single extraction removes **3 cyc** from the parent by encapsulating the qty guard, runner guard, price guard with `Print`, `RoundToTickSize`, `SymmetryTrim`, and `acct.CreateOrder`. The cold `Print` path is further extracted into a `[NoInlining]` `LogTargetSkip` helper to keep the hot path inline-friendly.

**Critical:** `ref int runnerQty` parameter must accumulate `runnerQty += targetQty` when `IsRunnerTarget(targetNum)` is true, preserving the audit-log value used in the parent's final `Print` call.

### Target Signatures

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool TryBuildTargetOrder(
    Account acct,
    PositionInfo pos,
    int targetNum,
    string fleetEntryName,
    string ocoId,
    OrderAction exitAction,
    ref int runnerQty,
    out (int targetNum, Order order) staged)
{
    // qty guard, runner guard, price guard → return false if any fail
    // RoundToTickSize, SymmetryTrim, acct.CreateOrder
    // staged = (targetNum, order); return true
}

[MethodImpl(MethodImplOptions.NoInlining)]
private void LogTargetSkip(string fleetEntryName, int targetNum, double targetPrice)
{
    Print($"[SymmetryGuard] target {targetNum} skip: price={targetPrice} fleet={fleetEntryName}");
}
```

### Call Site in Parent (replace extracted inner loop body with)

```csharp
for (int targetNum = 1; targetNum <= 5; targetNum++)
{
    if (!TryBuildTargetOrder(acct, pos, targetNum, fleetEntryName, ocoId, exitAction, ref runnerQty, out var staged))
        continue;
    stagedTargets.Add(staged);
    ordersToSubmit.Add(staged.order);
    nonRunnerLimitQty += staged.order.Quantity;
}
```

### Acceptance Criteria

- [ ] `TryBuildTargetOrder` private method added to same partial class (not a separate file)
- [ ] `LogTargetSkip` private method added to same partial class
- [ ] `TryBuildTargetOrder` decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] `LogTargetSkip` decorated with `[MethodImpl(MethodImplOptions.NoInlining)]`
- [ ] Signature uses `ref int runnerQty` and `out (int targetNum, Order order) staged`
- [ ] **cyc** of `TryBuildTargetOrder` = **5** (base + qty guard + runner guard + price guard + Print path)
- [ ] **cyc** of `LogTargetSkip` ≤ **1** (cold path, no branches)
- [ ] `ordersToSubmit.Insert(0, stop)` line in parent **NOT modified** (stop order remains first)
- [ ] `Enqueue(ctx => { ... })` lambda remains in parent (NOT moved into helper)
- [ ] Build passes: `dotnet build` → 0 errors
- [ ] Callers `SymmetryGuardOnFollowerFill` and `SymmetryGuardTryResolveFollower` compile unchanged
- [ ] `lock(` count in file remains 0
- [ ] No LINQ used in extracted helpers (`foreach` only, no `.Select`, `.Where`, etc.)
- [ ] xUnit `[Fact]` test stub added: `TryBuildTargetOrder_ReturnsFalse_WhenTargetQtyZero`

---

## Ticket W7-126-T3 — Extract `CommitFsmAndDictionaries`

| Field | Value |
|-------|-------|
| **ID** | W7-126-T3 |
| **Type** | extraction |
| **File** | [`src/V12_002.Symmetry.Follower.cs`](src/V12_002.Symmetry.Follower.cs) |
| **CYC Baseline (parent after T1+T2)** | 12 |
| **CYC Target (this helper)** | 6 |
| **CYC Removed From Parent** | -4 |
| **Execution Order** | 3 (after T2 — FSM commit block; largest single block) |
| **Jane Street Rules** | `[AggressiveInlining]`; no `lock()` — actor Enqueue pattern preserved in parent (carl_cook + gjengset) |

### Description

Extract the FSM initialization and dictionary commit block into `CommitFsmAndDictionaries`. This extraction removes **4 cyc** from the parent by encapsulating: the `for(i<5)` zero-init of `ExpectedTargetPrices`, both `foreach(stagedTargets)` loops (FSM assignment + dict write), the compound `tNum >= 1 && tNum <= 5` guard, and the `_followerBrackets[fleetEntryName] = fsm` assignment.

**Critical:** No `lock()` blocks introduced — all state commits remain atomic via existing actor Enqueue pipeline or direct dict assignment (gjengset mandate).

### Target Signature

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private void CommitFsmAndDictionaries(
    FollowerBracketFSM fsm,
    string fleetEntryName,
    List<(int targetNum, Order order)> stagedTargets)
{
    // for(i<5) zero-init ExpectedTargetPrices
    // foreach(stagedTargets) FSM target/price assignment with tNum guard
    // _followerBrackets[fleetEntryName] = fsm
    // foreach(stagedTargets) GetTargetOrdersDictionary(tNum)[fleetEntryName] = order
}
```

### Call Site in Parent (replace extracted block with)

```csharp
CommitFsmAndDictionaries(fsm, fleetEntryName, stagedTargets);
```

### Acceptance Criteria

- [ ] `CommitFsmAndDictionaries` private method added to same partial class
- [ ] Method decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] **cyc** of `CommitFsmAndDictionaries` = **6** (base + for init + foreach FSM + if tNum>=1 + if tNum<=5 + foreach dict)
- [ ] Zero `lock()` blocks introduced (grep: `lock(` → 0 in file)
- [ ] Both `foreach(stagedTargets)` loops moved into helper (none remain in parent for this block)
- [ ] `_followerBrackets[fleetEntryName] = fsm` assignment inside helper
- [ ] `Enqueue(ctx => { ... })` lambda **NOT moved** — remains in parent
- [ ] `ordersToSubmit.Insert(0, stop)` line in parent **NOT modified**
- [ ] Build passes: `dotnet build` → 0 errors
- [ ] Callers `SymmetryGuardOnFollowerFill` and `SymmetryGuardTryResolveFollower` compile unchanged
- [ ] xUnit `[Fact]` test stub added: `CommitFsmAndDictionaries_PopulatesDictionaries_ForAllStagedTargets`

---

## Ticket W7-126-T4 — Verification: Parent CYC ≤ 6 + Build Gate

| Field | Value |
|-------|-------|
| **ID** | W7-126-T4 |
| **Type** | verification |
| **File** | [`src/V12_002.Symmetry.Follower.cs`](src/V12_002.Symmetry.Follower.cs) |
| **CYC Baseline (parent before T1)** | 16 |
| **CYC Target (parent after T1+T2+T3)** | ≤6 |
| **Execution Order** | 4 (final gate — all extractions must be complete) |
| **Depends On** | W7-126-T1, W7-126-T2, W7-126-T3 |

### Description

Final verification ticket. After all 3 extraction tickets are applied, confirm the parent method and all new helpers meet the CYC≤8 Jane Street standard, the build is clean, and no regressions were introduced. Run `complexity_audit.py` and `dotnet build` as the definitive gate checks.

### Acceptance Criteria

- [ ] `python scripts/complexity_audit.py` → `SymmetryGuardSubmitFollowerBracket` CYC ≤ **6**
- [ ] `python scripts/complexity_audit.py` → `ResolveOcoGroupId` CYC = **2**
- [ ] `python scripts/complexity_audit.py` → `TryBuildTargetOrder` CYC = **5**
- [ ] `python scripts/complexity_audit.py` → `CommitFsmAndDictionaries` CYC = **6**
- [ ] `python scripts/complexity_audit.py` → `LogTargetSkip` CYC ≤ **1**
- [ ] `dotnet build` → **0 errors, 0 new warnings**
- [ ] `grep -c "lock(" src/V12_002.Symmetry.Follower.cs` → **0**
- [ ] Parent method body reduced from **141 lines → ≤55 lines** (verified via `wc -l` or manual review)
- [ ] Callers `SymmetryGuardOnFollowerFill` (ln 17) and `SymmetryGuardTryResolveFollower` (ln 230) signatures **unchanged**
- [ ] `Enqueue(ctx => { ... })` lambda present in parent method body
- [ ] `ordersToSubmit.Insert(0, stop)` is the **first** insert call in parent
- [ ] 3 xUnit `[Fact]` test stubs present (one per extraction helper per T1/T2/T3 ACs)
- [ ] `powershell -File .\deploy-sync.ps1` → NinjaTrader hard links synchronized
- [ ] Manifest `phase_4.status` = `"completed"`, `phase_5.status` = `"pending"`

---

## Execution Order Summary

```
W7-126-T1 (ResolveOcoGroupId)       — extract trivial ternary     CYC 2
     ↓
W7-126-T2 (TryBuildTargetOrder)     — extract inner loop body     CYC 5
     ↓
W7-126-T3 (CommitFsmAndDictionaries)— extract FSM commit block    CYC 6
     ↓
W7-126-T4 (Verification)            — complexity_audit + build    parent CYC ≤6
```

All tickets execute in **[`src/V12_002.Symmetry.Follower.cs`](src/V12_002.Symmetry.Follower.cs)** only. Zero cross-file blast radius.

---

## MCP Evidence

| Tool | Result |
|------|--------|
| `resolve_repo` | Repo indexed: 5147 symbols, 2000 files, status=loadable |
| `get_symbol_complexity` (confirmed) | `SymmetryGuardSubmitFollowerBracket` → cyclomatic=**16**, max_nesting=5, 141 lines, assessment=high |
| `get_extraction_candidates` | Called for `src/V12_002.Symmetry.Follower.cs` — 0 results (index pre-dates extraction plan; Phase 2 source analysis used as authoritative CYC data) |
| `search_symbols` (fuzzy) | Symbol confirmed at `src/V12_002.Symmetry.Follower.cs:285`, signature `private void SymmetryGuardSubmitFollowerBracket(string fleetEntryName, PositionInfo pos)` |

---

## Sequential Thinking Evidence

| Thought | Topic | Conclusion |
|---------|-------|------------|
| 1 | Ticket structure options — per-helper vs monolithic | 4-ticket split (T1/T2/T3/T4) chosen: surgical, reviewable, sequential |
| 2 | MCP index gap for extraction_candidates | Relied on Phase 2 source analysis (CYC=16 confirmed by get_symbol_complexity); 4-ticket plan valid |
| 3 | Ticket sequencing strategy | T1 first (trivial, validates toolchain), T2 second (heaviest -3 cyc), T3 third (FSM commit -4 cyc), T4 final gate |
| 4 | Final decision — signatures, ACs, Jane Street attributes | All tickets finalized with complete acceptance criteria and cyc targets |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | batch |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-126 |
| **Method** | SymmetryGuardSubmitFollowerBracket |
| **CYC Baseline** | 16 (MCP confirmed: cyclomatic=16, max_nesting=5, 141 lines) |
| **CYC Projected (parent)** | 6 |
| **CYC Projected (max helper)** | 6 |
| **Ticket Count** | 4 |
| **Extraction Count** | 3 |
| **DNA Verdict** | PASS |
| **Lane** | P4-L8 |
