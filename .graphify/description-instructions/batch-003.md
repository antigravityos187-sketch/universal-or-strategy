# Node Description Batch 4 of 61

Graphify is running in assistant/skill mode (no API key). You are the host
assistant (Claude Code / Codex / Gemini CLI). Read the prompt below and write
your JSON answer to the answer file.

## Prompt

You are documenting nodes in a knowledge graph.
For each entry below, write ONE concise factual plain-language sentence
describing what it is or does. Use only the provided context.
For an entity node (any other kind — e.g. a person, place, event, object),
describe what the entity is and its role, grounded in its type, its
relations (neighbors) and the provided citations/evidence — e.g.
"Lady Carfax, a wealthy heiress who disappears en route to Lausanne.".
Ground entity descriptions in the citations/evidence when present; do not
speculate beyond the context, so a node with no supporting context may be
left out of the reply.
No marketing language.
Respond ONLY with a JSON object mapping each node id (as a string) to its
one-sentence description — no prose, no markdown fences.

- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@168dfd052315e99f92630c65c64820785f4f767b": "168dfd0 test: Add 4 TDD tests for AdoptFleetOrders refactoring (EPIC-CCN-17 T3)" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@17622acd7c4fed6beb957eb704006572dc8cd82c": "17622ac security: Fix firebase-key.json gitignore and add template" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@17bd4ccbb168df747af3a44457e3f0915cac634e": "17bd4cc feat: Add launch script for remaining 144 epics with 16 fresh API keys" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@17cce2775b1a1d78572f454d361d4183fb6ae238": "17cce27 EPIC-CCN-15 [T2]: Extract dedup guard (CYC 45->43)" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@1a69df5896a29eb25638948b4b67fddb34df2c4d": "1a69df5 protocol: Comprehensive hardening post-Wave 4 rollback (V12.38)" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@1adc2a6383409081b7ee405760524701bf1b480a": "1adc2a6 test: Add 6 TDD tests for AdoptSingleOrder extraction (EPIC-CCN-17 T2)" | kind=Commit | source=git | neighbors=[097eff6 EPIC-CCN-17 Ticket 1: Extract R…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@22e4b75d60fb2cbe76f51ec8bd3300a3fa720323": "22e4b75 fix(mcp): Rename jcodemunch to jcodemunch-mcp and add greptile server" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@232a061dccdd540a7f690a57b01658f73babea7f": "232a061 feat(epic-ccn-16-t1): extract MapOrderStateToFSMState pure function" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@24d630d9dee83667e632819f606d3a4bc447530b": "24d630d feat(epic-ccn-14-t01): extract ValidateCommandFormat (CYC 76->70)" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@253305dc72cf540a5d356cf579091a16b700cc9d": "253305d Restore .cs changes from autonomous wave execution" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@25b90fcf7b75e34f2cbefa4324981723b478f0d3": "25b90fc test: Add 8 TDD tests for cancellation helper extraction (EPIC-CCN-18 T…" | kind=Commit | source=git | neighbors=[166ed53 refactor(EPIC-CCN-18-T1): Extra…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@27510048988391edcc183c19f26e9335e69b06b7": "2751004 refactor(EPIC-CCN-17-T3): Simplify AdoptFleetOrders main method (CYC 14…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@2af27ceba858c8312eb31ed30edc8bddb36deb65": "2af27ce feat: Add Mise integration and VM continuation docs" | kind=Commit | source=git | neighbors=[1440258 docs: Wave 2 session artifacts …, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@2d9be1dbea5a3522d309a28fe6e8b093448e537d": "2d9be1d docs: add PR review cluster strategy for Wave 4" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@36124c48fdff31f7c18cb969a3be14bbb9bb53c0": "36124c4 fix: Remove Wave 4 completion file from EPIC-CCN-001" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@37929eb1e345ec2248b9bfbf6e8dcad77a07b276": "37929eb feat(epic-ccn-16-t1): extract MapOrderStateToFSMState pure function" | kind=Commit | source=git | neighbors=[2d19d68 feat(epic-ccn-16): phase 0 hots…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@395a19b30e99166e9331ae500fb59f7bb0e3ce21": "395a19b test: Add 11 TDD tests for boolean helpers extraction (EPIC-CCN-18 T1)" | kind=Commit | source=git | neighbors=[0ded90c refactor(EPIC-CCN-17-T3): Simpl…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@3c1ecd517b1bf6b634774afc428f085604ccf360": "3c1ecd5 docs: clarify branch strategy - tests are Tier 1" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@3c5ba87fcb60e1101805ee7dc85bd1b3f36ecf9a": "3c5ba87 EPIC-CCN-17 Ticket 1: Extract RouteOrderToTargetDict (CYC 37->17, extra…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@3de15a1a0b006b18ffd10e71f7863df7abf6293f": "3de15a1 docs(wave7): Add Screen Session Script Protocol and syntax validation" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@3ea0fd8919cd1ce3bc81c7213a97d77c343ff85f": "3ea0fd8 docs(protocol): Add branch strategy enforcement protocol (V12.24)" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@3ee121b763372fce8f98eddf1bd41eb091efea2f": "3ee121b GitButler Workspace Commit" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@3f879999c57576998a6e2c33ccaf7d39d120e702": "3f87999 docs: Wave 4 Phase 6 completion - remaining 10 epics (79/80 total)" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@40719d4d6184520bd3da57cb97e31e21bf631328": "40719d4 [EPIC-CCN-13] ticket-03: extract HandleConfigure + InitializeMmioMirror…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@408efb10bf9bb444c4e5996da4156acb982e096a": "408efb1 [SRC] Restore REAPER infrastructure declarations - fix 42 compilation e…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@40f2187b45bd62a064e7334d63366561e3183d43": "40f2187 docs(epic-ccn-14): complete documentation and roadmap update" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@4756c0fb22d2602a294618c6359053aca27b6017": "4756c0f test: Add 8 TDD tests for cancellation helper extraction (EPIC-CCN-18 T…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@4858b3e5aa0cf4ebb3945c2a1d431b8b64cfae7e": "4858b3e protocol: Add VM Setup Protocol + Skill Reading Mandate (V12.39)" | kind=Commit | source=git | neighbors=[041b48f rollback: Wave 4 Phase 5-6 (EPI…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@48ce023f982221baec1adc83c8a0f681b57c6206": "48ce023 docs(workflow): Add workflow repair and testing documentation" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@4a93ff3de855a793a9e51c86e3905a366212367d": "4a93ff3 Remove PAT file from tracking - security fix" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@4b7ebe87243df25eda4ec4c87583edd729653506": "4b7ebe8 docs: Wave 4 rollback continuation prompt" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@4fa58f6fe421a3b114141a6d36492c45966ce28d": "4fa58f6 docs: Wave 4 PR execution plan and pending notes update" | kind=Commit | source=git | neighbors=[3c2723d docs: Wave 4 PR cluster analysi…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@52c8cd1032cd1782a4d5d37c2bbbad3755764910": "52c8cd1 [EPIC-CCN-13] ticket-05: extract HandleRealtime + AttachUiComponents --…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@559e6315790f909d1f051ed9175b44ec472d3c9b": "559e631 fix: Change HydrateFromOpenPositions signature to ConcurrentDictionary …" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@575925b6002d77b33e0765fa0ba2972ad853b400": "575925b refactor(EPIC-CCN-18-T1): Extract boolean helpers (CYC 37->≤8)" | kind=Commit | source=git | neighbors=[395a19b test: Add 11 TDD tests for bool…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@5a6cb961618f35837cf2cf0a7620deb9ee49b292": "5a6cb96 docs(epic-ccn-13): Add complete epic documentation and recovery report" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@5e1eb99cae9f61a7aa4ec8b3a052ac15e7f5005c": "5e1eb99 feat: Lamport clock as causality enforcement gate — HALT if dependency …" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@666faa1f8115ca9becd4f4472d3424b46d3a62f3": "666faa1 Wave 7: Preserve 16 completed epics from VM (Phase 0)" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@6847aa50f816ca2bca4c1111872097f5c85ce819": "6847aa5 feat(epic-ccn-16-t4): Extract RegisterFSM helper (BUILD 1111.036)" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@68524930e4c3980237515032ef0c03355e0e8783": "6852493 feat: Phase 3 DNA Audit VERIFIED_COMPLETE 161/161" | kind=Commit | source=git | neighbors=[129997e fix: remove all PR references f…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-003.json

Keep each description factual and concise (one sentence). No markdown, no prose
outside the JSON object. It is acceptable to omit a node if context is
insufficient — but include every node you can ground confidently.

Example answer format:
```json
{
  "node_id_1": "Resolves the configured ontology profile from graphify.yaml.",
  "node_id_2": "Colonel James Barclay, an antagonist in The Crooked Man."
}
```
