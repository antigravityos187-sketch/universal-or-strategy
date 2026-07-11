# EPIC-W7-018 Architecture Plan — Phase 2

## Agent Tracking
- **Epic ID**: EPIC-W7-018
- **Phase**: 2 — Architecture Planning
- **Agent**: v12-phase2-architecture (Wave 7)
- **Timestamp**: 2026-06-29
- **Status**: COMPLETE

---

## 1. Scope Clarification

> **NOTE**: The task list specifies `IsSymbolMatch` in `src/V12_002.UI.IPC.cs` with CYC=0.  
> MCP investigation reveals:
> - `IsSymbolMatch` does **NOT** exist in `src/V12_002.UI.IPC.cs` (it only exists in `src-vm-backup/`).
> - The canonical `src/` file contains `IsCommandForThisInstrument` (line 294), which is the **evolved production version** of the same functionality.
> - `IsCommandForThisInstrument` has **CYC=38** (MCP-confirmed), far exceeding the Jane Street threshold of 8.
> - This EPIC is treated as targeting `IsCommandForThisInstrument` in `src/V12_002.UI.IPC.cs`.

---

## 2. Original Method (MCP-Confirmed)

| Field | Value |
|---|---|
| **Method** | `IsCommandForThisInstrument` |
| **File** | `src/V12_002.UI.IPC.cs` |
| **Line** | 294 |
| **End Line** | 352 |
| **Signature** | `private bool IsCommandForThisInstrument(string action, string targetSymbol)` |
| **CYC (MCP-confirmed)** | **38** (task-list listed 0 — incorrect placeholder) |
| **max_nesting** | 4 |
| **lines** | 59 |
| **params** | 2 (`string action`, `string targetSymbol`) |
| **Assessment** | high (CYC > 8 — extraction required) |

### Original Source (abbreviated)
```csharp
private bool IsCommandForThisInstrument(string action, string targetSymbol)
{
    // 12 global-command OR conditions
    bool isGlobalCommand =
        action == "TOGGLE_ACCOUNT" || action == "SET_SIMA" || ... || action.StartsWith("MOVE_TARGET") || ...;

    // 3 string normalizations
    string mySym = Instrument.MasterInstrument.Name.ToUpperInvariant();
    string myFull = Instrument.FullName.ToUpperInvariant();
    string target = targetSymbol.Trim().ToUpperInvariant();

    // 15 symbol-match OR conditions
    bool isForMe = isGlobalCommand || target == "GLOBAL" || target == "ALL" || ... 
        || (target == "MGC" && mySym.Contains("GC"));

    // Diagnostic Print (cold path)
    Print(string.Format("V12 IPC: Received '{0}' for '{1}'. ...", action, target, isForMe, mySym, ...));
    return isForMe;
}
```

---

## 3. CYC Analysis

### CYC Drivers (total 38)
| Driver | Branch Count |
|---|---|
| Global-command OR chain (12 actions + StartsWith) | +13 |
| Symbol-match keyword checks (GLOBAL/ALL/ON/OFF/RMA/ORB/OR/MOMO) | +8 |
| Symbol-match string comparisons (mySym==, StartsWith, Contains x2) | +4 |
| Micro-contract alias checks (MES/ES, MYM/YM, MGC/GC) | +6 |
| Conditional Print suffix (isGlobalCommand ternary) | +1 |
| Method base | +1 |
| **Reported by MCP** | **38** |

---

## 4. Extraction Plan

### Strategy: Extract 3 helpers, keep parent as coordinator

| # | New Method | Signature | Responsibility | Projected CYC |
|---|---|---|---|---|
| 1 | `IsGlobalCommand` | `private static bool IsGlobalCommand(string action)` | HashSet.Contains lookup for global commands + StartsWith("MOVE_TARGET") check | **3** |
| 2 | `IsMicroContractAlias` | `private static bool IsMicroContractAlias(string target, string mySym)` | 3 micro-contract alias checks (MES/ES, MYM/YM, MGC/GC) | **4** |
| 3 | `IsSymbolMatch` | `private bool IsSymbolMatch(string target, string mySym, string myFull)` | Keyword matches (8 literals) + mySym/myFull comparisons + IsMicroContractAlias call | **8** |
| Parent | `IsCommandForThisInstrument` | *(unchanged signature)* | Orchestrates: IsGlobalCommand + IsSymbolMatch + Print (cold diagnostic) | **2** |

### max_cyc_projected: **8**

---

## 5. Method Signatures After Extraction

```csharp
// Extracted helper 1 — zero-alloc HashSet lookup, AggressiveInlining
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private static bool IsGlobalCommand(string action)
{
    return GlobalCommandsSet.Contains(action) || action.StartsWith("MOVE_TARGET");
}

// Static readonly HashSet — defined once, O(1) lookup, no LINQ
private static readonly HashSet<string> GlobalCommandsSet =
    new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "TOGGLE_ACCOUNT", "SET_SIMA", "GET_FLEET", "DIAG_FLEET",
        "CANCEL_ALL", "FLATTEN", "SYNC_ALL", "MKT_SYNC",
        "REQUEST_FLEET_STATE", "RESET_MEMORY", "DIAG_IPC",
        "LOCK_50", "SET_TARGETS", "SET_TRAIL", "SET_CIT", "BE_CUSTOM"
    };

// Extracted helper 2 — pure predicate, micro-contract aliases only
private static bool IsMicroContractAlias(string target, string mySym)
{
    return (target == "MES" && mySym.Contains("ES"))
        || (target == "MYM" && mySym.Contains("YM"))
        || (target == "MGC" && mySym.Contains("GC"));
}

// Extracted helper 3 — symbol matching, calls IsMicroContractAlias
private bool IsSymbolMatch(string target, string mySym, string myFull)
{
    if (target == "GLOBAL" || target == "ALL" || target == "ON" || target == "OFF") return true;
    if (target == "RMA"    || target == "ORB" || target == "OR" || target == "MOMO") return true;
    return mySym == target
        || mySym.StartsWith(target)
        || target.StartsWith(mySym)
        || myFull.Contains(target)
        || IsMicroContractAlias(target, mySym);
}

// Parent after extraction — CYC 2
private bool IsCommandForThisInstrument(string action, string targetSymbol)
{
    bool isGlobalCommand = IsGlobalCommand(action);
    string mySym   = Instrument.MasterInstrument.Name.ToUpperInvariant();
    string myFull  = Instrument.FullName.ToUpperInvariant();
    string target  = targetSymbol.Trim().ToUpperInvariant();
    bool isForMe   = isGlobalCommand || IsSymbolMatch(target, mySym, myFull);

    // Cold-path diagnostic logging — stays in parent (out-of-line per carl_cook)
    Print(
        string.Format(
            "V12 IPC: Received '{0}' for '{1}'. For Me? {2} (My Symbol: {3}){4}",
            action, target, isForMe, mySym,
            isGlobalCommand ? " [GLOBAL CMD]" : ""
        )
    );
    return isForMe;
}
```

---

## 6. Parent After Extraction

| Metric | Before | After |
|---|---|---|
| CYC | 38 | 2 |
| Lines | 59 | ~18 |
| max_nesting | 4 | 2 |
| Responsibilities | 3 (global check + symbol match + logging) | 1 (coordinator + cold log) |

---

## 7. Jane Street Alignment

| Principle | Application |
|---|---|
| **carl_cook: zero-alloc hot path** | `GlobalCommandsSet.Contains()` is O(1) HashSet lookup — no LINQ, no allocations; `ToUpperInvariant()` called once per field in parent, passed down as `string` args |
| **carl_cook: extract cold logging out-of-line** | `Print(string.Format(...))` stays in parent, NOT in extracted helpers — cold diagnostic path isolated |
| **carl_cook: AggressiveInlining hot / NoInlining cold** | `IsGlobalCommand` marked `AggressiveInlining` (tiny predicate, hot path); Print block is cold path |
| **carl_cook: avoid LINQ** | Zero LINQ in extracted helpers; HashSet used instead of `.Any()` |
| **gjengset: no new lock() blocks** | All helpers are pure predicates — no shared state, no lock required |
| **trading_billions: single responsibility per helper** | `IsGlobalCommand` = command-type check only; `IsSymbolMatch` = symbol routing only; `IsMicroContractAlias` = micro-contract alias only |
| **trading_billions: each helper CYC <= 8** | IsGlobalCommand=3, IsMicroContractAlias=4, IsSymbolMatch=8, parent=2. All <= 8. |
| **trading_billions: defense in depth** | Parent preserves isGlobalCommand || IsSymbolMatch logic unchanged — semantics preserved |

---

## 8. MCP Evidence

| Tool | Result |
|---|---|
| `resolve_repo` | `antigravityos187-sketch/universal-or-strategy` — indexed, 5147 symbols |
| `search_symbols("IsSymbolMatch")` | Found in `src-vm-backup/` only (CYC=18); `src/` has `IsCommandForThisInstrument` as equivalent |
| `get_symbol_complexity(IsCommandForThisInstrument)` | CYC=**38**, max_nesting=4, lines=59, params=2, assessment=high |
| `get_symbol_source(IsCommandForThisInstrument)` | Full source: 12 global-action checks + 15 symbol-match checks + Print |
| `get_call_hierarchy(IsSymbolMatch@src-vm-backup)` | Callers: ProcessIpc_MatchSymbol, ProcessIpcCommands — 2 callers |
| `get_dependency_graph(src/V12_002.UI.IPC.cs)` | 0 cross-file imports/importers — partial file, self-contained |

---

## 9. Sequential Thinking Evidence

**Thought 1** — CYC probe:  
Task listed CYC=0 (placeholder). MCP confirms actual CYC=38 for `IsCommandForThisInstrument` in `src/V12_002.UI.IPC.cs`, which is the production equivalent of the backup's `IsSymbolMatch`. Extraction required.

**Thought 2** — Extraction strategy:  
Split into 3 helpers: `IsGlobalCommand` (HashSet-based, CYC=3), `IsMicroContractAlias` (3-condition alias, CYC=4), `IsSymbolMatch` (keyword+string matching, CYC=8). Parent reduced to coordinator + cold Print (CYC=2). HashSet chosen over OR-chain per carl_cook zero-alloc principle.

**Thought 3** — Validation:  
max_cyc_projected=8 (`IsSymbolMatch`). All 4 methods <= 8. HashSet eliminates LINQ. Print stays in parent (cold path). No lock() introduced. Jane Street alignment confirmed across all 3 principles (carl_cook, gjengset, trading_billions).

---

## 10. Risks & Notes

- `IsSymbolMatch` conflicts with same name in `src-vm-backup/` — no risk since backup is not compiled into production.
- `GlobalCommandsSet` should be co-located in `src/V12_002.UI.IPC.cs` (same partial class) to avoid cross-file dependency.
- The `[AggressiveInlining]` attribute on `IsGlobalCommand` is safe — method body is 2 branches, well within inlining budget.
- Existing callers of `IsCommandForThisInstrument` are unchanged — signature not modified.
