# Phase 1: Scope Definition - EPIC-W7-125

## Agent Tracking
- Agent Name: v12-phase1-scope
- Phase: 1 (Scope Definition)
- Input: 00-hotspots.md
- Execution Time: 2026-06-24T19:41:09Z

## Epic Overview
- **Target Method**: `SymmetryGuardTryResolveFollower`
- **File**: `src/V12_002.Symmetry.Follower.cs`
- **Current CYC**: 20 (2.5x threshold)
- **Target CYC**: ≤8 per method
- **Lines**: 118
- **Blast Radius**: ZERO (internal only - safe to refactor)

## Scope Boundary Definition

### What Will Be EXTRACTED (New Methods)

#### 1. Validation & Guard Clauses
**Extract to**: `ValidateFollowerResolutionInputs()`
- **Lines**: Early validation checks (first ~15 lines)
- **Responsibility**: Input validation, null checks, fleet entry validation
- **Returns**: `bool` (true if valid, false if should skip)
- **CYC Target**: ≤3
- **Rationale**: Separate validation from business logic, enable early returns

#### 2. Master Anchor Resolution
**Extract to**: `ResolveAndApplyMasterAnchor()`
- **Lines**: Master anchor lookup and application logic
- **Responsibility**: Find master position, apply anchor to follower
- **Returns**: `bool` (true if anchor applied successfully)
- **CYC Target**: ≤5
- **Rationale**: Isolate anchor coordination logic, reduce nesting

#### 3. Bracket Management Decision
**Extract to**: `DetermineBracketAction()`
- **Lines**: Logic deciding whether to retarget existing or submit new bracket
- **Responsibility**: Analyze existing brackets, decide action path
- **Returns**: `BracketActionType` enum (Retarget, SubmitNew, Skip)
- **CYC Target**: ≤4
- **Rationale**: Separate decision logic from execution

#### 4. Bracket Execution
**Extract to**: `ExecuteBracketAction()`
- **Lines**: Actual bracket submission/retargeting calls
- **Responsibility**: Execute the decided bracket action
- **Returns**: `bool` (true if action succeeded)
- **CYC Target**: ≤3
- **Rationale**: Separate execution from decision-making

### What Will REMAIN (Orchestration)

The original `SymmetryGuardTryResolveFollower` will become a **thin orchestrator**:
- Call validation helper
- Call anchor resolution helper
- Call bracket decision helper
- Call bracket execution helper
- Coordinate error handling and logging
- **Target CYC**: ≤6 (orchestration only)

### Extraction Strategy: Vertical Slicing

```
Original Method (CYC 20, 118 lines)
    ↓
┌─────────────────────────────────────┐
│ SymmetryGuardTryResolveFollower     │ ← Orchestrator (CYC ≤6)
│ - Coordinates flow                  │
│ - High-level error handling         │
└─────────────────────────────────────┘
    ↓ calls
┌─────────────────────────────────────┐
│ ValidateFollowerResolutionInputs    │ ← Validation (CYC ≤3)
│ - Input checks                      │
│ - Fleet entry validation            │
└─────────────────────────────────────┘
    ↓ if valid
┌─────────────────────────────────────┐
│ ResolveAndApplyMasterAnchor         │ ← Anchor Logic (CYC ≤5)
│ - Master position lookup            │
│ - Anchor application                │
└─────────────────────────────────────┘
    ↓ if anchored
┌─────────────────────────────────────┐
│ DetermineBracketAction              │ ← Decision (CYC ≤4)
│ - Analyze existing brackets         │
│ - Decide action path                │
└─────────────────────────────────────┘
    ↓ execute
┌─────────────────────────────────────┐
│ ExecuteBracketAction                │ ← Execution (CYC ≤3)
│ - Submit or retarget bracket        │
│ - FSM state updates                 │
└─────────────────────────────────────┘
```

## Dependencies & Risks

### Internal Dependencies (Safe)
- **Callers**: Only 2 methods in same file
  - `SymmetryGuardOnFollowerFill` (line 17)
  - `SymmetryGuardProcessPendingFollowerFills` (line 97)
- **Callees**: 42 downstream symbols (helpers, FSM operations)
- **Risk**: LOW - All internal to Symmetry.Follower.cs

### External Dependencies (None)
- **Blast Radius**: ZERO
- **Cross-File Impact**: NONE
- **Risk**: MINIMAL - Isolated refactoring

### State Management
- **Pattern**: FSM/Actor (already uses `Enqueue`)
- **Risk**: LOW - Extraction preserves FSM pattern
- **Consideration**: Ensure extracted methods maintain lock-free correctness

### Testing Considerations
- **Current**: Likely untested (CYC 20 = exponential test paths)
- **Post-Refactor**: Each extracted method testable independently
- **Risk**: MEDIUM - Need comprehensive test coverage for new methods

## Success Criteria

### Quantitative Metrics
- [ ] Original method CYC reduced from 20 to ≤6
- [ ] All extracted methods have CYC ≤8 (target: ≤5)
- [ ] Max nesting depth reduced from 6 to ≤3
- [ ] Zero compilation errors
- [ ] Zero new Roslyn warnings

### Qualitative Metrics
- [ ] Each method has single, clear responsibility
- [ ] Method names clearly describe their purpose
- [ ] Logic flow is easier to follow (reduced nesting)
- [ ] FSM/Actor pattern preserved (lock-free)
- [ ] ASCII-only compliance maintained

### Verification Steps
1. **Build**: `dotnet build` passes
2. **Sync**: `powershell -File .\deploy-sync.ps1` succeeds
3. **Complexity**: `python scripts/complexity_audit.py` shows all methods ≤8
4. **Format**: `dotnet csharpier check src/` passes
5. **Integration**: F5 in NinjaTrader loads strategy successfully

## Boundary Constraints

### What MUST Stay Together
- FSM state transitions (atomic operations)
- Error logging context (maintain traceability)
- Orchestration flow (high-level coordination)

### What MUST Be Separated
- Validation logic (guard clauses)
- Anchor resolution (master position lookup)
- Bracket decision logic (analysis)
- Bracket execution (submission/retargeting)

### What MUST NOT Change
- Public API surface (method signature)
- FSM/Actor pattern (lock-free correctness)
- Caller contracts (return values, side effects)
- Logging verbosity (maintain observability)

## Risk Mitigation

### Low Risk Factors
✅ Zero blast radius (internal only)
✅ Clear caller/callee boundaries
✅ FSM pattern already established
✅ No cross-file coordination needed

### Medium Risk Factors
⚠️ Deep nesting (6 levels) - requires careful extraction
⚠️ 42 downstream callees - must preserve call order
⚠️ Untested code - need test coverage post-refactor

### High Risk Factors
🔴 CYC 20 (2.5x threshold) - complex logic to untangle
🔴 118 lines - substantial extraction effort

### Mitigation Strategy
1. **Incremental Extraction**: One helper at a time, verify build after each
2. **Preserve Semantics**: Maintain exact behavior, no logic changes
3. **Test Coverage**: Add unit tests for each extracted method
4. **Code Review**: Adversarial audit before merge (Phase 4.5)

## Jane Street Alignment

### Current Violations
- ❌ Cognitive Simplicity: CYC 20 too complex for microsecond reasoning
- ❌ Exhaustive Testing: 20 paths = exponential test case growth
- ❌ Race Condition Auditing: Deep nesting obscures lock-free correctness

### Post-Refactor Alignment
- ✅ Cognitive Simplicity: All methods CYC ≤8
- ✅ Exhaustive Testing: Each method independently testable
- ✅ Race Condition Auditing: Flat structure, clear FSM operations
- ✅ Single Responsibility: Each method has one clear purpose
- ✅ Make Illegal States Unrepresentable: Validation extracted to guard clauses

## Next Steps (Phase 1.5)

Phase 1.5 will validate this scope boundary using Sequential Thinking MCP to ensure:
1. No scope creep (extraction targets are minimal and focused)
2. Boundaries are clear and enforceable
3. Dependencies are fully mapped
4. Success criteria are measurable

---
**Phase 1 Status**: COMPLETED
**Generated**: 2026-06-24T19:41:09Z
**Agent**: v12-phase1-scope
**Ready for Phase 1.5**: Scope Boundary Validation
