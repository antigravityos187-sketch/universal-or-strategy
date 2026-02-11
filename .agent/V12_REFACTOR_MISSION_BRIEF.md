# MISSION BRIEF & HANDOFF: [V12 Architectural Refactor]

**Status**: V12.11 is the STABLE baseline (Build 1008). 
**Sync Results**: Passed Sunday High-Performance Sync (TestSprite AI Scan) today with zero errors.

## 🎯 Primary Objective
Decompose the massive `UniversalORStrategyV12_V12_001.cs` into modular components to improve maintainability and prevent AI context bloat.

## 🚨 SESSION CHECK-IN (Rules of Engagement)
1. **Current Status**: Baseline is stable.
2. **Immediate Goal**: Extract **SIMA** (Order Hub) and **REAPER** (Safety Hub) into partial classes. Keep Core Logic and UI Bridge in the main file for this pass.
3. **Safety First**: Backup `_001.cs` before any changes.

## 🏗️ TARGET ARCHITECTURE (V12.12)
1. **Main Strategy**: `UniversalORStrategyV12_V12_002.cs` (Class: `UniversalORStrategyV12_002` marked `partial`)
2. **SIMA Module**: `UniversalORStrategyV12_V12_002_SIMA.cs` (Handles `ExecuteSmartDispatchEntry`, `AccountRankInfo`, `GetSortedAccountFleet`)
3. **Safety Hub (REAPER)**: `UniversalORStrategyV12_V12_002_REAPER.cs` (Handles `ReaperAudit` thread and audit logic)

> [!TIP]
> Use the **Code Simplifier** skill during extraction to reduce logic nesting and improve readability of the SIMA/REAPER modules.

## 🚨 MISSION CRITICAL RULES
- **PRESERVE SIMA**: Do NOT break the Master/Fleet loop.
- **DNA ADHERENCE**: Follow patterns in `C:\Users\Mohammed Khalid\.gemini\antigravity\knowledge\v12_baseline\artifacts\v12_dna.md`.
- **PLATFORM PARITY**: Follow `PLATFORM_PARITY_PROTOCOL.md`.
- **CODE SIMPLIFICATION**: Apply guidelines from [.agent/skills/code-simplifier/SKILL.md](file:///C:/WSGTA/universal-or-strategy/.agent/skills/code-simplifier/SKILL.md) to all extracted modules.

## 🧠 CONTEXT FOR AGENT
- **Strategy File**: [UniversalORStrategyV12_V12_001.cs](file:///C:/WSGTA/universal-or-strategy/UniversalORStrategyV12_V12_001.cs)
- **MCP Access**: Use `delegation_bridge` and `supermemory` tools.

## 🚀 EXECUTION STEPS
1. Audit dependencies for `SIMA` and `REAPER`.
2. Extract into the new partial class files.
3. Provide NinjaTrader build/compile verification instructions after each step.
