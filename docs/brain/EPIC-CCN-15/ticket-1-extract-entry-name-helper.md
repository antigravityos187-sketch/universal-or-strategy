# Ticket 1: Extract Entry Name Helper

**Epic**: EPIC-CCN-15  
**Priority**: P0 (Foundation - Required by all other tickets)  
**Estimated CYC**: 4 → 4 (no reduction needed)  
**File**: `src/V12_002.Orders.Callbacks.Execution.cs`

---

## Objective

Convert the inline lambda function (lines 289-300) to a static helper method `ExtractEntryNameFromOrder`. This method will be reused by all 3 execution handlers (stop, target, trim).

---

## Current Code (Lines 289-300)

```csharp
// Helper: Extract entry name from order name (removes prefix and optional timestamp suffix)
Func<string, string, string> extractEntryName = (name, prefix) =>
{
    if (!name.StartsWith(prefix))
        return "";
    string entryPart = name.Substring(prefix.Length);
    // Strip timestamp suffix if present (format: _123456789012345)
    int lastUnderscore = entryPart.LastIndexOf('_');
    if (lastUnderscore > 0 && entryPart.Length - lastUnderscore > 10)
        entryPart = entryPart.Substring(0, lastUnderscore);
    return entryPart;
};
```

---

## Target Code (New Method)

**Location**: Insert after line 227 (before `ProcessOnExecutionUpdate`)

```csharp
/// <summary>
/// V12.CCN-15 [T1]: Extracts entry name from order name by removing prefix and optional timestamp suffix.
/// </summary>
/// <param name="orderName">Full order name (e.g., "Stop_Entry1_123456789012345")</param>
/// <param name="prefix">Prefix to remove (e.g., "Stop_", "T1_", "Trim_")</param>
/// <returns>Entry name without prefix or timestamp (e.g., "Entry1"), or empty string if prefix doesn't match</returns>
private static string ExtractEntryNameFromOrder(string orderName, string prefix)
{
    if (!orderName.StartsWith(prefix))
        return "";
    
    string entryPart = orderName.Substring(prefix.Length);
    
    // Strip timestamp suffix if present (format: _123456789012345)
    int lastUnderscore = entryPart.LastIndexOf('_');
    if (lastUnderscore > 0 && entryPart.Length - lastUnderscore > 10)
        entryPart = entryPart.Substring(0, lastUnderscore);
    
    return entryPart;
}
```

---

## Refactor Sites (Replace Lambda Calls)

### Site 1: Stop Loss Handler (Line 307)
**Before**:
```csharp
string entryName = extractEntryName(orderName, "Stop_");
```

**After**:
```csharp
string entryName = ExtractEntryNameFromOrder(orderName, "Stop_");
```

### Site 2: Target Handler (Line 379)
**Before**:
```csharp
string entryName = extractEntryName(orderName, targetPrefix);
```

**After**:
```csharp
string entryName = ExtractEntryNameFromOrder(orderName, targetPrefix);
```

### Site 3: Trim Handler (Line 460)
**Before**:
```csharp
string entryName = extractEntryName(orderName, "Trim_");
```

**After**:
```csharp
string entryName = ExtractEntryNameFromOrder(orderName, "Trim_");
```

---

## Removal (Delete Lambda Declaration)

**Delete lines 289-300** (the lambda declaration)

---

## Implementation Steps

1. **Insert new method** after line 227 (before `ProcessOnExecutionUpdate`)
2. **Replace lambda call** at line 307 (stop handler)
3. **Replace lambda call** at line 379 (target handler)
4. **Replace lambda call** at line 460 (trim handler)
5. **Delete lambda declaration** (lines 289-300)
6. **Verify ASCII-only**: All string literals use straight quotes
7. **Run complexity audit**: `python scripts/complexity_audit.py`
8. **Run deploy-sync**: `powershell -File .\deploy-sync.ps1`
9. **Bump BUILD_TAG** in `src/V12_002.cs`

---

## F5 Verification

**Test Scenario**: Place entry order (Buy 1 MES @ Market)

**Expected Behavior**:
1. Entry order name: `Entry_TestEntry1_123456789012345`
2. Stop order name: `Stop_TestEntry1_123456789012345`
3. Target order name: `T1_TestEntry1_123456789012345`
4. All handlers extract `TestEntry1` correctly
5. No errors in log

**Verification Command**:
```
F5 in NinjaTrader → Place entry → Check log for entry name parsing
```

---

## Success Criteria

- [ ] New method `ExtractEntryNameFromOrder` added after line 227
- [ ] Lambda call replaced at line 307 (stop handler)
- [ ] Lambda call replaced at line 379 (target handler)
- [ ] Lambda call replaced at line 460 (trim handler)
- [ ] Lambda declaration deleted (lines 289-300)
- [ ] ASCII-only compliance verified (no Unicode)
- [ ] Complexity audit passes (CYC unchanged)
- [ ] Deploy-sync passes (ASCII gate)
- [ ] BUILD_TAG bumped
- [ ] F5 verification passes (entry name parsed correctly)
- [ ] Build + tests pass

---

## Risk Assessment

**Risk Level**: LOW

**Rationale**:
- Pure function (no state mutation)
- Exact copy of lambda logic (zero drift)
- Reused 3x (DRY principle)

**Mitigation**: None needed (low risk)

---

## Rollback Plan

If F5 verification fails:
1. Revert commit: `git reset --hard HEAD~1`
2. Restore lambda declaration
3. Report issue to Director

---

**Ticket Created**: 2026-06-09T03:37:00Z  
**Assigned To**: Bob Shell (v12-engineer)  
**Status**: Ready for Execution