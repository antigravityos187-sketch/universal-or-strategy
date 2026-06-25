# Phase 0: Hotspot Analysis - EPIC-W7-035

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 1.51
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:41:33Z

## Target Method
- **Method**: SyncLimitTarget
- **File**: src/V12_002.Orders.Management.StopSync.cs
- **Line**: 176
- **Cyclomatic Complexity**: 21
- **Max Nesting Depth**: 6
- **Parameter Count**: 9

## Complexity Metrics

### Hotspot Analysis
From get_hotspots (top 50 methods by complexity x churn):
- **Rank**: #16 out of 50 hotspots
- **Hotspot Score**: 67.5964
- **Assessment**: HIGH
- **Churn (90 days)**: 24 commits
- **Complexity**: 21 (exceeds Jane Street threshold of 8)

### Comparison to Repository Hotspots
Top 5 hotspots for context:
1. HydrateFromOpenPositions (CYC 34, score 120.88)
2. IsCommandForThisInstrument (CYC 38, score 109.83)
3. HandleTerminated (CYC 30, score 102.04)
4. SweepBrokerOrders (CYC 28, score 99.55)
5. HydrateWorkingOrdersFromBroker (CYC 23, score 81.77)

**SyncLimitTarget is in the top 33% of hotspots**, indicating significant refactoring priority.

## Blast Radius

### Import Analysis
- **Direct Dependents**: 0
- **Importer Count**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files**: 0
- **Potential Files**: 0

### Interpretation
**LOW BLAST RADIUS**: This method has no external importers, meaning changes are isolated to the containing file. This is ideal for refactoring - low risk of breaking downstream code.

## Call Hierarchy

### Callers (Who calls this method)
1. **RefreshActivePositionOrders** (method)
   - File: src/V12_002.Orders.Management.StopSync.cs
   - Line: 37
   - Resolution: ast_resolved
   - **Single caller** - changes are highly localized

### Callees (What this method calls)
Total: 10 methods called

**Direct calls (depth 1)**:
1. CalculateTargetPriceFromPos (PositionInfo) - ast_inferred
2. LogBuffer.Format - ast_inferred

**Indirect calls (depth 2)**:
3. CalculateTargetPrice (PositionInfo) - ast_resolved
4. LogBuffer.ValidateThreadAffinity - ast_resolved
5. LogBuffer.FormatInternal - ast_resolved

### Call Graph Complexity
- **Caller depth**: 1 (shallow)
- **Callee depth**: 2 (moderate)
- **Total call graph nodes**: 11 (1 caller + 10 callees)

## Risk Assessment

### Overall Risk: **MEDIUM-LOW**

**Risk Factors**:
- LOW - Blast radius (0 external dependents)
- LOW - Call hierarchy (single caller, well-defined callees)
- MEDIUM - Complexity (CYC 21, exceeds threshold by 2.6x)
- MEDIUM - Churn (24 commits in 90 days = active code)
- MEDIUM - Hotspot ranking (#16/50 = top third)

**Refactoring Safety**:
- Isolated scope (no external dependencies)
- Single caller makes testing straightforward
- Well-defined call graph
- High complexity requires careful extraction
- Active churn means recent changes - verify git history

### Recommended Approach
1. **Extract decision logic** into smaller methods (target CYC <= 8 per method)
2. **Preserve call sites** - RefreshActivePositionOrders is the only caller
3. **Test coverage** - Add unit tests before refactoring
4. **Incremental refactoring** - Break into 2-3 tickets if needed

## Jane Street Alignment

### Complexity Threshold
- **Target**: CYC <= 8 (Jane Street strict standard)
- **Current**: CYC 21
- **Gap**: 13 points over threshold
- **Reduction needed**: ~62% complexity reduction

### HFT Principles
- **Cognitive simplicity**: Current complexity (21) makes microsecond-latency reasoning difficult
- **Exhaustive testing**: 21 cyclomatic paths = 2^21 potential test cases (impractical)
- **Race condition auditing**: High nesting (6 levels) increases lock-free verification difficulty

## Next Steps (Phase 1)
1. Review method source code for extraction candidates
2. Identify decision branches that can be isolated
3. Define scope boundary (what stays, what extracts)
4. Generate architecture plan with target CYC <= 8 per extracted method
