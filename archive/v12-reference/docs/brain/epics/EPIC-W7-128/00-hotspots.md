# EPIC-W7-128 · Phase 0 — Hotspot Analysis

## Method Under Analysis

| Field                | Value                                                     |
|----------------------|-----------------------------------------------------------|
| **Method**           | `SymmetryGuardReplaceExistingFollowerTarget`              |
| **CYC (confirmed)**  | 20                                                        |
| **Source file**      | `src/V12_002.Symmetry.Replace.cs`                        |
| **Lines**            | 27 – 97 (method body, 71 lines)                           |
| **Enclosing class**  | `V12_002 : Strategy` (partial)                            |
| **Build / Wave**     | Build 1004 · Wave 7                                       |

---

## Blast Radius Summary

`SymmetryGuardReplaceExistingFollowerTarget` is called exclusively by
`SymmetryGuardRetargetExistingFollowerBracket` (same file, L17–25), which fans
out to it **five times** (once per target slot T1–T5). The retarget method is
itself invoked from `SymmetryGuardTryResolveFollower` in
`src/V12_002.Symmetry.Follower.cs` L225 on every bracket-retarget event.

| Callee / Side-effect                          | File                                      | Description                                                         |
|-----------------------------------------------|-------------------------------------------|---------------------------------------------------------------------|
| `pos.ExecutingAccount.Cancel()`               | NinjaTrader broker API                    | Live broker cancel sent for stale or replaced target order          |
| `dict.TryRemove()`                            | `target1Orders`–`target5Orders` dicts     | Removes stale order from shared `ConcurrentDictionary`              |
| `_followerTargetReplaceSpecs[signalName] = …` | `V12_002.cs` L803 (`ConcurrentDictionary`)| Publishes `FollowerTargetReplaceSpec` for Phase-2 FSM pickup        |
| `StampReaperMoveGrace()`                      | REAPER subsystem (inferred calling file)  | Stamps grace window to suppress false desync during replace gap     |
| `SubmitFollowerTargetReplacement()`           | `src/V12_002.Orders.Callbacks.Propagation.cs` L897 | Phase-2 FSM: fires on cancel-confirm via `TriggerCustomEvent`   |

**Downstream touch-points:** `target1Orders`–`target5Orders`
(`ConcurrentDictionary<string,Order>`), `_followerTargetReplaceSpecs`
(`ConcurrentDictionary<string,FollowerTargetReplaceSpec>`), broker account
cancel path, REAPER grace-window state, plus the AccountOrders.cs cancel-confirm
handler (lines 430–675) that drives the two-phase FSM.

**Blast radius classification: HIGH** — direct live broker cancel I/O + shared
mutable concurrent dictionaries + two-phase FSM spec publishing + REAPER state
mutation on every retarget event across all 5 target slots per follower fill.

---

## Top 3 Complexity Drivers

### 1 · Dual OrderState fan-out (stale-cancel path vs. replace path)

The method contains **two independent `OrderState` multi-branch guards**, each
testing four states (`Working`, `Accepted`, `Submitted`, `ChangePending`) with
`||` chains. The first (L45–51) guards stale-target cancellation; the second
(L67–72) guards the replace-spec submission. Each `||` operator adds +1 to CYC,
contributing **+6 branch points** in total across the two guards. These two
blocks are structurally identical but semantically distinct, making naive
deduplication risky without an extraction.

### 2 · Three-condition compound guard at method entry (L41)

The early-return guard `if (isFilled || isRunner || qty <= 0)` is a
**three-branch compound expression** (+2 CYC from `||` operators) that governs
an entire stale-cleanup sub-block (L43–55) before returning. The combination of
the compound predicate, the nested `TryGetValue && staleTarget != null` check
(+2 CYC from `&&` + null guard), and the inner OrderState 4-way `||` (above)
means this single early-exit path contributes approximately **+7 decision points**
to the total CYC score.

### 3 · Inline spec-building with ternary direction assignment (L74–96)

The replace-spec construction block (L74–95) embeds a price validity guard
(`if (newPrice <= 0)` → +1), a ternary `OrderAction` assignment based on
`pos.Direction` (+1), and the `FollowerTargetReplaceSpec` initialiser — all
inside the already-nested OrderState conditional. The price guard in particular
creates a deep nesting level (3 levels: outer method body → OrderState if →
price if) that cannot be evaluated without also traversing the outer guards,
making test coverage combinatorially harder than the raw CYC count implies.

---

## Recommended Extraction Count

**3 targeted extractions** are recommended for Wave 7:

| # | Proposed extract                              | Reduces CYC by | Scope                                              |
|---|-----------------------------------------------|----------------|----------------------------------------------------|
| 1 | `IsOrderCancellable(Order o) → bool`          | ~3             | Shared 4-way `OrderState` guard used in both paths |
| 2 | `TryCancelStaleTarget(…) → void`              | ~4             | Entire stale-target cancel+remove block (L41–57)   |
| 3 | `BuildFollowerTargetReplaceSpec(…) → FollowerTargetReplaceSpec` | ~3 | Spec construction block (L78–91) |

After extraction, `SymmetryGuardReplaceExistingFollowerTarget` CYC target: **<= 7**.

---

## Agent Tracking Block

```
EPIC:          EPIC-W7-128
WAVE:          7
PHASE:         0 — Hotspot Analysis
STATUS:        completed
OUTPUT:        docs/brain/EPIC-W7-128/00-hotspots.md
CYC_CONFIRMED: 20
METHOD:        SymmetryGuardReplaceExistingFollowerTarget
FILE:          src/V12_002.Symmetry.Replace.cs
AGENT_NAME:    v12-phase0-hotspot
BOBCOINS_USED: 6
EXECUTION_TIME: ~45s
GENERATED_BY:  Bob (analysis agent)
TIMESTAMP_UTC: 2025-07-15T00:00:00Z
```
