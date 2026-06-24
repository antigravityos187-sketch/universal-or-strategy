# Phase 1: Scope Boundary Definition - EPIC-W7-004

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: TBD
- **API Key**: N/A (Plan mode)
- **Execution Time**: 2026-06-24T01:29:38Z

## Epic Scope Summary
**Target**: Reduce HandleFleetTargetFill from CYC 17 to 8 (Jane Street strict standard)

**Current State**:
- File: src/V12_002.UI.Compliance.cs
- Line: 624
- CYC: 17 (112% over threshold)
- Nesting: 8 levels (HIGH)
- Callees: 24 (HIGH coupling)

## IN SCOPE

### Primary Target
- HandleFleetTargetFill method (lines 624-697)
- Extract nested conditional blocks (nesting depth 8 to 3)
- Split into helper methods (CYC 17 to 8)
- Reduce cognitive complexity while preserving behavior

### Extraction Candidates
Based on hotspot analysis, extract:

1. **Fleet Order Validation Logic**
   - Guard clauses for null checks
   - Order state validation
   - Target: Extract to ValidateFleetOrderState()

2. **Target Fill Processing**
   - PositionInfo queries (IsTargetFilled, GetTargetContracts, etc.)
   - Target: Extract to ProcessTargetFillState()

3. **OCO Order Cancellation Logic**
   - CancelOrderOnAccount calls
   - Order terminal state checks
   - Target: Extract to CancelOcoOrderIfNeeded()

4. **Logging and Diagnostics**
   - LogBuffer.Format calls
   - Target: Extract to LogFleetTargetFillEvent()

### Refactoring Boundaries
- **Start Line**: 624 (method signature)
- **End Line**: 697 (method closing brace)
- **File**: src/V12_002.UI.Compliance.cs ONLY

## OUT OF SCOPE

### Callers (DO NOT MODIFY)
- ProcessQueuedExecution_HandleFleetOCO (line 698)
  - Reason: Separate responsibility, different complexity profile
  - Action: Leave unchanged

- ProcessQueuedExecution (line 787)
  - Reason: Entry point method, separate epic candidate
  - Action: Leave unchanged

### Callees (DO NOT MODIFY)
- PositionInfo methods (IsTargetFilled, GetTargetContracts, etc.)
  - Reason: Stable API, used across codebase
  - Action: Call from extracted helpers, do not modify

- LogBuffer methods (Format, ValidateThreadAffinity, FormatInternal)
  - Reason: Logging infrastructure, separate concern
  - Action: Call from extracted helpers, do not modify

- Order management methods (CancelOrderOnAccount, ApplyTargetFill)
  - Reason: Core trading logic, separate epic candidates
  - Action: Call from extracted helpers, do not modify

### Other Methods in File
- All other methods in V12_002.UI.Compliance.cs
  - Reason: Each method is a separate epic candidate
  - Action: Leave unchanged unless explicitly targeted

### Cross-File Changes
- No changes to other files
  - Reason: Zero blast radius (private method, no external deps)
  - Action: All work confined to src/V12_002.UI.Compliance.cs

## Scope Validation

### Jane Street Alignment
- Complexity Reduction: CYC 17 to 8 (Jane Street strict standard)
- Cognitive Simplicity: Nesting 8 to 3 (microsecond-latency reasoning)
- Single Responsibility: Each extracted method does ONE thing
- Testability: Extracted methods are unit-testable

### V12 DNA Compliance
- Lock-Free Actor Pattern: No lock() blocks (already compliant)
- ASCII-Only: No Unicode/emoji (already compliant)
- Correctness by Construction: Preserve existing state machine logic
- Hard-Link Integrity: deploy-sync.ps1 after changes

### Risk Mitigation
- Low Blast Radius: Private method, 0 external deps
- Localized Changes: Single file, single method
- Regression Risk: LOW (2 callers, same file)
- Breaking Change Risk: NONE (private method)

## Extraction Strategy

### Phase 2 Architecture Plan
1. **Guard Clause Extraction** (CYC reduction: -3)
   - Extract null checks and early returns
   - Reduce nesting depth

2. **State Validation Extraction** (CYC reduction: -4)
   - Extract order state validation logic
   - Group related PositionInfo queries

3. **OCO Cancellation Extraction** (CYC reduction: -3)
   - Extract cancellation logic
   - Isolate terminal state checks

4. **Logging Extraction** (CYC reduction: -1)
   - Extract diagnostic logging
   - Reduce method line count

**Target CYC After Extraction**: 6 (well below threshold of 8)

### Success Criteria
- HandleFleetTargetFill CYC less than or equal to 8
- All extracted methods CYC less than or equal to 8
- Nesting depth less than or equal to 3
- Build passes (dotnet build)
- deploy-sync.ps1 succeeds
- F5 in NinjaTrader succeeds

## Boundary Enforcement

### What Triggers Scope Creep
- Modifying caller methods (ProcessQueuedExecution_HandleFleetOCO, ProcessQueuedExecution)
- Modifying callee methods (PositionInfo, LogBuffer, order management)
- Touching other methods in V12_002.UI.Compliance.cs
- Making changes to other files
- Adding new features beyond complexity reduction

### Scope Creep Response
If scope creep detected:
1. STOP immediately
2. Document in failure-analysis.md
3. Revert to last clean state
4. Report to Director for approval

## Next Steps (Phase 1.5)
1. Validate scope boundaries against Jane Street KB
2. Query KB for "fleet order processing" patterns
3. Query KB for "complexity reduction" strategies
4. Generate Phase 1.5 boundary validation report
5. Proceed to Phase 2 (Architecture Planning)

## References
- Hotspot Analysis: docs/brain/EPIC-W7-004/00-hotspots.md
- Jane Street KB: Query "fleet order processing", "complexity reduction"
- V12 DNA: AGENTS.md, src/AGENTS.md
- No Scope Creep Protocol: V12.23 (AGENTS.md Section 11)
