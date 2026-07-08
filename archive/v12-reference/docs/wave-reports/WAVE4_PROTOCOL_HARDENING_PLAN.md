# Wave 4 Protocol Hardening Plan

**Version**: 1.0  
**Date**: 2026-06-16  
**Status**: 🔴 CRITICAL - Must complete before Wave 4 retry  
**Estimated Time**: 6-8 hours

---

## Executive Summary

Wave 4 Phase 5 execution produced 28 critical issues (9 P0, 12 P1, 6 P2) across 6 of 7 PRs due to Bob CLI over-optimization during autonomous execution. Before attempting Wave 4 retry, we must harden ALL protocols to prevent recurrence.

**Root Causes Identified**:
1. ❌ No "SURGICAL ONLY" mandate in Phase 5 prompt
2. ❌ Phase 5.V verification insufficient (only checked file existence + build)
3. ❌ No Jane Street KB query requirement
4. ❌ No semantic diff review
5. ❌ No behavioral verification
6. ❌ Missing local execution protocol for encoding-sensitive epics
7. ❌ Missing xUnit test generation protocol
8. ❌ No Greptile-style pre-PR checks

---

## Hardening Scope

### 1. Phase 5 Execution Protocol (CRITICAL)
**File**: `scripts/phase_5_execute_mcp.py` + Phase 5 MCP server  
**Priority**: P0 - Blocks Wave 4 retry

#### Changes Required

**A. Add SURGICAL ONLY Mandate**
```python
# In phase_5_execute_mcp.py execute_phase_5 tool

SURGICAL_MANDATE = """
**CRITICAL: SURGICAL EXTRACTION ONLY**

You MUST follow these rules EXACTLY:

1. **NO OPTIMIZATION**: Extract method AS-IS, preserve ALL logic
2. **NO LINQ**: Do not introduce LINQ on hot paths (Jane Street violation)
3. **NO SIMPLIFICATION**: Keep all guards, checks, and edge cases
4. **NO MAGIC NUMBERS**: Do not introduce hardcoded values
5. **PRESERVE SEMANTICS**: Extracted method must be functionally identical
6. **PRESERVE GUARDS**: Keep all null checks, bounds checks, validation
7. **NO BEHAVIORAL CHANGES**: Zero tolerance for logic modifications

**Verification**: Before reporting success, confirm:
- Extracted method is byte-for-byte semantically identical
- No LINQ introduced
- No guards removed
- No magic numbers added
- Build passes
- All tests pass
"""

# Prepend to Bob CLI message
message = f"{SURGICAL_MANDATE}\n\n{original_message}"
```

**B. Add Jane Street KB Query Requirement**
```python
# In phase_5_execute_mcp.py

JANE_STREET_QUERY = """
**MANDATORY: Query Jane Street Knowledge Base**

Before extraction, query the Jane Street KB for:
1. Extraction patterns: `python scripts/query_kb.py "method extraction"`
2. Hot path rules: `python scripts/query_kb.py "hot path optimization"`
3. Guard preservation: `python scripts/query_kb.py "safety guards"`

Apply ALL retrieved rules to your extraction plan.
"""

# Add to message
message = f"{SURGICAL_MANDATE}\n\n{JANE_STREET_QUERY}\n\n{original_message}"
```

**C. Add Encoding Detection**
```python
# In phase_5_execute_mcp.py

def detect_encoding_issues(epic_id: str, file_path: str) -> bool:
    """Detect if file has encoding issues requiring local execution."""
    # Check for non-ASCII characters
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            if any(ord(c) > 127 for c in content):
                return True
    except UnicodeDecodeError:
        return True
    
    # Check for known problematic files
    problematic_patterns = [
        'DrawingHelpers',  # Known encoding issues
        'ChartControl',    # Unicode in comments
    ]
    
    return any(p in file_path for p in problematic_patterns)

# In execute_phase_5 tool
if detect_encoding_issues(epic_id, target_file):
    return {
        "status": "requires_local",
        "reason": "File has encoding issues - must execute locally",
        "instructions": "Run Phase 5 locally using Bob CLI with --local flag"
    }
```

**D. Add Test Framework Detection**
```python
# In phase_5_execute_mcp.py

def detect_test_requirements(epic_id: str, method_name: str) -> dict:
    """Detect if method requires test generation and which framework."""
    # Check if method is in testable layer
    testable_patterns = [
        'src/V12_002.Atm.cs',
        'src/V12_002.Execution.cs',
        'src/V12_002.SIMA.cs',
    ]
    
    # Check if tests already exist
    test_file = f"tests/V12_Performance.Tests/{method_name}Tests.cs"
    
    return {
        "requires_tests": True,  # Always require tests for complexity reduction
        "framework": "xUnit",    # MANDATORY per V12.32
        "test_file": test_file,
        "coverage_target": 80    # Minimum coverage
    }

# In execute_phase_5 tool
test_req = detect_test_requirements(epic_id, method_name)
if test_req["requires_tests"]:
    message += f"""
    
**TEST GENERATION REQUIRED**

Framework: {test_req["framework"]} (MANDATORY - no NUnit/MSTest)
Target: {test_req["test_file"]}
Coverage: {test_req["coverage_target"]}%

Generate tests that verify:
1. Extracted method produces identical output to original
2. All edge cases covered
3. All guards tested
4. No behavioral changes
"""
```

---

### 2. Phase 5.V Verification Protocol (CRITICAL)
**File**: `scripts/phase_5_verify_mcp.py` + Phase 5.V MCP server  
**Priority**: P0 - Blocks Wave 4 retry

#### Changes Required

**A. Add Semantic Diff Review**
```python
# In phase_5_verify_mcp.py execute_phase_5_verify tool

def semantic_diff_review(epic_id: str, ticket_id: str) -> dict:
    """Perform semantic diff review of changes."""
    # Get git diff
    result = subprocess.run(
        ['git', 'diff', 'HEAD~1', 'HEAD', '--', 'src/'],
        capture_output=True, text=True
    )
    diff = result.stdout
    
    # Check for red flags
    red_flags = []
    
    # Check 1: LINQ introduced
    if '.Where(' in diff or '.Select(' in diff or '.Any(' in diff:
        red_flags.append({
            "severity": "P0",
            "issue": "LINQ introduced on hot path",
            "rule": "Jane Street: No LINQ on hot paths"
        })
    
    # Check 2: Guards removed
    if '- if (' in diff and '+ if (' not in diff:
        red_flags.append({
            "severity": "P0",
            "issue": "Guard removed",
            "rule": "V12 DNA: Preserve all safety guards"
        })
    
    # Check 3: Magic numbers added
    import re
    magic_pattern = r'\+ .*\d{2,}'  # 2+ digit numbers in added lines
    if re.search(magic_pattern, diff):
        red_flags.append({
            "severity": "P1",
            "issue": "Magic number introduced",
            "rule": "Jane Street: No magic numbers"
        })
    
    # Check 4: Exception handling weakened
    if '- try' in diff or '- catch' in diff:
        red_flags.append({
            "severity": "P0",
            "issue": "Exception handling removed",
            "rule": "V12 DNA: Preserve error handling"
        })
    
    return {
        "passed": len(red_flags) == 0,
        "red_flags": red_flags,
        "diff_size": len(diff.split('\n'))
    }
```

**B. Add Jane Street Compliance Check**
```python
# In phase_5_verify_mcp.py

def jane_street_compliance_check(epic_id: str) -> dict:
    """Run Jane Street rule checker on changed files."""
    result = subprocess.run(
        ['python', 'scripts/jane_street_rule_checker.py', 'src/', '--severity', 'ALL', '--json'],
        capture_output=True, text=True
    )
    
    violations = json.loads(result.stdout)
    
    # Filter to only new violations (compare to baseline)
    baseline_file = f"docs/brain/{epic_id}/jane_street_baseline.json"
    if os.path.exists(baseline_file):
        with open(baseline_file) as f:
            baseline = json.load(f)
        
        new_violations = [
            v for v in violations 
            if v not in baseline
        ]
    else:
        new_violations = violations
    
    return {
        "passed": len(new_violations) == 0,
        "new_violations": new_violations,
        "total_violations": len(violations)
    }
```

**C. Add Behavioral Verification**
```python
# In phase_5_verify_mcp.py

def behavioral_verification(epic_id: str, ticket_id: str) -> dict:
    """Verify no behavioral changes."""
    # Run unit tests
    test_result = subprocess.run(
        ['dotnet', 'test', '--no-build', '--verbosity', 'quiet'],
        capture_output=True, text=True
    )
    
    tests_passed = test_result.returncode == 0
    
    # Check for test coverage of extracted method
    # (Requires test generation in Phase 5)
    
    # Run complexity audit
    complexity_result = subprocess.run(
        ['python', 'scripts/complexity_audit.py'],
        capture_output=True, text=True
    )
    
    # Verify complexity reduction achieved
    # (Parse output and compare to target)
    
    return {
        "tests_passed": tests_passed,
        "coverage_adequate": True,  # TODO: Implement coverage check
        "complexity_reduced": True   # TODO: Implement complexity check
    }
```

**D. Update Verification Report**
```python
# In phase_5_verify_mcp.py execute_phase_5_verify tool

# Add to verification report
verification_report = f"""# Phase 5.V Verification Report - {epic_id}

## Ticket: {ticket_id}

### 1. File Existence ✅
- Completion file: {completion_file_exists}
- Modified files: {modified_files}

### 2. Build Status ✅
- Build result: {build_passed}
- Errors: {build_errors}

### 3. Semantic Diff Review {'✅' if semantic_diff['passed'] else '❌'}
- Red flags: {len(semantic_diff['red_flags'])}
- Diff size: {semantic_diff['diff_size']} lines

{format_red_flags(semantic_diff['red_flags'])}

### 4. Jane Street Compliance {'✅' if jane_street['passed'] else '❌'}
- New violations: {len(jane_street['new_violations'])}
- Total violations: {jane_street['total_violations']}

{format_violations(jane_street['new_violations'])}

### 5. Behavioral Verification {'✅' if behavioral['tests_passed'] else '❌'}
- Tests passed: {behavioral['tests_passed']}
- Coverage adequate: {behavioral['coverage_adequate']}
- Complexity reduced: {behavioral['complexity_reduced']}

### Overall Status: {'PASS' if all_checks_passed else 'FAIL'}

**Action Required**: {'None - proceed to Phase 6' if all_checks_passed else 'Fix issues before Phase 6'}
"""
```

---

### 3. Local Execution Protocol (NEW)
**File**: `docs/protocol/LOCAL_EXECUTION_PROTOCOL.md`  
**Priority**: P1 - Required for encoding-sensitive epics

#### Protocol Definition

```markdown
# Local Execution Protocol

**Version**: 1.0  
**Applies To**: Epics with encoding issues, test generation requirements, or complex refactoring

## When to Execute Locally

Execute Phase 5 locally (not on VM) when:

1. **Encoding Issues**: File contains non-ASCII characters
2. **Test Generation**: Epic requires xUnit test creation
3. **Complex Refactoring**: Method has CYC > 30 (requires human oversight)
4. **Known Problematic Files**: DrawingHelpers, ChartControl, etc.

## Local Execution Steps

### Step 1: Detect Local Requirement
```bash
# Phase 5 MCP server detects and returns:
{
  "status": "requires_local",
  "reason": "File has encoding issues",
  "epic_id": "EPIC-CCN-X"
}
```

### Step 2: Execute Locally
```bash
# On local machine (not VM)
cd c:/WSGTA/universal-or-strategy

# Run Bob CLI with local flag
bob --mode v12-engineer --local "Execute Phase 5 for EPIC-CCN-X"
```

### Step 3: Verify Locally
```bash
# Run pre-push validation
powershell -File .\scripts\pre_push_validation.ps1

# Verify encoding
python scripts/verify_ascii_only.py src/

# Run tests
dotnet test
```

### Step 4: Commit and Sync
```bash
# Commit locally
git add src/ tests/
git commit -m "feat: EPIC-CCN-X Phase 5 (local execution)"

# Push to VM for Phase 6
git push origin gitbutler/workspace
```

## Success Criteria

- ✅ No encoding errors
- ✅ All tests pass (including new xUnit tests)
- ✅ Build passes
- ✅ Pre-push validation passes
- ✅ No Jane Street violations
```

---

### 4. xUnit Test Generation Protocol (NEW)
**File**: `docs/protocol/TEST_GENERATION_PROTOCOL.md`  
**Priority**: P1 - Required per V12.32

#### Protocol Definition

```markdown
# Test Generation Protocol

**Version**: 1.0  
**Framework**: xUnit (MANDATORY - no NUnit/MSTest per V12.32)

## When to Generate Tests

Generate xUnit tests for EVERY Phase 5 extraction:

1. **Complexity Reduction**: Method extracted from CYC > 15
2. **Hot Path**: Method on critical execution path
3. **State Mutation**: Method modifies shared state
4. **Error Handling**: Method contains try/catch blocks

## Test Generation Steps

### Step 1: Create Test File
```bash
# Location: tests/V12_Performance.Tests/
# Naming: {MethodName}Tests.cs
```

### Step 2: Test Template
```csharp
using Xunit;
using V12_002;

namespace V12_Performance.Tests
{
    public class ExtractedMethodTests
    {
        [Fact]
        public void ExtractedMethod_WithValidInput_ReturnsExpectedOutput()
        {
            // Arrange
            var input = /* test data */;
            var expected = /* expected output */;
            
            // Act
            var actual = TargetClass.ExtractedMethod(input);
            
            // Assert
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void ExtractedMethod_WithNullInput_ThrowsArgumentNullException()
        {
            // Arrange
            object input = null;
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                TargetClass.ExtractedMethod(input));
        }
        
        [Theory]
        [InlineData(/* edge case 1 */)]
        [InlineData(/* edge case 2 */)]
        public void ExtractedMethod_WithEdgeCases_HandlesCorrectly(/* params */)
        {
            // Test edge cases
        }
    }
}
```

### Step 3: Coverage Target
- **Minimum**: 80% line coverage
- **Target**: 90% line coverage
- **Ideal**: 100% branch coverage

### Step 4: Verification
```bash
# Run tests
dotnet test tests/V12_Performance.Tests/

# Check coverage (if tool available)
dotnet test /p:CollectCoverage=true
```

## Success Criteria

- ✅ Test file created in correct location
- ✅ Uses xUnit framework (not NUnit/MSTest)
- ✅ All tests pass
- ✅ Coverage ≥ 80%
- ✅ Tests verify behavioral equivalence
```

---

### 5. Autonomous Refactor Command Updates (CRITICAL)
**File**: `.bob/commands/autonomous-refactor.md`  
**Priority**: P0 - Blocks future waves

#### Changes Required

**A. Add Prerequisites Section**
```markdown
## PREREQUISITES (MANDATORY - CHECK BEFORE EVERY WAVE)

### 1. Protocol Hardening Verification
```bash
# Verify all protocols updated
python scripts/verify_protocols.py --wave N

# Expected output:
# ✅ Phase 5 Execution Protocol: v2.0 (SURGICAL ONLY mandate)
# ✅ Phase 5.V Verification Protocol: v2.0 (semantic diff review)
# ✅ Local Execution Protocol: v1.0 (encoding detection)
# ✅ Test Generation Protocol: v1.0 (xUnit mandatory)
```

### 2. Pilot Test Requirement
```bash
# MANDATORY: Test 1 epic before launching wave
python scripts/pilot_test.py --epic EPIC-CCN-001 --phase 5

# Success criteria:
# ✅ No behavioral changes
# ✅ No Jane Street violations
# ✅ Tests generated and passing
# ✅ Greptile review clean (0 P0/P1 issues)
```

### 3. Rollback Plan
```bash
# Document rollback procedure BEFORE launching wave
python scripts/create_rollback_plan.py --wave N

# Creates: docs/brain/wave-N-rollback-plan.md
```
```

**B. Add Phase 5 Execution Gate**
```markdown
## PHASE 5 EXECUTION GATE (NEW)

Before executing Phase 5 for ANY epic:

### Gate 5.1: Encoding Check
```bash
python scripts/detect_encoding_issues.py --epic EPIC-CCN-X

# If encoding issues detected:
# → Execute locally (not on VM)
# → Follow LOCAL_EXECUTION_PROTOCOL.md
```

### Gate 5.2: Test Requirements Check
```bash
python scripts/detect_test_requirements.py --epic EPIC-CCN-X

# If tests required:
# → Add test generation to Phase 5 prompt
# → Follow TEST_GENERATION_PROTOCOL.md
```

### Gate 5.3: Jane Street KB Query
```bash
# MANDATORY: Query KB before extraction
python scripts/query_kb.py "method extraction"
python scripts/query_kb.py "hot path optimization"

# Apply ALL retrieved rules to extraction plan
```
```

**C. Add Phase 5.V Verification Gate**
```markdown
## PHASE 5.V VERIFICATION GATE (ENHANCED)

Phase 5.V now includes 5 checks (not just 2):

### Check 1: File Existence ✅
- Completion file exists
- Modified files tracked

### Check 2: Build Status ✅
- Build passes
- No compilation errors

### Check 3: Semantic Diff Review (NEW) ✅
- No LINQ introduced
- No guards removed
- No magic numbers added
- No exception handling weakened

### Check 4: Jane Street Compliance (NEW) ✅
- Zero new P0/P1/P2 violations
- All hot path rules followed

### Check 5: Behavioral Verification (NEW) ✅
- All tests pass
- Coverage ≥ 80%
- Complexity reduction achieved

**GATE**: ALL 5 checks must pass before Phase 6
```

---

### 6. Building-Blocks Method Updates
**File**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`  
**Priority**: P1 - Prevent script generation errors

#### Changes Required

**A. Add Phase 5 Script Template**
```bash
# Add to SOP V3

## Phase 5 Script Template (SURGICAL ONLY)

#!/bin/bash
# Phase 5 (Ticket Execution) for EPIC-CCN-XXX
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-XXX"
API_KEY="bob_prod_..."

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check
if ! ls docs/brain/${EPIC_ID}/04-tickets.md 1> /dev/null 2>&1; then
    echo "ERROR: Missing Phase 4 tickets for ${EPIC_ID}"
    exit 1
fi

# Create message file with SURGICAL ONLY mandate
cat > /tmp/phase5_msg_XXX.txt << 'EOFMSG'
**CRITICAL: SURGICAL EXTRACTION ONLY**

You MUST follow these rules EXACTLY:
1. NO OPTIMIZATION: Extract method AS-IS
2. NO LINQ: Do not introduce LINQ on hot paths
3. NO SIMPLIFICATION: Keep all guards and checks
4. NO MAGIC NUMBERS: Do not introduce hardcoded values
5. PRESERVE SEMANTICS: Functionally identical
6. PRESERVE GUARDS: Keep all null checks
7. NO BEHAVIORAL CHANGES: Zero tolerance

**MANDATORY: Query Jane Street KB**
Before extraction:
1. python scripts/query_kb.py "method extraction"
2. python scripts/query_kb.py "hot path optimization"
3. Apply ALL retrieved rules

Use the phase-5-execute MCP server to execute Phase 5 for EPIC-CCN-XXX.

Call the execute_phase_5 tool with epic_id="EPIC-CCN-XXX".

**Verification**: Confirm completion file exists on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell
bob --yolo "$(cat /tmp/phase5_msg_XXX.txt)"

# Verify completion file created
if [ -f "docs/brain/${EPIC_ID}/ticket-1-completion.md" ]; then
    echo "SUCCESS: Phase 5 complete for ${EPIC_ID}"
else
    echo "ERROR: No completion file created for ${EPIC_ID}"
    exit 1
fi
```

---

### 7. Greptile Pre-PR Integration (NEW)
**File**: `scripts/pre_pr_greptile_check.py`  
**Priority**: P2 - Catch issues before PR creation

#### Implementation

```python
#!/usr/bin/env python3
"""
Pre-PR Greptile Check
Run Greptile-style checks BEFORE creating PR to catch issues early.
"""

import subprocess
import json
import sys

def run_greptile_checks(branch: str) -> dict:
    """Run Greptile-style checks on branch."""
    
    checks = {
        "compilation": check_compilation(),
        "linq_on_hot_path": check_linq_hot_path(),
        "guards_removed": check_guards_removed(),
        "magic_numbers": check_magic_numbers(),
        "exception_handling": check_exception_handling(),
        "jane_street": check_jane_street_compliance(),
    }
    
    issues = []
    for check_name, result in checks.items():
        if not result["passed"]:
            issues.extend(result["issues"])
    
    return {
        "passed": len(issues) == 0,
        "issues": issues,
        "checks_run": len(checks)
    }

def check_compilation() -> dict:
    """Check if code compiles."""
    result = subprocess.run(
        ['dotnet', 'build', '--no-restore'],
        capture_output=True, text=True
    )
    
    return {
        "passed": result.returncode == 0,
        "issues": [] if result.returncode == 0 else [{
            "severity": "P0",
            "type": "compilation",
            "message": "Code does not compile",
            "details": result.stderr
        }]
    }

def check_linq_hot_path() -> dict:
    """Check for LINQ on hot paths."""
    # Get diff
    result = subprocess.run(
        ['git', 'diff', 'origin/main', 'HEAD', '--', 'src/'],
        capture_output=True, text=True
    )
    diff = result.stdout
    
    linq_patterns = ['.Where(', '.Select(', '.Any(', '.First(', '.OrderBy(']
    issues = []
    
    for pattern in linq_patterns:
        if pattern in diff:
            issues.append({
                "severity": "P0",
                "type": "linq_on_hot_path",
                "message": f"LINQ method {pattern} introduced on hot path",
                "rule": "Jane Street: No LINQ on hot paths"
            })
    
    return {
        "passed": len(issues) == 0,
        "issues": issues
    }

# ... (implement other checks)

if __name__ == "__main__":
    branch = sys.argv[1] if len(sys.argv) > 1 else "HEAD"
    
    print(f"Running Greptile-style checks on {branch}...")
    result = run_greptile_checks(branch)
    
    if result["passed"]:
        print(f"✅ All {result['checks_run']} checks passed")
        sys.exit(0)
    else:
        print(f"❌ {len(result['issues'])} issues found:")
        for issue in result["issues"]:
            print(f"  [{issue['severity']}] {issue['type']}: {issue['message']}")
        sys.exit(1)
```

---

## Implementation Timeline

### Phase 1: Critical Protocols (4 hours)
- [ ] Update Phase 5 Execution Protocol (SURGICAL ONLY mandate)
- [ ] Update Phase 5.V Verification Protocol (5 checks)
- [ ] Update autonomous-refactor command (prerequisites + gates)
- [ ] Test on 1 epic locally

### Phase 2: New Protocols (2 hours)
- [ ] Create Local Execution Protocol
- [ ] Create Test Generation Protocol
- [ ] Create pre-PR Greptile check script
- [ ] Update building-blocks SOP V3

### Phase 3: Pilot Test (2 hours)
- [ ] Execute EPIC-CCN-001 with hardened protocols
- [ ] Verify 0 issues in Greptile review
- [ ] Document any remaining gaps
- [ ] Update protocols if needed

### Phase 4: Wave 4 Retry (24 hours VM + 4 hours human)
- [ ] Execute rollback (30 minutes)
- [ ] Launch Phase 5-6 with hardened protocols
- [ ] Monitor execution (4-minute polling)
- [ ] Sync files and create PRs
- [ ] Verify 0 P0/P1 issues in all PRs

---

## Success Criteria

### Protocol Hardening Complete When:
- ✅ All 7 protocols updated with lessons learned
- ✅ Pilot test passes with 0 Greptile issues
- ✅ Building-blocks SOP V3 updated
- ✅ Pre-PR checks integrated
- ✅ Local execution protocol documented
- ✅ Test generation protocol documented

### Wave 4 Retry Success When:
- ✅ All 79 epics execute cleanly
- ✅ All PRs pass Greptile with 0 P0/P1 issues
- ✅ Build passes after each PR merge
- ✅ No behavioral changes detected
- ✅ All Jane Street rules followed
- ✅ All tests pass (including new xUnit tests)

---

## Risk Mitigation

### Risk 1: Protocols Still Insufficient
**Mitigation**: Pilot test on 1 epic before full wave launch

### Risk 2: New Issues Discovered
**Mitigation**: Document in protocol, update immediately, re-pilot

### Risk 3: Local Execution Bottleneck
**Mitigation**: Identify encoding-sensitive epics upfront, execute in parallel locally

### Risk 4: Test Generation Overhead
**Mitigation**: Use test generation templates, automate with Bob CLI

---

## Next Steps

1. **Immediate**: Review this plan with Director for approval
2. **Phase 1**: Implement critical protocol updates (4 hours)
3. **Phase 2**: Implement new protocols (2 hours)
4. **Phase 3**: Pilot test EPIC-CCN-001 (2 hours)
5. **Phase 4**: Execute Wave 4 rollback and retry (28 hours)

**Estimated Total Time**: 36 hours (8 hours human + 28 hours VM)

---

**Status**: 🔴 AWAITING DIRECTOR APPROVAL  
**Next Action**: Review and approve protocol hardening plan  
**Blocker**: Cannot proceed with Wave 4 retry until protocols hardened