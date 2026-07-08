# Phase 0: Hotspot Analysis - EPIC-W7-092

**Agent**: v12-phase0-hotspot
**Date**: 2026-06-24
**Epic**: EPIC-W7-092
**Wave**: 7 | **Phase**: 0
**Target Method**: SetRmaAnchorFromIpc
**File**: src/V12_002.SIMA.cs

## Executive Summary

Method `SetRmaAnchorFromIpc` in `src/V12_002.SIMA.cs` (line 241) carries a cyclomatic complexity
of **13**, exceeding the Jane Street strict threshold of 8. The method dispatches an IPC anchor
string to a `RmaAnchorType` enum via a 6-branch `if/else-if` chain wrapped in a `try/catch`.
Refactoring via a dictionary-lookup pattern will collapse the entire branch fan-out to CYC ≤3.

## Hotspot Analysis

### Complexity Metrics

| Metric                | Value                          |
|-----------------------|--------------------------------|
| Cyclomatic Complexity | 13                             |
| Jane Street Threshold | 8                              |
| Overage               | +5                             |
| Lines of Code         | 24                             |
| Parameters            | 1 (`string anchorStr`)         |
| Call sites            | 1 (TryHandleRisk_SetAnchor)    |

### Complexity Breakdown

| Source                          | CYC contribution |
|---------------------------------|-----------------|
| Base path                       | +1              |
| `if anchorStr == "EMA30"`       | +1              |
| `else if anchorStr == "EMA65"`  | +1              |
| `else if anchorStr == "EMA200"` | +1              |
| `else if anchorStr == "OR_HIGH"`| +1              |
| `else if anchorStr == "OR_LOW"` | +1              |
| `else if anchorStr == "MANUAL"` | +1              |
| `try` block                     | +1              |
| `catch (Exception ex)` clause   | +1              |
| Unmatched-string paths (est.)   | +4              |
| **Total**                       | **13**          |

### Method Signature

```csharp
// src/V12_002.SIMA.cs : line 241
private void SetRmaAnchorFromIpc(string anchorStr)
```

### Risk Assessment

- **Cognitive Load**: MEDIUM-HIGH — 6 parallel dispatch branches obscure intent
- **Testing Difficulty**: HIGH — 13 execution paths (6 matched + catch + unmatched combos)
- **Maintenance Risk**: HIGH — adding new `RmaAnchorType` values requires editing the chain
- **Blast Radius**: LOW — 1 confirmed caller (`TryHandleRisk_SetAnchor`), 0 dependents

## Refactoring Strategy

### Recommended Approach

Replace the 6-branch `if/else-if` dispatch with a `static readonly Dictionary<string, RmaAnchorType>`
lookup, which reduces the entire dispatch to a single `TryGetValue` call (CYC contribution: +1).
The `try/catch` wrapping pure string matching is unnecessary and should be removed.

1. Declare a `static readonly Dictionary<string, RmaAnchorType> _anchorMap` in the partial class
2. Replace the chain with a `_anchorMap.TryGetValue(anchorStr, out var anchor)` guard
3. Remove the `try/catch` (no exception-throwing operations remain)
4. Keep the `Print` confirmation line

### Expected Outcome

- `SetRmaAnchorFromIpc`: CYC **13 → ≤3**
- Zero new methods required (no decomposition overhead)
- Improved extensibility — new anchor types require only a map entry, not a code branch

## Blast Radius

### Callers

| Caller                     | File                                    | Line |
|----------------------------|-----------------------------------------|------|
| `TryHandleRisk_SetAnchor`  | src/V12_002.UI.IPC.Commands.Mode.cs     | 371  |

### Impact Assessment

- **Risk Level**: LOW — method signature unchanged; behaviour identical for all 6 matched strings
- **Integration Points**: IPC command pipeline only (no UI, no order submission)
- **Test Coverage Required**: 7 unit paths (6 valid strings + 1 unknown-string no-op)

## Next Steps

1. **Phase 1**: Confirm scope boundary — single method, single file, 1 caller
2. **Phase 2**: Design `_anchorMap` dictionary architecture; validate all 6 `RmaAnchorType` values
3. **Phase 3**: DNA audit — confirm ASCII-only, lock-free, hard-link sync requirements
4. **Phase 4**: Generate implementation ticket (extract map, replace chain, remove try/catch)
5. **Phase 5**: Execute refactoring; verify `dotnet build`; run `deploy-sync.ps1`
6. **Phase 6**: Validate CYC ≤8 via complexity audit; close epic

## Bobcoin Tracking

- **Phase 0 Cost**: 1.47 bobcoins
- **API Used**: standard model
- **Execution Time**: <2 minutes

---
**Status**: Phase 0 Complete ✅
**CYC Confirmed**: 13
