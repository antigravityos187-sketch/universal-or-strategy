# EPIC-W7-014 — Phase 1: Scope Definition

## Single Method in Scope

This epic targets exactly one method — `TryHandleFleetCommand` — located in
[`src/V12_002.UI.IPC.Commands.Fleet.cs:37`](../../src/V12_002.UI.IPC.Commands.Fleet.cs).
No other method is included in Phase 1 refactoring scope. This is a **single method** epic
boundary enforced by the V12.23 complexity-reduction protocol.

---

## Complexity Profile

| Metric | Value |
|--------|-------|
| **Current CYC (manual McCabe, lines 37–81)** | **20** |
| Audit-list CYC (`precomputed.json`) | 0 (measurement gap, not genuine simplicity) |
| Task-spec fallback CYC | 9 |
| **Target CYC (post-refactor)** | **≤ 8** |

The audit-list value of 0 is a precompute artefact: the tool skips methods whose complexity is
dominated by sub-handler delegation. The real McCabe count is 20:

| Branch source | Count |
|---|---|
| Base path | 1 |
| `senderTicks > 0` ternary (line 40) | 1 |
| 18 × `if (TryHandleFleet_*)` guards (lines 44–79) | 18 |
| **Total** | **20** |

Recommended extraction (extractions 1–2 from hotspot analysis) reduces dispatcher CYC 20 → ≤ 3,
well within the ≤ 8 target.

---

## Source File

**File:** [`src/V12_002.UI.IPC.Commands.Fleet.cs`](../../src/V12_002.UI.IPC.Commands.Fleet.cs)

**Method signature:**
```csharp
private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)
```
**Lines:** 37–81 (dispatcher body, 44 LOC)

---

## Callers

`TryHandleFleetCommand` has **2 callers** (confirmed via `grep` across all `src/*.cs` files):

| # | Caller | File | Call site | Path |
|---|--------|------|-----------|------|
| 1 | `ProcessIpcCommandCore` | [`src/V12_002.UI.IPC.cs:466`](../../src/V12_002.UI.IPC.cs) | `if (TryHandleFleetCommand(action, parts, senderTicks))` | TCP IPC listener thread |
| 2 | Panel button handler | [`src/V12_002.UI.Panel.Handlers.cs:952`](../../src/V12_002.UI.Panel.Handlers.cs) | `if (ctx.TryHandleFleetCommand(action, parts, senderTicks))` | WPF dispatcher thread |

Both call sites are **live trading paths**. The method signature
`(string action, string[] parts, long senderTicks)` is fixed — any change to the signature
would require coordinated updates at both call sites.

---

## Scope Boundary

The **scope boundary** for this epic is drawn at the dispatcher body of `TryHandleFleetCommand`
(lines 37–81) only. The boundary is defined as:

- **In scope:** the `cmdId` ternary construction (lines 39–42) and the 18-arm `if`-chain
  (lines 44–79) that form the dispatcher body.
- **Out of scope in Phase 1:** all 18 `TryHandleFleet_*` sub-handler bodies. They are called
  by the dispatcher but carry their own internal CYC. Sub-handler refactoring is deferred to
  Phase 2 per the V12.23 single-phase, single-method constraint.
- **Out of scope permanently (Phase 1):** callers `ProcessIpcCommandCore` and the panel button
  handler. Their signatures and logic are not modified.

---

## Why Other Methods Are NOT in Scope (V12.23)

The V12.23 complexity-reduction protocol enforces a **single method** constraint per phase for
the following reasons:

1. **Blast-radius containment.** `TryHandleFleetCommand` touches 2 callers, 18 direct
   sub-handlers, and 9+ downstream files. Expanding scope to sub-handlers in the same phase
   multiplies the regression surface exponentially across live trading paths.

2. **Incremental verifiability.** A single-method refactor produces a diff that can be reviewed
   and regression-tested in isolation. Including sub-handlers (`TryHandleFleet_LongShort`,
   `TryHandleFleet_CancelAll`, etc.) in the same phase makes the diff non-atomic.

3. **Sub-handler complexity is independent.** The 18 `TryHandleFleet_*` methods each carry their
   own internal CYC (SIMA/non-SIMA forks, `OrderState` filters, nested guards). That complexity
   is not caused by the dispatcher body — it exists regardless of how the dispatcher routes.
   Reducing sub-handler CYC is a separate Phase 2 concern.

4. **Protocol compliance.** V12.23 explicitly names `TryHandleFleetCommand` as the single target.
   Any co-modification of sibling methods (`TryHandleFleet_*`), caller methods
   (`ProcessIpcCommandCore`), or sibling command modules
   (`V12_002.UI.IPC.Commands.Config.cs`, `V12_002.UI.IPC.Commands.Misc.cs`) would constitute
   an out-of-scope change and must be rejected at review.

---

## Sub-Handlers (Callees — Phase 2 Scope, Not Phase 1)

The following 18 methods are **direct callees** of `TryHandleFleetCommand`. They are listed here
for traceability but are **not in scope** for Phase 1:

`TryHandleFleet_Trim`, `TryHandleFleet_Lock50`, `TryHandleFleet_FlattenOnly`,
`TryHandleFleet_Flatten`, `TryHandleFleet_CancelAll`, `TryHandleFleet_ResetMemory`,
`TryHandleFleet_LongShort`, `TryHandleFleet_OrLong`, `TryHandleFleet_OrShort`,
`TryHandleFleet_TrendManualLimit`, `TryHandleFleet_RetestManualLimit`,
`TryHandleFleet_FfmaManualLimit`, `TryHandleFleet_FfmaManualMarket`,
`TryHandleFleet_CloseTarget`, `TryHandleFleet_MoveTarget`, `TryHandleFleet_FleetState`,
`TryHandleFleet_ToggleAccount`, `TryHandleFleet_SetShadow`

---

## Symbol Search Evidence

Caller count confirmed by source-code grep over all `src/*.cs` files:

| Tool / Method | Query | Hits |
|---|---|---|
| `grep` — `src/*.cs` | `TryHandleFleetCommand` | 4 lines total: 1 definition + 1 comment header (Fleet.cs), 1 reference (IPC.cs:466), 1 reference (Panel.Handlers.cs:952) |
| `search_symbols` (jcodemunch MCP) | `repo="universal-or-strategy"`, `query="TryHandleFleetCommand"` | 1 definition hit (Fleet.cs:37), 2 reference hits (IPC.cs:466, Panel.Handlers.cs:952) — consistent with grep |

**Callers count: 2**

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase1-scope |
| **Bobcoins Used** | 0 (scope definition only — no code mutation) |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition (REDO) |
| **Epic** | EPIC-W7-014 |
| **Output** | `docs/brain/EPIC-W7-014/00-scope.md` |
| **Method in Scope** | `TryHandleFleetCommand` (single method) |
| **Source File** | `src/V12_002.UI.IPC.Commands.Fleet.cs` |
| **Current CYC** | 20 (manual McCabe; audit-list value 0 is measurement gap) |
| **Target CYC** | ≤ 8 |
| **Callers** | 2 (`ProcessIpcCommandCore` @ IPC.cs:466, panel handler @ Panel.Handlers.cs:952) |
| **Scope Boundary** | Dispatcher body lines 37–81 only; sub-handlers deferred to Phase 2 |
| **Protocol** | V12.23 single-method constraint enforced |
