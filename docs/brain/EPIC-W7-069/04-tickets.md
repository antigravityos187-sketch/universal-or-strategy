# EPIC-W7-069 — Phase 4 Ticket Definitions

Agent Name: v12-phase4-tickets
Wave: 7
Epic: EPIC-W7-069
Phase: 4 — Ticket Generation
Source: [`src/V12_002.Symmetry.BracketFSM.cs`](src/V12_002.Symmetry.BracketFSM.cs:422)
Method: `GetFsmExpectedPosition`
CYC (current): 14 (modified Lizard) / 7 (McCabe)
CYC (projected): 7 (post-extraction)
Extractions Planned: 2
Ticket Count: 4
dna_verdict: PASS (Phase 3)

---

## Dependency Chain

```
T1 (IsActiveFollowerState extraction)  --\
                                           +--> T3 (parent rewrite) --> T4 (build verify)
T2 (ComputeEntrySignedQuantity extraction) --/
```

T1 and T2 are independent and may be applied sequentially in the same file by the same engineer.
T3 depends on T1 and T2 being present in the file.
T4 depends on T3 completing successfully.

---

## T1 — Extract `IsActiveFollowerState` Helper

| Field | Value |
|---|---|
| **Ticket ID** | T1 |
| **Title** | Extract `IsActiveFollowerState` — 6-way state classification helper |
| **File Target** | `src/V12_002.Symmetry.BracketFSM.cs` |
| **Depends On** | None |
| **CYC Projection** | 2 |
| **Jane Street Rule** | trading_billions: single responsibility; carl_cook: AggressiveInlining, zero-alloc |

### Work Description

Add the following **new private static method** to the same partial class as `GetFsmExpectedPosition`
(class `V12_002`, file `src/V12_002.Symmetry.BracketFSM.cs`). Place it immediately before or after
`GetFsmExpectedPosition` within the file.

**Exact method signature and body to insert:**

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool IsActiveFollowerState(FollowerBracketState state)
{
    return state switch
    {
        FollowerBracketState.Active
        or FollowerBracketState.Accepted
        or FollowerBracketState.Submitted
        or FollowerBracketState.PendingSubmit
        or FollowerBracketState.Replacing
        or FollowerBracketState.Modifying => true,
        _ => false
    };
}
```

**Notes:**
- `private static` — no instance state captured; pure enum classification.
- Switch `or`-pattern syntax eliminates the 6-way OR chain from the parent body.
- `[MethodImpl(MethodImplOptions.AggressiveInlining)]` must be present (AggressiveInlining hot path).
- All identifiers are ASCII-only.
- No `lock()` blocks.
- This extraction reduces the parent method's modified cyc by 5 decision points (6 OR conditions → 1 helper call).

### Acceptance Criteria

- [ ] Method `IsActiveFollowerState(FollowerBracketState state)` exists in `src/V12_002.Symmetry.BracketFSM.cs`
- [ ] Decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] Modifier is `private static`
- [ ] Uses C# switch `or`-pattern expression (not if-else chain)
- [ ] All 6 states covered: `Active`, `Accepted`, `Submitted`, `PendingSubmit`, `Replacing`, `Modifying`
- [ ] Default arm returns `false`
- [ ] cyc projection = 2 (verified by complexity_audit.py)
- [ ] Zero new `lock()` blocks (`grep -n "lock(" src/V12_002.Symmetry.BracketFSM.cs` count unchanged)
- [ ] ASCII-only identifiers and literals
- [ ] `dotnet build` passes with zero errors after this ticket

---

## T2 — Extract `ComputeEntrySignedQuantity` Helper

| Field | Value |
|---|---|
| **Ticket ID** | T2 |
| **Title** | Extract `ComputeEntrySignedQuantity` — sign+quantity computation helper |
| **File Target** | `src/V12_002.Symmetry.BracketFSM.cs` |
| **Depends On** | None |
| **CYC Projection** | 3 |
| **Jane Street Rule** | trading_billions: single responsibility; carl_cook: AggressiveInlining, no LINQ; gjengset: no mutation |

### Work Description

Add the following **new private static method** to the same partial class as `GetFsmExpectedPosition`
(class `V12_002`, file `src/V12_002.Symmetry.BracketFSM.cs`). Place it immediately adjacent to
`IsActiveFollowerState` (T1) or `GetFsmExpectedPosition`.

**Exact method signature and body to insert:**

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static int ComputeEntrySignedQuantity(Order entryOrder)
{
    // Caller contract: entryOrder != null (guarded at call site)
    int sign = (entryOrder.OrderAction == OrderAction.Buy
        || entryOrder.OrderAction == OrderAction.BuyToCover) ? 1 : -1;
    return entryOrder.Quantity * sign;
}
```

**Notes:**
- `private static` — no instance state captured; pure arithmetic.
- Caller (`GetFsmExpectedPosition` after T3) guarantees `entryOrder != null` before calling — no null guard inside helper.
- `[MethodImpl(MethodImplOptions.AggressiveInlining)]` required.
- `Buy || BuyToCover → +1`, all other actions → `-1` (short/sell).
- No LINQ, no allocation.
- ASCII-only identifiers.
- No `lock()` blocks.

### Acceptance Criteria

- [ ] Method `ComputeEntrySignedQuantity(Order entryOrder)` exists in `src/V12_002.Symmetry.BracketFSM.cs`
- [ ] Decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] Modifier is `private static`
- [ ] Uses ternary for sign: `(Buy || BuyToCover) ? 1 : -1`
- [ ] Returns `entryOrder.Quantity * sign`
- [ ] No internal null guard on `entryOrder` (caller contract)
- [ ] cyc projection = 3 (base 1 + 2 boolean sub-expressions for `Buy || BuyToCover`)
- [ ] Zero new `lock()` blocks
- [ ] ASCII-only identifiers and literals
- [ ] `dotnet build` passes with zero errors after this ticket

---

## T3 — Rewrite `GetFsmExpectedPosition` Body

| Field | Value |
|---|---|
| **Ticket ID** | T3 |
| **Title** | Rewrite `GetFsmExpectedPosition` body to call T1 and T2 helpers |
| **File Target** | `src/V12_002.Symmetry.BracketFSM.cs` (line 422) |
| **Depends On** | T1, T2 |
| **CYC Projection** | 7 (parent method post-extraction) |
| **Jane Street Rule** | trading_billions: aggregation-only responsibility; gjengset: no new lock(); carl_cook: zero-alloc |

### Work Description

Replace the **body** of the existing `GetFsmExpectedPosition` method (lines ~422-460) with the
skeleton below. The method **signature must remain unchanged**.

**Signature (unchanged):**
```csharp
private int GetFsmExpectedPosition(string accountName)
```

**New body (exact replacement):**
```csharp
private int GetFsmExpectedPosition(string accountName)
{
    int sum = 0;
    foreach (var kvp in _followerBrackets)
    {
        FollowerBracketFSM f = kvp.Value;
        if (f == null || f.AccountName != accountName)
            continue;
        if (!IsActiveFollowerState(f.State))
            continue;
        if (f.EntryOrder != null)
            sum += ComputeEntrySignedQuantity(f.EntryOrder);
        else if (f.State == FollowerBracketState.Active)
        {
            // Hydrated Active FSM -- caller handles fallback to broker position
        }
    }
    return sum;
}
```

**CYC breakdown post-extraction:**

| Decision Point | Count |
|---|---|
| Base | 1 |
| `foreach` | +1 |
| `f == null \|\| f.AccountName != accountName` | +2 |
| `IsActiveFollowerState` call-site (1 if) | +1 |
| `f.EntryOrder != null` | +1 |
| `else if (f.State == Active)` | +1 |
| **Total** | **7** |

**Removed from body (delegated to helpers):**
- The 6-way OR state check (`Active || Accepted || Submitted || PendingSubmit || Replacing || Modifying`) — now `IsActiveFollowerState(f.State)` (T1)
- The sign ternary + quantity multiplication — now `ComputeEntrySignedQuantity(f.EntryOrder)` (T2)

**Notes:**
- Docstring: Preserve or replace with: `// Computes the net expected position for a given account by summing all non-terminal FollowerBracketFSMs. SOLE authority for follower expected position (Build 1105).`
- ASCII-only comment text (double-dash `--` not em-dash).
- No `lock()` added.

### Acceptance Criteria

- [ ] Signature `private int GetFsmExpectedPosition(string accountName)` is unchanged
- [ ] Body calls `IsActiveFollowerState(f.State)` (T1 helper)
- [ ] Body calls `ComputeEntrySignedQuantity(f.EntryOrder)` (T2 helper)
- [ ] Original 6-way OR chain is removed from body
- [ ] Original sign ternary is removed from body
- [ ] cyc projection for `GetFsmExpectedPosition` = 7 (verified by complexity_audit.py)
- [ ] max_cyc_projected = 7 <= 8 (Jane Street strict standard PASS)
- [ ] Zero new `lock()` blocks
- [ ] ASCII-only string literals and comments
- [ ] `dotnet build` passes with zero errors

---

## T4 — Build Verification and CYC Audit

| Field | Value |
|---|---|
| **Ticket ID** | T4 |
| **Title** | Build verification, CYC audit, and deploy-sync after extraction |
| **File Target** | Repository-wide verification commands |
| **Depends On** | T3 |
| **CYC Projection** | N/A (verification only) |
| **Jane Street Rule** | All V12 pre-push quality gates |

### Work Description

Run the following verification sequence in order. Each step must pass before continuing.

**Step 1 — Build:**
```bash
dotnet build
```
Expected: Zero errors, zero new warnings.

**Step 2 — Formatting:**
```bash
dotnet csharpier check src/
```
Expected: Zero formatting issues. If any, run `dotnet csharpier format src/` and re-verify.

**Step 3 — Lock audit:**
```bash
grep -n "lock(" src/V12_002.Symmetry.BracketFSM.cs
```
Expected: Zero new `lock()` occurrences (same count as before the epic — must not have increased).

**Step 4 — CYC audit (all three methods):**
```bash
python scripts/complexity_audit.py
```
Verify in output:
- `GetFsmExpectedPosition` cyc <= 7
- `IsActiveFollowerState` cyc <= 2
- `ComputeEntrySignedQuantity` cyc <= 3

**Step 5 — Deploy sync:**
```bash
powershell -File ./deploy-sync.ps1
```
Expected: Hard links re-synchronized, DIFF GUARD passes (diff < 10,000 chars).

**Step 6 — Pre-push validation (fast mode):**
```bash
powershell -File ./scripts/pre_push_validation.ps1 -Fast
```
Expected: All blocking checks pass (Build, Tests, Lint, Formatting, ASCII, PR Hygiene, Complexity).

### Acceptance Criteria

- [ ] `dotnet build` exits with code 0, zero errors
- [ ] `dotnet csharpier check src/` exits with zero issues
- [ ] `grep -n "lock(" src/V12_002.Symmetry.BracketFSM.cs` count unchanged from pre-epic baseline
- [ ] `complexity_audit.py` reports `GetFsmExpectedPosition` cyc <= 7
- [ ] `complexity_audit.py` reports `IsActiveFollowerState` cyc <= 2
- [ ] `complexity_audit.py` reports `ComputeEntrySignedQuantity` cyc <= 3
- [ ] `deploy-sync.ps1` DIFF GUARD passes
- [ ] `pre_push_validation.ps1 -Fast` all blocking gates pass
- [ ] extraction count = 2 confirmed in file (both helpers present)
- [ ] Epic EPIC-W7-069 marked for Phase 5 (ticket execution)

---

## Summary

| Ticket | Title | CYC | Depends On | Status |
|---|---|---|---|---|
| T1 | Extract `IsActiveFollowerState` | 2 | None | Pending |
| T2 | Extract `ComputeEntrySignedQuantity` | 3 | None | Pending |
| T3 | Rewrite `GetFsmExpectedPosition` body | 7 | T1, T2 | Pending |
| T4 | Build verification + CYC audit | N/A | T3 | Pending |

**max_cyc_projected: 7** (all methods <= 8 — Jane Street PASS)
**extraction count: 2**
**dna_verdict: PASS** (from Phase 3 audit)

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Epic** | EPIC-W7-069 |
| **Phase** | 4 |
| **Ticket Count** | 4 |
| **max_cyc_projected** | 7 |
| **extraction_count** | 2 |
| **dna_verdict** | PASS |
| **Sequential Thinking Calls** | 4 |
| **jCodemunch Calls** | resolve_repo |
| **Bobcoins Used** | 0.6 |
