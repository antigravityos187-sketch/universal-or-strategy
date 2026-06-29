# Phase 2: Architecture Plan — EPIC-W7-078

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2
**Generated:** 2026-06-29T01:10:00Z

---

## Method Under Extraction

- **Method:** `StopIpcServer`
- **Source File:** `src/V12_002.UI.IPC.Server.cs`
- **Lines:** 451–510
- **Original CYC:** ~11 (seed of 0 was unanalyzed placeholder; measured via static branch count)

- **jcodemunch get_context_bundle result:** Resolved symbol `src/V12_002.UI.IPC.Server.cs::V12_002.StopIpcServer#method` (line 451, `private void StopIpcServer()`). Full source retrieved: 60-line method body containing 3-level nested try/catch, foreach over `connectedClients.ToArray()`, and 4 distinct responsibilities — signal stop, listener teardown, thread join, and client cleanup. Initial call with bare `StopIpcServer` returned "not found" (ambiguous); disambiguated using full symbol ID.

- **jcodemunch get_call_hierarchy result:** get_call_hierarchy (direction=both, depth=2) identified 1 direct caller — `StartIpcServer` (line 52, guard call) — and 6 callees: `ipcListener` (field, both src/ and src-vm-backup/ copies), `ipcThread` (field), and `connectedClients` (field). No higher-level callers of `StopIpcServer` itself beyond `StartIpcServer` visible in the index; `OnStateChange` caller referenced in hotspots is not indexed at depth=1.

- **jcodemunch get_dependency_graph result:** get_dependency_graph (direction=both, depth=1) on `src/V12_002.UI.IPC.Server.cs` returned 1 node, 0 edges, no imports or importers tracked. The file is a partial class slice with no standalone import graph edges — all dependencies flow through the parent `V12_002.cs` partial class assembly, which is consistent with NinjaTrader partial-class architecture.

- **jcodemunch get_extraction_candidates result:** get_extraction_candidates (min_complexity=3, min_callers=1) returned empty — the static analysis index does not have multi-caller evidence for helpers-to-be, which is expected since the helpers do not yet exist. The hotspot analysis (00-hotspots.md) provides the authoritative complexity breakdown (CYC ~11) used as input.

---

## Sequential Thinking Summary

The sequentialthinking chain (5 thoughts) validated the extraction plan as follows:
- **Thought 1:** Confirmed actual CYC=11 from hotspots; identified 4 distinct responsibilities in the method body via get_context_bundle source.
- **Thought 2:** Mapped each responsibility to a helper boundary and calculated per-responsibility CYC contribution (A: signal+stop=CYC 2, B: thread join=CYC 3, C: per-client close=CYC 5, D: close-all loop=CYC 3).
- **Thought 3:** Projected post-extraction CYC for each helper — all ≤5, all ≤ Jane Street threshold of 8. Parent drops to CYC 2.
- **Thought 4:** Verified all Jane Street KB rules: CYC≤8 ✓, single-responsibility ✓, no lock() (Interlocked preserved) ✓, illegal-states guard clauses moved to helper early-returns ✓, zero new heap allocations ✓.
- **Thought 5:** Final verdict — 4-helper extraction plan complete, max projected CYC=5, extraction_count=4.

---

## Extraction Plan

| Helper Method Name | Signature | Responsibility | Projected CYC |
|---|---|---|---|
| `StopIpcServer_SignalAndStopListener` | `private void StopIpcServer_SignalAndStopListener()` | Sets `isIpcRunning = false`; null-guards then calls `ipcListener.Stop()` and nulls the field | 2 |
| `StopIpcServer_JoinThread` | `private void StopIpcServer_JoinThread()` | Null-and-alive guard on `ipcThread`; calls `ipcThread.Join(500)` | 3 |
| `CloseIpcClientSession` | `private void CloseIpcClientSession(IpcClientSession session, string clientId)` | Single-client socket shutdown via `SocketShutdown.Both`; zombie catch with `Interlocked.Increment(_ipcZombieConnections)`; socket `Close()`; per-client catch with `Interlocked.Increment(_ipcCleanupFailures)`. Deduplicates from `HandleClient` lines 193–217. | 5 |
| `StopIpcServer_CloseAllClients` | `private void StopIpcServer_CloseAllClients()` | Null-guards `connectedClients`; iterates `.ToArray()`; delegates each entry to `CloseIpcClientSession`; calls `connectedClients.Clear()`; resets `Interlocked.Exchange(ref ipcQueuedCommandCount, 0)` | 3 |

---

## Parent Method After Extraction

**Remaining logic in `StopIpcServer()` after extraction:**
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

- **Remaining logic:** 3 sequential helper calls inside a single outer catch
- **Projected CYC:** 2 (base=1, outer catch=+1)

---

## max_cyc_projected: 5
## extraction_count: 4

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 achieved | YES — max projected CYC=5, parent CYC=2 |
| Single-responsibility per helper | YES — each helper does exactly one thing |
| Lock-free/Actor pattern preserved | YES — all Interlocked primitives retained in helpers, no lock() blocks added |
| Illegal states unrepresentable | YES — null guard clauses moved to helper early-returns; invalid state cannot reach inner logic |
| Zero-allocation hot paths | YES — no new heap allocations; ToArray() unavoidable (pre-existing) |
| Extract Guard Clauses applied | YES — null checks become early-return guards in each helper |
| Deduplication bonus | YES — CloseIpcClientSession eliminates copy with HandleClient lines 193-217 |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 3 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **Method** | StopIpcServer |
| **Original CYC** | ~11 |
| **max_cyc_projected** | 5 |
| **extraction_count** | 4 |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Output** | docs/brain/EPIC-W7-078/02-architecture-plan.md |
