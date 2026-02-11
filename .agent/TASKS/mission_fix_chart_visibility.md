# Mission: Fix Orders Not Visible on Chart

**Assigned To:** Project Director (Codex 5.2)
**Structure:** This mission is part of the V12 Restoration. See `AGENTS.md` for role definition.
**Objective:** Ensure all submitted orders (Entry, Stop, Target) are visually represented on the NinjaTrader chart.

## Context
The V12.11 strategy is compiling and running, but orders submitted via the panel or strategy logic are not appearing on the chart visual.

## Investigation Targets
1. **Properties:** Check `UniversalORStrategyV12_Dev.cs` for:
   - `DrawOnPricePanel` (Ensure it is `true`).
   - `TraceOrders` (Ensure it is `true`).
   - `Display` attribute in class definition.
2. **Layering:** Check `ZOrder` or `IsOverlay`.
3. **Knowledge Base:** Search for "orders not visible" in `knowledge/v12_baseline`.

## Acceptance Criteria
- [ ] User confirms orders are visible on the chart after fix.
