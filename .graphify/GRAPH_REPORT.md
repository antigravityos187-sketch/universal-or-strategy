# Graph Report - .  (2026-07-04)

## Corpus Check
- Large corpus: 4252 files · ~3,960,155 words. Semantic extraction will be expensive (many Claude tokens). Consider running on a subfolder, or use --no-semantic to run AST-only.

## Summary
- 2591 nodes · 9085 edges · 0 communities detected
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output
- Edge kinds: ON_BRANCH: 4822 · MODIFIES: 1021 · calls: 928 · contains: 857 · rationale_for: 679 · PARENT_OF: 562 · method: 140 · imports: 33 · references: 27 · imports_from: 11 · inherits: 4 · requires_env: 1


## Input Scope
- Requested: auto
- Resolved: committed (source: default-auto)
- Included files: 4252 · Candidates: 7545
- Excluded: 34 untracked · 2566 ignored · 10 sensitive · 16 missing committed
- Recommendation: Use --scope all or graphify.yaml inputs.corpus for a knowledge-base folder.

## Graph Freshness
- Built from Git commit: `ff9936f`
- Compare this hash to `git rev-parse HEAD` before trusting freshness-sensitive graph output.
## God Nodes (most connected - your core abstractions)

## Surprising Connections (you probably didn't know these)
- None detected - all connections are within the same source files.

## Communities

## Knowledge Gaps
- **696 isolated node(s):** `Extract lesson from forensic report.          Returns:         {`, `Capture lesson to Firebase using existing script.`, `Update autonomous_refactor_session.json with failure.`, `Read current max Lamport clock from event log.`, `Append a Lamport-clocked event to the wave 7 event log.` (+691 more)
  These have ≤1 connection - possible missing edges or undocumented components.