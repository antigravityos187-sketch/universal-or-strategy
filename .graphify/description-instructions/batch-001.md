# Node Description Batch 2 of 61

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

- "card_board_main_c": "c()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, An(), f(), r(), cn(), Co()]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@ceae42c25b25e5482e923e6b2560c5517ce64cf6": "ceae42c feat: Wave 6 Phase 0 preparation complete - V12.52 Lamport causal verif…" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@2cc64b76276d117042ae7fdfc61761c5d692a8c0": "2cc64b7 chore(workspace): merge main to sync workspace (resolved conflicts)" | kind=Commit | source=git | neighbors=[0b3d7dc feat(protocol): register GitBut…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@662aedbbdceba9b7b23c4e2d882a94d6f38456f0": "662aedb feat(infra): Add epic failure hook and utility scripts" | kind=Commit | source=git | neighbors=[080c7b7 docs(epic-ccn-13): Add complete…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@8fd8b93a104c06bcebade170f3159eed3828e140": "8fd8b93 Merge documentation: EPIC-CCN-16/17/18 + parallel workflow setup" | kind=Commit | source=git | neighbors=[5efc4e4 docs: update plugins and toolin…, gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@db71df4ad935dca4f4cd4d7861254ed5e9ce7812": "db71df4 feat(infra): Add epic failure hook and utility scripts" | kind=Commit | source=git | neighbors=[5a6cb96 docs(epic-ccn-13): Add complete…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "card_board_main_e": "e()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, b(), d(), i(), r(), er()]
- "card_board_main_s": "s()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, gn(), i(), ir(), N(), nr()]
- "card_board_main_u": "u()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, b(), d(), h(), jr(), l()]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@0029dd50634a285fa40ca69aa7045b9c1aefa89e": "0029dd5 V12.53: ALL 10 phases now use custom modes for deterministic execution" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "scripts_lamport_clock_deterministicworkflow": "DeterministicWorkflow" | kind=code-symbol | source=scripts/lamport_clock.py:L24 | neighbors=[lamport_clock.py, ._append_event(), .check_dependencies(), ._compute_state_hash(), .get_event_log(), .get_next_phases()]
- "scripts_validate_epic": "validate_epic.py" | kind=code-symbol | source=scripts/validate_epic.py:L1 | neighbors=[06af2f1 feat: 4-worker parallel epic ex…, 1392356 docs: Wave 4 documentation merg…, 48e0777 feat: 4-worker parallel epic ex…, 61b8a9c feat(locking): Implement atomic…, 96e2f6d feat(locking): Implement atomic…, be6c8a1 docs: Wave 4 documentation merg…]
- "v12_performance_tests_v12_performance_tests": "V12_Performance.Tests.csproj" | kind=code-symbol | source=tests/V12_Performance.Tests/V12_Performance.Tests.csproj:L1 | neighbors=[V12_Performance.Benchmarks.csproj, 0840dd7 fix: Add Verify.Xunit package a…, 1392356 docs: Wave 4 documentation merg…, 19a2a6f fix: Remove broken V12_002.cspr…, 4ccbce8 fix: Remove broken V12_002.cspr…, 6a013c6 fix: Add Verify.Xunit package a…]
- "card_board_main_h": "h()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, a(), e(), f(), i(), r()]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@7c00b3d0ae4868e6d69ac31f241b7daf66d5a5ee": "7c00b3d docs: Wave 4 protocol hardening and special case detection" | kind=Commit | source=git | neighbors=[mcp.json, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@b0a803b6c110b375319dc2e87e3ecfbae6ac1776": "b0a803b feat(wave7): Phase 2 Architecture Planning COMPLETE — 161/161 epics pas…" | kind=Commit | source=git | neighbors=[66d490b fix(wave7): OKF integration com…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "scripts_jane_street_utils": "jane_street_utils.py" | kind=code-symbol | source=scripts/jane_street_utils.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…, format_violation_report(), get_files_with_violations()]
- "scripts_wave2_parallel_executor": "wave2_parallel_executor.py" | kind=code-symbol | source=scripts/wave2_parallel_executor.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, execute_bob_for_epic(), execute_phase_parallel()]
- "scripts_worker_agent_mcp": "worker_agent_mcp.py" | kind=code-symbol | source=scripts/worker_agent_mcp.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, call_tool(), claim_epic_tool()]
- "testing": "Testing.csproj" | kind=code-symbol | source=Testing.csproj:L1 | neighbors=[2cc64b7 chore(workspace): merge main to…, 3d4c012 Merge branch 'dependabot/nuget/…, 46e163d chore(workspace): merge main to…, 7a0625a Merge origin/main into workspac…, 80dba42 [INFRA] Tier 6 consolidation - …, df27b2e [INFRA] Tier 6 consolidation - …]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@0b3d7dc2c64b5d3269aa55fe9aa87f871a4b40a5": "0b3d7dc feat(protocol): register GitButler after_task hook" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@46e163dcebc5851282a72b74627c1271857acde2": "46e163d chore(workspace): merge main to sync workspace (resolved conflicts)" | kind=Commit | source=git | neighbors=[09e0307 feat(infra): implement minimal …, gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@48e0777748dc6d92e5356650a74316b86dcf05e4": "48e0777 feat: 4-worker parallel epic execution infrastructure" | kind=Commit | source=git | neighbors=[mcp.json, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@4d0445882ab9da962f7e181e9548be06c3deccb5": "4d04458 docs: EPIC-CCN-16/17/18 documentation + parallel workflow setup" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@6040402010d2e2060b3daad744acb52787e2fa36": "6040402 GitButler Workspace Commit" | kind=Commit | source=git | neighbors=[0b3d7dc feat(protocol): register GitBut…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@75e2ef2833e7020818dba5d79d5285966b10e574": "75e2ef2 docs: EPIC-CCN-16/17/18 documentation + parallel workflow setup" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@b12f44058130aa7fb7a2766aeef85a702efeebc3": "b12f440 [MERGE] Epic CCN-14: PropagateMaster refactoring (CYC 18->4) - conflict…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@bd33127812b32fe46fa6bf07a3ce9c09ef611646": "bd33127 feat(infra): Add epic failure hook and utility scripts" | kind=Commit | source=git | neighbors=[3294e24 docs(epic-ccn-13): Add complete…, gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@c6f6b0f0baec258d88113d24b48b98b4859388a7": "c6f6b0f feat(infra): Add epic failure hook and utility scripts" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@dad307457878f678134abfdf360ea43954be2486": "dad3074 feat: Wave 5 preparation - protocol hardening complete" | kind=Commit | source=git | neighbors=[7ef25a3 rollback: Wave 4 Phase 5-6 (78 …, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "card_board_main_m": "m()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, a(), e(), f(), h(), i()]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@19a2a6fe7413b31737e959d021c7a327864e08db": "19a2a6f fix: Remove broken V12_002.csproj reference from test project" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@278fbf74f0130596dbe5534234bcab48c2b0f643": "278fbf7 docs(gitbutler): restore workflow documentation and hooks" | kind=Commit | source=git | neighbors=[0b3d7dc feat(protocol): register GitBut…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@2d19d68d6fd9bd070992407ed0e542cac21531a9": "2d19d68 feat(epic-ccn-16): phase 0 hotspot analysis complete" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@3c2723da3e91ea40e05e5ad840317616b7556d11": "3c2723d docs: Wave 4 PR cluster analysis - 7 PRs, 7,712 lines, well-balanced" | kind=Commit | source=git | neighbors=[2d9be1d docs: add PR review cluster str…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@49a791fba9a7137ad35ca2039641047306465ffc": "49a791f Merge main into gitbutler/workspace - resolve conflicts" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@5535f79b5a88f872aa23c864b47c400c3f8db262": "5535f79 feat(infra): implement minimal GitButler after_task hook for workspace …" | kind=Commit | source=git | neighbors=[3ee121b GitButler Workspace Commit, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@5925e89f4f29847e4f9854392f17d2c1a91ee756": "5925e89 Fix API key list for VM (replace missing b (2).json with iyanajackson.j…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@6159ec17bc7cef5936f9584e0fcbc57bc5f84571": "6159ec1 GitButler Workspace Commit" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@61751fbe1726f113509b09794c234721b6fc2e24": "61751fb feat: V12 Epic Workflow Refactoring - Phase 1 Foundation" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-001.json

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
