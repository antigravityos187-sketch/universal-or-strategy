# EPIC-W7-130 — Phase 0: Hotspot Analysis

## Method Overview

| Field        | Value                                                       |
|--------------|-------------------------------------------------------------|
| Method Name  | `SymmetryGuardCascadeFollowerCleanup`                       |
| CYC Score    | **0** (tool-reported) / **4** (manual structural count)     |
| File Path    | `src/V12_002.Symmetry.Replace.cs`                           |
| Lines        | 198 – 243                                                   |
| Visibility   | `private void`                                              |
| Class        | `V12_002` (partial)                                         |

> ⚠️ **CYC = 0 reported by tooling.** Manual inspection of the method body confirms a
> structural cyclomatic complexity of **4** (1 base + 3 branching predicates).
> The tool score is likely a parse miss on a partial-class file. Manual count is authoritative
> for this phase. No extraction is strictly required at this score; see recommendation below.

---

## Blast Radius Summary

The method is called from exactly **one site**:

```
src/V12_002.Orders.Callbacks.cs : HandleOrderCancelled_RollbackUnfilledEntry() (line 771)
```

Call condition:
```csharp
if (EnableSIMA && !kvp.Value.IsFollower)
    SymmetryGuardCascadeFollowerCleanup(kvp.Key);
```

It then **touches** (reads/writes) the following shared state:

| Resource                          | Access          | Concurrency Risk |
|-----------------------------------|-----------------|------------------|
| `symmetryMasterEntryToDispatch`   | `TryGetValue`   | Low — ConcurrentDictionary |
| `symmetryDispatchById`            | `TryGetValue`   | Low — ConcurrentDictionary |
| `activePositions`                 | `TryGetValue`   | Low — ConcurrentDictionary |
| `entryOrders`                     | `TryGetValue`   | Low — ConcurrentDictionary |
| `CancelOrderSafe(order, pos)`     | Side-effect     | **Medium** — triggers broker round-trip |
| `Print(...)` (×2)                 | Side-effect     | Low |

Downstream consequence chain:
```
CancelOrderSafe
  └─► OnAccountOrderUpdate (confirmed cancel)
        └─► HandleMatchedFollower_DeltaRollback
              └─► RollbackExpectedPosition / CleanupPosition (deferred, Build 960 A2-3)
```

A reference comment in `src/V12_002.Orders.Callbacks.AccountOrders.cs` line 693 explicitly
names this method as the deferral origin point for delta rollback — so the blast radius
extends to the REAPER desync guard even though no direct call chain is visible here.

---

## Top 3 Complexity Drivers

### 1 — Multi-Guard Early-Exit Chain (lines 200 – 206)
```csharp
if (!symmetryMasterEntryToDispatch.TryGetValue(...)) return;
if (!symmetryDispatchById.TryGetValue(...))          return;
```
Two sequential guard returns before any work begins. Low individual complexity but they
establish a prerequisite ladder that makes the happy path non-obvious to a reader
following the call from `HandleOrderCancelled_RollbackUnfilledEntry`.

### 2 — Triple-State Order Guard Inside Foreach (lines 218 – 241)
```csharp
foreach (string followerName in followers)
{
    if (!activePositions.TryGetValue(...))  continue;   // guard 1
    if (!entryOrders.TryGetValue(...))       continue;   // guard 2
    if (order == null)                       continue;   // guard 3
    if (order.OrderState == Working
     || order.OrderState == Submitted
     || order.OrderState == Accepted)                    // compound branch
    { ... CancelOrderSafe ... }
}
```
The loop body contains 3 guard `continue`s plus a 3-way compound `||` condition.
This is the primary complexity hotspot: any future state addition (e.g., `ChangePending`)
must be inserted in two different locations in this file (compare
`SymmetryGuardReplaceExistingFollowerTarget` lines 45–50 which already has 4 states).

### 3 — Implicit Null Safety via Ternary Inside Print (lines 231 – 237)
```csharp
pos.ExecutingAccount != null ? pos.ExecutingAccount.Name : "Master"
```
Inline null-coalescing inside a `string.Format` call. This is low complexity numerically
but represents a hidden invariant: `pos.ExecutingAccount` is expected to be null for
master-domain followers. The comment `"Master"` fallback is load-bearing documentation
masquerading as a format argument.

---

## Recommended Extraction Count

| Verdict       | Count | Rationale                                                                        |
|---------------|-------|----------------------------------------------------------------------------------|
| **Recommended** | **1** | Extract the `foreach` body into `CancelFollowerEntryIfPending(string followerName)` |
| Optional      | 0     | The guard chain (drivers 1 & 2) is idiomatic C# early-return; no value in extracting |

The single extraction would:
- Reduce the foreach body from ~18 lines to 1 call
- Allow independent unit-testing of the cancel guard logic
- Mirror the pattern already used in `SymmetryGuardReplaceExistingFollowerTarget`
  (which is itself a per-target extraction from the bracket handler)

---

## Notes

- **CYC = 0 flag**: The JCodemunch tool returned 0. This is a known artefact when
  `search_symbols` cannot resolve a method inside a `partial class` spread across
  multiple files. The manual CYC of **4** should be used for all downstream phase decisions.
- No switch statements are present in this method.
- No nested loops are present; the only loop is a single `foreach` with early-continue guards.
- The method is marked `private` — blast radius is bounded to this assembly.

---

## Agent Tracking

| Field            | Value                        |
|------------------|------------------------------|
| Agent Name       | `v12-phase0-hotspot`         |
| Bobcoins Used    | 6                            |
| Execution Time   | ~45 seconds                  |
| Manual CYC       | 4                            |
| Tool CYC         | 0 (parse miss — partial class) |
| Review Required  | Yes — confirm tool CYC score |
