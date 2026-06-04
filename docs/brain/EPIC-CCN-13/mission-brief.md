# EPIC-CCN-13: Extract MonitorRmaProximity

## Priority: P0 (Highest Hotspot Score: 92.49)

## Target Method
- **File**: `src/V12_002.Entries.RMA.cs`
- **Method**: `MonitorRmaProximity` (lines 382-487)
- **Current Metrics**:
  - Cyclomatic Complexity: 32 (jcodemunch) / 18 (CodeScene)
  - Lines of Code: 91
  - Nesting Depth: 4 conditionals
  - Churn: 17 commits (90 days)
  - **Hotspot Score**: 92.49 (Complexity × log(1 + churn))

## CodeScene Findings
```
Overall code health score: 8.08

[!] Issue: Bumpy Road Ahead
    Function: MonitorRmaProximity at line 382 (bumps = 4)

[!] Issue: Deep, Nested Complexity
    Function: MonitorRmaProximity at line 382 (Nesting depth = 4 conditionals)

[!] Issue: Complex Method
    Functions: MonitorRmaProximity at line 382 (cc = 18)

[!] Issue: Large Method
    Functions: MonitorRmaProximity at line 382 (LoC = 91 lines)
```

## V12 DNA Violations
1. **Complexity**: CYC 32 >> threshold 15 (Jane Street alignment)
2. **Nesting**: 4 levels deep (cognitive load)
3. **Size**: 91 lines (should be <50)
4. **Bumpy Road**: 4 decision points create unpredictable control flow

## Extraction Strategy

### Phase 1: Identify Cohesive Concerns
The method handles 3 distinct responsibilities:
1. **Proximity Detection** (lines 402-413): Distance calculation + closest approach tracking
2. **State Transition Logic** (lines 414-438): Proximity zone entry/exit + hysteresis
3. **Exhaustion Handling** (lines 444-471): Probe count enforcement + cancellation

### Phase 2: Extract Helper Methods
```csharp
// Target signature (CYC ≤ 5 each):
private double CalculateProximityDistance(PositionInfo pos, double currentPrice)
private void HandleProximityEntry(string entryName, PositionInfo pos, double distTicks, double level)
private void HandleProximityExit(string entryName, Order order, PositionInfo pos)
private bool ShouldCancelForExhaustion(PositionInfo pos)
```

### Phase 3: Orchestrator Pattern
```csharp
private void MonitorRmaProximity()
{
    var probe = LatencyProbe.Start();
    try
    {
        if (!RmaIntelligenceEnabled) return;
        
        foreach (var kvp in entryOrders)
        {
            if (!ShouldMonitorOrder(kvp.Value, kvp.Key, out var pos))
                continue;
                
            double distTicks = CalculateProximityDistance(pos, Close[0]);
            
            if (distTicks <= RmaProximityTicks)
                HandleProximityEntry(kvp.Key, pos, distTicks, pos.EntryPrice);
            else if (distTicks >= RmaCancellationTicks)
                HandleProximityExit(kvp.Key, kvp.Value, pos);
        }
    }
    finally
    {
        probe = probe.Stop();
        _histMonitorRmaProximity.Record(probe);
    }
}
```

## Success Criteria
1. **Complexity**: MonitorRmaProximity CYC ≤ 8 (orchestrator)
2. **Helpers**: All extracted methods CYC ≤ 5
3. **Nesting**: Max 2 levels in any method
4. **Size**: No method >50 lines
5. **Build**: Zero compilation errors
6. **Tests**: Existing behavior preserved (no logic changes)
7. **ASCII**: Zero non-ASCII characters

## V12 Constraints
- ✅ **Lock-Free**: Method already uses Actor pattern (no locks)
- ✅ **Atomic**: State mutations via `pos` object (safe)
- ✅ **ASCII-Only**: Verify no Unicode in extracted code
- ⚠️ **Hard-Link**: Run `deploy-sync.ps1` after extraction

## Jane Street Alignment
- **Cognitive Simplicity**: Break 91-line method into 5 focused functions
- **Testability**: Extracted helpers can be unit tested in isolation
- **Auditability**: Clear separation of concerns for race condition review

## Verification Protocol
1. **Pre-extraction**: `dotnet build` (baseline)
2. **Post-extraction**: `dotnet build` (zero new errors)
3. **Complexity audit**: `python scripts/complexity_audit.py` (verify CYC ≤ 15)
4. **ASCII check**: `python check_ascii.py` (zero violations)
5. **Hard-link sync**: `powershell -File .\deploy-sync.ps1`
6. **Pre-push validation**: `powershell -File .\scripts\pre_push_validation.ps1 -Fast`

## Bob CLI Execution
```bash
bob /task "Execute EPIC-CCN-13: Extract MonitorRmaProximity per mission-brief.md"
```

## References
- Hotspot Analysis: `scripts/epic_planner.py` output
- CodeScene Report: Terminal output (2026-06-03)
- V12 DNA: `AGENTS.md` Section 2
- Jane Street Intel: `docs/intel/jane-street/`