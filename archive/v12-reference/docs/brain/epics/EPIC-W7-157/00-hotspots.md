# EPIC-W7-157 — Phase 0: Hotspot Analysis

## Method Under Analysis

| Field       | Value                                              |
|-------------|----------------------------------------------------|
| Method Name | `TryHandleFleet_MoveTarget`                        |
| CYC Score   | **17**                                             |
| File Path   | `src/V12_002.UI.IPC.Commands.Fleet.cs`             |
| Lines       | 645–693                                            |
| Class       | `V12_002` (partial — `IPC Commands Fleet` region) |

---

## Blast Radius Summary

`TryHandleFleet_MoveTarget` is an IPC command handler called from within
[`TryHandleFleetCommand`](src/V12_002.UI.IPC.Commands.Fleet.cs:37), the
central fleet command dispatcher. It dispatches to two downstream methods:

| Callee | Location | Risk |
|--------|----------|------|
| [`MoveSpecificTarget`](src/V12_002.Trailing.Breakeven.cs:335) | `src/V12_002.Trailing.Breakeven.cs` | **High** — iterates all active positions, touches FollowerBracket FSM, issues `ChangeOrder` calls |
| [`MoveSpecificTargetAbsolute`](src/V12_002.Trailing.Breakeven.cs:559) | `src/V12_002.Trailing.Breakeven.cs` | **High** — mirrors `MoveSpecificTarget` structure, absolute price variant for live control center |

**Affected surfaces:**
- IPC command routing path (any client sending `MOVE_TARGET*` or `SET_TARGET_PRICE`)
- Bracket FSM state machine (`FollowerBracketState.PendingCancel` transitions)
- Order management for targets T1–T5 across master + fleet accounts
- SIMA multi-account follower execution path

**No UI, no SIMA entrypoint, no strategy lifecycle change** is touched
directly by this method — changes are safely scoped to parse + dispatch logic.

---

## Top 3 Complexity Drivers

### 1. Short-circuit compound guard in `targetId` validation (lines 655–661) — **+5 CYC**

```csharp
if (
    targetId.Length >= 2          // branch 1
    && targetId.StartsWith("T")   // branch 2
    && int.TryParse(...)          // branch 3 (out-param side-effect)
    && targetNum >= 1             // branch 4
    && targetNum <= 5             // branch 5
)
```

Five short-circuit `&&` operands in a single `if` guard create five
independent execution paths. Each can short-circuit early, but McCabe
counts each boolean operand as +1.

**Extraction opportunity:** Extract as `TryParseTargetId(string targetId, out int targetNum) → bool`.

---

### 2. Action-discriminated `if/else` block (lines 663–688) — **+5 CYC**

```csharp
if (action == "SET_TARGET_PRICE")            // branch 1
{
    if (double.TryParse(...) && absPrice > 0) // branches 2 + 3
    { ... }
}
else                                          // branch 4
{
    if (distance == "1pt") ...               // branch 5
    else if (distance == "2pt") ...
    else return true;
}
```

Two separate execution strategies (`absolute price` vs. `relative offset`)
are inlined within the same method. Each contains its own guard logic,
producing nested branching.

**Extraction opportunity:**
- `HandleSetTargetPriceAction(int targetNum, string priceStr) → void`
- `HandleMoveTargetRelativeAction(int targetNum, string priceStr) → void`

---

### 3. Top-level dual-entry guard (line 647) — **+2 CYC**

```csharp
if (!action.StartsWith("MOVE_TARGET") && action != "SET_TARGET_PRICE")
    return false;
```

A compound negative guard with `&&` routes two distinct action name
namespaces into one method body. This means the method inherently handles
two protocol messages — a responsibility boundary smell.

**Extraction opportunity:** Separate the two dispatch paths in the
caller (`TryHandleFleetCommand`) with distinct handler delegates, or split
into `TryHandleFleet_MoveTargetRelative` and `TryHandleFleet_SetTargetPrice`.

---

## Recommended Extraction Count

| Extraction | Target Method | CYC Reduction (est.) |
|------------|--------------|----------------------|
| `TryParseTargetId` — 5-way compound guard → validator | −5 |
| `HandleSetTargetPriceAction` — absolute price branch | −3 |
| `HandleMoveTargetRelativeAction` — relative offset branch + string dispatch | −4 |
| Split top-level dual-entry (caller-side) | −2 |

**Total recommended extractions: 3–4**
**Projected post-refactor CYC: 3–5** (well within the ≤10 threshold)

---

## Agent Tracking

| Field           | Value                        |
|-----------------|------------------------------|
| Agent Name      | `v12-phase0-hotspot`         |
| Bobcoins Used   | 9                            |
| Execution Time  | ~65 seconds                  |
| Phase           | 0 — Hotspot Analysis         |
| Status          | ✅ Completed                  |
