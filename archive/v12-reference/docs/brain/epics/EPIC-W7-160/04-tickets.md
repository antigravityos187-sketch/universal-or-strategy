# Phase 4: Tickets — EPIC-W7-160

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-160/02-architecture-plan.md + docs/brain/EPIC-W7-160/03-audit-report.md

---

## Method Under Extraction

| Field | Value |
|---|---|
| **Method** | `SendResponseToRemote` |
| **Source File** | [`src/V12_002.UI.IPC.Commands.Misc.cs`](src/V12_002.UI.IPC.Commands.Misc.cs) |
| **Class** | `V12_002` (partial, `Strategy`) |
| **Original CYC** | 10 |
| **target_cyc** | <= 8 (Jane Street strict standard) |

---

## ticket_count: 2

---

## Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-160-T1 |
| **helper_name** | `TrySendToClient` |
| **concern** | Attempt TCP write to a single connected `IpcClientSession`; on failure or disconnected/non-writable state, add `clientId` to the `disconnectedClientIds` list |
| **signature** | `private void TrySendToClient(int clientId, IpcClientSession session, byte[] responseBytes, List<int> disconnectedClientIds)` |
| **lines_to_move** | Inner body of the per-client `foreach` send loop in `SendResponseToRemote` (approximately lines 221–240 of `src/V12_002.UI.IPC.Commands.Misc.cs`) |
| **cyc_reduction** | -3 (parent CYC: 10 → 7) |
| **projected_helper_cyc** | **4** |
| **sequencing** | Must execute BEFORE Ticket 2 |

### Ticket 1 — Implementation Steps

1. Create `private void TrySendToClient(int clientId, IpcClientSession session, byte[] responseBytes, List<int> disconnectedClientIds)` as a new private method in [`src/V12_002.UI.IPC.Commands.Misc.cs`](src/V12_002.UI.IPC.Commands.Misc.cs).
2. Move the inner foreach body from `SendResponseToRemote`'s send loop into `TrySendToClient`:
   - `if (session.Client.Connected && session.Stream.CanWrite)` → write `responseBytes` via `session.Stream.Write` + `session.Stream.Flush`
   - `else` → `disconnectedClientIds.Add(clientId)`
   - Wrap in `try { ... } catch { disconnectedClientIds.Add(clientId); }`
3. Replace the moved inner body in `SendResponseToRemote`'s foreach with a single call: `TrySendToClient(kvp.Key, kvp.Value, responseBytes, disconnectedClientIds);`
4. Verify: `TrySendToClient` CYC = 4, parent CYC = 7.
5. Build must pass: `dotnet build src/` with zero errors.

### Ticket 1 — CYC Breakdown for `TrySendToClient`

| Branch | +CYC |
|---|---|
| Base | 1 |
| `if (session.Client.Connected && session.Stream.CanWrite)` | +1 |
| `else` / not-connected branch | +1 |
| `catch` block | +1 |
| **Total** | **4** |

---

## Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-160-T2 |
| **helper_name** | `CleanupStaleClient` |
| **concern** | Remove stale entry from `connectedClients` `ConcurrentDictionary` via `TryRemove`; if successful, close `staleClient.Client`; on exception log error and `Interlocked.Increment(ref _ipcCleanupFailures)` |
| **signature** | `private void CleanupStaleClient(int clientId)` |
| **lines_to_move** | Inner body of the stale-client cleanup `foreach` loop in `SendResponseToRemote` (approximately lines 241–256 of `src/V12_002.UI.IPC.Commands.Misc.cs`) |
| **cyc_reduction** | -2 (parent CYC: 7 → 5 after Ticket 1 already applied) |
| **projected_helper_cyc** | **3** |
| **sequencing** | Must execute AFTER Ticket 1 |

### Ticket 2 — Implementation Steps

1. Create `private void CleanupStaleClient(int clientId)` as a new private method in [`src/V12_002.UI.IPC.Commands.Misc.cs`](src/V12_002.UI.IPC.Commands.Misc.cs).
2. Move the inner cleanup foreach body from `SendResponseToRemote` into `CleanupStaleClient`:
   - `if (connectedClients.TryRemove(clientId, out var staleClient))` → `staleClient.Client.Close()`
   - Wrap in `try { ... } catch { /* log error */ Interlocked.Increment(ref _ipcCleanupFailures); }`
3. Replace the moved inner body in `SendResponseToRemote`'s cleanup foreach with a single call: `CleanupStaleClient(clientId);`
4. Verify: `CleanupStaleClient` CYC = 3, parent CYC = 5.
5. Build must pass: `dotnet build src/` with zero errors.

### Ticket 2 — CYC Breakdown for `CleanupStaleClient`

| Branch | +CYC |
|---|---|
| Base | 1 |
| `if (connectedClients.TryRemove(...))` | +1 |
| `catch` block | +1 |
| **Total** | **3** |

---

## Parent Method After All Tickets Applied

### `SendResponseToRemote` — Remaining Logic

1. `if (connectedClients == null) return;` — guard clause
2. Diagnostic log if `response.Contains("SYNC_TARGET_STATE")`
3. UTF-8 encode `response + "\n"` into `byte[] responseBytes`
4. Allocate `List<int> disconnectedClientIds`
5. `foreach (var kvp in connectedClients.ToArray())` → `TrySendToClient(kvp.Key, kvp.Value, responseBytes, disconnectedClientIds)`
6. `foreach (int clientId in disconnectedClientIds)` → `CleanupStaleClient(clientId)`

### projected_parent_cyc_after_all: 5

| Branch | +CYC |
|---|---|
| Base | 1 |
| `if (connectedClients == null)` | +1 |
| `if response.Contains("SYNC_TARGET_STATE")` | +1 |
| `foreach` send loop | +1 |
| `foreach` cleanup loop | +1 |
| **Total** | **5** |

---

## Full CYC Summary

| Method | Projected CYC | <= 8? |
|---|---|---|
| `SendResponseToRemote` (parent, after all tickets) | 5 | **YES** |
| `TrySendToClient` (helper 1) | 4 | **YES** |
| `CleanupStaleClient` (helper 2) | 3 | **YES** |
| **Max across all 3** | **5** | **YES** |

Original CYC: 10 → Maximum projected: 5 (50% reduction)

---

## MCP Evidence

### Tool: `resolve_repo`
- **Path:** `/home/malhitticrypto/universal-or-strategy`
- **Result:** `found=true, indexed=true, repo=antigravityos187-sketch/universal-or-strategy`
- **Symbol count:** 5147 | **File count:** 2000

### Tool: `get_symbol_complexity` — `SendResponseToRemote`
- **File:** `src/V12_002.UI.IPC.Commands.Misc.cs`
- **Result:** `{"error":"Symbol 'SendResponseToRemote' not found in index."}`
- **Note:** Symbol absent from current index snapshot (index captured at 2026-06-29T01:05:21Z before Phase 2 analysis); CYC=10 confirmed via Phase 0 complexity_audit.py output and Phase 2 jcodemunch get_context_bundle analysis. Architecture plan documents original CYC=10 at line 18.

### Tool: `get_extraction_candidates`
- **File:** `src/V12_002.UI.IPC.Commands.Misc.cs`
- **Result:** `candidates=[], min_complexity=5, min_callers=2`
- **Note:** Expected result — extraction candidates tool requires pre-existing callers of helpers. Helpers `TrySendToClient` and `CleanupStaleClient` do not yet exist; they are being designed in this phase. Absence of candidates is correct behavior at Phase 4.

---

## Sequential Thinking Evidence

### Thought 1 — Ticket Count Analysis
**Content:** Identified 2 distinct logical concerns within the `SendResponseToRemote` method body matching the Phase 2 architecture plan. Per-client TCP send loop body (→ Ticket 1: `TrySendToClient`) and stale client cleanup loop body (→ Ticket 2: `CleanupStaleClient`). Ticket 2 depends on Ticket 1 completing first (sequential execution required).

### Thought 2 — Per-Ticket Detail Breakdown
**Content:**
- Ticket 1 (`TrySendToClient`): moves inner send foreach body; CYC=4 (base+if-connected+else+catch); parent loses ~3 CYC (10→7).
- Ticket 2 (`CleanupStaleClient`): moves inner cleanup foreach body; CYC=3 (base+if-TryRemove+catch); parent loses ~2 CYC (7→5).
- Total: 2 tickets. Sequencing: T1 before T2.

### Thought 3 — CYC Verification
**Content:** Final state after all tickets: `SendResponseToRemote`=5, `TrySendToClient`=4, `CleanupStaleClient`=3. All three <= 8 (Jane Street threshold). Maximum CYC=5. Original CYC=10 reduced 50%. Jane Street lock-free preserved: `ConcurrentDictionary.TryRemove` (atomic) + `Interlocked.Increment` (atomic); no `lock()` blocks. Verification: PASS.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | ~10 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **Wave** | 7 |
| **Phase** | 4 |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 breakdown thoughts) |
| **Input** | docs/brain/EPIC-W7-160/02-architecture-plan.md, docs/brain/EPIC-W7-160/03-audit-report.md |
| **Output** | docs/brain/EPIC-W7-160/04-tickets.md |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 5 |
