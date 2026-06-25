# Phase 1: Scope Definition - EPIC-W7-068

**Agent**: v12-phase1-scope
**Epic**: EPIC-W7-068
**Target Method**: TryParseTargetMode
**File**: V12_002.UI.IPC.cs
**Baseline Complexity**: 13
**Target Complexity**: ≤8
**Date**: 2026-06-24

## Executive Summary

This scope definition establishes clear boundaries for extracting the `TryParseTargetMode` method (CYC 13) into smaller, focused methods that comply with the Jane Street threshold of CYC ≤8.

## IN SCOPE

### Primary Extraction Target
- **Method**: `TryParseTargetMode` in V12_002.UI.IPC.cs
- **Current CYC**: 13
- **Target CYC**: ≤8 per extracted method

### Extraction Candidates

#### 1. Mode String Validation
- Input validation logic
- Null/empty checks
- Format verification
- **Estimated CYC**: 3-4

#### 2. Mode Enum Conversion
- String-to-enum parsing
- Case-insensitive matching
- Default value handling
- **Estimated CYC**: 4-5

#### 3. Error Handling Paths
- Invalid input handling
- Error message generation
- Logging statements
- **Estimated CYC**: 2-3

### Testing Requirements
- Unit tests for each extracted method
- Integration test for original functionality
- Edge case coverage (null, empty, invalid inputs)

### Build Verification
- `dotnet build` must pass
- `deploy-sync.ps1` execution required
- F5 in NinjaTrader IDE verification

## OUT OF SCOPE

### Excluded from This Epic
- **IPC Command Dispatcher**: Caller of TryParseTargetMode - separate concern
- **Mode Validation Logic**: Downstream validation - separate epic if needed
- **Other IPC Parsing Methods**: Focus on single method per epic
- **UI Layer Changes**: No UI modifications required
- **FSM State Management**: No state machine dependencies

### Deferred Work
- Performance optimization (not a current bottleneck)
- Additional IPC command parsing methods (future epics)
- Comprehensive IPC subsystem refactoring (out of wave scope)

### Explicitly Not Changing
- Method signature of `TryParseTargetMode`
- Public API contracts
- Error handling behavior (preserve existing semantics)
- Logging patterns (maintain consistency)

## Scope Boundaries

### File Boundaries
- **Primary**: V12_002.UI.IPC.cs only
- **No Changes**: Other partial class files
- **No Changes**: Test files (only additions)

### Functional Boundaries
- **Preserve**: Exact input/output behavior
- **Preserve**: Error handling semantics
- **Preserve**: Logging output format
- **Extract**: Internal complexity only

### Risk Boundaries
- **Low Risk**: Isolated IPC layer changes
- **Medium Risk**: None identified
- **High Risk**: None identified

## Success Criteria

### Complexity Targets
- [ ] All extracted methods have CYC ≤8
- [ ] Original `TryParseTargetMode` reduced to CYC ≤8
- [ ] No new methods exceed CYC threshold

### Functional Verification
- [ ] All unit tests pass
- [ ] Integration tests pass
- [ ] Build succeeds with zero errors
- [ ] deploy-sync.ps1 executes successfully
- [ ] F5 in NinjaTrader IDE loads strategy

### Code Quality
- [ ] ASCII-only compliance maintained
- [ ] No lock() statements introduced
- [ ] Jane Street patterns followed
- [ ] Consistent naming conventions

## Estimated Effort

- **Tickets**: 1-2
- **Complexity**: LOW
- **Risk**: LOW
- **Timeline**: Single wave execution

## Dependencies

### Prerequisites
- jCodemunch index current
- Clean build baseline
- GitButler virtual branch active

### Blockers
- None identified

## Agent Tracking

- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: ~50 (scope analysis)
- **API Key**: jcodemunch-mcp
- **Execution Time**: <2 minutes

## Next Phase

Proceed to Phase 1.5 (Scope Boundary Validation) for Sequential Thinking MCP verification.
