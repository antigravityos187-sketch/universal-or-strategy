# Phase 4: Tickets — EPIC-W7-078

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4
**Generated:** 2026-06-29T01:20:00Z
**Method:** `StopIpcServer` | **Source:** `src/V12_002.UI.IPC.Server.cs` (lines 451–510)
**Original CYC:** ~11 | **extraction_count:** 4 | **max_cyc_projected:** 5

---

## Extraction Summary

This ticket set covers the full complexity reduction of `StopIpcServer` (CYC ~11) via extraction
of 4 focused private helpers. Each helper is single-responsibility, CYC ≤ 8 (Jane Street threshold),
and the parent method drops to CYC=2 after refactoring. DNA audit verdict: PASS.

| ticket | Helper / Action | CYC Before | CYC After |
|--------|-----------------|-----------|-----------|
| W7-078-T1 | Extract `StopIpcServer_SignalAndStopListener` | n/a (new) | 2 |
| W7-078-T2 | Extract `StopIpcServer_JoinThread` | n/a (new) | 3 |
| W7-078-T3 | Extract `CloseIpcClientSession` | n/a (new) | 5 |
| W7-078-T4 | Extract `StopIpcServer_CloseAllClients` | n/a (new) | 3 |
| W7-078-T5 | Refactor parent `StopIpcServer` | ~11 | 2 |
| W7-078-T6 | Verify CYC compliance | — | max=5 |
| W7-078-T7 | Update manifest phase_5 inputs | — | — |

---

## W7-078-T1: Extract StopIpcServer_SignalAndStopListener

**Title:** Extract signal-and-stop-listener logic from `StopIpcServer` into dedicated helper

**Description:**
The opening block of `StopIpcServer` sets `isIpcRunning = false`, null-guards the `ipcListener`
field, calls `ipcListener.Stop()`, and nulls the field reference. This extraction creates a
standalone helper `StopIpcServer_SignalAndStopListener()` containing solely the signal-and-stop
responsibility. This is ticket T1 of the 4-helper extraction plan.

**Acceptance Criteria:**
- [ ] `private void StopIpcServer_SignalAndStopListener()` exists in `V12_002.UI.IPC.Server.cs`
- [ ] Body sets `isIpcRunning = false` before any listener access
- [ ] `ipcListener` is null-guarded before calling `.Stop()`
- [ ] Field is nulled after Stop() to prevent double-stop
- [ ] No `lock()` blocks introduced
- [ ] All string literals are ASCII-only
- [ ] Build passes: `dotnet build src/` exits 0

**CYC Impact:** New helper CYC = 2 (base=1, null-guard branch=+1). Satisfies CYC ≤ 8.

---

## W7-078-T2: Extract StopIpcServer_JoinThread

**Title:** Extract thread-join logic from `StopIpcServer` into dedicated helper

**Description:**
The thread-join block of `StopIpcServer` performs a null-and-alive guard on `ipcThread` then
calls `ipcThread.Join(500)` with a timeout. This extraction creates
`StopIpcServer_JoinThread()` with sole responsibility for background thread teardown.
This is ticket T2 of the 4-helper extraction plan.

**Acceptance Criteria:**
- [ ] `private void StopIpcServer_JoinThread()` exists in `V12_002.UI.IPC.Server.cs`
- [ ] Null-guard on `ipcThread` with early-return when null
- [ ] `ipcThread.IsAlive` checked before Join to prevent no-op hang
- [ ] Join called with 500ms timeout to prevent indefinite block
- [ ] No `lock()` blocks introduced
- [ ] All string literals are ASCII-only
- [ ] Build passes: `dotnet build src/` exits 0

**CYC Impact:** New helper CYC = 3 (base=1, null-guard=+1, IsAlive-guard=+1). Satisfies CYC ≤ 8.

---

## W7-078-T3: Extract CloseIpcClientSession

**Title:** Extract per-client close logic into `CloseIpcClientSession` helper (deduplication bonus)

**Description:**
The per-client teardown logic inside `StopIpcServer` performs socket shutdown via
`SocketShutdown.Both`, catches zombie/ObjectDisposed exceptions with
`Interlocked.Increment(_ipcZombieConnections)`, calls socket `Close()`, and wraps
everything in a per-client catch with `Interlocked.Increment(_ipcCleanupFailures)`.
This extraction creates `CloseIpcClientSession(IpcClientSession session, string clientId)`.

**Deduplication bonus:** This helper also eliminates the copy of the same pattern in
`HandleClient` lines 193–217 in the same file — two code paths collapse to one.

This is ticket T3 of the 4-helper extraction plan; it has the highest projected CYC of the set.

**Acceptance Criteria:**
- [ ] `private void CloseIpcClientSession(IpcClientSession session, string clientId)` exists
- [ ] Session null-guard with early-return when session is null
- [ ] Socket shutdown performed via `SocketShutdown.Both` inside inner try
- [ ] `ObjectDisposedException` caught and counted via `Interlocked.Increment(_ipcZombieConnections)`
- [ ] `socket.Close()` called in all non-exception paths
- [ ] Outer per-client catch counts via `Interlocked.Increment(_ipcCleanupFailures)`
- [ ] `HandleClient` lines 193–217 refactored to delegate to `CloseIpcClientSession`
- [ ] No `lock()` blocks introduced
- [ ] All string literals are ASCII-only
- [ ] Build passes: `dotnet build src/` exits 0

**CYC Impact:** New helper CYC = 5 (base=1, null-guard=+1, inner try=+1, ODE catch=+1,
outer catch=+1). This is the max_cyc_projected for this extraction set. Satisfies CYC ≤ 8.

---

## W7-078-T4: Extract StopIpcServer_CloseAllClients

**Title:** Extract close-all-clients loop from `StopIpcServer` into dedicated helper

**Description:**
The client-cleanup loop in `StopIpcServer` null-guards `connectedClients`, iterates
`.ToArray()` to prevent mutation-during-iteration, delegates each entry to
`CloseIpcClientSession`, calls `connectedClients.Clear()`, and resets the queue count via
`Interlocked.Exchange(ref ipcQueuedCommandCount, 0)`. This extraction creates
`StopIpcServer_CloseAllClients()` with sole responsibility for bulk client teardown.
This is ticket T4 of the 4-helper extraction plan. Depends on T3 (CloseIpcClientSession).

**Acceptance Criteria:**
- [ ] `private void StopIpcServer_CloseAllClients()` exists in `V12_002.UI.IPC.Server.cs`
- [ ] `connectedClients` null-guarded with early-return
- [ ] Iterates `connectedClients.ToArray()` (snapshot) to prevent ConcurrentModificationException
- [ ] Each entry delegated to `CloseIpcClientSession` (not inlined)
- [ ] `connectedClients.Clear()` called after iteration
- [ ] `Interlocked.Exchange(ref ipcQueuedCommandCount, 0)` resets queue counter
- [ ] No `lock()` blocks introduced
- [ ] All string literals are ASCII-only
- [ ] Build passes: `dotnet build src/` exits 0

**CYC Impact:** New helper CYC = 3 (base=1, null-guard=+1, foreach=+1). Satisfies CYC ≤ 8.

---

## W7-078-T5: Refactor Parent StopIpcServer to Delegate to All 4 Helpers

**Title:** Refactor `StopIpcServer` parent body to orchestrate helpers (CYC 11 → 2)

**Description:**
After all 4 helpers are extracted (T1–T4), the parent `StopIpcServer` is refactored to
contain only the orchestration shell — 3 sequential helper calls inside a single outer
try/catch. All inline logic is removed. This is the final structural refactoring ticket.

Depends on T1, T2, T3, T4.

**Target body after extraction:**
```csharp
private void StopIpcServer()
{
    try
    {
        StopIpcServer_SignalAndStopListener();
        StopIpcServer_JoinThread();
        StopIpcServer_CloseAllClients();
    }
    catch (Exception ex)
    {
        Interlocked.Increment(ref _ipcCleanupFailures);
        Print($"[IPC_CLEANUP] Server shutdown failed: {ex.Message}");
    }
}
```

**Acceptance Criteria:**
- [ ] `StopIpcServer` body contains exactly 3 helper calls and 1 outer catch — no inline logic
- [ ] All original shutdown responsibilities preserved through helper delegation
- [ ] No `lock()` blocks present
- [ ] No Unicode characters in string literals
- [ ] `dotnet build src/` exits 0
- [ ] All existing tests pass

**CYC Impact:** Parent CYC drops from ~11 → 2 (base=1, outer catch=+1). 82% reduction.
All helpers individually satisfy CYC ≤ 8. max_cyc_projected = 5 (CloseIpcClientSession).

---

## W7-078-T6: Verify CYC Compliance

**Title:** Verify all extracted methods satisfy CYC ≤ 8 (max_cyc_projected = 5)

**Description:**
Run the project-wide complexity audit to confirm every method introduced or modified by
this extraction set meets the Jane Street standard of CYC ≤ 8. The max projected CYC
for this extraction is 5 (CloseIpcClientSession). The parent StopIpcServer must read 2.

**Acceptance Criteria:**
- [ ] `python scripts/complexity_audit.py` shows no new violations in `V12_002.UI.IPC.Server.cs`
- [ ] `StopIpcServer_SignalAndStopListener`: CYC = 2 ✓
- [ ] `StopIpcServer_JoinThread`: CYC = 3 ✓
- [ ] `CloseIpcClientSession`: CYC = 5 ✓ (max_cyc_projected)
- [ ] `StopIpcServer_CloseAllClients`: CYC = 3 ✓
- [ ] `StopIpcServer` (parent): CYC = 2 ✓
- [ ] All extracted methods CYC ≤ 8 (Jane Street threshold)
- [ ] `dotnet build src/` exits 0 after all extractions
- [ ] No regressions in existing test suite

**CYC Impact:** Validates cyc compliance across entire extraction set. Confirms max_cyc_projected = 5.

---

## W7-078-T7: Update Manifest

**Title:** Update EPIC-W7-078 manifest to mark Phase 4 complete and Phase 5 ready

**Description:**
After all tickets (T1–T6) are written, update `docs/brain/EPIC-W7-078/manifest.json` to
record Phase 4 completion and signal Phase 5 can begin. This is the final bookkeeping
ticket in the Phase 4 ticket generation sequence.

**Acceptance Criteria:**
- [ ] `manifest.json` field `phase_4.status` = `"completed"`
- [ ] `manifest.json` field `phase_4.output` = `"04-tickets.md"`
- [ ] `manifest.json` field `phase_5.status` = `"pending"` (ready to execute)
- [ ] All 7 ticket IDs (W7-078-T1 through W7-078-T7) referenced in manifest tickets array
- [ ] `manifest.json` is valid JSON (no syntax errors)

**CYC Impact:** No code change. Bookkeeping only.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Method** | StopIpcServer |
| **Source File** | src/V12_002.UI.IPC.Server.cs |
| **Original CYC** | ~11 |
| **extraction_count** | 4 |
| **max_cyc_projected** | 5 |
| **parent_cyc_after** | 2 |
| **ticket_count** | 7 |
| **DNA Verdict** | PASS |
| **Bobcoins Used** | 3 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **Input** | 02-architecture-plan.md, 03-audit-report.md |
| **Output** | 04-tickets.md |
