# EPIC-W7-043 · Phase 0 — Hotspot Analysis

## Method Name

`SymmetryGuardSubmitFollowerBracket`

## CYC (Reported / Actual)

| Field | Value |
|---|---|
| Reported (task header) | 0 (placeholder/default) |
| Actual McCabe CYC | ~10 |

**CYC decision-point inventory** (lines 285–425, `src/V12_002.Symmetry.Follower.cs`):

| # | Line | Decision |
|---|---|---|
| 1 | 287 | `if (pos.BracketSubmitted) return` |
| 2 | 290 | `if (acct == null) return` |
| 3 | 293 | Ternary — `exitAction` Long vs Short |
| 4 | 298 | Ternary — `ocoId` OcoGroupId vs Ticks fallback |
| 5 | 324 | `for (targetNum = 1..5)` loop |
| 6 | 327 | `if (targetQty <= 0) continue` |
| 7 | 330 | `if (IsRunnerTarget) continue` |
| 8 | 337 | `if (targetPrice <= 0) continue` |
| 9 | 388 | `if (tNum >= 1 && tNum <= 5)` FSM array guard |
| 10 | (implied) | Exit path through `ordersToSubmit` construction |

## File Path

[`src/V12_002.Symmetry.Follower.cs`](../../src/V12_002.Symmetry.Follower.cs) · lines 285–425

## Blast Radius Summary

`SymmetryGuardSubmitFollowerBracket` is called from **2 sites** within the same file:

| Caller | Line | Context |
|---|---|---|
| `SymmetryGuardOnFollowerFill` | 62 | ANCHOR-01 pre-check path — master anchor already resolved at fill time |
| `SymmetryGuardTryResolveFollower` | 230 | Normal deferred-resolve path — bracket not yet submitted |

**Shared infrastructure touched by this method** (blast surface across 16 source files):

| Symbol | Files referencing it |
|---|---|
| `_followerBrackets` (ConcurrentDictionary) | `V12_002.cs`, `Symmetry.BracketFSM.cs`, `SIMA.Fleet.cs`, `SIMA.Shadow.cs`, `UI.IPC.Commands.Fleet.cs` |
| `FollowerBracketFSM` / `FollowerBracketState` | `Symmetry.BracketFSM.cs`, `SIMA.Fleet.cs`, `SIMA.Shadow.cs`, `UI.IPC.Commands.Fleet.cs` |
| `GetTargetOrdersDictionary` | `Orders.Callbacks.cs`, `Orders.Callbacks.Propagation.cs`, `Orders.Callbacks.Execution.cs`, `Orders.Management.cs`, `Orders.Management.Cleanup.cs`, `Orders.Management.Flatten.cs`, `Orders.Management.StopSync.cs`, `Trailing.StopUpdate.cs`, `UI.Snapshot.cs`, `UI.SnapshotPool.cs`, `SIMA.Dispatch.cs`, `SIMA.Fleet.cs` |
| `ValidateStopPrice` | `Orders.Management.StopSync.cs`, `Trailing.StopUpdate.cs`, `SIMA.Dispatch.cs` |
| `GetTargetContracts` / `GetTargetPrice` | `PositionInfo.cs`, `UI.Callbacks.cs`, `UI.Snapshot.cs`, `UI.SnapshotPool.cs`, `Orders.Callbacks.cs`, `Orders.Management.cs`, `SIMA.Dispatch.cs` |

**Total directly impacted source files: 16**
Any refactor of the method signature, FSM write protocol, or `GetTargetOrdersDictionary` commit order risks regressions in all 16 files.

## Top 3 Complexity Drivers

### 1 · OCO Target-Order Construction Loop (lines 324–372, ~48 lines, CYC contribution +4)

The `for (targetNum = 1..5)` block performs **seven distinct concerns in sequence**: qty lookup → runner discrimination → price validation → tick rounding → signal-name trimming → `acct.CreateOrder` → staged-list append. Each concern adds a branch. The loop is not decomposed — all logic lives inline with no helper. This is the single largest complexity cluster.

**Risk:** Silent `continue` on `targetPrice <= 0` means a misconfigured target silently drops contracts from the bracket without audit-log correlation to the position's fill event.

---

### 2 · FollowerBracketFSM Inline Construction & State-Straddling Commit (lines 376–413, ~38 lines)

The FSM is built inline (`new FollowerBracketFSM { ... }`), then manually zeroed with a separate `for (i=0..5)` loop, then populated via a `foreach` over `stagedTargets`, then written to `_followerBrackets`, then the state is mutated from `PendingSubmit → Submitted` **after** the `Enqueue()` side-effect and the `foreach` dictionary writes. The state transition boundary is non-obvious and straddles a broker-visible side effect.

**Risk:** If `Enqueue()` or `GetTargetOrdersDictionary` throws, the FSM is committed to `_followerBrackets` in `PendingSubmit` state with no rollback, creating a ghost entry visible to `GetFsmExpectedPosition()` and the REAPER.

---

### 3 · Non-Deterministic OcoGroupId Fallback (line 298–300)

Build 936 documented a fix for non-deterministic OCO IDs, yet the same method retains a `DateTime.UtcNow.Ticks` fallback:
```csharp
: ("SG_" + DateTime.UtcNow.Ticks.ToString())
```
This is unreachable in correct operation (OcoGroupId is set at position creation) but becomes active on any path where `ExecuteSmartDispatchEntry` fails to set it — silently reverting to the pre-fix behavior. The fallback is a latent defect masked by complexity.

**Risk:** Under broker reconnect or partial dispatch failure the fallback activates, breaking broker-native OCO re-linkage across NT8 restarts — exactly the scenario Build 936 was designed to prevent.

## Recommended Extraction Count

**3 extractions** targeting the three complexity drivers above:

| Extraction | Proposed Name | Scope | CYC reduction |
|---|---|---|---|
| 1 | `BuildFollowerStopOrder` | Lines 293–316 — stop order factory | −1 (ternary inline) |
| 2 | `BuildFollowerTargetOrders` | Lines 318–372 — loop + staging, returns `(staged, nonRunnerQty, runnerQty)` | −4 (loop + 3 continues) |
| 3 | `CommitFollowerBracketFSM` | Lines 376–413 — FSM init, dictionary writes, Enqueue, state transition | −3 (array guard + loop + state mutation) |

Post-extraction `SymmetryGuardSubmitFollowerBracket` reduces to ~20 lines of orchestration with CYC ≈ 2.

---

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | v12-phase0-hotspot |
| Bobcoins Used | 1.0 |
| Execution Time | ~42s |
| Wave | 7 |
| Phase | 0 |
| Epic | EPIC-W7-043 |
| Source File | `src/V12_002.Symmetry.Follower.cs` |
| Output | `docs/brain/EPIC-W7-043/00-hotspots.md` |
