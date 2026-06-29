# EPIC-W7-154 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29
**Input:** docs/brain/EPIC-W7-154/01-scope-boundary.md

---

## 1. Method Identity (Confirmed)

| Field         | Value                                                                 |
|---------------|-----------------------------------------------------------------------|
| Method Name   | `TryHandleFleet_LongShort`                                            |
| File          | `src/V12_002.UI.IPC.Commands.Fleet.cs`                               |
| Lines         | 383 – 458 (76 lines)                                                  |
| Visibility    | `private`                                                             |
| Return Type   | `bool`                                                                |
| Signature     | `private bool TryHandleFleet_LongShort(string action, string cmdId)` |
| CYC Baseline  | **11**                                                                |
| CYC Target    | **<= 8**                                                              |
| Symbol ID     | `src/V12_002.UI.IPC.Commands.Fleet.cs::V12_002.TryHandleFleet_LongShort#method` |

---

## 2. Complexity Driver Summary

| # | Construct                                              | Lines   | CYC |
|---|--------------------------------------------------------|---------|-----|
| 0 | Base count                                             | —       | +1  |
| 1 | `if (action != "LONG" && action != "SHORT")`           | 385     | +1  |
| 2 | `if (!MetadataGuardDuplicate(cmdId, action))`          | 388     | +1  |
| 3 | `if (isTosSyncMode)` outer branch                      | 392     | +1  |
| 4 | Ternary `action == "LONG" ? isLongArmed : isShortArmed`| 393     | +1  |
| 5 | `if (!armed)` — IGNORED path                           | 394     | +1  |
| 6 | `if (action == "LONG")` — arm-reset branch             | 403     | +1  |
| 7 | `if (EnableSIMA)` — routing fork                       | 409     | +1  |
| 8 | `try / catch` block                                    | 424     | +1  |
| 9 | `if (stopDist <= 0)` — ATR latency fallback            | 417     | +1  |
|10 | `if (EnablePathB)` — PATH B vs market branch           | 430     | +1  |
|   | **Total**                                              |         |**11**|

**Primary complexity drivers:**
- **ToS-Sync gate block (lines 392–406):** Interleaved arm-flag ternary, `!armed` early-return, and `action=="LONG"` arm-reset — orthogonal concern to SIMA dispatch (+3 extractable CYC).
- **ATR sizing try/catch+fallback (lines 413–429):** try/catch, `stopDist<=0` guard, ternary — independently reusable sizing logic (+3 extractable CYC).
- **PATH B routing fork (lines 430–441):** Simple `if (EnablePathB)` dispatch — kept in host (only +1 CYC, minimal complexity).

---

## 3. Caller Analysis

| Caller                 | File                                   | Line | How Called            |
|------------------------|----------------------------------------|------|-----------------------|
| `TryHandleFleetCommand`| `src/V12_002.UI.IPC.Commands.Fleet.cs` | 57   | On `LONG`/`SHORT` match |

**Resolution:** AST-resolved via jCodemunch call hierarchy. Caller count = 1.  
**Caller impact:** NONE — the method signature `private bool TryHandleFleet_LongShort(string action, string cmdId)` is unchanged. Extraction is internal only.

---

## 4. Extraction Plan

### 4.1 Helper 1 — `HandleTosSyncArming`

| Field           | Value                                                          |
|-----------------|----------------------------------------------------------------|
| Method Name     | `HandleTosSyncArming`                                          |
| Signature       | `private bool HandleTosSyncArming(string action)`              |
| Return Semantics| `true` = proceed with entry; `false` = signal ignored, caller returns `true` |
| Location        | Same partial class (`src/V12_002.UI.IPC.Commands.Fleet.cs`)   |
| Visibility      | `private`                                                      |
| CYC of Helper   | **4** (base+1, ternary armed+1, if(!armed)+1, if(action=="LONG")+1) |

**Extracted logic (lines 393–406 from host):**

```csharp
// Before extraction — inside if (isTosSyncMode) { ... }
bool armed = (action == "LONG") ? isLongArmed : isShortArmed;
if (!armed)
{
    Print($"[SYNC] ToS Signal IGNORED: {action} received but {action} is not ARMED locally.");
    return true; // suppress — becomes return false in helper
}
else
{
    Print($"[SYNC] ToS Handshake Received -> Executing {action} Fleet Entry");
    if (action == "LONG")
        isLongArmed = false;
    else
        isShortArmed = false;
}
```

**New helper signature:**

```csharp
private bool HandleTosSyncArming(string action)
{
    bool armed = (action == "LONG") ? isLongArmed : isShortArmed;
    if (!armed)
    {
        Print($"[SYNC] ToS Signal IGNORED: {action} received but {action} is not ARMED locally.");
        return false;
    }
    Print($"[SYNC] ToS Handshake Received -> Executing {action} Fleet Entry");
    if (action == "LONG")
        isLongArmed = false;
    else
        isShortArmed = false;
    return true;
}
```

**Host call site replacement (lines 392–406):**

```csharp
if (isTosSyncMode && !HandleTosSyncArming(action))
    return true;
```

**CYC reduction from host:** −3 (ternary, `!armed`, `action=="LONG"` all move to helper; `isTosSyncMode` branch stays in host but now as single-line guard).

---

### 4.2 Helper 2 — `CalculateIpcEntryQty`

| Field           | Value                                                          |
|-----------------|----------------------------------------------------------------|
| Method Name     | `CalculateIpcEntryQty`                                         |
| Signature       | `private int CalculateIpcEntryQty()`                           |
| Return Semantics| Calculated position size (contracts), minimum 1               |
| Location        | Same partial class (`src/V12_002.UI.IPC.Commands.Fleet.cs`)   |
| Visibility      | `private`                                                      |
| CYC of Helper   | **4** (base+1, try/catch+1, if(stopDist<=0)+1, ternary stopDist>0+1) |

**Extracted logic (lines 413–429 from host):**

```csharp
// Before extraction — inside if (EnableSIMA) { ... }
int qty;
try
{
    double stopDist = CalculateATRStopDistance(RMAStopATRMultiplier);
    if (stopDist <= 0)
    {
        stopDist = MinimumStop;
        Print($"[IPC SIZING] ATR latency detected. Falling back to MinimumStop={MinimumStop:F4}");
    }
    qty = stopDist > 0 ? CalculatePositionSize(stopDist) : Math.Max(1, minContracts);
    Print($"[IPC SIZING] Calculation: StopDist={stopDist:F4}, Risk={MaxRiskAmount}, TargetQty={qty}");
}
catch
{
    qty = Math.Max(1, minContracts);
}
qty = Math.Max(1, qty);
```

**New helper signature:**

```csharp
private int CalculateIpcEntryQty()
{
    try
    {
        double stopDist = CalculateATRStopDistance(RMAStopATRMultiplier);
        if (stopDist <= 0)
        {
            stopDist = MinimumStop;
            Print($"[IPC SIZING] ATR latency detected. Falling back to MinimumStop={MinimumStop:F4}");
        }
        int qty = stopDist > 0 ? CalculatePositionSize(stopDist) : Math.Max(1, minContracts);
        Print($"[IPC SIZING] Calculation: StopDist={stopDist:F4}, Risk={MaxRiskAmount}, TargetQty={qty}");
        return Math.Max(1, qty);
    }
    catch
    {
        return Math.Max(1, minContracts);
    }
}
```

**Host call site replacement (inside `if (EnableSIMA)` block):**

```csharp
int qty = CalculateIpcEntryQty();
```

**CYC reduction from host:** −3 (try/catch, `stopDist<=0`, ternary all move to helper).

---

## 5. Post-Extraction CYC Validation

| Symbol                    | CYC Before | CYC After | Status       |
|---------------------------|-----------|-----------|--------------|
| `TryHandleFleet_LongShort`| 11        | **7**     | ✅ <= 8 PASS |
| `HandleTosSyncArming`     | —         | **4**     | ✅ <= 8 PASS |
| `CalculateIpcEntryQty`    | —         | **4**     | ✅ <= 8 PASS |

**Host CYC reduction path:** 11 → (−3 from Helper 1) → 8 → (−3 from Helper 2) → **5** ... wait — recounting host after both extractions:

Host retains:
- base (+1)
- `if (action != "LONG" && action != "SHORT")` (+1)
- `if (!MetadataGuardDuplicate(...))` (+1)
- `if (isTosSyncMode && !HandleTosSyncArming(action))` (+1, single combined guard — no additional branches from inner body)
- `if (EnableSIMA)` (+1)
- `if (EnablePathB)` (+1, inside SIMA block)
- `if (currentPrice <= 0)` (+1, inside non-SIMA block)

**Host CYC = 7** ✅ ≤ 8

**Max CYC across all new/modified symbols = 7** ✅

---

## 6. Jane Street KB Compliance

| Principle                    | Check                                                                | Status |
|------------------------------|----------------------------------------------------------------------|--------|
| **carl_cook** — zero-alloc   | Both helpers return value types (bool, int). No heap allocation.    | ✅     |
| **carl_cook** — no LINQ      | No LINQ in extracted or host code.                                   | ✅     |
| **gjengset** — no new lock() | No lock() blocks added. Arm-flag mutations stay in existing methods. | ✅     |
| **gjengset** — MemoryBarrier | No new volatile mutations without barrier.                           | ✅     |
| **trading_billions** — SRP   | `HandleTosSyncArming` = ToS gate only. `CalculateIpcEntryQty` = sizing only. | ✅ |
| **trading_billions** — CYC ≤ 8 | All symbols ≤ 8.                                                  | ✅     |

---

## 7. Blast Radius Confirmation

- **Files modified:** 1 (`src/V12_002.UI.IPC.Commands.Fleet.cs`)
- **Caller changes:** NONE — `TryHandleFleet_LongShort` signature unchanged
- **New methods added:** 2 private methods in same partial class
- **Interface changes:** NONE
- **Cross-file impact:** NONE

Satisfies V12.23 No Scope Creep Protocol: ONE EPIC = ONE CONCERN.

---

## 8. Implementation Sequence

```
Step 1: Extract HandleTosSyncArming(string action) : bool
        - Add new private method to class
        - Replace if(isTosSyncMode){...} block with:
          if (isTosSyncMode && !HandleTosSyncArming(action)) return true;
        - Verify: host compiles, CYC drops to ~8

Step 2: Extract CalculateIpcEntryQty() : int
        - Add new private method to class
        - Replace int qty + try/catch block with:
          int qty = CalculateIpcEntryQty();
        - Verify: host compiles, CYC drops to ~7

Step 3: Run dotnet build — zero errors
Step 4: Run dotnet csharpier check src/ — zero issues
Step 5: Run complexity audit — host CYC <= 8 confirmed
```

---

## 9. Agent Tracking

| Field             | Value                                         |
|-------------------|-----------------------------------------------|
| **Agent Name**    | v12-phase2-architecture                       |
| **Epic**          | EPIC-W7-154                                   |
| **Wave**          | 7                                             |
| **Phase**         | 2 — Architecture Planning                     |
| **Method**        | `TryHandleFleet_LongShort`                    |
| **File**          | `src/V12_002.UI.IPC.Commands.Fleet.cs`        |
| **CYC Baseline**  | 11                                            |
| **CYC Projected** | 7 (host), 4 (each helper)                    |
| **Max CYC**       | 7                                             |
| **Helpers**       | 2 (`HandleTosSyncArming`, `CalculateIpcEntryQty`) |
| **Bobcoins Used** | 3                                             |
| **MCP Tools**     | `resolve_repo`, `search_symbols`, `get_symbol_source`, `get_call_hierarchy`, `sequentialthinking` (x4) |
| **Status**        | ✅ Completed                                  |
