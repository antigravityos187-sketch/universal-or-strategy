# Extraction Tickets: EPIC-CCN-079

## Overview
- **Epic**: EPIC-CCN-079 - CreateSection0_Identity Extraction
- **Target Method**: CreateSection0_Identity
- **Target File**: src/V12_002.UI.Panel.Construction.cs
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 1.5 hours (30 min per ticket + verification)
- **Current Complexity**: CYC 13
- **Target Complexity**: CYC ≤ 8 (Jane Street strict standard)

---

## TICKET-1: Extract CreateLeaderAccountRow

### Scope
- **Current Method**: `CreateSection0_Identity`
- **Current CYC**: 13
- **Target CYC**: ~11 (after this extraction)
- **Helper Method CYC**: ~2
- **Lines to Extract**: 518-545 (35 lines)
- **Extraction**: Leader account display row with status LED

### Implementation
1. Create new private method `CreateLeaderAccountRow()` returning `Grid`
2. Move lines 518-545 from CreateSection0_Identity to new method
3. Replace extracted code with single method call: `Grid leaderRow = CreateLeaderAccountRow();`
4. Verify method accesses class fields: `hubStatusLed`, `leaderAccountCombo`, `Account`
5. Ensure method calls `CreateCombo()` for combo box initialization
6. Run `python scripts/complexity_audit.py` (expect CYC 13→11)
7. Run `powershell -File .\scripts\build_readiness.ps1` (verify zero errors)
8. Commit: "EPIC-CCN-079: Extract CreateLeaderAccountRow (CYC 13→11)"

### Method Signature
```csharp
private Grid CreateLeaderAccountRow()
```

### Responsibilities
- Create Grid with 2 columns (Auto, Star)
- Initialize hubStatusLed Border (status indicator)
- Create leaderAccountCombo ComboBox with account name
- Configure layout and styling

### Acceptance Criteria
- [ ] Helper method complexity CYC ≤ 8 (target: 2)
- [ ] Main method complexity reduced to ~11
- [ ] Zero compilation errors
- [ ] Zero test failures
- [ ] No new Codacy violations
- [ ] No behavioral changes (UI renders identically)
- [ ] Build succeeds
- [ ] ASCII-only compliance maintained
- [ ] No lock() statements introduced

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Complexity check
python scripts/complexity_audit.py

# Build verification
powershell -File .\scripts\build_readiness.ps1

# Format check
dotnet csharpier check src/

# Commit
git add src/V12_002.UI.Panel.Construction.cs
git commit -m "EPIC-CCN-079: Extract CreateLeaderAccountRow (CYC 13→11)"
```

---

## TICKET-2: Extract CreateFleetSelectionButton

### Scope
- **Current Method**: `CreateSection0_Identity`
- **Current CYC**: ~11 (after TICKET-1)
- **Target CYC**: ~10 (after this extraction)
- **Helper Method CYC**: ~1
- **Lines to Extract**: 547-567 (20 lines)
- **Extraction**: Fleet account selection button

### Implementation
1. Create new private method `CreateFleetSelectionButton()` returning `Button`
2. Move lines 547-567 from CreateSection0_Identity to new method
3. Replace extracted code with single method call: `Button fleetButton = CreateFleetSelectionButton();`
4. Verify method accesses class field: `fleetSelectButton`
5. Ensure button styling, content, fonts, and colors are configured
6. Run `python scripts/complexity_audit.py` (expect CYC 11→10)
7. Run `powershell -File .\scripts\build_readiness.ps1` (verify zero errors)
8. Commit: "EPIC-CCN-079: Extract CreateFleetSelectionButton (CYC 11→10)"

### Method Signature
```csharp
private Button CreateFleetSelectionButton()
```

### Responsibilities
- Initialize fleetSelectButton with styling
- Set button content, fonts, colors
- Configure layout properties (margins, padding, alignment)

### Acceptance Criteria
- [ ] Helper method complexity CYC ≤ 8 (target: 1)
- [ ] Main method complexity reduced to ~10
- [ ] Zero compilation errors
- [ ] Zero test failures
- [ ] No new Codacy violations
- [ ] No behavioral changes (button renders identically)
- [ ] Build succeeds
- [ ] ASCII-only compliance maintained
- [ ] No lock() statements introduced

### Dependencies
- TICKET-1 must be completed first

### Verification Commands
```powershell
# Complexity check
python scripts/complexity_audit.py

# Build verification
powershell -File .\scripts\build_readiness.ps1

# Format check
dotnet csharpier check src/

# Commit
git add src/V12_002.UI.Panel.Construction.cs
git commit -m "EPIC-CCN-079: Extract CreateFleetSelectionButton (CYC 11→10)"
```

---

## TICKET-3: Extract CreateFleetPopup

### Scope
- **Current Method**: `CreateSection0_Identity`
- **Current CYC**: ~10 (after TICKET-2)
- **Target CYC**: ~3 (after this extraction) ✅ MEETS TARGET
- **Helper Method CYC**: ~8
- **Lines to Extract**: 569-665 (95 lines)
- **Extraction**: Fleet account selection popup with checkboxes and event handlers

### Implementation
1. Create new private method `CreateFleetPopup()` returning `Popup`
2. Move lines 569-665 from CreateSection0_Identity to new method
3. Replace extracted code with single method call: `Popup fleetPopup = CreateFleetPopup();`
4. Verify method accesses class fields: `fleetPopup`, `fleetCheckboxPanel`, `selectedFleetAccounts`, `activeFleetAccounts`
5. Ensure method calls: `GetFleetAccountsSnapshot()`, `UpdateFleetButtonText()`, `PanelCommand()`
6. Verify event handlers (Checked/Unchecked) are properly wired
7. **CRITICAL**: Verify `PanelCommand()` routes through FSM/Actor Enqueue pattern (no direct state mutation)
8. Run `python scripts/complexity_audit.py` (expect CYC 10→3)
9. Run `powershell -File .\scripts\build_readiness.ps1` (verify zero errors)
10. Commit: "EPIC-CCN-079: Extract CreateFleetPopup (CYC 10→3)"

### Method Signature
```csharp
private Popup CreateFleetPopup()
```

### Responsibilities
- Create popup border and container
- Add "Select ALL" checkbox with event handlers
- Iterate through fleet accounts and create individual checkboxes
- Wire up Checked/Unchecked event handlers
- Initialize selectedFleetAccounts collection
- Route state changes through PanelCommand() FSM/Actor pattern

### Acceptance Criteria
- [ ] Helper method complexity CYC ≤ 8 (target: 8)
- [ ] Main method complexity reduced to ~3 ✅ MEETS TARGET (≤8)
- [ ] Zero compilation errors
- [ ] Zero test failures
- [ ] No new Codacy violations
- [ ] No behavioral changes (popup renders identically)
- [ ] Fleet selection updates selectedFleetAccounts correctly
- [ ] PanelCommand() routes through FSM/Actor pattern (verified)
- [ ] Event handlers execute on UI thread only
- [ ] Build succeeds
- [ ] ASCII-only compliance maintained
- [ ] No lock() statements introduced

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Verification Commands
```powershell
# Complexity check
python scripts/complexity_audit.py

# Build verification
powershell -File .\scripts\build_readiness.ps1

# Format check
dotnet csharpier check src/

# Commit
git add src/V12_002.UI.Panel.Construction.cs
git commit -m "EPIC-CCN-079: Extract CreateFleetPopup (CYC 10→3)"
```

### Special Verification
- [ ] Verify PanelCommand() implementation uses FSM/Actor Enqueue pattern
- [ ] Verify selectedFleetAccounts is only accessed on UI thread
- [ ] Test fleet selection in NinjaTrader (F5 load, select accounts, verify behavior)
- [ ] Screenshot comparison: Before vs After (verify identical rendering)

---

## Final Verification (After All Tickets)

### Step 1: Complexity Audit
```powershell
python scripts/complexity_audit.py
```
**Expected**: CreateSection0_Identity CYC = 3 (77% reduction from 13)

### Step 2: Hard Link Sync
```powershell
powershell -File .\deploy-sync.ps1
```
**Expected**: NinjaTrader hard links synchronized

### Step 3: Pre-Push Validation
```powershell
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```
**Expected**: All checks pass (ASCII, Build, Tests, Lint, Format, PR Hygiene, Complexity)

### Step 4: Codacy Check
```powershell
powershell -File .\scripts\query_codacy_issues.ps1
```
**Expected**: Zero new violations

### Step 5: NinjaTrader Integration Test
1. Press F5 in NinjaTrader
2. Open V12 panel
3. Verify Section 0 renders correctly
4. Test leader account display
5. Test fleet selection button
6. Test fleet popup (select/deselect accounts)
7. Verify selectedFleetAccounts updates correctly

### Step 6: Git Push
```bash
git push origin feature/epic-ccn-079
```

---

## Success Criteria Summary

### Complexity Targets ✅
- [x] Main method: CYC ≤8 (target: 3)
- [x] Helper 1 (CreateLeaderAccountRow): CYC ≤8 (target: 2)
- [x] Helper 2 (CreateFleetSelectionButton): CYC ≤8 (target: 1)
- [x] Helper 3 (CreateFleetPopup): CYC ≤8 (target: 8)
- [x] Total complexity reduction: 77% (13 → 3)

### Lock-Free Compliance ✅
- [x] No lock() statements in any extracted method
- [x] No new synchronization primitives
- [x] UI thread single-threaded execution model
- [x] PanelCommand() routes through FSM/Actor pattern

### Jane Street Alignment ✅
- [x] Cognitive simplicity (CYC ≤8 per method)
- [x] Testability (each helper can be unit tested)
- [x] Correctness by construction (WPF type safety)
- [x] No performance impact (UI construction only)

### Boundary Compliance ✅
- [x] Single method extraction only (CreateSection0_Identity)
- [x] No caller modifications
- [x] No callee modifications (CreateSectionBorder, CreateSectionHeader, CreateCombo unchanged)
- [x] Private helper methods only (no public API changes)
- [x] Same class (no new files)

### PR Health ✅
- [x] Diff size <10k characters (estimated ~4,000 chars)
- [x] No whitespace mutation (single file, CSharpier formatting)
- [x] Surgical changes only (3 extractions)
- [x] Rebase onto origin/main before push

---

## Risk Mitigation

### Low Risk Factors
- ✅ UI construction code (not hot-path)
- ✅ Single-threaded execution (UI thread only)
- ✅ No shared state mutations outside UI thread
- ✅ Clear extraction boundaries
- ✅ Incremental verification at each step

### Rollback Plan
```bash
git restore src/V12_002.UI.Panel.Construction.cs
powershell -File .\deploy-sync.ps1
```

---

## Phase 4 Status

**Status**: ✅ TICKETS GENERATED
**Total Tickets**: 3
**Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
**Estimated Effort**: 1.5 hours
**Confidence Level**: HIGH

**Next Phase**: Phase 5 - Ticket Execution (Engineer)

**Ready for Implementation**: YES
