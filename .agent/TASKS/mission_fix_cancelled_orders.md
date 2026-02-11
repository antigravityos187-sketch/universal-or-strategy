# Mission: Fix Cancelled Orders Persistence

**Assigned To:** Project Director (Codex 5.2)
**Structure:** This mission is part of the V12 Restoration. See `AGENTS.md` for role definition.
**Objective:** Ensure cancelled orders are properly removed or filtered from the Control Center "Orders" tab (if possible) or ensure the strategy is not ghost-tracking them.

## Context
Cancelled orders remain visible in the Orders tab. User described them as "remaining," implying clutter.

## Investigation Targets
1. **Strategy Logic:** Check `OnOrderUpdate` in `UniversalORStrategyV12_Dev.cs`.
   - Verify `OrderState.Cancelled` is handled.
   - Ensure `activePositions` or `workingOrders` collections are cleaned up.
2. **NinjaTrader Behavior:** Determine if this is default NT8 behavior (history retention) or if the strategy is failing to send a cleanup signal (if applicable, though NT8 lists usually persist history).
   - *Goal:* Minimizing clutter or ensuring "Ghost Orders" are dead.

## Acceptance Criteria
- [ ] "Ghost Orders" are verified dead in strategy memory.
- [ ] User understands NT8 history vs. active order persistence.
