# EPIC-W7-128 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-128/02-architecture-plan.md + docs/brain/EPIC-W7-128/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Epic ID** | EPIC-W7-128 |
| **Method** | `SymmetryGuardReplaceExistingFollowerTarget` |
| **Source File** | `src/V12_002.Symmetry.Replace.cs` |
| **CYC Baseline** | 20 (MCP confirmed: cyclomatic=20, assessment=high) |
| **CYC Target (max)** | ≤ 8 |
| **max_cyc_projected** | 7 |
| **Extraction helpers** | 3 |
| **Total Tickets** | 5 |
| **DNA Verdict** | PASS |
| **Lane** | P4-L8 |

---

## MCP Evidence (Phase 4 Probe)

### resolve_repo
```json
{
  "found": true,
  "indexed": true,
  "repo": "antigravityos187-sketch/universal-or-strategy",
  "symbol_count": 5147,
  "status": "loadable"
}
```

### get_symbol_complexity
```json
{
  "symbol_id": "src/V12_002.Symmetry.Replace.cs::V12_002.SymmetryGuardReplaceExistingFollowerTarget#method",
  "cyclomatic": 20,
  "max_nesting": 5,
  "param_count": 5,
  "lines": 71,
  "assessment": "high"
}
```

### get_extraction_candidates
- **Result:** `candidates=[]`
- **Note:** Consistent with Phase 2 finding — C# partial class file has 0 import edges in index; extraction candidates require cross-file caller data. Architecture plan provides the definitive extraction design.

### Sequential Thinking Validation (3 thoughts)
- **Thought 1:** Identified 5-ticket breakdown: T1=IsOrderLive, T2=TryCancelStaleTarget, T3=BuildFollowerTargetReplaceSpec, T4=Parent rewrite, T5=Verify
- **Thought 2:** Confirmed T2 depends on T1 (calls IsOrderLive); T1/T3 are independent; T4 depends on T1+T2+T3; T5 depends on T4
- **Thought 3:** Verified CYC targets: all 4 methods ≤8 after extraction; max_cyc_projected=7; zero scope creep

---

## Dependency Graph

```
T1 (IsOrderLive)
T3 (BuildFollowerTargetReplaceSpec)
     |
     +---> T2 depends on T1
     |
T1 + T2 + T3 ---> T4 (Parent rewrite)
                        |
                        v
                   T5 (Verify)
```

**T1 and T3 are independent** — can be implemented in any order.
**T2 depends on T1** — IsOrderLive must exist before TryCancelStaleTarget calls it.
**T4 depends on T1+T2+T3** — all helpers must exist before parent body is replaced.
**T5 depends on T4** — verification runs after full extraction is complete.

---

## Ticket Definitions

---

### TICKET-W7-128-T1

| Field | Value |
|---|---|
| **ID** | TICKET-W7-128-T1 |
| **Type** | extraction |
| **Title** | Add `IsOrderLive` hot-path boolean predicate helper |
| **File** | `src/V12_002.Symmetry.Replace.cs` |
| **CYC Target** | 4 |
| **Priority** | P1 (unblocks T2) |
| **Depends On** | none |

**Description:**
Extract the duplicated 4-way `OrderState` guard into a new private static method `IsOrderLive`. This predicate is used in two locations (stale-cancel path L45–50 and replace path L67–72) in the current body. Deduplication via this helper removes 6 CYC from the parent.

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool IsOrderLive(Order order)
```

**Body:**
```csharp
return order.OrderState == OrderState.Working
    || order.OrderState == OrderState.Accepted
    || order.OrderState == OrderState.Submitted
    || order.OrderState == OrderState.ChangePending;
```

**Acceptance Criteria:**
- [ ] Method `IsOrderLive` exists in `src/V12_002.Symmetry.Replace.cs`
- [ ] Decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] Signature is `private static bool IsOrderLive(Order order)`
- [ ] Body is a single return expression with 4-way `||` predicate — no LINQ, no lock()
- [ ] `python scripts/complexity_audit.py` reports CYC = **4** for `IsOrderLive`
- [ ] CYC 4 ≤ 8 threshold ✅
- [ ] Build passes: `dotnet build` zero errors

---

### TICKET-W7-128-T2

| Field | Value |
|---|---|
| **ID** | TICKET-W7-128-T2 |
| **Type** | extraction |
| **Title** | Add `TryCancelStaleTarget` cold-path stale-cleanup helper |
| **File** | `src/V12_002.Symmetry.Replace.cs` |
| **CYC Target** | 6 |
| **Priority** | P1 |
| **Depends On** | TICKET-W7-128-T1 |

**Description:**
Extract the entire stale-cleanup sub-block (lines L41–57 of current body) into a new private method `TryCancelStaleTarget`. This block contains the compound entry guard (`isFilled || isRunner || qty <= 0`), the `TryGetValue + null` check, the `IsOrderLive` call, the `Cancel` call, and the `TryRemove`. Returns `bool` — `true` means stale path taken, caller should return immediately.

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private bool TryCancelStaleTarget(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    ConcurrentDictionary<string, Order> dict,
    bool isFilled,
    bool isRunner,
    int qty)
```

**CYC breakdown:** entry=1 + `isFilled||isRunner||qty<=0`=+2 + `TryGetValue&&null`=+2 + `IsOrderLive()` if-branch=+1 = **6**

**Acceptance Criteria:**
- [ ] Method `TryCancelStaleTarget` exists in `src/V12_002.Symmetry.Replace.cs`
- [ ] Decorated with `[MethodImpl(MethodImplOptions.NoInlining)]`
- [ ] Returns `bool` — `true` if stale path taken
- [ ] Internally calls `IsOrderLive(staleTarget)` (from T1)
- [ ] No LINQ, no lock() blocks
- [ ] `python scripts/complexity_audit.py` reports CYC = **6** for `TryCancelStaleTarget`
- [ ] CYC 6 ≤ 8 threshold ✅
- [ ] Build passes: `dotnet build` zero errors

---

### TICKET-W7-128-T3

| Field | Value |
|---|---|
| **ID** | TICKET-W7-128-T3 |
| **Type** | extraction |
| **Title** | Add `BuildFollowerTargetReplaceSpec` cold-path spec-construction helper |
| **File** | `src/V12_002.Symmetry.Replace.cs` |
| **CYC Target** | 3 |
| **Priority** | P1 |
| **Depends On** | none (independent of T1/T2) |

**Description:**
Extract the inline spec-construction block (lines L74–91 of current body) into a new private method `BuildFollowerTargetReplaceSpec`. This block contains the `newPrice <= 0` guard, the `pos.Direction` ternary, the `SymmetryTrim` call, and the `FollowerTargetReplaceSpec` struct initializer. Returns a nullable struct — `null` means price invalid, caller should return immediately.

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private FollowerTargetReplaceSpec? BuildFollowerTargetReplaceSpec(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    string targetTag,
    int qty)
```

**CYC breakdown:** entry=1 + `newPrice<=0` guard=+1 + `pos.Direction` ternary=+1 = **3**

**Acceptance Criteria:**
- [ ] Method `BuildFollowerTargetReplaceSpec` exists in `src/V12_002.Symmetry.Replace.cs`
- [ ] Decorated with `[MethodImpl(MethodImplOptions.NoInlining)]`
- [ ] Returns `FollowerTargetReplaceSpec?` (nullable struct)
- [ ] Returns `null` when `newPrice <= 0`
- [ ] No LINQ, no lock() blocks
- [ ] `python scripts/complexity_audit.py` reports CYC = **3** for `BuildFollowerTargetReplaceSpec`
- [ ] CYC 3 ≤ 8 threshold ✅
- [ ] Build passes: `dotnet build` zero errors

---

### TICKET-W7-128-T4

| Field | Value |
|---|---|
| **ID** | TICKET-W7-128-T4 |
| **Type** | rewrite |
| **Title** | Replace `SymmetryGuardReplaceExistingFollowerTarget` body with 7-branch orchestrator |
| **File** | `src/V12_002.Symmetry.Replace.cs` |
| **CYC Target** | 7 |
| **Priority** | P0 (core extraction) |
| **Depends On** | TICKET-W7-128-T1, TICKET-W7-128-T2, TICKET-W7-128-T3 |

**Description:**
Replace the 71-line body of `SymmetryGuardReplaceExistingFollowerTarget` with the linear 7-branch orchestrator that delegates to the 3 extracted helpers. The method signature is **unchanged** — all 5 callers (`SymmetryGuardRetargetExistingFollowerBracket` called 5×) remain unmodified.

**New Body:**
```csharp
private void SymmetryGuardReplaceExistingFollowerTarget(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    ConcurrentDictionary<string, Order> dict
)
{
    if (pos.ExecutingAccount == null)                                           // +1
        return;

    string targetTag = "T" + targetNumber;
    bool isRunner = IsRunnerTarget(targetNumber);
    bool isFilled = IsTargetFilled(pos, targetNumber);
    int qty = GetTargetContracts(pos, targetNumber);

    if (TryCancelStaleTarget(fleetEntryName, pos, targetNumber, dict,          // +1
            isFilled, isRunner, qty))
        return;

    if (!dict.TryGetValue(fleetEntryName, out var oldTarget) || oldTarget == null) // +2
        return;

    if (!IsOrderLive(oldTarget))                                               // +1
        return;

    string signalName = SymmetryTrim(targetTag + "_" + fleetEntryName, 40);
    var tSpec = BuildFollowerTargetReplaceSpec(fleetEntryName, pos,            // 0
                    targetNumber, targetTag, qty);
    if (tSpec == null)                                                         // +1
        return;

    _followerTargetReplaceSpecs[signalName] = tSpec.Value;
    StampReaperMoveGrace();
    pos.ExecutingAccount.Cancel(new[] { oldTarget });
}
// Total CYC = 7
```

**CYC breakdown:** entry=1 + `ExecutingAccount==null`=+1 + `TryCancelStaleTarget` if=+1 + `TryGetValue||null`=+2 + `IsOrderLive` if=+1 + `tSpec==null`=+1 = **7**

**Acceptance Criteria:**
- [ ] `SymmetryGuardReplaceExistingFollowerTarget` body replaced with orchestrator form above
- [ ] Method signature **unchanged** (same 4 parameters)
- [ ] No inline OrderState checks remain in parent body — all delegated to helpers
- [ ] Calls `TryCancelStaleTarget`, `IsOrderLive`, `BuildFollowerTargetReplaceSpec` (T1/T2/T3)
- [ ] `python scripts/complexity_audit.py` reports CYC = **7** for parent
- [ ] CYC 7 ≤ 8 threshold ✅
- [ ] Caller `SymmetryGuardRetargetExistingFollowerBracket` is **unmodified**
- [ ] No other files modified (V12.23 no scope creep)
- [ ] Build passes: `dotnet build` zero errors

---

### TICKET-W7-128-T5

| Field | Value |
|---|---|
| **ID** | TICKET-W7-128-T5 |
| **Type** | verification |
| **Title** | Full verification: complexity audit + build readiness + deploy-sync |
| **File** | `src/V12_002.Symmetry.Replace.cs` |
| **CYC Target** | max_cyc_projected = 7 (all 4 methods ≤ 8) |
| **Priority** | P0 (completion gate) |
| **Depends On** | TICKET-W7-128-T4 |

**Description:**
Run the full V12 verification suite to confirm that the extraction is complete, all CYC targets are met, and the build is clean. This ticket gates Phase 5 completion.

**Steps:**
1. Run complexity audit: `python scripts/complexity_audit.py`
2. Run CSharpier check: `dotnet csharpier check src/`
3. Run build readiness: `powershell -File ./scripts/build_readiness.ps1`
4. Run deploy-sync: `powershell -File ./deploy-sync.ps1`

**Acceptance Criteria:**
- [ ] `IsOrderLive` — CYC = **4** ≤ 8 ✅
- [ ] `TryCancelStaleTarget` — CYC = **6** ≤ 8 ✅
- [ ] `BuildFollowerTargetReplaceSpec` — CYC = **3** ≤ 8 ✅
- [ ] `SymmetryGuardReplaceExistingFollowerTarget` — CYC = **7** ≤ 8 ✅
- [ ] **max_cyc_projected = 7** (reduced from baseline 20) ✅
- [ ] CSharpier check reports zero formatting issues
- [ ] Build readiness: `dotnet build` zero errors, zero warnings on changed file
- [ ] deploy-sync completes successfully (NinjaTrader hard links resynced)
- [ ] No lock() blocks in any new or modified method (`grep -r "lock(" src/V12_002.Symmetry.Replace.cs` = 0 matches)
- [ ] No LINQ in any new or modified method
- [ ] ASCII-only: no Unicode/emoji in any new string literal

---

## CYC Reduction Summary

| Method | CYC Before | CYC After | Delta | Threshold | Status |
|---|---|---|---|---|---|
| `SymmetryGuardReplaceExistingFollowerTarget` | 20 | 7 | -13 | ≤ 8 | ✅ PASS |
| `IsOrderLive` (new) | — | 4 | new | ≤ 8 | ✅ PASS |
| `TryCancelStaleTarget` (new) | — | 6 | new | ≤ 8 | ✅ PASS |
| `BuildFollowerTargetReplaceSpec` (new) | — | 3 | new | ≤ 8 | ✅ PASS |
| **max_cyc_projected** | **20** | **7** | **-13** | ≤ 8 | ✅ **PASS** |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 4 |
| **Execution Time** | batch |
| **Phase** | 4 — Ticket Generation |
| **Wave** | 7 |
| **Epic** | EPIC-W7-128 |
| **MCP Tools Used** | `resolve_repo`, `get_symbol_complexity`, `get_extraction_candidates`, `search_symbols` |
| **Sequential Thinking Steps** | 3 |
| **Total Tickets** | 5 |
| **Ticket Types** | 3 extraction, 1 rewrite, 1 verification |
| **max_cyc_projected** | 7 |
| **Status** | completed |
