# Graph Report - .  (2026-07-02)

## Corpus Check
- Large corpus: 4206 files · ~3,849,601 words. Semantic extraction will be expensive (many Claude tokens). Consider running on a subfolder, or use --no-semantic to run AST-only.

## Summary
- 2433 nodes · 7226 edges · 0 communities detected
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output
- Edge kinds: ON_BRANCH: 3201 · MODIFIES: 970 · calls: 898 · contains: 834 · rationale_for: 660 · PARENT_OF: 447 · method: 140 · imports: 33 · references: 27 · imports_from: 11 · inherits: 4 · requires_env: 1


## Input Scope
- Requested: auto
- Resolved: committed (source: default-auto)
- Included files: 4206 · Candidates: 7482
- Excluded: 4 untracked · 2570 ignored · 10 sensitive · 16 missing committed
- Recommendation: Use --scope all or graphify.yaml inputs.corpus for a knowledge-base folder.

## Graph Freshness
- Built from Git commit: `8b12f6b`
- Compare this hash to `git rev-parse HEAD` before trusting freshness-sensitive graph output.
## God Nodes (most connected - your core abstractions)

## Surprising Connections (you probably didn't know these)
- None detected - all connections are within the same source files.

## Communities

## Knowledge Gaps
- **679 isolated node(s):** `Extract lesson from forensic report.          Returns:         {`, `Capture lesson to Firebase using existing script.`, `Update autonomous_refactor_session.json with failure.`, `Read current max Lamport clock from event log.`, `Append a Lamport-clocked event to the wave 7 event log.` (+674 more)
  These have ≤1 connection - possible missing edges or undocumented components.