# Extraction Tickets: EPIC-CCN-040

## Overview
- **Total Tickets**: 2
- **Execution Order**: Sequential (TICKET-1 → TICKET-2)
- **Estimated Effort**: 1.5 hours
- **Target Method**: FindTargetOrderForPosition
- **File**: src/V12_002.Trailing.Breakeven.cs
- **Current Complexity**: 9
- **Target Complexity**: 3 (main method)

## TICKET-1: Extract GetSearchAccount Helper

### Scope
- **Current Method**: `FindTargetOrderForPosition`
- **Current CYC**: 9
- **Target CYC**: 8 (after this extraction)
- **Extraction**: Account selection logic (line 206)

### Implementation
1. Create private method `GetSearchAccount(PositionInfo pos)` returning `Account`
2. Move account selection ternary logic: `(pos.IsFollower && pos.ExecutingAccount != null) ? pos.ExecutingAccount : this.Account`
3. Replace inline ternary at line 206 with method call: `Account searchAccount = GetSearchAccount(pos);`
4. Add XML documentation comment explaining Master vs Follower account selection
5. Verify compilation succeeds

### Code Template
```csharp
/// <summary>
/// Determines which account to search for orders based on follower status.
/// </summary>
/// <param name="pos">Position information containing follower status and executing account</param>
/// <returns>The executing account for followers, or the strategy account for masters</returns>
private Account GetSearchAccount(PositionInfo pos)
{
    if (pos.IsFollower && pos.ExecutingAccount != null)
    {
        return pos.ExecutingAccount;
    }
    return this.Account;
}
```

### Acceptance Criteria
- [ ] Method `GetSearchAccount` created with CYC = 2
- [ ] Original ternary replaced with method call
- [ ] XML documentation added
- [ ] Build succeeds (zero errors)
- [ ] CSharpier format passes
- [ ] Method complexity reduced from 9 to 8

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Build check
dotnet build src/V12_002.Trailing.Breakeven.cs

# Format check
dotnet csharpier check src/V12_002.Trailing.Breakeven.cs

# Complexity audit
python scripts/complexity_audit.py
```

---

## TICKET-2: Extract IsMatchingTargetOrder Helper

### Scope
- **Current Method**: `FindTargetOrderForPosition`
- **Current CYC**: 8 (after TICKET-1)
- **Target CYC**: 3 (final)
- **Extraction**: Order matching predicate (lines 211-216)

### Implementation
1. Create private method `IsMatchingTargetOrder(Order order, string targetOrderName)` returning `bool`
2. Move 4-condition AND chain from foreach loop:
   - Null check: `order != null`
   - Name match: `order.Name == targetOrderName`
   - Instrument match: `order.Instrument == pos.Instrument`
   - State check: `order.OrderState == OrderState.Working || order.OrderState == OrderState.Accepted`
3. Replace inline condition with method call: `if (IsMatchingTargetOrder(order, targetOrderName))`
4. Add XML documentation comment explaining matching criteria
5. Verify compilation succeeds

### Code Template
```csharp
/// <summary>
/// Checks if an order matches the target criteria for a position.
/// </summary>
/// <param name="order">The order to validate</param>
/// <param name="targetOrderName">The expected order name</param>
/// <returns>True if order matches all criteria (non-null, name match, instrument match, active state)</returns>
private bool IsMatchingTargetOrder(Order order, string targetOrderName)
{
    if (order == null)
    {
        return false;
    }
    
    return order.Name == targetOrderName
        && order.Instrument == pos.Instrument
        && (order.OrderState == OrderState.Working || order.OrderState == OrderState.Accepted);
}
```

### Acceptance Criteria
- [ ] Method `IsMatchingTargetOrder` created with CYC = 5
- [ ] Original 4-condition AND chain replaced with method call
- [ ] XML documentation added
- [ ] Build succeeds (zero errors)
- [ ] CSharpier format passes
- [ ] Main method complexity reduced to 3
- [ ] All unit tests pass (100%)
- [ ] No behavioral changes (integration tests pass)

### Dependencies
- **TICKET-1** must be completed first

### Verification Commands
```powershell
# Build check
dotnet build src/V12_002.Trailing.Breakeven.cs

# Format check
dotnet csharpier check src/V12_002.Trailing.Breakeven.cs

# Complexity audit (verify CYC ≤ 8 for all methods)
python scripts/complexity_audit.py

# Unit tests
dotnet test tests/V12_Performance.Tests/

# Hard-link sync
powershell -File .\deploy-sync.ps1
```

---

## Final Complexity Breakdown

### Before Extraction
- **FindTargetOrderForPosition**: CYC = 9 ❌

### After TICKET-1
- **FindTargetOrderForPosition**: CYC = 8 ✅
- **GetSearchAccount**: CYC = 2 ✅

### After TICKET-2 (Final)
- **FindTargetOrderForPosition**: CYC = 3 ✅
- **GetSearchAccount**: CYC = 2 ✅
- **IsMatchingTargetOrder**: CYC = 5 ✅

**Total Distributed Complexity**: 10 (across 3 methods)
**Max Method Complexity**: 5 ≤ 8 ✅

## Quality Gates (Post-Extraction)

### Build Pillar
- [ ] `dotnet build` succeeds (zero errors)
- [ ] `dotnet csharpier format src/` succeeds
- [ ] `dotnet csharpier check src/` passes

### Style Pillar
- [ ] `powershell -File .\scripts\lint.ps1` passes (zero new warnings)
- [ ] `python scripts/complexity_audit.py` confirms CYC ≤ 8

### Testing Pillar
- [ ] `dotnet test tests/V12_Performance.Tests/` passes (100%)
- [ ] No behavioral regressions detected

### Deployment Pillar
- [ ] `powershell -File .\deploy-sync.ps1` succeeds
- [ ] Hard-link integrity verified
- [ ] F5 in NinjaTrader loads without errors

## Risk Mitigation

### Checkpointing Strategy
1. **Before TICKET-1**: Git checkpoint via Bob CLI
2. **After TICKET-1**: Verify build + tests before TICKET-2
3. **After TICKET-2**: Full quality gates before push

### Rollback Plan
- **TICKET-1 fails**: Revert via Bob CLI `/restore` command
- **TICKET-2 fails**: Revert to post-TICKET-1 checkpoint
- **Quality gates fail**: Revert entire extraction

### Blast Radius
- **Files Modified**: 1 (src/V12_002.Trailing.Breakeven.cs)
- **Methods Modified**: 1 (FindTargetOrderForPosition)
- **Callers**: 1 (MoveSpecificTarget at line 356)
- **Impact**: Localized to trailing breakeven functionality

## Bobcoin Accounting

### Ticket Execution Costs (Projected)
- **TICKET-1**: 0.20 Bobcoins (simple extraction)
- **TICKET-2**: 0.20 Bobcoins (predicate extraction)
- **Total Phase 4**: 0.40 Bobcoins

### Cumulative Epic Costs
- **Phase 0**: 0.20 Bobcoins (hotspot analysis)
- **Phase 1.0**: 0.15 Bobcoins (scope definition)
- **Phase 1.5**: 0.10 Bobcoins (boundary validation)
- **Phase 2**: 0.25 Bobcoins (architecture planning)
- **Phase 3**: 0.30 Bobcoins (DNA & PR audit)
- **Phase 4**: 0.40 Bobcoins (ticket execution - projected)
- **Total to Date**: 1.40 Bobcoins

## Metadata
- **Phase**: 4 (Ticket Generation)
- **Epic ID**: EPIC-CCN-040
- **Date**: 2026-06-15
- **Ticket Count**: 2
- **Execution Model**: Sequential (TICKET-1 → TICKET-2)
- **Estimated Duration**: 1.5 hours
- **Risk Level**: LOW (approved by Phase 3 audit)
- **Next Phase**: Phase 5 (Recursive Execution via Bob CLI v12-engineer)

---

**TICKETS READY**: EPIC-CCN-040 approved for Phase 5 execution.
