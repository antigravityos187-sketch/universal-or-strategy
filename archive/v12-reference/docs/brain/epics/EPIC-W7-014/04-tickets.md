# 04-Tickets — EPIC-W7-014

## Epic Metadata

| Field | Value |
|-------|-------|
| Epic ID | EPIC-W7-014 |
| Wave | 7 |
| Phase | 4 — Ticket Generation |
| Agent | v12-phase4-tickets |
| Method | `TryHandleFleetCommand` |
| Source File | `src/V12_002.UI.IPC.Commands.Fleet.cs` |
| CYC (MCP-confirmed) | **20** |
| ticket_count | **3** |

---

## MCP Evidence (Phase 4)

| Tool | Inputs | Result |
|------|--------|--------|
| `resolve_repo` | `path="/home/malhitticrypto/universal-or-strategy"` | `found=true`, `indexed=true`, repo=`antigravityos187-sketch/universal-or-strategy`, 5147 symbols, status=loadable |
| `get_symbol_complexity` | `symbol_id="src/V12_002.UI.IPC.Commands.Fleet.cs::V12_002.TryHandleFleetCommand#method"` | `cyclomatic=20`, `max_nesting=2`, `param_count=3`, `lines=45`, `assessment="high"` |
| `get_extraction_candidates` | `file_path="src/V12_002.UI.IPC.Commands.Fleet.cs"` | `candidates=[]` (expected — leaf helpers already extracted; parent if-chain is the remaining complexity) |

---

## Sequential Thinking Evidence (Phase 4)

### Thought 1 — Ticket Count Decision
The architecture plan groups the 19 if-checks in `TryHandleFleetCommand` into 3 semantic sub-dispatchers: `TryHandleFleet_BasicOps` (6 if-checks, CYC=7), `TryHandleFleet_DirectionalOps` (7 if-checks, CYC=8), `TryHandleFleet_StateOps` (5 if-checks, CYC=6). Each new helper owns one distinct concern. **ticket_count = 3**.

### Thought 2 — Per-Ticket Line / Helper Details
- **Ticket 1** (`TryHandleFleet_BasicOps`): ~12 lines — 6 if-dispatch blocks (Trim, Lock50, FlattenOnly, Flatten, CancelAll, ResetMemory). Signature: `private bool TryHandleFleet_BasicOps(string action, string[] parts, string cmdId)`. Projected CYC: 7. CYC reduction from parent: 6.
- **Ticket 2** (`TryHandleFleet_DirectionalOps`): ~14 lines — 7 if-dispatch blocks (LongShort, OrLong, OrShort, TrendManualLimit, RetestManualLimit, FfmaManualLimit, FfmaManualMarket). Signature: `private bool TryHandleFleet_DirectionalOps(string action, string[] parts, string cmdId)`. Projected CYC: 8. CYC reduction from parent: 7.
- **Ticket 3** (`TryHandleFleet_StateOps`): ~10 lines — 5 if-dispatch blocks (CloseTarget, MoveTarget, FleetState, ToggleAccount, SetShadow). Signature: `private bool TryHandleFleet_StateOps(string action, string[] parts)`. Projected CYC: 6. CYC reduction from parent: 5.

### Thought 3 — CYC Compliance Verification
All projected values checked against Jane Street strict standard (threshold = 8):
- `TryHandleFleetCommand` (parent after all extractions): CYC 5 — **PASS**
- `TryHandleFleet_BasicOps`: CYC 7 — **PASS**
- `TryHandleFleet_DirectionalOps`: CYC 8 — **PASS** (at limit, acceptable)
- `TryHandleFleet_StateOps`: CYC 6 — **PASS**

**max_cyc_projected = 8 <= 8. All methods compliant. Hypothesis verified.**

---

## Tickets

---

### Ticket 1 of 3

| Field | Value |
|-------|-------|
| ticket_id | 1 |
| helper_name | `TryHandleFleet_BasicOps` |
| concern | Routes basic flat/cancel/reset fleet commands — Trim, Lock50, FlattenOnly, Flatten, CancelAll, ResetMemory |
| lines_to_move | The 6 sequential `if (TryHandleFleet_Trim(...)) return true;` ... `if (TryHandleFleet_ResetMemory(...)) return true;` blocks from the parent dispatcher body (~12 lines) |
| cyc_reduction | 6 (6 if-branch points removed from parent) |
| projected_helper_cyc | **7** (base=1 + 6 if-checks) |
| jane_street_compliant | YES (7 <= 8) |

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool TryHandleFleet_BasicOps(string action, string[] parts, string cmdId)
```

**Body structure:**
```csharp
if (TryHandleFleet_Trim(action, parts, cmdId)) return true;
if (TryHandleFleet_Lock50(action, parts, cmdId)) return true;
if (TryHandleFleet_FlattenOnly(action, parts, cmdId)) return true;
if (TryHandleFleet_Flatten(action, parts, cmdId)) return true;
if (TryHandleFleet_CancelAll(action, parts, cmdId)) return true;
if (TryHandleFleet_ResetMemory(action, parts, cmdId)) return true;
return false;
```

---

### Ticket 2 of 3

| Field | Value |
|-------|-------|
| ticket_id | 2 |
| helper_name | `TryHandleFleet_DirectionalOps` |
| concern | Routes directional and entry-order fleet commands — LongShort, OrLong, OrShort, TrendManualLimit, RetestManualLimit, FfmaManualLimit, FfmaManualMarket |
| lines_to_move | The 7 sequential `if (TryHandleFleet_LongShort(...)) return true;` ... `if (TryHandleFleet_FfmaManualMarket(...)) return true;` blocks from the parent dispatcher body (~14 lines) |
| cyc_reduction | 7 (7 if-branch points removed from parent) |
| projected_helper_cyc | **8** (base=1 + 7 if-checks) |
| jane_street_compliant | YES (8 <= 8, at limit) |

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool TryHandleFleet_DirectionalOps(string action, string[] parts, string cmdId)
```

**Body structure:**
```csharp
if (TryHandleFleet_LongShort(action, parts, cmdId)) return true;
if (TryHandleFleet_OrLong(action, parts, cmdId)) return true;
if (TryHandleFleet_OrShort(action, parts, cmdId)) return true;
if (TryHandleFleet_TrendManualLimit(action, parts, cmdId)) return true;
if (TryHandleFleet_RetestManualLimit(action, parts, cmdId)) return true;
if (TryHandleFleet_FfmaManualLimit(action, parts, cmdId)) return true;
if (TryHandleFleet_FfmaManualMarket(action, parts, cmdId)) return true;
return false;
```

---

### Ticket 3 of 3

| Field | Value |
|-------|-------|
| ticket_id | 3 |
| helper_name | `TryHandleFleet_StateOps` |
| concern | Routes state and target management fleet commands — CloseTarget, MoveTarget, FleetState, ToggleAccount, SetShadow |
| lines_to_move | The 5 sequential `if (TryHandleFleet_CloseTarget(...)) return true;` ... `if (TryHandleFleet_SetShadow(...)) return true;` blocks from the parent dispatcher body (~10 lines) |
| cyc_reduction | 5 (5 if-branch points removed from parent) |
| projected_helper_cyc | **6** (base=1 + 5 if-checks) |
| jane_street_compliant | YES (6 <= 8) |

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool TryHandleFleet_StateOps(string action, string[] parts)
```

**Body structure:**
```csharp
if (TryHandleFleet_CloseTarget(action, parts)) return true;
if (TryHandleFleet_MoveTarget(action, parts)) return true;
if (TryHandleFleet_FleetState(action, parts)) return true;
if (TryHandleFleet_ToggleAccount(action, parts)) return true;
if (TryHandleFleet_SetShadow(action, parts)) return true;
return false;
```

---

## Parent Method After All Extractions

```csharp
private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)
{
    string cmdId =
        senderTicks > 0
            ? action + "|" + senderTicks.ToString()
            : action + "|" + (DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute).ToString();

    if (TryHandleFleet_BasicOps(action, parts, cmdId))
        return true;
    if (TryHandleFleet_DirectionalOps(action, parts, cmdId))
        return true;
    if (TryHandleFleet_StateOps(action, parts))
        return true;
    return false;
}
```

---

## Projected CYC Summary (All Methods)

| Method | CYC Before | CYC After | Delta | Compliant (<= 8) |
|--------|-----------|-----------|-------|-----------------|
| `TryHandleFleetCommand` (parent) | 20 | **5** | -15 | YES |
| `TryHandleFleet_BasicOps` (new) | — | **7** | new | YES |
| `TryHandleFleet_DirectionalOps` (new) | — | **8** | new | YES (at limit) |
| `TryHandleFleet_StateOps` (new) | — | **6** | new | YES |

**projected_parent_cyc_after_all: 5**
**max_cyc_projected: 8**
**All methods compliant with Jane Street strict standard (<= 8).**

---

## DNA Audit Passthrough

| Check | Status |
|-------|--------|
| dna_verdict (Phase 3) | PASS |
| violations | [] |
| lock() blocks | NONE |
| ASCII-only strings | CONFIRMED |
| Scope creep | NONE |
| max_cyc_projected <= 8 | CONFIRMED |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase4-tickets |
| Bobcoins Used | 5 |
| Execution Time | ~50s |
| MCP Tools Called | resolve_repo, sequentialthinking (probe), get_symbol_complexity, get_extraction_candidates, sequentialthinking (x3 thoughts) |
| Sequential Thoughts | 4 (1 probe + 3 ticket-breakdown thoughts) |
| Phase | 4 — Ticket Generation |
| Status | COMPLETE |
| ticket_count | 3 |
| projected_parent_cyc_after_all | 5 |
| max_cyc_projected | 8 |
