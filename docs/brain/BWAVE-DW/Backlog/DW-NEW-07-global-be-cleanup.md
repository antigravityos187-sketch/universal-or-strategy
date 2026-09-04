## DW-NEW-07 -- Global BE Cleanup Orphaned on Last Panel Close

**Source**: Cubic/CodeRabbit review of PR #40, `src/PropTraderTools/TradeCopierPanel.cs:608`
**Discovered**: BWAVE-DW validation session, 2026-09-04
**Status**: DEFERRED -- not blocking BWAVE-DW merge

### Finding

DW-C38-03 removed `DisarmAllAccounts()` from `Detach()` because it was incorrectly
disarming sibling panels' BE state (the contracted bug fix). However, the removal creates
a new edge case:

When `BE ALL` arms multiple accounts and the **last registered panel** is closed, the
other accounts' pending BE watchers remain active. These watchers can still place
break-even orders on accounts that have no visible panel managing them.

The original `DisarmAllAccounts()` call served dual purpose:
1. (BUG) Disarm siblings on any panel close -- CORRECTLY removed
2. (VALID) Global cleanup on last-panel close -- ACCIDENTALLY removed

### Director Live-Trading Observations (to be added)

Director has live-trading observations from pre-refactor code collected during trading
on the session prior to BWAVE-DW completion. These observations describe the original
symptoms that led to the DW-C38-03 fix and may also inform the correct shape of
a global-cleanup solution. Director will provide these observations when this item
is next actioned.

### Proposed Fix Direction (for future wave)

Preserve DW-C38-03 sibling isolation. Add a separate global cleanup path triggered
only when the **last panel unregisters** from `CopyEngine` or `TradeCopierAddOn`.
Owner: `CopyEngine.DisarmAllAccountsIfLastPanel()` or equivalent, called from
`TradeCopierAddOn` on panel count reaching zero.

### Acceptance Criteria

- [ ] DW-C38-03 sibling isolation preserved (no regression)
- [ ] When last panel closes: all pending BE slots cleared
- [ ] Unit test: two-panel scenario -- close panel 1, watchers of panel 2 unaffected;
  close panel 2, all pending slots cleared
- [ ] Jane Street: no lock(), actor/enqueue pattern if state mutation needed

### Blocking?
No. Deferred to post-BWAVE-DW backlog.