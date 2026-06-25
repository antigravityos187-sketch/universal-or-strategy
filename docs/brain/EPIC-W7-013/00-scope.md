# Phase 1: Scope Definition - EPIC-W7-013

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:24:57Z
- **Input**: docs/brain/EPIC-W7-013/00-hotspots.md

## Target Method
- **Method**: UpdatePanelState
- **File**: src/V12_002.UI.Panel.StateSync.cs
- **Line**: 13
- **Current CYC**: 22
- **Target CYC**: <=8 (Jane Street threshold)
- **Reduction Required**: 14 points

## Scope Boundary Decision

### IN SCOPE: Core Orchestration Simplification

**Primary Goal**: Reduce UpdatePanelState from CYC=22 to CYC<=8 by extracting logical UI update groups.

**Extraction Strategy**: Group 13 direct callees into 4 extracted methods:

1. **ExtractStatusIndicatorUpdates** (3 callees)
   - UpdateHubStatusLed
   - UpdateTelemetryDisplay
   - UpdateComplianceDisplay
   - **Rationale**: Status indicators are a cohesive subsystem

2. **ExtractVisualSynchronization** (4 callees)
   - SyncModeChipVisuals
   - SyncCountChipVisuals
   - UpdateTrendIndicator
   - UpdateRmaButtonVisual
   - **Rationale**: Visual sync operations share common purpose

3. **ExtractConfigurationUI** (3 callees)
   - SyncPanelConfigFromSnapshot
   - SetConfigTargetButtonsVisible
   - SyncLiveTargetRows
   - **Rationale**: Configuration UI is a distinct concern

4. **ExtractContextualControls** (3 callees)
   - GetUiSnapshot (snapshot retrieval)
   - UpdateContextualUI
   - UpdateTargetVisibility
   - **Rationale**: Contextual control logic forms a unit

**Expected Outcome**:
- UpdatePanelState becomes a 4-call orchestrator (CYC<=8)
- Each extracted method has CYC<=8
- Preserves high-level coordination logic
- Maintains existing call signatures

### OUT OF SCOPE: External Dependencies

**Explicitly Excluded**:

1. **The 13 Direct Callees** (already implemented)
   - SyncModeChipVisuals
   - UpdateContextualUI
   - SyncPanelConfigFromSnapshot
   - SyncCountChipVisuals
   - UpdateTargetVisibility
   - UpdateRmaButtonVisual
   - UpdateHubStatusLed
   - UpdateTelemetryDisplay
   - UpdateComplianceDisplay
   - UpdateTrendIndicator
   - SetConfigTargetButtonsVisible
   - SyncLiveTargetRows
   - GetUiSnapshot
   - **Rationale**: These are leaf methods, not refactoring targets

2. **Depth-2 Callees** (42 indirect dependencies)
   - **Rationale**: Outside blast radius, no changes needed

3. **Caller Analysis** (zero callers detected)
   - **Rationale**: Method likely called via UI event handler or reflection
   - **Action**: No caller modifications needed
   - **Verification**: Deferred to Phase 5 (runtime testing)

4. **Cross-File Changes**
   - **Rationale**: Zero blast radius (no external importers)
   - **Action**: All changes confined to src/V12_002.UI.Panel.StateSync.cs

5. **Signature Changes**
   - **Rationale**: UpdatePanelState has zero parameters (simple signature)
   - **Action**: Preserve existing signature

### Scope Validation

**Boundary Enforcement**:
- IN SCOPE: Extract 4 methods from UpdatePanelState body
- IN SCOPE: Reduce UpdatePanelState CYC from 22 to <=8
- OUT OF SCOPE: Modify any of the 13 existing callees
- OUT OF SCOPE: Change UpdatePanelState signature
- OUT OF SCOPE: Modify files outside src/V12_002.UI.Panel.StateSync.cs

**Risk Mitigation**:
- Low blast radius (0.0 risk score) = safe refactoring
- Low churn (not in top 50 hotspots) = stable code
- Orchestrator pattern = preserve coordination logic
- Zero external dependencies = no cross-file impact

## Success Criteria

**Phase 1 Complete When**:
1. Scope boundary clearly defined (IN vs OUT)
2. 4 extraction targets identified with rationale
3. OUT OF SCOPE items explicitly listed
4. Risk mitigation strategy documented
5. Expected CYC reduction path validated (22 to <=8)

**Ready for Phase 2**: Architecture Planning
- Input: This scope definition
- Output: Detailed extraction plan with code structure

## Scope Creep Prevention

**If Discovered During Phase 2-5**:
- Additional complexity in callees -> OUT OF SCOPE (separate epic)
- Cross-file dependencies -> OUT OF SCOPE (re-analyze blast radius)
- Signature changes needed -> OUT OF SCOPE (requires Director approval)
- Dead code detection -> OUT OF SCOPE (log for future cleanup)

**Escalation Trigger**: Any deviation from this scope requires Phase 1.5 (Boundary Validation).
