# Wave 4 EPIC-027 Build Blocker Resolution

**Date**: 2026-06-16T01:54:00Z  
**Status**: CRITICAL ARCHITECTURAL CONSTRAINT DISCOVERED  
**Impact**: Changes resolution strategy for all 3 build errors

## Critical Discovery

**V12_002.csproj Cannot Be Created as Standalone Project**

### Root Cause
The V12_002 source code depends on **NinjaTrader 8 proprietary assemblies**:
- `NinjaTrader.Cbi` (Commodity Broker Interface)
- `NinjaTrader.Gui` (GUI components)
- `NinjaTrader.Data` (Market data)
- `NinjaTrader.NinjaScript.DrawingTools`
- `NinjaTrader.NinjaScript.Indicators`
- `System.Windows.Controls` (WPF - requires .NET Framework)
- `System.Windows.Media` (WPF - requires .NET Framework)

**Total Errors**: 1,143 compilation errors (all missing assembly references)

### Why This Matters
1. **NinjaTrader strategies compile ONLY within NT8 environment**
2. **No standalone SDK available** for external compilation
3. **Test project reference to `../../src/V12_002.csproj` is fundamentally broken**
4. **Cannot fix with .csproj creation** - requires NT8 assemblies

## Architectural Constraint

**NinjaTrader 8 Compilation Model**:
```
User writes strategy → Copies to NT8/bin/Custom/Strategies/ → NT8 compiles at runtime → Loads into NT8 process
```

**NOT**:
```
User writes strategy → dotnet build → Standalone executable
```

## Impact on Build Errors

### Error 1: Missing V12_002.csproj ❌ CANNOT FIX
- **Original Plan**: Create standalone .csproj
- **Reality**: Requires NT8 assemblies (not available)
- **Resolution**: Remove test project reference OR mock NT8 assemblies

### Error 2: HandleFlatPositionUpdateTests.cs Syntax ✅ CAN FIX
- **Status**: Independent of NT8 assemblies
- **Resolution**: Fix syntax errors in test file

### Error 3: Testing.csproj .NET 4.8 Target ✅ CAN FIX
- **Status**: Configuration issue
- **Resolution**: Remove net48 target or install SDK

## Resolution Options

### Option A: Remove Test Project Reference (RECOMMENDED)
**Pros**:
- ✅ Fastest (5 minutes)
- ✅ Aligns with NT8 architecture
- ✅ No mock assemblies needed
- ✅ Tests can still run via NT8 Strategy Analyzer

**Cons**:
- ❌ Loses IDE test integration
- ❌ Cannot run `dotnet test` locally

**Implementation**:
1. Edit `tests/V12_Performance.Tests/V12_Performance.Tests.csproj`
2. Remove `<ProjectReference Include="../../src/V12_002.csproj" />`
3. Tests reference NT8 assemblies directly (if available)
4. OR tests become integration tests (run in NT8 only)

### Option B: Mock NinjaTrader Assemblies (NOT RECOMMENDED)
**Pros**:
- ✅ Enables local `dotnet test`
- ✅ IDE test integration

**Cons**:
- ❌ Requires creating 10+ mock assemblies
- ❌ Mocks diverge from real NT8 behavior
- ❌ 2-4 hours of work
- ❌ Maintenance burden

### Option C: Accept NT8-Only Testing (PRAGMATIC)
**Pros**:
- ✅ Zero code changes
- ✅ Aligns with NT8 architecture
- ✅ Tests run in production environment

**Cons**:
- ❌ No local test execution
- ❌ Requires NT8 installation

## Recommended Resolution Strategy

**REVISED 3-COMMIT PLAN**:

### Commit 1: Remove Broken Test Project Reference (5 min)
```xml
<!-- tests/V12_Performance.Tests/V12_Performance.Tests.csproj -->
<!-- REMOVE THIS LINE: -->
<ProjectReference Include="../../src/V12_002.csproj" />
```

**Rationale**: Test project cannot reference NT8 strategy code outside NT8 environment.

### Commit 2: Fix HandleFlatPositionUpdateTests.cs Syntax (30 min)
- Fix syntax errors at lines 480, 589
- Verify test file compiles independently

### Commit 3: Fix Testing.csproj .NET 4.8 Target (30 min)
- Remove net48 target OR install .NET 4.8 SDK
- Verify solution builds cleanly

**Total Time**: 1 hour (down from 1.5 hours)

## EPIC-027 Execution Strategy

**AFTER Build Fixes**:

### Step 1: Verify Clean Build (5 min)
```powershell
dotnet build universal-or-strategy.sln
```
Expected: Zero errors (test project compiles without src/ reference)

### Step 2: Execute EPIC-027 in NinjaTrader 8 (2 hours)
**CRITICAL**: All V12_002 work MUST happen in NT8 environment:
1. Open NinjaTrader 8
2. Load V12_002 strategy in Strategy Analyzer
3. Execute TICKET-2 (RegisterBracketState extraction)
4. Execute TICKET-3 (DispatchToPhotonKernel extraction)
5. Run tests via Strategy Analyzer (not `dotnet test`)
6. Verify complexity via CodeScene/Codacy (not local tools)

### Step 3: Sync Results to VM (5 min)
- Copy completion reports to VM
- Update roadmap to 78/80

## V12.23 Protocol Compliance

**Question**: Does removing test project reference violate "ONE EPIC = ONE CONCERN"?

**Answer**: NO - This is a **prerequisite fix**, not scope creep:
- EPIC-027 is blocked by broken test infrastructure
- Fixing test infrastructure is separate from EPIC-027 extraction work
- Analogous to fixing broken CI before running tests

**Commit Message**:
```
fix: Remove broken V12_002.csproj reference from test project

NinjaTrader strategies cannot be compiled outside NT8 environment.
Test project reference to src/V12_002.csproj is fundamentally broken
because V12_002 depends on NT8 proprietary assemblies (NinjaTrader.Cbi,
NinjaTrader.Gui, etc.) which are not available in standalone builds.

Tests must run via NT8 Strategy Analyzer, not dotnet test.

This is a prerequisite fix to unblock EPIC-027 execution.
```

## Lessons Learned

### For Future Waves
1. **Never create .csproj for NT8 strategies** - they compile in NT8 only
2. **Test projects should NOT reference src/** - use NT8 Strategy Analyzer
3. **Complexity audits must use NT8 tools** - not standalone dotnet tools
4. **All V12_002 work happens in NT8** - not in VS Code/CLI

### Protocol Updates Needed
- **V12.31**: Document NT8 compilation constraint
- **V12.32**: Update test strategy (NT8 Strategy Analyzer only)
- **V12.33**: Update complexity audit workflow (CodeScene/Codacy, not local)

## Next Actions

1. ✅ Remove test project reference (Commit 1)
2. ✅ Fix HandleFlatPositionUpdateTests.cs syntax (Commit 2)
3. ✅ Fix Testing.csproj .NET 4.8 target (Commit 3)
4. ✅ Execute EPIC-027 in NT8 environment (2 hours)
5. ✅ Sync to VM and update roadmap (78/80)

---

**Status**: Resolution strategy defined | NT8 architectural constraint documented  
**Next Step**: Remove broken test project reference (Commit 1)  
**Timeline**: 1 hour fixes + 2 hours EPIC-027 = 3 hours to 78/80