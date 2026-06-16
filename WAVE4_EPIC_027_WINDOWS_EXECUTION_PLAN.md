# EPIC-027 Windows Execution Plan - Full Verification

## Decision: Windows Environment with Full Verification

**Rationale**: Complete .NET build/test/complexity verification for production-quality results.

**Trade-off**: Slower execution (~2-3 hours) vs VM-only (~30 minutes), but higher quality assurance.

## Current Status

### Completed on VM
- ✅ Phase 0: Hotspot Analysis
- ✅ Phase 1: Scope Definition
- ✅ Phase 1.5: Boundary Validation
- ✅ Phase 2: Architecture Planning
- ✅ Phase 3: DNA & PR Audit
- ✅ Phase 4: Ticket Generation (3 tickets)
- ⚠️ Phase 5: TICKET-1 extraction only (verification pending)

### Pending on Windows
- ❌ TICKET-1 Verification (build/test/complexity)
- ❌ TICKET-2 Execution (RegisterBracketState)
- ❌ TICKET-3 Execution (DispatchToPhotonKernel)
- ❌ Phase 5.V: Verification Report
- ❌ Phase 6: Final Review

## Execution Steps

### Step 1: Sync Files from VM to Local (5 minutes)

**Files to Sync**:
```bash
# Sync EPIC-027 brain directory
gcloud compute scp --recurse v12-test-golden-v2:~/universal-or-strategy/docs/brain/EPIC-CCN-027 ./docs/brain/ --zone=us-central1-a

# Verify sync
ls -la docs/brain/EPIC-CCN-027/
```

**Expected Files**:
- `00-hotspots.md`
- `00-scope.md`
- `01-scope-boundary.md`
- `02-architecture-plan.md`
- `03-audit-report.md`
- `04-tickets.md`
- `05-execution-plan.md`
- `ticket-1-completion.md` (partial)
- `manifest.json`

### Step 2: Verify TICKET-1 Locally (10 minutes)

**Commands**:
```powershell
# Navigate to project
cd c:/WSGTA/universal-or-strategy

# Build
dotnet build
# Expected: SUCCESS (0 errors)

# Run TICKET-1 tests
dotnet test --filter "FullyQualifiedName~SIMADispatchTests.CreateBracketOrders"
# Expected: 6/6 tests PASS

# Complexity audit
python scripts/complexity_audit.py
# Expected: CreateBracketOrders CYC ≤ 8

# Format check
dotnet csharpier check src/
# Expected: No formatting issues
```

**Success Criteria**:
- ✅ Build succeeds (0 errors)
- ✅ All 6 tests pass
- ✅ Complexity ≤ 8
- ✅ No formatting issues

**If Failures**: Fix issues before proceeding to TICKET-2.

### Step 3: Execute TICKET-2 (RegisterBracketState) (30 minutes)

**Bob CLI Command**:
```powershell
bob --mode v12-engineer "Execute TICKET-2 for EPIC-CCN-027: Extract RegisterBracketState method. Follow TDD workflow: RED tests → extraction → GREEN tests → complexity audit. Target CYC ≤ 8. Use docs/brain/EPIC-CCN-027/04-tickets.md for requirements."
```

**Verification**:
```powershell
# Build
dotnet build

# Run TICKET-2 tests
dotnet test --filter "FullyQualifiedName~SIMADispatchTests.RegisterBracketState"

# Complexity audit
python scripts/complexity_audit.py

# Format
dotnet csharpier format src/
```

**Output**: `docs/brain/EPIC-CCN-027/ticket-2-completion.md`

### Step 4: Execute TICKET-3 (DispatchToPhotonKernel) (30 minutes)

**Bob CLI Command**:
```powershell
bob --mode v12-engineer "Execute TICKET-3 for EPIC-CCN-027: Extract DispatchToPhotonKernel method. Follow TDD workflow: RED tests → extraction → GREEN tests → complexity audit. Target CYC ≤ 8. Use docs/brain/EPIC-CCN-027/04-tickets.md for requirements."
```

**Verification**:
```powershell
# Build
dotnet build

# Run TICKET-3 tests
dotnet test --filter "FullyQualifiedName~SIMADispatchTests.DispatchToPhotonKernel"

# Complexity audit
python scripts/complexity_audit.py

# Format
dotnet csharpier format src/
```

**Output**: `docs/brain/EPIC-CCN-027/ticket-3-completion.md`

### Step 5: Execute Phase 5.V (Verification) (15 minutes)

**Bob CLI Command**:
```powershell
bob --mode advanced "Execute Phase 5.V (Verification) for EPIC-CCN-027. Verify all 3 tickets executed successfully, complexity targets met, build passes, tests pass. Create verification report at docs/brain/EPIC-CCN-027/05-verification-report.md."
```

**Manual Verification**:
```powershell
# Full build
dotnet build
# Expected: SUCCESS

# All tests
dotnet test
# Expected: All PASS (including 19 EPIC-027 tests)

# Complexity audit
python scripts/complexity_audit.py
# Expected: All extracted methods CYC ≤ 8

# Format check
dotnet csharpier check src/
# Expected: No issues
```

**Output**: `docs/brain/EPIC-CCN-027/05-verification-report.md`

### Step 6: Execute Phase 6 (Final Review) (15 minutes)

**Bob CLI Command**:
```powershell
bob --mode advanced "Execute Phase 6 (Final Review) for EPIC-CCN-027. Review all phase outputs, verify epic completion, update manifest and roadmap. Create completion report at docs/brain/EPIC-CCN-027/06-completion-report.md."
```

**Manual Verification**:
```powershell
# Verify completion report exists
ls docs/brain/EPIC-CCN-027/06-completion-report.md

# Verify manifest updated
cat docs/brain/EPIC-CCN-027/manifest.json | grep "epic_status"
# Expected: "COMPLETED"

# Verify roadmap updated
cat epic_roadmap.json | grep -A 5 "EPIC-CCN-027"
# Expected: status = "COMPLETED"
```

**Output**: `docs/brain/EPIC-CCN-027/06-completion-report.md`

### Step 7: Sync Results Back to VM (5 minutes)

**Commands**:
```bash
# Sync updated brain directory
gcloud compute scp --recurse ./docs/brain/EPIC-CCN-027 v12-test-golden-v2:~/universal-or-strategy/docs/brain/ --zone=us-central1-a

# Sync updated source files
gcloud compute scp --recurse ./src v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a

# Sync updated roadmap
gcloud compute scp ./epic_roadmap.json v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a

# Verify sync
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls -la ~/universal-or-strategy/docs/brain/EPIC-CCN-027/"
```

**Verification**:
- ✅ All completion files synced (ticket-1, ticket-2, ticket-3)
- ✅ Verification report synced (05-verification-report.md)
- ✅ Completion report synced (06-completion-report.md)
- ✅ Source files synced (src/)
- ✅ Roadmap synced (epic_roadmap.json)

### Step 8: Update Wave 4 Status (5 minutes)

**Actions**:
1. Update `WAVE4_PROGRESS_SUMMARY_2026-06-16_01-03.md`
2. Mark EPIC-027 as COMPLETED
3. Update completion: 78/80 (97.5%)
4. Document Windows execution approach

## Timeline Estimate

| Step | Duration | Cumulative |
|------|----------|------------|
| 1. Sync from VM | 5 min | 5 min |
| 2. Verify TICKET-1 | 10 min | 15 min |
| 3. Execute TICKET-2 | 30 min | 45 min |
| 4. Execute TICKET-3 | 30 min | 75 min |
| 5. Phase 5.V | 15 min | 90 min |
| 6. Phase 6 | 15 min | 105 min |
| 7. Sync to VM | 5 min | 110 min |
| 8. Update status | 5 min | 115 min |

**Total**: ~2 hours (115 minutes)

## Success Criteria

### Per Ticket
- ✅ Build succeeds (0 errors)
- ✅ All tests pass
- ✅ Complexity ≤ 8 CYC
- ✅ No formatting issues
- ✅ Completion file created

### Phase 5.V
- ✅ All 3 tickets verified
- ✅ 19 tests passing (16 unit + 3 integration)
- ✅ Verification report created

### Phase 6
- ✅ Completion report created
- ✅ Manifest updated (status = COMPLETED)
- ✅ Roadmap updated (EPIC-027 = COMPLETED)

### Overall
- ✅ All files synced to VM
- ✅ Wave 4 status: 78/80 (97.5%)
- ✅ EPIC-027 fully complete with verification

## Risk Mitigation

### Risk 1: Build Failures
- **Mitigation**: Fix immediately before proceeding
- **Fallback**: Revert to last known good state

### Risk 2: Test Failures
- **Mitigation**: Debug and fix tests
- **Fallback**: Document failures, proceed with caution

### Risk 3: Complexity Violations
- **Mitigation**: Refactor to meet CYC ≤ 8
- **Fallback**: Document technical debt

### Risk 4: Time Overrun
- **Mitigation**: Focus on TICKET-2 and TICKET-3 first
- **Fallback**: Complete verification in next session

## Next Steps After EPIC-027

1. **EPIC-016**: Manual re-scope (~2 hours)
2. **EPIC-016**: Execute Phase 5 + Phase 6 (~1 hour)
3. **Wave 4 Complete**: 80/80 (100%)
4. **Final Report**: Document results, lessons learned

## Commands Quick Reference

```powershell
# Sync from VM
gcloud compute scp --recurse v12-test-golden-v2:~/universal-or-strategy/docs/brain/EPIC-CCN-027 ./docs/brain/ --zone=us-central1-a

# Verify TICKET-1
dotnet build
dotnet test --filter "FullyQualifiedName~SIMADispatchTests.CreateBracketOrders"
python scripts/complexity_audit.py
dotnet csharpier check src/

# Execute TICKET-2
bob --mode v12-engineer "Execute TICKET-2 for EPIC-CCN-027..."

# Execute TICKET-3
bob --mode v12-engineer "Execute TICKET-3 for EPIC-CCN-027..."

# Phase 5.V
bob --mode advanced "Execute Phase 5.V for EPIC-CCN-027..."

# Phase 6
bob --mode advanced "Execute Phase 6 for EPIC-CCN-027..."

# Sync to VM
gcloud compute scp --recurse ./docs/brain/EPIC-CCN-027 v12-test-golden-v2:~/universal-or-strategy/docs/brain/ --zone=us-central1-a
gcloud compute scp --recurse ./src v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp ./epic_roadmap.json v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a
```

---

**Status**: Ready to execute
**Environment**: Windows (local) with full .NET verification
**Estimated Duration**: 2 hours
**Next Action**: Sync files from VM (Step 1)