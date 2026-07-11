# Universal OR Strategy Director

**Active Project**: NinjaTrader 8 Trade Copier  
**Spec**: [`specs/002-trade-copier-spec.html`](specs/002-trade-copier-spec.html)  
**Stack**: C# / NinjaTrader 8 / Rithmic / Apex

---

## What This Is

A NinjaTrader 8 trade copier that replicates brackets from 1 Master account to up to 18 Follower accounts (Apex/Rithmic fleet) with full lifecycle mirroring — entries, stops, targets, cancels, modifications, and trailing stops.

Architecture: Actor Mailbox pattern with per-follower FSMs and lock-free `ConcurrentQueue<AccountEvent>` as the mailbox. Designed for skip-and-protect resilience on Rithmic connection flickers (500ms–2s).

Design reference: [`docs/copy_trader_design.md`](docs/copy_trader_design.md)

---

## Active Specs

| Spec | Status | Description |
|------|--------|-------------|
| [`specs/002-trade-copier-spec.html`](specs/002-trade-copier-spec.html) | **Active** | NT8 Trade Copier — full UI + engine spec |
| [`specs/001-agent-arena-platform/`](specs/001-agent-arena-platform/) | Backlog | Agent Arena Platform — AI agent competition infrastructure |

---

## Repository Structure

```
.bob/                    # Bob IDE modes, skills, hooks, commands
specs/
  002-trade-copier-spec.html   # NT8 Trade Copier spec (primary)
  001-agent-arena-platform/    # Agent Arena Platform spec (backlog)
  assets/                      # Competitor research, roadmap, screenshots
docs/
  copy_trader_design.md        # Actor Mailbox architecture design
  standards/jane-street/       # Jane Street coding standards (OKF)
  intel/jane-street/           # Jane Street engineering patterns
  protocol/                    # Development protocols
  brain/                       # Active session docs (non-epic)
scripts/                 # Active utility scripts
archive/
  v12-reference/         # V12 OR Strategy source — reference only
  wave-scripts/          # Wave 1–7 execution scripts — reference only
  morpheus/              # Morpheus agent — back burner
```

---

## Reference Archive

The V12 Universal OR Strategy source code is preserved at [`archive/v12-reference/src/`](archive/v12-reference/src/) for architectural reference. All wave execution scripts (Waves 1–7, 689+ shell scripts) are at [`archive/wave-scripts/`](archive/wave-scripts/).

**V12 is a reference implementation — not the active project.**

---

## Standards

All code follows Jane Street rules per [`docs/standards/jane-street/RULES_CATALOG.md`](docs/standards/jane-street/RULES_CATALOG.md):

- Lock-free Actor/Enqueue pattern (no `lock()`)
- CYC ≤ 8 per function
- xUnit only for tests
- No `async void`, no `return null`, no heap alloc on hot path

---

## Agent Config

- **Modes**: `.bob/custom_modes.yaml`
- **Skills**: `.bob/skills/`
- **Hooks**: `.bob/hooks/`
- **MCP**: `.mcp.json`
