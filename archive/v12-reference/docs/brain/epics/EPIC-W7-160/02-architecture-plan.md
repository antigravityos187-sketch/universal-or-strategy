# Phase 2: Architecture Plan — EPIC-W7-160

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-160/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `SendResponseToRemote`
- **Source File:** `src/V12_002.UI.IPC.Commands.Misc.cs`
- **Class:** `V12_002` (partial, `Strategy`)
- **Visibility:** `private void`
- **Lines:** 206–258 (52 LOC)
- **Original CYC:** 10

### jcodemunch get_context_bundle result

Full source retrieved. Method performs three distinct logical phases:
1. Guard + diagnostic log (null check on `connectedClients`, log if SYNC_TARGET_STATE response)
2. Per-client send loop: iterates `connectedClients.ToArray()`, attempts `Stream.Write`/`Flush`, marks client ID in `disconnectedClientIds` on failure or disconnected state, wrapped in try/catch
3. Stale client cleanup loop: iterates `disconnectedClientIds`, calls `connectedClients.TryRemove` + `staleClient.Client.Close()` with try/catch and `Interlocked.Increment(ref _ipcCleanupFailures)` on error

**Signature:** `private void SendResponseToRemote(string response)`
**Key types used:** `IpcClientSession` (`.Client`, `.Stream`), `ConcurrentDictionary<int, IpcClientSession> connectedClients`, `Interlocked`

### jcodemunch get_call_hierarchy result

**Callers (depth 1):**
- `HandleFleet_GetFleet` — line 96, `src/V12_002.UI.IPC.Commands.Misc.cs`
- `HandleFleet_RequestFleetState` — line 174, `src/V12_002.UI.IPC.Commands.Misc.cs`

**Callers (depth 2):**
- `HandleFleetCommand` — line 83, same file (calls HandleFleet_GetFleet/HandleFleet_RequestFleetState)

**Callees:**
- `connectedClients` constant — `src/V12_002.cs` line 650 (ConcurrentDictionary field on partial class)

All callers are within the same partial class file. No external callers confirmed. Signature must remain unchanged.

### jcodemunch get_dependency_graph result

- **Direction:** both (imports + importers)
- **Result:** No cross-file import edges. `src/V12_002.UI.IPC.Commands.Misc.cs` has 0 importers and 0 explicit import edges in the graph (single partial-class file pattern). Blast radius is fully contained within the file.

### jcodemunch get_extraction_candidates result

- **Candidates returned:** 0 (expected — extraction candidates require pre-existing callers of helpers that don't exist yet)
- **Note:** The absence of candidates is correct at Phase 2; helpers are being designed here.

---

## Sequential Thinking Summary

**Thought 1:** Analyzed full method source. CYC=10 driven by: null guard (1), Contains diagnostic (1), outer foreach (1), if Connected&&CanWrite (1), catch send (1), cleanup foreach (1), if TryRemove (1), catch cleanup (1) — confirming 8+ branches needing reduction.

**Thought 2:** Identified primary extraction: `TrySendToClient` removes inner foreach body (if/else + try/catch). Parent loses 2+ CYC points, falling to CYC=7. A second extraction `CleanupStaleClient` removes cleanup loop body, bringing parent to CYC=5.

**Thought 3:** Evaluated second extraction: warranted by single-responsibility principle. Two distinct concerns (send attempt vs. stale cleanup) justify two helpers. Parent CYC=5 provides comfortable margin below threshold.

**Thought 4:** Validated signatures against Jane Street constraints. Lock-free confirmed: `ConcurrentDictionary.TryRemove` is atomic, `Interlocked.Increment` is atomic, no `lock()` blocks anywhere. Zero new allocations introduced by helpers.

**Thought 5:** Final plan — 2 extractions. `TrySendToClient` CYC=4, `CleanupStaleClient` CYC=3, parent CYC=5. All ≤8. Jane Street alignment verified on all four axes.

---

## Extraction Plan

| Helper Method Name | Signature | Responsibility | Projected CYC |
|---|---|---|---|
| `TrySendToClient` | `private void TrySendToClient(int clientId, IpcClientSession session, byte[] responseBytes, List<int> disconnectedClientIds)` | Attempt TCP write to a single connected client session; on failure or disconnected state, add clientId to disconnectedClientIds list | 4 |
| `CleanupStaleClient` | `private void CleanupStaleClient(int clientId)` | Remove stale entry from connectedClients ConcurrentDictionary and close the underlying TCP client; log error and increment `_ipcCleanupFailures` counter on exception | 3 |

---

## Parent Method After Extraction

**Remaining logic in `SendResponseToRemote`:**
1. `if (connectedClients == null) return;` — guard clause
2. Diagnostic log if response contains `"SYNC_TARGET_STATE"`
3. UTF-8 encode response + `"\n"` into `byte[]`
4. Allocate `List<int> disconnectedClientIds`
5. `foreach (var kvp in connectedClients.ToArray())` → call `TrySendToClient(kvp.Key, kvp.Value, responseBytes, disconnectedClientIds)`
6. `foreach (int clientId in disconnectedClientIds)` → call `CleanupStaleClient(clientId)`

**Projected CYC:** 5
- Base: 1
- Null guard: +1
- Contains diagnostic: +1
- foreach send loop: +1
- foreach cleanup loop: +1
- **Total: 5**

---

## max_cyc_projected: 5
## extraction_count: 2

---

## Jane Street Alignment

| Constraint | Status | Notes |
|---|---|---|
| CYC<=8 achieved | **YES** | Parent=5, TrySendToClient=4, CleanupStaleClient=3 |
| Single-responsibility per helper | **YES** | TrySendToClient=send only; CleanupStaleClient=teardown only |
| Lock-free/Actor pattern preserved | **YES** | ConcurrentDictionary.TryRemove + Interlocked.Increment; no lock() blocks |
| Illegal states unrepresentable | **YES** | Session always from live kvp.Value; null session never passed |
| Zero-allocation hot paths | **YES** | No new allocations in helpers; byte[] allocated once in parent |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | ~14 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **jcodemunch tools called** | search_symbols, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Output** | docs/brain/EPIC-W7-160/02-architecture-plan.md |
