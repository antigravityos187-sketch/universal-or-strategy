# Phase 1: Scope Definition — EPIC-W7-115

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 1.0
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T21:52:38Z

---

## 1. Method Under Refactoring

| Attribute           | Value                                    |
|---------------------|------------------------------------------|
| **Method**          | `SweepTrackedOrders`                     |
| **File**            | `src/V12_002.SIMA.Lifecycle.cs`          |
| **Line**            | 1308                                     |
| **Signature**       | `private int SweepTrackedOrders(bool force)` |
| **Current CYC**     | 11                                       |
| **Target CYC**      | ≤ 8                                      |
| **LOC**             | 46                                       |
| **Visibility**      | `private` — internal to the class        |

### Current Method Body (annotated)

```csharp
// 1308–1353: src/V12_002.SIMA.Lifecycle.cs
private int SweepTrackedOrders(bool force)
{
    // [BRANCH 1] ternary: force → all 7 dicts vs. entry-only
    var trackedDicts = force
        ? new ConcurrentDictionary<string, Order>[]
          { entryOrders, stopOrders, target1Orders,
            target2Orders, target3Orders, target4Orders, target5Orders }
        : new ConcurrentDictionary<string, Order>[] { entryOrders };

    int trackedCancels = 0;
    foreach (var dict in trackedDicts)          // [BRANCH 2] loop
    {
        if (dict == null) continue;             // [BRANCH 3] null guard
        foreach (var kvp in dict.ToArray())     // [BRANCH 4] loop
        {
            Order ord = kvp.Value;
            if (ord == null) continue;          // [BRANCH 5] null guard
            if (                                // [BRANCHES 6-10] 5-arm state guard
                ord.OrderState != OrderState.Working
                && ord.OrderState != OrderState.Accepted
                && ord.OrderState != OrderState.Submitted
                && ord.OrderState != OrderState.ChangePending
                && ord.OrderState != OrderState.ChangeSubmitted
            ) continue;
            try                                 // [BRANCH 11] try/catch
            {
                CancelOrderOnAccount(ord, ord.Account);
                trackedCancels++;
            }
            catch { }
        }
    }
    return trackedCancels;
}
```

**Complexity drivers**:
- `&&`-chained 5-state `OrderState` guard = 5 decision points (branches 6–10)
- Ternary dict selector = 1 decision point (branch 1)
- Remaining 5 points: 2 foreach loops + 2 null guards + 1 try/catch

---

## 2. IN SCOPE — Extractions to Achieve CYC ≤ 8

### 2.1 `ResolveTrackedDicts(bool force)` — extract ternary dict-selector

**What it does**: Encapsulates the ternary branch that decides which set of
`ConcurrentDictionary<string, Order>` instances to sweep based on the `force`
flag. Removes 1 decision point from `SweepTrackedOrders`.

**Proposed signature**:
```csharp
private ConcurrentDictionary<string, Order>[] ResolveTrackedDicts(bool force)
```

**Body extracted from**: lines 1313–1324  
**CYC contributed to parent after extraction**: −1  
**New helper CYC**: 2 (base 1 + ternary)

---

### 2.2 `IsOrderCancellable(Order ord)` — extract 5-arm OrderState guard

**What it does**: Encapsulates the compound `&&` predicate that decides whether
an order's current `OrderState` makes it eligible for cancellation. Removes 5
decision points from `SweepTrackedOrders` and replaces them with a single
boolean call.

**Proposed signature**:
```csharp
private static bool IsOrderCancellable(Order ord)
```

**Body extracted from**: lines 1336–1343  
**CYC contributed to parent after extraction**: −4 (5 branches → 1 call = net −4)  
**New helper CYC**: 6 (base 1 + null-guard + 5 `&&` comparisons → logically 6,
but single-expression form can be expressed as a `switch` expression or a `return`
with `||`, keeping helper CYC ≤ 8)

> **Note**: `ord == null` guard (branch 5) can be folded into
> `IsOrderCancellable` as an early-return, further reducing inner-loop
> complexity in the parent.

---

### Net CYC Reduction

| Decision point         | Before | After extraction |
|------------------------|--------|-----------------|
| Ternary dict selector  | +1     | 0 (moved to `ResolveTrackedDicts`) |
| 5-arm `OrderState` `&&`| +5     | +1 (single call to `IsOrderCancellable`) |
| `ord == null` guard    | +1     | 0 (folded into `IsOrderCancellable`) |
| Remaining (loops, dict null-guard, try/catch) | +4 | +4 (unchanged) |
| **Total**              | **11** | **≤ 6** ✅      |

Post-refactor `SweepTrackedOrders` CYC estimate: **6** (comfortably ≤ 8).

---

## 3. OUT OF SCOPE

1. **Signature of `SweepTrackedOrders` is unchanged** — `private int SweepTrackedOrders(bool force)` remains identical. Return type, parameter count, parameter name, and access modifier are frozen.
2. **No behavioral change** — observable outputs (return value `trackedCancels`, side-effect calls to `CancelOrderOnAccount`) must be byte-for-byte equivalent under all inputs.
3. **`SweepBrokerOrders` and all other methods** in the file are untouched.
4. **Field declarations** (`entryOrders`, `stopOrders`, `target1Orders` … `target5Orders`) are not moved or renamed.
5. **Callers are not modified** — `CancelAllV12GtcOrders`, `ProcessShutdownSIMA`, and `ProcessApplySimaState` call sites are frozen.
6. **No new public API** — all extracted helpers are `private` (or `private static` where stateless).
7. **No dependency additions** — no new `using` directives, packages, or external calls.
8. **No unit test files created in this phase** — test scaffolding is deferred to Phase 3+.
9. **`try/catch { }` block** — the bare catch is pre-existing defensive code; it is moved intact and not refactored.

---

## 4. Extraction Plan

### Step 1 — Extract `ResolveTrackedDicts`

1. Copy the ternary expression (lines 1313–1324) into a new `private` method
   `ResolveTrackedDicts(bool force)` placed immediately above `SweepTrackedOrders`.
2. Replace the inline ternary in `SweepTrackedOrders` with a call:
   ```csharp
   var trackedDicts = ResolveTrackedDicts(force);
   ```
3. Verify: method compiles; return type matches.

### Step 2 — Extract `IsOrderCancellable`

1. Copy the `ord == null` check and the 5-arm `OrderState` predicate into a new
   `private static bool IsOrderCancellable(Order ord)` method placed immediately
   above `SweepTrackedOrders`.
2. The helper returns `false` for null or a non-cancellable state; `true` otherwise:
   ```csharp
   private static bool IsOrderCancellable(Order ord)
   {
       if (ord == null) return false;
       return ord.OrderState == OrderState.Working
           || ord.OrderState == OrderState.Accepted
           || ord.OrderState == OrderState.Submitted
           || ord.OrderState == OrderState.ChangePending
           || ord.OrderState == OrderState.ChangeSubmitted;
   }
   ```
   > Logic inversion: original used `&&` + negation (`!=`) → equivalent to `||` + positive (`==`).
3. Replace the two guards in `SweepTrackedOrders` with:
   ```csharp
   if (!IsOrderCancellable(ord)) continue;
   ```
4. Remove the now-redundant `if (ord == null) continue;` line (absorbed by helper).

### Step 3 — Verify post-refactor `SweepTrackedOrders`

After both extractions the inner loop body reads:
```csharp
foreach (var dict in trackedDicts)
{
    if (dict == null) continue;
    foreach (var kvp in dict.ToArray())
    {
        Order ord = kvp.Value;
        if (!IsOrderCancellable(ord)) continue;
        try
        {
            CancelOrderOnAccount(ord, ord.Account);
            trackedCancels++;
        }
        catch { }
    }
}
```
CYC = 1 (base) + 1 (foreach) + 1 (null guard) + 1 (foreach) + 1 (if) + 1 (try/catch) = **6** ✅

---

## 5. Risk Assessment

| Risk                              | Likelihood | Severity | Mitigation                                          |
|-----------------------------------|------------|----------|-----------------------------------------------------|
| Logic inversion error (`!=` → `==` + `\|\|`) | Low | High | Side-by-side truth-table review in Phase 2 architecture doc |
| `try/catch {}` placement drift    | Low        | Medium   | Moved verbatim; no new logic inside catch           |
| `ord == null` guard removal breaks null safety | Very Low | Medium | Absorbed into `IsOrderCancellable` as first check   |
| Thread-safety regression (`dict.ToArray()`) | None | N/A | `ToArray()` call site is unchanged                 |
| Caller breakage                   | None       | None     | Signature frozen; callers not touched               |
| `force=false` semantic violation  | None       | High     | `ResolveTrackedDicts` moves logic verbatim; no new entries added |
| New helper introduces state mutation | None    | N/A      | Both helpers are read-only / stateless              |

**Overall Phase Risk: LOW**

---

## 6. Success Criteria

| Criterion                                      | Pass Condition                                         |
|------------------------------------------------|--------------------------------------------------------|
| `SweepTrackedOrders` CYC post-refactor         | ≤ 8 (target: 6)                                        |
| `ResolveTrackedDicts` CYC                      | ≤ 8 (expected: 2)                                      |
| `IsOrderCancellable` CYC                       | ≤ 8 (expected: 6)                                      |
| Signature of `SweepTrackedOrders` unchanged    | `private int SweepTrackedOrders(bool force)` identical |
| Return value equivalence                       | `trackedCancels` identical for all inputs              |
| `CancelOrderOnAccount` call-count equivalence  | Called exactly once per eligible order, no new calls   |
| `force=false` semantic preserved               | Only `entryOrders` dict swept when `force` is false    |
| `force=true` semantic preserved                | All 7 order dicts swept when `force` is true           |
| No other methods modified                      | `git diff` shows only new helpers + `SweepTrackedOrders` body changes |
| Build passes                                   | Zero new compile errors or warnings                    |
| Max nesting depth of `SweepTrackedOrders`      | ≤ 3 (reduced from 4)                                   |
