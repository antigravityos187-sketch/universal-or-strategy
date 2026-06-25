# Phase 1: Scope Definition - EPIC-W7-149

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Mode**: Plan
- **Phase**: 1 (Scope Definition)
- **Input**: 00-hotspots.md
- **Execution Time**: 2026-06-24T19:44:47Z

## Epic Overview
- **Target Method**: LogApexPerformance
- **File**: src/V12_002.UI.Compliance.cs
- **Line**: 810
- **Current CYC**: 20
- **Target CYC**: ≤8 per extracted method
- **Reduction Required**: 60% (12 CYC points)

## Scope Boundaries

### What Will Be Extracted

#### Extraction Target 1: CollectComplianceMetrics
**Purpose**: Isolate compliance data collection logic
**Scope**:
- All dictionary lookups (9 operations):
  - accountDailyProfit
  - accountTotalProfit
  - accountTradeCount
  - accountMaxDrawdown
  - accountEquityPeak
  - accountDailyTradeCount
  - accountTradingDays
  - accountLastSummaryDate
  - expectedPositions
- GetComplianceAccounts call
- GetComplianceNow call
- GetUniqueTradingDays call

**Estimated CYC**: 4-6 (conditional dictionary access)
**Return Type**: ComplianceMetricsSnapshot (new struct)

#### Extraction Target 2: PublishApexUiSnapshot
**Purpose**: Isolate UI snapshot publishing logic
**Scope**:
- GetPooledSnapshot call
- UpdateConfigSnapshot call
- UpdateComplianceSnapshot call
- UpdateLivePositionSnapshot call
- PublishUiSnapshot call
- ReturnPooledSnapshot call

**Estimated CYC**: 2-3 (linear flow with minimal branching)
**Parameters**: ComplianceMetricsSnapshot, account data
**Return Type**: void

#### Extraction Target 3: FinalizeAccountDailySummaries
**Purpose**: Isolate daily summary finalization logic
**Scope**:
- MaybeFinalizeDailySummaries call
- UpdateAccountMetricsFromAccount call
- Daily summary date checks
- Trading day calculations

**Estimated CYC**: 3-4 (date comparison logic)
**Parameters**: Account list, compliance metrics
**Return Type**: void

#### Extraction Target 4: FormatApexPerformanceLog
**Purpose**: Isolate performance log formatting
**Scope**:
- LogBuffer.Format calls
- ConvertToSelectedTimeZone calls
- PathValidation class usage
- ExpKey generation
- String formatting logic

**Estimated CYC**: 2-3 (formatting logic)
**Parameters**: ComplianceMetricsSnapshot, account data
**Return Type**: string

### What Will Remain in Original Method

**Orchestration Logic Only**:
```csharp
private void LogApexPerformance()
{
    // 1. Collect compliance metrics
    var metrics = CollectComplianceMetrics();
    
    // 2. Finalize daily summaries
    FinalizeAccountDailySummaries(metrics.Accounts, metrics);
    
    // 3. Format performance log
    string logMessage = FormatApexPerformanceLog(metrics);
    
    // 4. Publish UI snapshot
    PublishApexUiSnapshot(metrics);
    
    // 5. Write log (if applicable)
    if (!string.IsNullOrEmpty(logMessage))
    {
        // Existing log write logic
    }
}
```

**Estimated CYC**: 2-3 (orchestration only)

## Boundary Definitions

### Clear Boundaries

#### Boundary 1: Compliance Data Collection
**IN SCOPE**:
- Dictionary lookups for account metrics
- Compliance account retrieval
- Trading day calculations
- Null/default value handling

**OUT OF SCOPE**:
- UI snapshot creation
- Log formatting
- Daily summary finalization

#### Boundary 2: UI Snapshot Publishing
**IN SCOPE**:
- Snapshot pooling (get/return)
- Snapshot updates (config, compliance, positions)
- Snapshot publishing

**OUT OF SCOPE**:
- Metric collection
- Log formatting
- Daily summary logic

#### Boundary 3: Daily Summary Finalization
**IN SCOPE**:
- Date-based summary checks
- Account metrics updates
- Trading day tracking

**OUT OF SCOPE**:
- UI snapshot operations
- Log formatting
- Initial metric collection

#### Boundary 4: Log Formatting
**IN SCOPE**:
- String formatting
- Time zone conversion
- Path validation
- Key generation

**OUT OF SCOPE**:
- Metric collection
- UI operations
- Summary finalization

### Shared Dependencies

**Data Structure**: ComplianceMetricsSnapshot (new)
```csharp
internal struct ComplianceMetricsSnapshot
{
    public List<Account> Accounts;
    public DateTime ComplianceNow;
    public Dictionary<string, double> DailyProfit;
    public Dictionary<string, double> TotalProfit;
    public Dictionary<string, int> TradeCount;
    public Dictionary<string, double> MaxDrawdown;
    public Dictionary<string, double> EquityPeak;
    public Dictionary<string, int> DailyTradeCount;
    public Dictionary<string, int> TradingDays;
    public Dictionary<string, DateTime> LastSummaryDate;
    public Dictionary<string, int> ExpectedPositions;
    public int UniqueTradingDays;
}
```

## Dependencies and Risks

### Internal Dependencies
- **ProcessAccountExecutionQueue** (caller) - No changes required
- **OnAccountExecutionUpdate** (indirect caller) - No changes required
- **72 callees** - Will be distributed across 4 extracted methods

### External Dependencies
- **NONE** - Zero external importers (blast radius = 0)

### Risk Assessment

#### LOW RISK
- ✅ Zero external importers (isolated scope)
- ✅ Only 2 callers (both in same file)
- ✅ No cross-file ripple effects
- ✅ Clear separation of concerns

#### MEDIUM RISK
- ⚠️ 72 callees require careful distribution
- ⚠️ New data structure (ComplianceMetricsSnapshot) introduces coupling
- ⚠️ 7 nesting levels may hide edge cases

#### MITIGATION STRATEGIES
1. **Unit Tests**: Each extracted method must have xUnit tests
2. **Incremental Extraction**: Extract one method at a time, verify build
3. **Preserve Behavior**: Use ComplianceMetricsSnapshot to maintain data flow
4. **Guard Clauses**: Replace deep nesting with early returns

### Churn Risk
- **12 commits in 90 days** - Moderate activity
- **Mitigation**: Complete extraction in single PR to avoid merge conflicts

## Success Criteria

### Phase 1 Success (This Document)
- ✅ Scope boundaries clearly defined
- ✅ 4 extraction targets identified
- ✅ Boundary definitions documented
- ✅ Dependencies mapped
- ✅ Risk assessment complete

### Phase 2 Success (Architecture Planning)
- [ ] Extraction order determined
- [ ] ComplianceMetricsSnapshot struct designed
- [ ] Method signatures defined
- [ ] Test strategy documented

### Phase 5 Success (Ticket Execution)
- [ ] All 4 methods extracted
- [ ] Original method CYC ≤8
- [ ] Each extracted method CYC ≤8
- [ ] Build passes
- [ ] Unit tests pass
- [ ] F5 in NinjaTrader successful

### Final Success Criteria
- [ ] **CYC Reduction**: 20 → ≤8 (60% reduction)
- [ ] **Method Count**: 1 → 5 (4 extractions + orchestrator)
- [ ] **Test Coverage**: 5 new xUnit tests (1 per method)
- [ ] **Nesting Depth**: 7 → ≤3 (guard clauses)
- [ ] **Blast Radius**: 0 importers maintained
- [ ] **Build Status**: PASS
- [ ] **NinjaTrader Load**: SUCCESS

## Extraction Strategy

### Recommended Order
1. **First**: CollectComplianceMetrics (foundation - provides data structure)
2. **Second**: FormatApexPerformanceLog (independent - uses metrics)
3. **Third**: FinalizeAccountDailySummaries (independent - uses metrics)
4. **Fourth**: PublishApexUiSnapshot (independent - uses metrics)
5. **Final**: Refactor orchestrator (LogApexPerformance)

### Rationale
- Extract data collection first to establish ComplianceMetricsSnapshot
- Extract independent operations in parallel (tickets 2-4 can run concurrently)
- Refactor orchestrator last to wire everything together

## Jane Street Alignment

### Current Violations
- ❌ CYC=20 (2.5x threshold) - Violates cognitive simplicity
- ❌ 72 callees - Violates single responsibility
- ❌ 7 nesting levels - Violates "make illegal states unrepresentable"

### Target Alignment
- ✅ CYC ≤8 per method (microsecond-latency reasoning)
- ✅ Single responsibility per method (testability)
- ✅ Shallow nesting (guard clauses, early returns)
- ✅ Explicit data flow (ComplianceMetricsSnapshot)

### Testing Strategy (Jane Street Standard)
- **Unit Tests**: Each extracted method (5 tests total)
- **Integration Test**: F5 in NinjaTrader
- **Regression Test**: Verify BUILD_TAG appears
- **Coverage Target**: 100% for extracted methods

## Next Phase

**Phase 2: Architecture Planning**
- Design ComplianceMetricsSnapshot struct
- Define method signatures
- Document extraction order
- Create test specifications
- Generate 4 tickets (one per extraction)

**Estimated Effort**: 4 tickets × 30 minutes = 2 hours
**Risk Level**: LOW (isolated scope, clear boundaries)
**Approval**: RECOMMENDED for Phase 2
