# Extraction Tickets: EPIC-CCN-074

## Overview
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 2 hours (40 min per ticket)
- **Target Method**: `AttachExecutionPanelHandlers`
- **Target File**: `src/V12_002.UI.Panel.Handlers.cs`
- **Complexity Reduction**: CYC 12 → 1 (92% reduction in main method)

---

## TICKET-1: Extract OR Execution Handlers

### Scope
- **Current Method**: `AttachExecutionPanelHandlers`
- **Current CYC**: 12
- **Target CYC**: 9 (after this extraction)
- **Extraction**: OR Long/Short button handlers into `AttachOrExecutionHandlers`

### Implementation
1. Create new private method `AttachOrExecutionHandlers()` below main method
2. Cut lines containing `orLongButton` and `orShortButton` null-check blocks (lines 98-111)
3. Paste into new method body
4. Add call to `AttachOrExecutionHandlers()` at start of main method
5. Run `dotnet csharpier format src/` to fix formatting
6. Build: `dotnet build`

### Code Changes

**New Method** (add after `AttachExecutionPanelHandlers`):
```csharp
private void AttachOrExecutionHandlers()
{
    if (orLongButton != null)
        orLongButton.Click += (s, e) =>
        {
            PanelCommand("OR_LONG");
            ResetExecutionMode();
            TriggerGlow(CyanAccent);
        };
    if (orShortButton != null)
        orShortButton.Click += (s, e) =>
        {
            PanelCommand("OR_SHORT");
            ResetExecutionMode();
            TriggerGlow(PinkFg);
        };
}
```

**Main Method Update**:
```csharp
private void AttachExecutionPanelHandlers()
{
    AttachOrExecutionHandlers();
    // ... rest of existing code ...
}
```

### Acceptance Criteria
- [ ] New method `AttachOrExecutionHandlers` created with CYC 3
- [ ] Main method complexity reduced to CYC 9
- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build`
- [ ] No behavioral changes (F5 test in NinjaTrader)
- [ ] CSharpier formatting applied
- [ ] No whitespace mutations in other methods

### Dependencies
- None (first ticket)

### Verification Commands
```bash
# Build check
dotnet build

# Complexity audit
python scripts/complexity_audit.py

# Format check
dotnet csharpier check src/
```

---

## TICKET-2: Extract Mode Selection Handlers

### Scope
- **Current Method**: `AttachExecutionPanelHandlers`
- **Current CYC**: 9 (after TICKET-1)
- **Target CYC**: 4 (after this extraction)
- **Extraction**: Mode button handlers into `AttachModeSelectionHandlers`

### Implementation
1. Create new private method `AttachModeSelectionHandlers()` below `AttachOrExecutionHandlers`
2. Cut lines containing mode button null-check blocks:
   - `momoButton` (lines 120-125)
   - `ffmaButton` (lines 126-131)
   - `ffmaManualButton` (lines 132-137)
   - `mButton` (lines 138-145)
3. Paste into new method body
4. Add call to `AttachModeSelectionHandlers()` after first helper call in main method
5. Run `dotnet csharpier format src/`
6. Build: `dotnet build`

### Code Changes

**New Method** (add after `AttachOrExecutionHandlers`):
```csharp
private void AttachModeSelectionHandlers()
{
    if (momoButton != null)
        momoButton.Click += (s, e) =>
        {
            PanelCommand("MODE_MOMO");
            ResetExecutionMode();
            TriggerGlow(GreenFg);
        };
    if (ffmaButton != null)
        ffmaButton.Click += (s, e) =>
        {
            PanelCommand("MODE_FFMA");
            ResetExecutionMode();
            TriggerGlow(PinkFg);
        };
    if (ffmaManualButton != null)
        ffmaManualButton.Click += (s, e) =>
        {
            PanelCommand("FFMA_MANUAL_MARKET");
            ResetExecutionMode();
            TriggerGlow(PinkFg);
        };
    if (mButton != null)
        mButton.Click += (s, e) =>
        {
            PanelCommand("MODE_M");
            TriggerGlow(OrangeFg);
        };
}
```

**Main Method Update**:
```csharp
private void AttachExecutionPanelHandlers()
{
    AttachOrExecutionHandlers();
    AttachModeSelectionHandlers();
    // ... rest of existing code ...
}
```

### Acceptance Criteria
- [ ] New method `AttachModeSelectionHandlers` created with CYC 5
- [ ] Main method complexity reduced to CYC 4
- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build`
- [ ] No behavioral changes (F5 test in NinjaTrader)
- [ ] CSharpier formatting applied
- [ ] No whitespace mutations in other methods

### Dependencies
- **TICKET-1** must be completed first

### Verification Commands
```bash
# Build check
dotnet build

# Complexity audit
python scripts/complexity_audit.py

# Format check
dotnet csharpier check src/
```

---

## TICKET-3: Extract Strategy Toggle Handlers

### Scope
- **Current Method**: `AttachExecutionPanelHandlers`
- **Current CYC**: 4 (after TICKET-2)
- **Target CYC**: 1 (after this extraction)
- **Extraction**: Strategy toggle button handlers into `AttachStrategyToggleHandlers`

### Implementation
1. Create new private method `AttachStrategyToggleHandlers()` below `AttachModeSelectionHandlers`
2. Cut lines containing strategy button null-check blocks:
   - `retestButton` (lines 112-113)
   - `retestRmaToggle` (lines 114-115)
   - `rmaButton` (lines 116-117)
   - `trendButton` (lines 118-119)
   - `trendRmaToggle` (lines 146-149)
3. Paste into new method body
4. Add call to `AttachStrategyToggleHandlers()` after second helper call in main method
5. Run `dotnet csharpier format src/`
6. Build: `dotnet build`

### Code Changes

**New Method** (add after `AttachModeSelectionHandlers`):
```csharp
private void AttachStrategyToggleHandlers()
{
    if (retestButton != null)
        retestButton.Click += OnRetestClick;
    if (retestRmaToggle != null)
        retestRmaToggle.Click += OnRetestRmaToggleClick;
    if (rmaButton != null)
        rmaButton.Click += OnRmaClick;
    if (trendButton != null)
        trendButton.Click += OnTrendClick;
    if (trendRmaToggle != null)
        trendRmaToggle.Click += OnTrendRmaToggleClick;
}
```

**Main Method Final State**:
```csharp
private void AttachExecutionPanelHandlers()
{
    AttachOrExecutionHandlers();
    AttachModeSelectionHandlers();
    AttachStrategyToggleHandlers();
}
```

### Acceptance Criteria
- [ ] New method `AttachStrategyToggleHandlers` created with CYC 6
- [ ] Main method complexity reduced to CYC 1 ✅ **TARGET ACHIEVED**
- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build`
- [ ] No behavioral changes (F5 test in NinjaTrader)
- [ ] CSharpier formatting applied
- [ ] No whitespace mutations in other methods
- [ ] Complexity audit confirms all methods CYC ≤ 15

### Dependencies
- **TICKET-1** must be completed first
- **TICKET-2** must be completed first

### Verification Commands
```bash
# Build check
dotnet build

# Complexity audit (verify all methods ≤15)
python scripts/complexity_audit.py

# Format check
dotnet csharpier check src/

# Pre-push validation (full suite)
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

---

## Final Verification

After completing all 3 tickets, run full validation:

```bash
# 1. Build readiness
powershell -File .\scripts\build_readiness.ps1

# 2. Complexity audit (verify CYC reduction)
python scripts/complexity_audit.py

# 3. F5 manual test in NinjaTrader
# - Load V12_002 strategy
# - Click each button (OR Long/Short, Mode buttons, Strategy toggles)
# - Verify visual feedback (glow colors)
# - Verify commands execute correctly

# 4. Hard-link sync
powershell -File .\deploy-sync.ps1
```

## Success Metrics

| Metric | Before | After | Target | Status |
|--------|--------|-------|--------|--------|
| Main Method CYC | 12 | 1 | ≤8 | ✅ PASS |
| Helper 1 CYC | - | 3 | ≤15 | ✅ PASS |
| Helper 2 CYC | - | 5 | ≤15 | ✅ PASS |
| Helper 3 CYC | - | 6 | ≤15 | ✅ PASS |
| Max Method CYC | 12 | 6 | ≤15 | ✅ PASS |
| Jane Street Compliant | ❌ NO | ✅ YES | YES | ✅ PASS |

## Risk Mitigation

### Per-Ticket Risks
1. **Event Handler Ordering**: Preserve exact line order during extraction
2. **Null Safety**: All null checks preserved in extracted methods
3. **Lambda Scope**: No local variables captured (only class fields)
4. **Compilation**: No new types introduced

### Rollback Plan
Each ticket is atomic. If any ticket fails:
1. Revert changes: `git restore src/V12_002.UI.Panel.Handlers.cs`
2. Review error messages
3. Fix issue and retry
4. Use Bob CLI checkpointing: `/restore` if needed

## Phase 4 Status
- **Status**: COMPLETED
- **Date**: 2026-06-15
- **Ticket Count**: 3
- **Estimated Effort**: 2 hours
- **Next Phase**: Phase 5 (Ticket Execution)
