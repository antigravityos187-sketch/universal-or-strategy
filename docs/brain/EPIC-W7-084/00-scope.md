# Phase 1: Scope Definition - EPIC-W7-084

## Agent Tracking
- Agent Name: v12-phase1-scope
- Bobcoins Used: 0.50
- API Key: jCodemunch MCP
- Execution Time: 2026-06-24T19:35:12Z

## Epic Overview
Target: AuditFleet_CalculateExpectedActual (CYC 16 to 8 or less)
File: src/V12_002.REAPER.Audit.cs
Line: 382
Objective: Extract complex audit calculation logic into focused helper methods

## Scope Boundary Definition

### IN SCOPE
1. Core Audit Calculation Logic
   - Expected position calculation (GetFsmExpectedPosition calls)
   - Actual position calculation (fleet account aggregation)
   - Position delta computation (expected - actual)
   - Pass/fail determination logic

2. Grace Period Management
   - Fill grace period checks (IsReaperFillGraceActive)
   - Sync pending state checks (_dispatchSyncPendingExpKeys)
   - Grace period expiration logic (_accountFillGraceTicks)

3. FSM Collection Filtering
   - Follower bracket filtering (_followerBrackets)
   - FSM state validation (IsActive, IsTerminated checks)
   - Order ID mapping lookups

4. State Tracking Updates
   - _positionPassFailedFirstSeen dictionary updates
   - ExpKey generation and tracking
   - Audit result aggregation (out parameters)

5. Logging Infrastructure
   - LogBuffer.Format calls
   - Audit event logging
   - Debug trace statements

### OUT OF SCOPE
1. FSM Lifecycle Management
   - TryTerminateFollowerBracket calls (separate concern)
   - RemoveFsmOrderIdMappings calls (cleanup logic)
   - FSM state transitions (belongs in FSM subsystem)

2. Caller Modifications
   - AuditSingleFleetAccount (depth 1 caller)
   - AuditApexPositions (depth 2 caller)
   - No changes to calling contracts

3. Broader REAPER Refactoring
   - Other audit methods in V12_002.REAPER.Audit.cs
   - REAPER configuration logic
   - Fleet account management

4. Data Structure Changes
   - _followerBrackets collection structure
   - _positionPassFailedFirstSeen dictionary structure
   - _dispatchSyncPendingExpKeys collection structure

5. External Dependencies
   - LogBuffer implementation
   - ExpKey generation logic
   - FSM state machine implementation

## Extraction Strategy

### Target Architecture
AuditFleet_CalculateExpectedActual (CYC 5 or less - orchestration)
- CalculateExpectedPosition (CYC 8 or less)
- CalculateActualPosition (CYC 8 or less)
- CheckGracePeriods (CYC 8 or less)
- FilterActiveFsms (CYC 8 or less)
- UpdateAuditState (CYC 8 or less)

### Parameter Reduction
Current: 10 out parameters (poor encapsulation)
Target: Introduce AuditResult value object

## Complexity Targets

Before Extraction:
- AuditFleet_CalculateExpectedActual: CYC 16

After Extraction:
- AuditFleet_CalculateExpectedActual: CYC 5 or less
- CalculateExpectedPosition: CYC 8 or less
- CalculateActualPosition: CYC 8 or less
- CheckGracePeriods: CYC 8 or less
- FilterActiveFsms: CYC 8 or less
- UpdateAuditState: CYC 8 or less

## Risk Mitigation

Low Blast Radius:
- Private method (no external callers)
- Changes isolated to REAPER audit subsystem
- No public API modifications

Testing Strategy:
- xUnit tests for each extracted method
- Integration test for orchestration logic
- NinjaTrader F5 verification

Rollback Plan:
- Git branch isolation (GitButler virtual branch)
- Atomic commits per extraction
- Build verification after each extraction

## Success Criteria

Functional Requirements:
- All extracted methods have CYC 8 or less
- Original method reduced to CYC 5 or less
- No behavioral changes (audit logic identical)
- All 26 callee interactions preserved

Quality Requirements:
- xUnit tests for all extracted methods
- Build passes (dotnet build)
- NinjaTrader F5 successful
- ASCII-only compliance maintained
- No lock() usage introduced

Documentation Requirements:
- XML doc comments for all extracted methods
- Inline comments for complex logic
- Architecture diagram in 02-architecture-plan.md

## Dependencies

Prerequisites:
- Phase 0 complete (00-hotspots.md exists)
- jCodemunch index current
- Git status clean

Blockers:
- None identified

## Next Phase
Phase 1.5: Scope Boundary Validation (Jane Street gate)
- Verify no scope creep
- Confirm extraction boundaries
- Validate complexity targets
