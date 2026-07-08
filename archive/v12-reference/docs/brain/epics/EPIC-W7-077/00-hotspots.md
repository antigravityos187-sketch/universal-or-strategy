# EPIC-W7-077 — Phase 0: Hotspot Analysis
**Wave**: 7 | **Phase**: 0 | **Method**: `ProcessClientStream` | **Source**: `src/V12_002.UI.IPC.Server.cs`

---

## 1. Complexity Verdict

| Metric | Value | Threshold | Status |
|---|---|---|---|
| Cyclomatic Complexity (CYC) | **0** (reported) | ≤ 8 | ✅ PASS |
| Structural branches in body | 6 | — | See note |
| Lines of method body | 34 | — | Nominal |

> **Note on CYC=0**: The tool-reported CYC of 0 reflects the method's state *after* prior extraction work. The original
> pre-extraction body held all read/decode/extract/dispatch logic inline; those branches now live in four dedicated
> private helpers (`_ReadChunk`, `_DecodeUtf8`, `_ExtractLines`, `_DispatchLine`). The orchestrator loop itself
> contains 6 structural decision points (`while`, 2× `if`, `if`, `if`, `foreach`) — a true CYC of ~7, just under the
> project threshold of 8 (Jane Street GODMODE, `src/AGENTS.md` §3).

---

## 2. Method Anatomy

```
ProcessClientStream(IpcClientSession session)          [lines 221-255]
 ├── while (isIpcRunning && client.Connected)           ← loop guard
 │    ├── ProcessClientStream_ReadChunk()               ← I/O poll + sleep
 │    │    ├── if bytesRead < 0  → continue             ← no data yet
 │    │    └── if bytesRead == 0 → break                ← peer disconnect
 │    ├── ProcessClientStream_DecodeUtf8()              ← strict UTF-8
 │    │    └── if !ok            → break                ← invalid payload
 │    ├── ProcessClientStream_ExtractLines()            ← newline framing
 │    │    ├── if disconnectClient → break              ← buffer overflow
 │    │    └── if lines == null    → continue           ← partial frame
 │    └── foreach line → ProcessClientStream_DispatchLine()
 │                         └── HandleIncomingIpcLine()
 │                              ├── _RespondLayout()    ← sync GET_LAYOUT
 │                              ├── _TryEnqueueCommand()
 │                              └── _TriggerProcessing()
 └── [HandleClient finally: remove from connectedClients, socket shutdown]
```

---

## 3. Call Chain & Blast Radius

| Caller | File | Notes |
|---|---|---|
| `HandleClient` | `V12_002.UI.IPC.Server.cs:179` | Wraps in `using NetworkStream`; owns session lifetime |
| `Task.Run(HandleClient)` | `ListenForRemote` line 99 | One task per accepted TCP client |

| Callee | File | CYC | Risk |
|---|---|---|---|
| `ProcessClientStream_ReadChunk` | Server.cs:257 | ~2 | `DataAvailable` busy-spin → 50 ms sleep |
| `ProcessClientStream_DecodeUtf8` | Server.cs:268 | ~2 | `_ipcInvalidUtf8Count` telemetry |
| `ProcessClientStream_ExtractLines` | Server.cs:292 | ~5 | `IpcMaxBufferedChars=8192` guard; double overflow check |
| `ProcessClientStream_DispatchLine` | Server.cs:332 | 1 | Thin shim |
| `HandleIncomingIpcLine` | Server.cs:337 | ~4 | Routes GET_LAYOUT or enqueues command |
| `TryEnqueueIpcCommand` | `V12_002.UI.IPC.cs:150` | — | `IpcMaxQueueDepth=2000` gate |
| `TriggerCustomEvent → ProcessIpcCommands` | `V12_002.UI.IPC.cs:378` | **61** | **Primary downstream hotspot (EPIC-CCN-2)** |

**Shared state written by this call chain:**
- `_ipcInvalidUtf8Count` (atomic int, `V12_002.UI.IPC.cs:45`)
- `ipcCommandQueue` (ConcurrentQueue, `V12_002.cs:647`)
- `ipcQueuedCommandCount` (atomic int, `V12_002.UI.IPC.cs:43`)
- `connectedClients` (ConcurrentDictionary, `V12_002.cs:650`) — removed in `HandleClient` finally

---

## 4. Identified Risks

### R-01 · Busy-Wait I/O Poll (Medium)
`ProcessClientStream_ReadChunk` spins on `DataAvailable` with a 50 ms `Thread.Sleep`. Under load with many
concurrent clients (each on its own `Task.Run` thread-pool task), this saturates thread-pool slots and degrades
throughput. Mitigation path: migrate to `async/await` with `ReadAsync`.

### R-02 · Double Buffer-Overflow Check (Low)
`ProcessClientStream_ExtractLines` checks `IpcMaxBufferedChars` twice — before and after the residue append.
The second check is theoretically reachable only if a single line is wider than the buffer cap, which is benign
but represents dead-logic complexity.

### R-03 · Stream Ownership Mismatch (Low)
`HandleClient` opens `using (NetworkStream stream = session.Stream)` yet `session.Stream` is also accessed
directly by `HandleIncomingIpcLine_RespondLayout`. The `using` disposes the stream on exit; callee writes prior
to dispose are safe, but the pattern is fragile if the code is later reorganised.

### R-04 · Downstream CYC-61 Funnel (High — tracked)
Every successfully parsed line ultimately reaches `ProcessIpcCommands` (CYC 61), the primary complexity
hotspot for EPIC-CCN-2. `ProcessClientStream` is the sole ingestion path, making it the **upstream gatekeeper**
for that risk.

---

## 5. Hotspot Classification

| Symbol | File | CYC | Priority | Epic |
|---|---|---|---|---|
| `ProcessClientStream` | UI.IPC.Server.cs | 0 (tool) / ~7 (structural) | MONITOR | EPIC-W7-077 |
| `ProcessIpcCommands` | UI.IPC.cs | **61** | HIGH | EPIC-CCN-2 |
| `HandleIncomingIpcLine_RespondLayout` | UI.IPC.Server.cs | ~8 | WATCH | — |
| `ProcessClientStream_ExtractLines` | UI.IPC.Server.cs | ~5 | LOW | — |

---

## 6. Sequential Thinking Summary

1. **CYC=0 is an artefact of prior decomposition**, not evidence of a trivial method. The orchestration loop
   holds ~7 real branches. This is compliant with the ≤8 mandate and no immediate refactoring is required.

2. **The blast radius is bounded upstream** — `ProcessClientStream` is reachable only from `HandleClient`,
   which runs on an isolated `Task.Run` per accepted connection. Side-effects are limited to atomic counters
   and `ConcurrentQueue`/`ConcurrentDictionary` operations, satisfying the Lock-Free Actor mandate.

3. **The real complexity debt lies downstream** in `ProcessIpcCommands` (CYC 61). Phase 0 confirms
   `ProcessClientStream` is a well-decomposed ingestion gate; subsequent wave phases should focus on
   the command-dispatch layer (EPIC-CCN-2) rather than this orchestrator.

---

## 7. Outputs & Next Steps

- **Status**: ✅ Phase 0 complete — no immediate action required on `ProcessClientStream`.
- **Recommended next phase**: Phase 1 — audit `ProcessIpcCommands` (CYC 61) decomposition candidates.
- **Telemetry counters to monitor**: `_ipcInvalidUtf8Count`, `_ipcCleanupFailures`, `_ipcZombieConnections`.
- **Deploy gate**: Any src/ change requires `powershell -File .\deploy-sync.ps1` (83 hard-linked files).

---
*Generated: Wave 7 / EPIC-W7-077 / Phase 0 — Hotspot Analysis*
