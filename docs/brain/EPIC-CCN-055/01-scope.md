# Phase 1.0: Scope Definition - EPIC-CCN-055

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**: DrainPhotonQueuesOnShutdown
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Current Complexity**: 11
- **Target Complexity**: <=8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

## Boundary Definition

### IN SCOPE
- DrainPhotonQueuesOnShutdown method body only
- Extract 2-3 helper methods to reduce cyclomatic complexity from 11 to <=8
- Maintain lock-free Actor/FSM pattern
- Preserve existing behavior (no logic changes)

### OUT OF SCOPE
- Callers: No changes to methods that call DrainPhotonQueuesOnShutdown
- Callees: No changes to methods called by DrainPhotonQueuesOnShutdown
- Other methods: No changes to other methods in V12_002.SIMA.Lifecycle.cs
- Pre-existing issues: No fixing unrelated compilation errors or warnings
- Scope creep: No "while we're here" improvements

### NO SCOPE CREEP
- ONE EPIC = ONE CONCERN: This epic focuses solely on reducing complexity of DrainPhotonQueuesOnShutdown
- No bundling: Do not combine with other refactoring tasks
- No opportunistic fixes: Resist temptation to fix adjacent code

## Success Criteria

### Primary Goals
1. Complexity Reduction: Reduce cyclomatic complexity from 11 to <=8
2. Test Pass: All existing tests pass without modification
3. Behavior Preservation: Zero behavior changes (pure refactoring)
4. Lock-Free Pattern: Maintain Actor/FSM Enqueue model (no locks)

### Quality Gates
- Build: Zero compilation errors
- Lint: Zero new Roslyn violations
- Formatting: CSharpier compliant
- ASCII-Only: No Unicode characters in string literals
- Diff Size: Changes limited to V12_002.SIMA.Lifecycle.cs only

### Verification
- Complexity Audit: python3 scripts/complexity_audit.py shows CYC <=8 for DrainPhotonQueuesOnShutdown
- Pre-Push Validation: powershell -File .\scripts\pre_push_validation.ps1 -Fast passes
- Hard-Link Sync: powershell -File .\deploy-sync.ps1 succeeds

## Rationale

**Why This Method?**
- Current complexity: 11 (approaching Jane Street threshold of 15)
- Proactive cleanup: Prevent future threshold violations
- Lifecycle critical: Shutdown sequence requires high reliability
- Low risk: Method is compliant, refactoring is preventive maintenance

**Jane Street Alignment**
- Cognitive simplicity: Functions with CYC >8 are harder to reason about under microsecond latency constraints
- Testability: Lower complexity enables exhaustive path testing
- Auditability: Simpler logic reduces race condition audit surface

## Extraction Strategy

**Proposed Decomposition** (2-3 helper methods):
1. Helper 1: Extract queue state validation logic
2. Helper 2: Extract drain operation coordination
3. Helper 3 (if needed): Extract timeout/error handling

**Naming Convention**:
- Use descriptive names that reflect single responsibility
- Prefix with Drain or Validate to indicate lifecycle context
- Follow existing V12 naming patterns

## Risk Mitigation

**Low Risk Factors**:
- Method is already compliant (CYC=11 < 15)
- No Jane Street violations detected
- Proactive refactoring (not emergency fix)

**Mitigation Strategy**:
- Single-method focus prevents cascading changes
- Mandatory checkpointing via Bob CLI
- Automated rollback if tests fail
- Hard-link sync verification before merge

## Next Steps

1. Phase 1.5: Boundary validation (mandatory per V12.23 Protocol)
2. Phase 2: Architecture planning (implementation plan + Mermaid diagrams)
3. Phase 3: DNA & PR audit (Arena AI red team review)
4. Phase 4: Recursive execution (Bob CLI surgical extraction)
5. Phase 5: Verification (compare against implementation plan)
6. Phase 6: Sign-off (deploy-sync + NinjaTrader F5 test)
