# EPIC-W7-078 · Phase 0 — Hotspot Analysis

**Method:** `StopIpcServer`
**Source:** `src/V12_002.UI.IPC.Server.cs` (lines 451–510)
**Wave:** 7 | **Phase:** 0
**Reported CYC (seed):** 0 → **Measured CYC: ~11**

---

## 1. CYC Confirmation

The task seed value of `0` reflects an unanalyzed placeholder. Static branch count of
`StopIpcServer` (base = 1):

| Branch / Decision Point | +CYC |
|---|---|
| `if (ipcListener != null)` | +1 |
| `if (ipcThread != null && ipcThread.IsAlive)` (2 conditions) | +2 |
| `if (connectedClients != null)` | +1 |
| `foreach` loop over `connectedClients.ToArray()` | +1 |
| `if (kvp.Value.Client != null)` | +1 |
| `if (kvp.Value.Client.Connected)` | +1 |
| inner `catch` (zombie shutdown) | +1 |
| outer per-client `catch` | +1 |
| method-level outer `catch` | +1 |
| **Total** | **~11** |

**CYC confirmed: ~11** (seed of `0` was unanalyzed).

---

## 2. Structural Hotspots

### 2a. Multi-Role Monolith
`StopIpcServer` performs four distinct responsibilities in one method body:
1. Signal stop (`isIpcRunning = false`)
2. Tear down `TcpListener`
3. Join background `ipcThread`
4. Iterate + close all active `IpcClientSession` objects

Each responsibility is independently testable and independently failable — they
should be separate helpers (mirroring the `StartIpcServer` decomposition pattern
already used in `ListenForRemote_*` helpers).

### 2b. 3-Level Nested Exception Handling
```
try {                              // method-level outer catch
  ...
  foreach (kvp) {
    try {                          // per-client catch
      if (client.Connected) {
        try {                      // zombie Shutdown catch
          client.Client?.Shutdown(...)
        } catch (shutdownEx) { ... }
      }
      client.Close();
    } catch (ex) { ... }
  }
} catch (ex) { ... }
```
Three nesting levels of try/catch create ambiguous recovery paths and suppress
genuine exceptions from reaching callers.

### 2c. Duplicated Client-Close Logic
The per-client `Shutdown` + `Close` + zombie counter sequence at lines 477–497
is a near-verbatim copy of the same block in `HandleClient` (lines 193–217).
Any fix to one site must be replicated manually to the other.

### 2d. Unsafe Thread Join (500 ms hard-coded)
`ipcThread.Join(500)` at line 463 blocks the calling thread for up to 500 ms
with no cancellation path. If `ListenForRemote` is blocked on `AcceptTcpClient`
(not `ipcListener.Pending()`) this silently times out and the thread is leaked.

---

## 3. Blast Radius

| Caller | File | Call Site |
|---|---|---|
| `StartIpcServer` (guard) | `V12_002.UI.IPC.Server.cs` | line 59 |
| `OnStateChange` teardown | `V12_002.Lifecycle.cs` | line 222 |

**Shared mutable state touched:**

| Field | Declared In |
|---|---|
| `isIpcRunning` (`volatile bool`) | `V12_002.cs:339` |
| `ipcListener` (`TcpListener`) | `V12_002.cs:337` |
| `ipcThread` (`Thread`) | `V12_002.cs:338` |
| `connectedClients` (`ConcurrentDictionary`) | `V12_002.cs:650` |
| `ipcQueuedCommandCount` (`int` via Interlocked) | `V12_002.UI.IPC.cs:43` |
| `_ipcCleanupFailures` / `_ipcZombieConnections` | `V12_002.Data.cs:11-12` |

---

## 4. Recommended Decomposition

Extract three private helpers to match the `ListenForRemote_*` naming convention:
- `StopIpcServer_SignalAndJoinThread()` — set flag, stop listener, join thread
- `StopIpcServer_CloseAllClients()` — iterate connectedClients, delegate per-client
  close to a shared `CloseIpcClientSession(IpcClientSession)` helper (eliminating
  the duplication with `HandleClient`)
- `CloseIpcClientSession(IpcClientSession)` — shared socket shutdown/close + zombie
  telemetry (deduplicated from `HandleClient` lines 193–217)

---

## 5. Summary

`StopIpcServer` is a low-visibility but structurally dense method. Its seed CYC of
`0` is incorrect; measured CYC is **~11**. Primary refactor value lies in eliminating
the duplicated client-close logic and reducing nesting depth, not in CYC reduction
per se. Risk is **low** (teardown path, not hot trading path).
