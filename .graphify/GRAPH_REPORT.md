# Graph Report - .  (2026-07-02)

## Corpus Check
- Large corpus: 4004 files · ~3,755,119 words. Semantic extraction will be expensive (many Claude tokens). Consider running on a subfolder, or use --no-semantic to run AST-only.

## Summary
- 2411 nodes · 7189 edges · 0 communities detected
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output
- Edge kinds: ON_BRANCH: 3200 · MODIFIES: 964 · calls: 891 · contains: 824 · rationale_for: 653 · PARENT_OF: 446 · method: 140 · imports: 30 · references: 25 · imports_from: 11 · inherits: 4 · requires_env: 1


## Input Scope
- Requested: auto
- Resolved: committed (source: default-auto)
- Included files: 4004 · Candidates: 6947
- Excluded: 522 untracked · 2565 ignored · 10 sensitive · 16 missing committed
- Recommendation: Use --scope all or graphify.yaml inputs.corpus for a knowledge-base folder.

## Graph Freshness
- Built from Git commit: `09581d0`
- Compare this hash to `git rev-parse HEAD` before trusting freshness-sensitive graph output.
## God Nodes (most connected - your core abstractions)

## Surprising Connections (you probably didn't know these)
- None detected - all connections are within the same source files.

## Communities

## Knowledge Gaps
- **672 isolated node(s):** `Extract lesson from forensic report.          Returns:         {`, `Capture lesson to Firebase using existing script.`, `Update autonomous_refactor_session.json with failure.`, `Read current max Lamport clock from event log.`, `Append a Lamport-clocked event to the wave 7 event log.` (+667 more)
  These have ≤1 connection - possible missing edges or undocumented components.