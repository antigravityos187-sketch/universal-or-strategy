# Graph Report - .  (2026-07-05)

## Corpus Check
- Large corpus: 4348 files · ~4,061,745 words. Semantic extraction will be expensive (many Claude tokens). Consider running on a subfolder, or use --no-semantic to run AST-only.

## Summary
- 2633 nodes · 9159 edges · 0 communities detected
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output
- Edge kinds: ON_BRANCH: 4866 · MODIFIES: 1006 · calls: 931 · contains: 858 · rationale_for: 680 · PARENT_OF: 606 · method: 140 · imports: 33 · references: 24 · imports_from: 11 · inherits: 4


## Input Scope
- Requested: auto
- Resolved: committed (source: default-auto)
- Included files: 4348 · Candidates: 7653
- Excluded: 2 untracked · 2675 ignored · 10 sensitive · 16 missing committed
- Recommendation: Use --scope all or graphify.yaml inputs.corpus for a knowledge-base folder.

## Graph Freshness
- Built from Git commit: `b5b4bb8`
- Compare this hash to `git rev-parse HEAD` before trusting freshness-sensitive graph output.
## God Nodes (most connected - your core abstractions)

## Surprising Connections (you probably didn't know these)
- None detected - all connections are within the same source files.

## Communities

## Knowledge Gaps
- **692 isolated node(s):** `Extract lesson from forensic report.          Returns:         {`, `Capture lesson to Firebase using existing script.`, `Update autonomous_refactor_session.json with failure.`, `Read current max Lamport clock from event log.`, `Append a Lamport-clocked event to the wave 7 event log.` (+687 more)
  These have ≤1 connection - possible missing edges or undocumented components.