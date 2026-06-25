# Phase 1: Scope Definition - EPIC-W7-082

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:34:47Z

## Target Method
- **Method**: AuditSingleFleetAccount
- **File**: src/V12_002.REAPER.Audit.cs
- **Line**: 121
- **Current CYC**: 12
- **Target CYC**: 8 or less

## Scope Boundary Definition

### IN SCOPE

#### Primary Extraction Target
**AuditSingleFleetAccount** (CYC 12 to Target: 3-4)
- Extract 6 audit operation helper methods
- Each helper must achieve CYC 8 or less
- Maintain single caller relationship (AuditApexPositions)

#### Extraction Candidates (6 helpers)
1. **ExtractCalculateExpectedActual**
   - Logic: AuditFleet_CalculateExpectedActual call chain
   - Estimated CYC: 2-3
   - Purpose: Position calculation isolation

2. **ExtractHandleDesyncRepair**
   - Logic: AuditFleet_HandleDesyncRepair call chain
   - Estimated CYC: 2-3
   - Purpose: Desync repair logic isolation

3. **ExtractCheckPositionPassGrace**
   - Logic: AuditFleet_CheckPositionPassGrace call chain
   - Estimated CYC: 2-3
   - Purpose: Grace period validation isolation

4. **ExtractHandleCriticalDesyncFlatten**
   - Logic: AuditFleet_HandleCriticalDesyncFlatten call chain
   - Estimated CYC: 2-3
   - Purpose: Critical desync handling isolation

5. **ExtractHandleNakedPosition**
   - Logic: AuditFleet_HandleNakedPosition call chain
   - Estimated CYC: 2-3
   - Purpose: Naked position handling isolation

6. **ExtractCheckWorkingStop**
   - Logic: AuditFleet_CheckWorkingStop call chain
   - Estimated CYC: 2-3
   - Purpose: Working stop validation isolation

#### File Modifications
- **ONLY**: src/V12_002.REAPER.Audit.cs
- **Lines**: 121-193 (AuditSingleFleetAccount method body)
- **New Methods**: 6 private helper methods (same file)

### OUT OF SCOPE

#### Caller Chain
- **AuditApexPositions** (line 16) - NO CHANGES
  - Maintains existing call to AuditSingleFleetAccount
  - No signature changes required

#### Callee Methods (90 total)
- **AuditFleet_CalculateExpectedActual** - NO CHANGES
- **AuditFleet_HandleDesyncRepair** - NO CHANGES
- **AuditFleet_CheckPositionPassGrace** - NO CHANGES
- **AuditFleet_HandleCriticalDesyncFlatten** - NO CHANGES
- **AuditFleet_HandleNakedPosition** - NO CHANGES
- **AuditFleet_CheckWorkingStop** - NO CHANGES
- All other 84 callees - NO CHANGES

#### Other Files
- **src/V12_002.cs** - NO CHANGES
- **src/V12_002.REAPER.*.cs** (other partials) - NO CHANGES
- **tests/** - NEW TESTS ONLY (no existing test modifications)

#### Infrastructure
- **deploy-sync.ps1** - NO CHANGES (runs post-refactor)
- **build configuration** - NO CHANGES
- **NinjaTrader hard links** - NO CHANGES (auto-synced)

## Extraction Strategy

### Method Signature (Unchanged)
private void AuditSingleFleetAccount(SIMA_FSM fsm, string accountName)

### Post-Extraction Structure
Orchestrator delegates to 6 helper methods, reducing CYC from 12 to 3-4.

## Complexity Reduction Plan

### Current State
- **CYC**: 12
- **Nesting**: 4 levels
- **LOC**: 72
- **Callees**: 90

### Target State
- **Orchestrator CYC**: 3-4
- **Helper CYC**: 8 or less each (target: 2-3)
- **Total Methods**: 7 (1 orchestrator + 6 helpers)
- **Nesting**: 2 levels or less per method

### Success Criteria
- AuditSingleFleetAccount CYC 8 or less
- All 6 helpers CYC 8 or less
- Zero external API changes
- Single file modification (V12_002.REAPER.Audit.cs)
- Build passes
- F5 in NinjaTrader successful

## Risk Mitigation

### Low Blast Radius
- Zero external importers
- Single caller (AuditApexPositions)
- No signature changes

### Testing Strategy
- Unit tests for each of 6 helpers
- Integration test for orchestrator
- Regression test via F5 in NinjaTrader

### Rollback Plan
- Git revert if build fails
- Hard link sync via deploy-sync.ps1
- No external dependencies to unwind

---

**Phase 1 Status**: COMPLETED
**Scope Boundary**: DEFINED
**Extraction Count**: 6 helpers
**Risk Level**: LOW-MEDIUM
**Recommendation**: PROCEED TO PHASE 2
