# Ticket 2 Completion — EPIC-W7-039

## Agent Tracking
- **Epic**: EPIC-W7-039
- **Ticket**: T2 — Extract UpdatePositionMetrics
- **File**: `src/V12_002.Trailing.cs`
- **Agent**: V12 Photon Engineer (v12-engineer mode)
- **Status**: COMPLETED

## Work Performed

### Extraction: UpdatePositionMetrics

Replaced the tick-increment and extreme-price ternary block (lines 62-69) in `ManageTrailingStops` with a single call to `UpdatePositionMetrics(pos)`.

**Removed from ManageTrailingStops:**
```csharp
// Increment tick counter on every call
pos.TicksSinceEntry++;

// Update extreme price
pos.ExtremePriceSinceEntry =
    pos.Direction == MarketPosition.Long
        ? Math.Max(pos.ExtremePriceSinceEntry, Close[0])
        : Math.Min(pos.ExtremePriceSinceEntry, Close[0]);
```

**Replaced with:**
```csharp
UpdatePositionMetrics(pos);
```

**New helper added:**
```csharp
private void UpdatePositionMetrics(PositionInfo pos)
{
    pos.TicksSinceEntry++;
    pos.ExtremePriceSinceEntry =
        pos.Direction == MarketPosition.Long
            ? Math.Max(pos.ExtremePriceSinceEntry, Close[0])
            : Math.Min(pos.ExtremePriceSinceEntry, Close[0]);
}
```

## Complexity

| Metric | Before | After |
|--------|--------|-------|
| UpdatePositionMetrics CYC | N/A (new) | 2 |
| ManageTrailingStops CYC (partial) | — | reduced |

## Validation
- No `lock()` introduced
- ASCII-only
- Zero logic drift (pure structural extraction)
- `dotnet csharpier format src/` — PASSED (83 files formatted)
- `dotnet build Linting.csproj` — PASSED (0 errors, 0 warnings)
