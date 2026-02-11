# Task 3: Account Performance Metrics

## Objectives
Implement a robust performance tracking and reporting system for the SIMA fleet, ensuring Apex compliance and real-time visibility.

## Checklist
- [x] Implement fixes for cancelled order handling
- [x] **Planning & Research**
    - [x] Analyze `UniversalORStrategyV12.cs` for existing account tracking logic.
    - [x] Research NinjaScript `Account` object properties for PnL and Drawdown.
    - [x] Define the data structure for fleet-wide metrics.
    - [x] Approved implementation plan and handoff to Codex.
- [ ] **Core Metrics Implementation**
    - [ ] Implement `AccountMetrics` class/struct to hold per-account data.
    - [ ] Create a `PerformaceTicker` background task or `OnAccountItemUpdate` handler.
    - [ ] Calculate Fleet-wide aggregates (Total PnL, Total Trades).
- [ ] **Apex Compliance & Safety**
    - [ ] Implement 30% Consistency Rule logic.
    - [ ] Add Trailing Drawdown monitoring for Apex accounts.
- [ ] **Persistence & Reporting**
    - [ ] Implement CSV logging for daily trade summaries.
    - [ ] Create a basic performance report generator.
- [ ] **UI Integration**
    - [ ] Update `UniversalORStrategyV12.cs` to show Fleet PnL and Health status in Dashboard.
    - [ ] Implement **Manual Account Selector Dropdown** in the chart overlay.
    - [ ] Add a "Metrics" toggle to the sidebar for detailed stats.
- [ ] **Verification**
    - [ ] Test PnL accuracy against NinjaTrader Account tab.
    - [ ] Verify Consistency Lock triggers correctly.

- [x] **Task 5: Legacy UI Extraction (Refactor)**
    - [x] Identify all legacy chart-based button logic in `UniversalORStrategyV12.cs`.
    - [x] Create a new standalone indicator: `LegacyChartButtonsIndicator.cs`.
    - [x] Move the WPF button creation and IPC relay logic to the new indicator.
    - [x] Clean up `UniversalORStrategyV12.cs` to remove old overlay button code.
    - [x] Verify IPC connectivity between the new Indicator and the Strategy.

- [x] **Task 4: Global Universalization Integration**
    - [x] Audit `.agent/` directory for any remaining platform-specific path references.
    - [x] Update `antigravity-bridge/SKILL.md` to emphasize repo-based brain.
    - [x] Update `universalorworkspacerules.md` to mandate `.agent/brain/` usage.
    - [x] Update all MISSION BRIEF templates to use relative repository paths.
    - [x] Verify all skills, prompts, and workflows are "Universal Brain" compliant.

- [x] **Task 7: Savings Dashboard (ROI Visualizer)**
    - [x] Build a React-based "Savings Dashboard" to track Execution Desk ROI.
    - [x] Implement logic to calculate "Hypothetical Cost" (Opus/Sonnet) vs "Actual Cost" (Free Pool).
    - [x] Display real-time savings metrics: Total USD Saved, Tasks Delegated, and Model Efficiency.
    - [x] Integrate the dashboard as a web artifact for easy review.

- [x] **Task 6: Workspace Hygiene & Archiving**
    - [x] Audit root directory for legacy strategy versions (V9 and below).
    - [x] Move all `UniversalORStrategyV[1-8]*.cs` files to `archived-versions/`.
    - [x] Move all `UniversalORSlaveV[1-8]*.cs` files to `archived-versions/`.
    - [x] Move legacy `.ps1` or `.md` files that no longer apply to `archived-versions/docs/`.
    - [x] Verify that only V12+ and active MasterHub files remain in the root.

- [x] **Task 8: Expert Tooling - TestSprite Integration**
    - [x] Research TestSprite MCP capabilities for NinjaScript/C# validation.
    - [x] Configure TestSprite as an automated "Pre-Deployment Gate" to catch errors.
    - [x] Link TestSprite to the Execution Desk via Project PRD.
    - [x] Implement a "1-Click Debug" workflow using TestSprite logs.

- [x] **Task 9: Context Optimization - Bloat Prevention & Ignore Rules**
    - [x] Implement `.claudeignore` and `.cursorignore` to exclude `bin/`, `obj/`, and `.git/`.
    - [ ] Identify and move massive `.csv` logs or Supermemory seeds causing 1M+ line diffs.
    - [ ] Add "Expert Gate: Context Audit" to the Knowledge Vault.
    - [ ] Standardize a 10KB threshold for file indexing to prevent Windows Pipe crashes.

