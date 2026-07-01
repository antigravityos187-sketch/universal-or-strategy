# Ticket 1 Completion — EPIC-W7-039

## Agent Tracking
- **Epic**: EPIC-W7-039
- **Ticket**: T1 — Extract ShouldSkipPosition
- **File**: `src/V12_002.Trailing.cs`
- **Agent**: V12 Photon Engineer (v12-engineer mode)
- **Status**: COMPLETED

## Work Performed

### Extraction: ShouldSkipPosition

Replaced the 3-guard block (lines 53-60) in `ManageTrailingStops` with a single call to `ShouldSkipPosition(entryName, pos)`.

**Removed from ManageTrailingStops:**
```csharp
// V8.30: Verify position still exists (may have been removed by callback thread)
if (!activePositions.ContainsKey(entryName))
    continue;

if (!pos.EntryFilled || !pos.BracketSubmitted)
    continue;
if (pos.IsFollower && SymmetryGuardIsAnchorPending(entryName))
    continue;
```

**Replaced with:**
```csharp
if (ShouldSkipPosition(entryName, pos))
    continue;
```

**New helper added after ManageTrailingStops:**
```csharp
private bool ShouldSkipPosition(string entryName, PositionInfo pos)
{
    if (!activePositions.ContainsKey(entryName))
        return true;
    if (!pos.EntryFilled || !pos.BracketSubmitted)
        return true;
    if (pos.IsFollower && SymmetryGuardIsAnchorPending(entryName))
        return true;
    return false;
}
```

## Complexity

| Metric | Before | After |
|--------|--------|-------|
| ShouldSkipPosition CYC | N/A (new) | 4 |
| ManageTrailingStops CYC (partial) | — | reduced |

## Validation
- No `lock()` introduced
- ASCII-only
- Zero logic drift (pure structural extraction)
- `dotnet csharpier format src/` — PASSED (83 files formatted)
- `dotnet build Linting.csproj` — PASSED (0 errors, 0 warnings)
