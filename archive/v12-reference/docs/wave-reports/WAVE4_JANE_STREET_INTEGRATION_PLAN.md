# Wave 4 Jane Street Integration Plan

**Date**: 2026-06-15
**Status**: CRITICAL - Phase 0 and Phase 1 executed WITHOUT Jane Street validation
**Action Required**: STOP and integrate before proceeding

---

## Executive Summary

**CRITICAL FINDING**: Wave 4 Phase 0 (80 epics completed) and Phase 1 (failed due to permissions) executed WITHOUT Jane Street validation. The workflow does NOT include the 299 P0 Jane Street violations that must be fixed.

### The 299 Violations

**Total**: 299 violations (ALL P0 - Critical Priority)

**By Category**:
- **Philosophy**: 223 violations (75%) - Magic numbers, naming conventions
- **Type Safety**: 69 violations (23%) - Exception handling in hot paths
- **Concurrency**: 5 violations (2%) - Lock-free patterns
- **Performance**: 2 violations (<1%) - Optimization issues

**Affected Files**: 52 files in `src/`

**Top 10 Files** (by violation count):
1. `V12_002.UI.Panel.Brushes.cs`: 38 violations
2. `V12_002.LogicAudit.cs`: 18 violations
3. `V12_002.Perf.LatencyHistogram.cs`: 18 violations
4. `V12_002.Lifecycle.cs`: 17 violations
5. `V12_002.UI.Panel.Helpers.cs`: 17 violations
6. `V12_002.Properties.cs`: 16 violations
7. `V12_002.SIMA.Execution.cs`: 16 violations
8. `V12_002.UI.IPC.Server.cs`: 10 violations
9. `V12_002.UI.Compliance.cs`: 9 violations
10. `V12_002.Orders.Management.StopSync.cs`: 8 violations

### Common Violation Types

**JS-100 (Philosophy - Magic Numbers)**: ~223 instances
- Example: `src\V12_002.cs:52` - "No magic numbers - use named constants"
- Fix: Extract to named constants with descriptive names

**JS-001 (Type Safety - Exception Handling)**: ~69 instances
- Example: `src\SignalBroadcaster.cs:286` - "Use Result<T,E> instead of exceptions in hot paths"
- Fix: Replace try-catch with Result<T,E> pattern for hot-path code

---

## Current State Assessment

### What Works ✅

1. **Firebase KB Connection**: Working
   - 10 Jane Street documents ingested
   - Query script functional: `python scripts/query_kb.py "term"`
   - Documents include: microsecond latency, concurrency, testing, hardware-software codesign

2. **Violations File**: Exists and valid
   - File: `jane_street_p0_violations.json` (237KB, UTF-16 encoded)
   - 299 violations cataloged with file/line/fix suggestions
   - Analysis script created: `scripts/analyze_jane_street_violations.py`

### What's Missing ❌

1. **Phase 0 (Hotspot Analysis)**:
   - ❌ No Jane Street KB query
   - ❌ No violation check for target methods
   - ❌ No validation that hotspot doesn't have existing violations

2. **Phase 1 (Scope + Boundary)**:
   - ❌ No Jane Street KB query (handoff says "Manual KB query required")
   - ❌ No violation check for extraction scope
   - ❌ Scripts just call Bob CLI without any Jane Street context

3. **Phase 2 (Architecture)**:
   - ⚠️ Handoff says "Automated Firebase hook" but no evidence in phase scripts
   - ❌ No validation that architecture plan addresses violations

4. **Phase 3 (Audit)**:
   - ⚠️ Handoff says "Manual KB query required"
   - ❌ No violation check in audit phase

5. **Phase 4 (Tickets)**:
   - ⚠️ Handoff says "Manual KB query required"
   - ❌ No validation that tickets include violation fixes

6. **Phase 4.5 (Ticket Review)**:
   - ⚠️ Handoff says "Automated Firebase hook"
   - ❌ Script exists but no evidence of Jane Street validation

7. **Phase 5 (Execution)**:
   - ⚠️ Handoff says "Automated Firebase hook (Bob CLI)"
   - ❌ No evidence Bob CLI queries Jane Street KB
   - ❌ No validation that refactored code fixes violations

8. **Phase 5.V (Verification)**:
   - ⚠️ Handoff says "Manual KB query required"
   - ❌ No violation re-check after refactoring

---

## Integration Requirements

### Mandatory Principle

**"Every file we touch must not have any Jane Street violations by the time we are done"**

This means:
1. **Before refactoring**: Identify all Jane Street violations in target file
2. **During refactoring**: Fix violations as part of the refactoring work
3. **After refactoring**: Verify zero violations remain in modified file

### Per-Phase Integration

#### Phase 0: Hotspot Analysis
**Current**: Identifies high-complexity methods using jCodemunch
**Required**:
1. Query Jane Street KB for "complexity reduction" patterns
2. Check if target method has Jane Street violations
3. Include violation count in hotspot report
4. Flag methods with >5 violations as "high-risk"

**Implementation**:
```python
# In phase_0_hotspot_mcp.py
violations = load_violations_for_file(target_file)
method_violations = [v for v in violations if v['line'] in method_range]
kb_guidance = query_kb("complexity reduction")

# Add to hotspot report
report += f"\nJane Street Violations: {len(method_violations)}"
for v in method_violations:
    report += f"\n  - {v['rule_id']}: {v['message']}"
```

#### Phase 1: Scope + Boundary
**Current**: Defines extraction scope
**Required**:
1. Query Jane Street KB for "single-method extraction" patterns
2. Verify scope includes ALL violations in target method
3. Reject scope if it would leave violations behind
4. Add violation fixes to scope definition

**Implementation**:
```python
# In phase_1_scope_mcp.py
kb_guidance = query_kb("single-method extraction")
violations_in_scope = check_violations_in_scope(scope_definition)

# Validate
if violations_outside_scope:
    raise ScopeError("Scope must include all Jane Street violations")

# Add to scope
scope += "\n## Jane Street Violations to Fix"
for v in violations_in_scope:
    scope += f"\n- {v['rule_id']}: {v['fix_suggestion']}"
```

#### Phase 2: Architecture
**Current**: Creates extraction plan
**Required**:
1. Query Jane Street KB for "FSM extraction patterns"
2. Include violation fixes in architecture plan
3. Validate plan addresses all violations
4. Add Result<T,E> pattern if exceptions found

**Implementation**:
```python
# In phase_2_architecture_mcp.py
kb_guidance = query_kb("FSM extraction patterns")
violations = load_violations_for_epic(epic_id)

# Add to architecture plan
plan += "\n## Jane Street Compliance"
plan += f"\nViolations to fix: {len(violations)}"
for v in violations:
    plan += f"\n- {v['rule_id']}: {v['fix_suggestion']}"
    
# Validate
if "JS-001" in [v['rule_id'] for v in violations]:
    plan += "\n- Replace exceptions with Result<T,E> pattern"
```

#### Phase 3: Audit
**Current**: V12 DNA compliance checks
**Required**:
1. Query Jane Street KB for "V12 DNA compliance"
2. Run Jane Street violation check on planned changes
3. Fail audit if new violations introduced
4. Verify all existing violations addressed in plan

**Implementation**:
```python
# In phase_3_audit_mcp.py
kb_guidance = query_kb("V12 DNA compliance")
violations_before = load_violations_for_files(affected_files)

# Audit
audit_report += "\n## Jane Street Compliance Audit"
audit_report += f"\nExisting violations: {len(violations_before)}"
audit_report += "\nAll violations must be fixed during execution"

# Fail if plan doesn't address violations
if not plan_addresses_violations(plan, violations_before):
    raise AuditError("Plan does not address all Jane Street violations")
```

#### Phase 4: Ticket Generation
**Current**: Generates surgical extraction tickets
**Required**:
1. Query Jane Street KB for "ticket validation"
2. Include violation fixes in each ticket
3. Assign violations to appropriate tickets
4. Validate ticket covers all violations in scope

**Implementation**:
```python
# In phase_4_tickets_mcp.py
kb_guidance = query_kb("ticket validation")
violations = load_violations_for_epic(epic_id)

# Add to each ticket
for ticket in tickets:
    ticket_violations = [v for v in violations if v['file'] in ticket['files']]
    ticket['jane_street_fixes'] = [
        {
            'rule_id': v['rule_id'],
            'message': v['message'],
            'fix': v['fix_suggestion']
        }
        for v in ticket_violations
    ]
```

#### Phase 4.5: Ticket Review
**Current**: Reviews generated tickets
**Required**:
1. Query Jane Street KB for "ticket validation"
2. Verify each ticket includes violation fixes
3. Check that all violations are assigned to tickets
4. Fail review if any violations unassigned

**Implementation**:
```python
# In phase_4_5_ticket_review_mcp.py
kb_guidance = query_kb("ticket validation")
all_violations = load_violations_for_epic(epic_id)
assigned_violations = get_assigned_violations(tickets)

# Validate
unassigned = set(all_violations) - set(assigned_violations)
if unassigned:
    raise ReviewError(f"{len(unassigned)} violations not assigned to tickets")
```

#### Phase 5: Execution (Bob CLI)
**Current**: Bob CLI executes surgical extractions
**Required**:
1. Bob CLI queries Jane Street KB automatically
2. Bob includes violation fixes in refactoring
3. Bob validates zero violations after changes
4. Bob fails if violations remain

**Implementation**:
```bash
# In Bob CLI custom mode (v12-engineer)
# Add to mode instructions:
"Before refactoring, query Jane Street KB and load violations for target file.
Include violation fixes in your refactoring work.
After refactoring, verify zero violations remain in modified file.
Fail the task if any violations remain."
```

#### Phase 5.V: Verification
**Current**: Verifies ticket execution
**Required**:
1. Query Jane Street KB for "testing patterns"
2. Re-run violation check on modified files
3. Fail verification if ANY violations remain
4. Document violation fixes in verification report

**Implementation**:
```python
# In phase_5_verify_mcp.py
kb_guidance = query_kb("testing patterns")
violations_after = load_violations_for_files(modified_files)

# Verify
if violations_after:
    raise VerificationError(f"{len(violations_after)} violations remain after refactoring")

# Document
verification_report += "\n## Jane Street Compliance Verification"
verification_report += "\n✅ Zero violations remain in modified files"
```

---

## Implementation Plan

### Step 1: Update Phase Scripts (IMMEDIATE)

**Priority**: P0 - Blocking

**Tasks**:
1. ✅ Create violation analysis script (`analyze_jane_street_violations.py`)
2. ⏳ Create violation loader utility (`scripts/jane_street_utils.py`)
3. ⏳ Update Phase 0 script to include violation check
4. ⏳ Update Phase 1 script to include violation validation
5. ⏳ Update Phase 2 script to include violation fixes in plan
6. ⏳ Update Phase 3 script to audit violations
7. ⏳ Update Phase 4 script to assign violations to tickets
8. ⏳ Update Phase 4.5 script to validate violation assignment
9. ⏳ Update Phase 5.V script to verify zero violations
10. ⏳ Update Bob CLI mode to include Jane Street validation

**Estimated Time**: 4-6 hours

### Step 2: Test Integration (CRITICAL)

**Priority**: P0 - Blocking

**Tasks**:
1. Test Phase 0 with violation check (1 epic)
2. Test Phase 1 with violation validation (1 epic)
3. Test Phase 2 with violation fixes (1 epic)
4. Test Phase 3 audit (1 epic)
5. Test Phase 4 ticket generation (1 epic)
6. Test Phase 5 execution with Bob CLI (1 epic)
7. Test Phase 5.V verification (1 epic)
8. Verify end-to-end: zero violations after completion

**Estimated Time**: 2-3 hours

### Step 3: Re-Execute Phase 0 (REQUIRED)

**Priority**: P0 - Blocking

**Decision**: **YES, we must repeat Phase 0**

**Rationale**:
- Phase 0 completed without Jane Street context
- Hotspot selection may be wrong (should prioritize high-violation methods)
- Epic roadmap may need adjustment based on violations

**Tasks**:
1. Re-run Phase 0 with Jane Street integration (80 epics)
2. Update epic roadmap with violation counts
3. Re-prioritize epics based on violation density
4. Document changes from original Phase 0

**Estimated Time**: 2-3 hours (mostly automated)

### Step 4: Execute Phase 1+ with Integration (MAIN WORK)

**Priority**: P1 - After integration complete

**Tasks**:
1. Execute Phase 1 with Jane Street validation (80 epics)
2. Execute Phase 2 with violation fixes in plans (80 epics)
3. Execute Phase 3 with violation audits (80 epics)
4. Execute Phase 4 with violation assignment (80 epics)
5. Execute Phase 4.5 with validation (80 epics)
6. Execute Phase 5 with Bob CLI (per ticket)
7. Execute Phase 5.V with verification (per ticket)
8. Execute Phase 6 final review (80 epics)

**Estimated Time**: 30-40 hours (as originally planned)

---

## Success Criteria

### Per Epic
- ✅ All phases include Jane Street validation
- ✅ All violations in target file identified in Phase 0
- ✅ All violations included in scope (Phase 1)
- ✅ All violations addressed in architecture plan (Phase 2)
- ✅ All violations assigned to tickets (Phase 4)
- ✅ All violations fixed during execution (Phase 5)
- ✅ Zero violations remain after verification (Phase 5.V)

### Wave 4 Completion
- ✅ 80 epics complete with Jane Street compliance
- ✅ Zero Jane Street violations in refactored code
- ✅ All 299 existing violations addressed (if files touched)
- ✅ Jane Street KB queried in all required phases
- ✅ Violation tracking documented in completion reports

---

## Risk Assessment

### High Risk ⚠️

1. **Scope Creep**: Fixing violations may expand scope beyond complexity reduction
   - **Mitigation**: Treat violations as separate mini-tasks within epic
   - **Budget**: Add 20% time buffer per epic for violation fixes

2. **Bob CLI Integration**: Bob may not handle Jane Street validation correctly
   - **Mitigation**: Test with 1 epic before full wave
   - **Fallback**: Manual violation fixes if Bob fails

3. **Violation File Staleness**: 299 violations may be outdated
   - **Mitigation**: Re-scan codebase before Phase 0
   - **Update**: Regenerate violations file if needed

### Medium Risk ⚠️

1. **Firebase KB Incomplete**: May not have all patterns needed
   - **Mitigation**: Use violations file as primary source
   - **Fallback**: Manual research for missing patterns

2. **Performance Impact**: Violation checks may slow down phases
   - **Mitigation**: Cache violation data per epic
   - **Optimization**: Parallel violation checks

---

## Next Steps (IMMEDIATE)

1. **STOP all Phase 1 execution** ✅ (Already stopped - permission errors)
2. **Create violation loader utility** (30 min)
3. **Update Phase 0 script** (1 hour)
4. **Test Phase 0 with 1 epic** (30 min)
5. **Update Phase 1 script** (1 hour)
6. **Test Phase 1 with 1 epic** (30 min)
7. **Update remaining phase scripts** (3 hours)
8. **Test end-to-end with 1 epic** (2 hours)
9. **Re-execute Phase 0 for all 80 epics** (2 hours)
10. **Launch Phase 1+ with full integration** (30+ hours)

**Total Estimated Time to Integration**: 8-10 hours
**Total Estimated Time to Wave 4 Completion**: 40-50 hours

---

## Conclusion

**Wave 4 cannot proceed without Jane Street integration.** The current workflow does NOT address the 299 P0 violations, violating the core requirement: "every file we touch must not have any Jane Street violations by the time we are done."

**Recommendation**: Implement integration plan immediately, re-execute Phase 0, then proceed with full wave.

**Status**: BLOCKED until integration complete

---

**Document Version**: 1.0
**Last Updated**: 2026-06-15T02:01:00Z
**Maintainer**: V12 Orchestration Team
**Critical Priority**: P0 - Blocking Wave 4 execution