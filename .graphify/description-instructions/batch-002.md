# Node Description Batch 3 of 61

Graphify is running in assistant/skill mode (no API key). You are the host
assistant (Claude Code / Codex / Gemini CLI). Read the prompt below and write
your JSON answer to the answer file.

## Prompt

You are documenting nodes in a knowledge graph.
For each entry below, write ONE concise factual plain-language sentence
describing what it is or does. Use only the provided context.
For a code symbol (kind=code-symbol — a function, class, or constant),
describe what the function/symbol does based on its name, source location
and neighbors — e.g. "Resolves the configured ontology profile from graphify.yaml.".
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

- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@61b8a9c5511836fa247633f584a9a684d2579a3a": "61b8a9c feat(locking): Implement atomic epic locking to prevent duplicate work" | kind=Commit | source=git | neighbors=[22e4b75 fix(mcp): Rename jcodemunch to …, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@65d35803f386c9152fd94875fe804574ee11d027": "65d3580 fix(wave7): Update generator to use 20 API keys with even distribution" | kind=Commit | source=git | neighbors=[100611b docs(wave7): Add Phase 0 final …, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@66d490b0f75f935652c4559220c707c37880cd60": "66d490b fix(wave7): OKF integration complete — replace Firebase refs, fix hook …" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@68cb090a6179d33afbb2db8a85dcb62730c8697b": "68cb090 feat(wave7): OKF wiki, phase orch templates, query_kb fallback, reset a…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@6a013c6cb2ebcf5787f37293e54e7a1d525baf76": "6a013c6 fix: Add Verify.Xunit package and resolve version conflicts" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@70cd659086e6fb37c33b237fe17286ae33e4d4f6": "70cd659 fix: Add missing V12.52 top-level manifest fields (description, status,…" | kind=Commit | source=git | neighbors=[3a92e1e feat: Wave 6 Phase 0 preparatio…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@792eaf525f2e30f5187558f377e5cf464e06240d": "792eaf5 docs: Add Git Hooks Consolidation documentation (Phases 1-3)" | kind=Commit | source=git | neighbors=[283eb34 Merge documentation: EPIC-CCN-1…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@be8b39933c0b74fdfa0b4fb48658ae49c3cc2c0d": "be8b399 feat(epic-ccn-51-t4): extract AdoptMasterOrders - CYC 37->19" | kind=Commit | source=git | neighbors=[807ac38 feat(epic-ccn-51): complete Tic…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@cbfa83c0d2ab69bdc18eae4e3ed59ee8a395c487": "cbfa83c fix(wave7): Add heredoc-free script generator and recovery script" | kind=Commit | source=git | neighbors=[5925e89 Fix API key list for VM (replac…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@cc7f610a555f14fb532ada92251ece381c8c3894": "cc7f610 [MERGE] PR #7: REAPER infrastructure restoration + Epic CCN-51 planning" | kind=Commit | source=git | neighbors=[29c4de0 Merge branch 'feature/src-epic-…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@d13973d37a7251abcbd7db4ba9f875528fa4ec3c": "d13973d Merge branch 'main' of https://github.com/antigravityos187-sketch/unive…" | kind=Commit | source=git | neighbors=[6159ec1 GitButler Workspace Commit, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@d1d14ade7adc71289dd0e5e6b1baf01f5fa08d17": "d1d14ad feat(epic-ccn-16): phase 0 hotspot analysis complete" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@d6f7a6fe035c12c5b9047bea5c021da4c3ff3e61": "d6f7a6f feat: Add parallel epic execution infrastructure" | kind=Commit | source=git | neighbors=[930113a docs: Update epic roadmap with …, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@edecfdb159c16ff15f8423c67375a685a0b4452c": "edecfdb fix: Update generator to use 16 fresh API keys (removed api_rotation.js…" | kind=Commit | source=git | neighbors=[02470e8 Wave 7: Remove exhausted API ke…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@fb5f81e48d1bfab6f432d527b6a8d01baca478bb": "fb5f81e Wave 7 Task 7: Master launch scripts (Building-Blocks Method)" | kind=Commit | source=git | neighbors=[82e1575 Fix Phase 3 template: Use v12-p…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@fe38b3c14057b0330c4b8dbfc355cc24eabafa67": "fe38b3c feat: V12 Epic Workflow Refactoring - Phase 1 Foundation" | kind=Commit | source=git | neighbors=[8fd5b36 EPIC-CCN-15 [T4]: Extract targe…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "scripts_lamport_clock": "lamport_clock.py" | kind=code-symbol | source=scripts/lamport_clock.py:L1 | neighbors=[3a92e1e feat: Wave 6 Phase 0 preparatio…, ceae42c feat: Wave 6 Phase 0 preparatio…, f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, epic_manifest.py, DeterministicWorkflow]
- "scripts_session_continuity_sessioncontinuity": "SessionContinuity" | kind=code-symbol | source=scripts/session_continuity.py:L26 | neighbors=[session_continuity.py, main(), Manages session checkpoints and restora…, ._auto_prune(), .auto_snapshot(), ._get_checkpoint_path()]
- "scripts_session_snapshot_sessionsnapshot": "SessionSnapshot" | kind=code-symbol | source=scripts/session_snapshot.py:L28 | neighbors=[session_snapshot.py, main(), Manages session state tracking for agen…, .check_read(), .get_state(), .__init__()]
- "wave2_phase4_with_checkpoints_v3_fixed": "phase4_with_checkpoints_v3_fixed.py" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, build_phase4_script(), check_phase_status_with_healing()]
- "card_board_main_a": "a()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, l(), m(), o(), p(), r()]
- "card_board_main_f": "f()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, b(), c(), d(), r(), rr()]
- "card_board_main_i": "i()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, e(), h(), d(), o(), r()]
- "card_board_main_qr": "qr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, Br(), fr(), Kr(), Mr(), N()]
- "card_board_main_ur": "Ur()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, nr(), qr(), rr(), b(), Br()]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@02470e83a20e839cbb5b126bac60ba9f65b660a3": "02470e8 Wave 7: Remove exhausted API keys (4 keys) and add recovery documentati…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@0388ef9eb93122e702051cd112e17209e2fc1471": "0388ef9 refactor(EPIC-CCN-18-T2): Extract cancellation helper (HandleFlatPositi…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@03e7c0121da184ac0de908957b44bbf6ea5deb49": "03e7c01 EPIC-CCN-15 [T1]: Extract entry name helper (CYC 67->45)" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@041b48f32b1cca70745f89ca523736da11486aae": "041b48f rollback: Wave 4 Phase 5-6 (EPIC-CCN-027 only)" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@06c181c44c3f9bbbfe66d60cf72d25f56c37a8ee": "06c181c docs(migration): add mandatory token cleanup to migration protocol" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@080c7b705dc9712052bbce21ee6a60d58ce11085": "080c7b7 docs(epic-ccn-13): Add complete epic documentation and recovery report" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@097eff6389f97997f97126db0f64baa4882cf5f2": "097eff6 EPIC-CCN-17 Ticket 1: Extract RouteOrderToTargetDict (CYC 37->17, extra…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@0b36fe2825101509c58a13393cdbe789fb68d33e": "0b36fe2 fix: remove UTF-8 replacement characters from SIMA.Dispatch.cs" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@0c1a6ca268267cbcf9c9c3995efe60256ef5a700": "0c1a6ca feat(epic-ccn-14-t03): extract HandleValidationFailure (CYC 62->57)" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@0ded90c4e9c0d58b65c857ce55a90940e7e6abd4": "0ded90c refactor(EPIC-CCN-17-T3): Simplify AdoptFleetOrders main method (CYC 14…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@0eab2b2615beb7b8b301947dc1e23352728b3265": "0eab2b2 docs: Wave 5 investigation and V12.40-V12.51 protocols - V12.40: MCP se…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@100611b5a4745981e2d5abf49323a0b133ae3cee": "100611b docs(wave7): Add Phase 0 final status report (138/161 complete)" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@129997e60e44bf1248cf7337911c38d058862d19": "129997e fix: remove all PR references from Phases 3-10 templates and modes — PR…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@143a42581fb983835a8dd11e268fc15b74af5ca8": "143a425 docs(protocol): Add branch strategy enforcement protocol (V12.24)" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@166ed534499b5b657c3a36b6dccd686ff8f73d6d": "166ed53 refactor(EPIC-CCN-18-T1): Extract boolean helpers (CYC 37->≤8)" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-002.json

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
