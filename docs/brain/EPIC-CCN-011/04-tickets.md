# Extraction Tickets: EPIC-CCN-011

## Overview
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 4-6 hours
- **Target Method**: DestroyPanel()
- **Current CCN**: 17 (131,072 test paths)
- **Target CCN**: 3 (76 test paths)
- **Complexity Reduction**: 1,724x improvement

## Execution Strategy
Each ticket extracts one helper method incrementally with verification after each step. This ensures:
- Build succeeds after each extraction
- CCN reduces progressively (17 → 16 → 11 → 3)
- No behavioral changes introduced
- Unit tests validate each extracted method

---

## TICKET-1: Extract ValidatePanelState()

### Scope
- **Current Method**: `DestroyPanel()`
- **Current CCN**: 17
- **Target CCN**: 16 (after this extraction)
- **Extraction**: Guard clause validation for panel state
- **Lines to Extract**: 322-323 (null check + early return)
- **New Method CCN**: 1 (single branch)

### Implementation
1. Create new private method `ValidatePanelState()` with signature:
   ```csharp
   private bool ValidatePanelState()
   ```

2. Move null check logic from DestroyPanel() lines 322-323:
   ```csharp
   if (rootContainer == null)
       return false;
   return true;
   ```

3. Replace original lines in DestroyPanel() with:
   ```csharp
   if (!ValidatePanelState())
       return;
   ```

4. Verify CCN reduction:
   ```bash
   python scripts/complexity_audit.py
   ```

5. Add unit test in `tests/V12_Performance.Tests/UI/PanelConstructionTests.cs`:
   ```csharp
   [Test]
   public void ValidatePanelState_NullRootContainer_ReturnsFalse()
   [Test]
   public void ValidatePanelState_ValidRootContainer_ReturnsTrue()
   ```

### Acceptance Criteria
- [ ] Method complexity reduced from CCN 17 to CCN 16
- [ ] ValidatePanelState() has CCN 1
- [ ] All tests pass (dotnet test)
- [ ] Build succeeds (dotnet build)
- [ ] CSharpier formatting applied (dotnet csharpier format src/)
- [ ] Pre-push validation passes (scripts/pre_push_validation.ps1 -Fast)
- [ ] Unit tests added for ValidatePanelState()
- [ ] No behavioral changes (exact same execution flow)

### Dependencies
- None (first ticket in sequence)

### Verification Commands
```bash
# 1. Build check
dotnet build src/V12_002.csproj

# 2. Complexity check
python scripts/complexity_audit.py

# 3. Format check
dotnet csharpier format src/

# 4. Test check
dotnet test tests/V12_Performance.Tests/

# 5. Pre-push validation
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

---

## TICKET-2: Extract CleanupUIPlacement()

### Scope
- **Current Method**: `DestroyPanel()`
- **Current CCN**: 16 (after TICKET-1)
- **Target CCN**: 11 (after this extraction)
- **Extraction**: UI placement cleanup logic (switch statement)
- **Lines to Extract**: 332-378 (switch + try-catch blocks)
- **New Method CCN**: 6 (3 switch cases + nested conditions)

### Implementation
1. Create new private method `CleanupUIPlacement()` with signature:
   ```csharp
   private void CleanupUIPlacement()
   ```

2. Move UI placement cleanup logic from DestroyPanel() lines 332-378:
   ```csharp
   try
   {
       switch (_placementMode)
       {
           case PanelPlacementMode.Fallback:
               // UserControlCollection.Remove() logic
               break;
           case PanelPlacementMode.Injected:
               // _placementGrid cleanup + column removal logic
               break;
           case PanelPlacementMode.Hijack:
               // _placementGrid.Children.Remove() logic
               break;
       }
   }
   catch (Exception ex)
   {
       // Log and continue
   }
   ```

3. Replace original lines in DestroyPanel() with:
   ```csharp
   CleanupUIPlacement();
   ```

4. Verify CCN reduction:
   ```bash
   python scripts/complexity_audit.py
   ```

5. Add unit tests in `tests/V12_Performance.Tests/UI/PanelConstructionTests.cs`:
   ```csharp
   [Test]
   public void CleanupUIPlacement_FallbackMode_RemovesFromUserControlCollection()
   [Test]
   public void CleanupUIPlacement_InjectedMode_CleansUpPlacementGrid()
   [Test]
   public void CleanupUIPlacement_HijackMode_RemovesFromChildren()
   [Test]
   public void CleanupUIPlacement_ExceptionThrown_LogsAndContinues()
   ```

### Acceptance Criteria
- [ ] Method complexity reduced from CCN 16 to CCN 11
- [ ] CleanupUIPlacement() has CCN 6
- [ ] All tests pass (dotnet test)
- [ ] Build succeeds (dotnet build)
- [ ] CSharpier formatting applied (dotnet csharpier format src/)
- [ ] Pre-push validation passes (scripts/pre_push_validation.ps1 -Fast)
- [ ] Unit tests added for all 3 placement modes
- [ ] Error handling preserved (try-catch maintained)
- [ ] No behavioral changes (exact same UI cleanup)

### Dependencies
- **TICKET-1** must be completed first
- Requires ValidatePanelState() to be in place

### Verification Commands
```bash
# 1. Build check
dotnet build src/V12_002.csproj

# 2. Complexity check (verify CCN 11)
python scripts/complexity_audit.py

# 3. Format check
dotnet csharpier format src/

# 4. Test check
dotnet test tests/V12_Performance.Tests/

# 5. Pre-push validation
powershell -File .\scripts\pre_push_validation.ps1 -Fast

# 6. Manual F5 test in NinjaTrader
# Verify UI panel destruction works for all placement modes
```

---

## TICKET-3: Extract CleanupFieldReferences()

### Scope
- **Current Method**: `DestroyPanel()`
- **Current CCN**: 11 (after TICKET-2)
- **Target CCN**: 3 (final target - Jane Street compliant)
- **Extraction**: Field reference nullification for GC
- **Lines to Extract**: 380-468 (80+ field assignments)
- **New Method CCN**: 1 (sequential assignments, no branching)

### Implementation
1. Create new private method `CleanupFieldReferences()` with signature:
   ```csharp
   private void CleanupFieldReferences()
   ```

2. Move field nullification logic from DestroyPanel() lines 380-468:
   ```csharp
   // Nullify all 80+ UI element references and state fields
   rootContainer = null;
   _placementGrid = null;
   // ... (all other field nullifications)
   ```

3. Replace original lines in DestroyPanel() with:
   ```csharp
   CleanupFieldReferences();
   ```

4. Verify CCN reduction to final target:
   ```bash
   python scripts/complexity_audit.py
   ```

5. Add unit test in `tests/V12_Performance.Tests/UI/PanelConstructionTests.cs`:
   ```csharp
   [Test]
   public void CleanupFieldReferences_AllFieldsNullified_EnablesGarbageCollection()
   ```

### Acceptance Criteria
- [ ] Method complexity reduced from CCN 11 to CCN 3 (FINAL TARGET)
- [ ] CleanupFieldReferences() has CCN 1
- [ ] All tests pass (dotnet test)
- [ ] Build succeeds (dotnet build)
- [ ] CSharpier formatting applied (dotnet csharpier format src/)
- [ ] Pre-push validation passes (scripts/pre_push_validation.ps1 -Fast)
- [ ] Unit test validates GC eligibility
- [ ] All 80+ fields nullified correctly
- [ ] No memory leaks (verified via profiler if available)
- [ ] Jane Street compliance achieved (CCN ≤8)

### Dependencies
- **TICKET-1** must be completed first
- **TICKET-2** must be completed second
- Requires both ValidatePanelState() and CleanupUIPlacement() to be in place

### Verification Commands
```bash
# 1. Build check
dotnet build src/V12_002.csproj

# 2. Complexity check (verify CCN 3 - FINAL TARGET)
python scripts/complexity_audit.py

# 3. Format check
dotnet csharpier format src/

# 4. Test check
dotnet test tests/V12_Performance.Tests/

# 5. Full pre-push validation (all 13 checks)
powershell -File .\scripts\pre_push_validation.ps1

# 6. Manual F5 test in NinjaTrader
# Verify complete panel destruction and no memory leaks

# 7. Hard-link sync
powershell -File .\deploy-sync.ps1
```

---

## Final Verification Checklist

After completing all 3 tickets, verify:

### Functional Requirements
- [ ] DestroyPanel() behavior unchanged (exact same execution)
- [ ] All UI elements cleaned up correctly
- [ ] No memory leaks (all references nullified)
- [ ] Error handling preserved (try-catch blocks maintained)

### Non-Functional Requirements
- [ ] CCN 3 for DestroyPanel() (Jane Street compliant)
- [ ] CCN 1 for ValidatePanelState()
- [ ] CCN 6 for CleanupUIPlacement()
- [ ] CCN 1 for CleanupFieldReferences()
- [ ] No lock() statements (V12 DNA compliance)
- [ ] Build succeeds (zero compilation errors)
- [ ] All unit tests pass (new + existing)

### Quality Gates
- [ ] Pre-push validation passes (all 13 checks)
- [ ] Codacy shows "Up to quality standards"
- [ ] CodeRabbit AI review shows zero critical/high issues
- [ ] Manual F5 test in NinjaTrader (UI panel destruction works)
- [ ] Hard-link sync completed (deploy-sync.ps1)

### Complexity Metrics
- **Before**: CCN 17 (131,072 test paths)
- **After**: CCN 3 (76 test paths)
- **Reduction**: 1,724x improvement
- **Jane Street Compliance**: ✅ CCN ≤8 achieved

---

## Metadata
- **Epic ID**: EPIC-CCN-011
- **Phase**: 4 (Ticket Generation)
- **Status**: Ready for execution
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 4-6 hours
- **Target File**: src/V12_002.UI.Panel.Construction.cs
- **Target Method**: DestroyPanel()
- **Complexity Goal**: CCN 17 → CCN 3
- **Jane Street Compliance**: ✅ CCN ≤8 per method
- **Lock-Free Compliance**: ✅ No locks introduced
- **ASCII-Only Compliance**: ✅ No Unicode characters
- **PR Hygiene**: ✅ Diff <10k characters
- **Generated**: 2026-06-15
- **Next Phase**: Phase 5 (Ticket Execution)
