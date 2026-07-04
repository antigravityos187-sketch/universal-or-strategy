# Node Description Batch 7 of 61

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

- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@edecfb989fb30394ab68768978dd3ebd4ea44df8": "edecfb9 docs(epic-ccn-14): complete documentation and roadmap update" | kind=Commit | source=git | neighbors=[0c1a6ca feat(epic-ccn-14-t03): extract …, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@ee1247f64e83d8b660184aaaf059f8a63d58a737": "ee1247f feat(epic-ccn-19-20): Parallel execution test - FFMA + CancelAll helpers" | kind=Commit | source=git | neighbors=[4d04458 docs: EPIC-CCN-16/17/18 documen…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@ee3e8073920c714faa5d3dac9fee0fdeebc1c17f": "ee3e807 refactor(EPIC-CCN-17-T2): Extract AdoptSingleOrder (CYC 17->14)" | kind=Commit | source=git | neighbors=[1adc2a6 test: Add 6 TDD tests for Adopt…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@f0473055cfcba632e4a81c52654776fbe5b0650d": "f047305 docs(workflow): Add workflow repair and testing documentation" | kind=Commit | source=git | neighbors=[8e961e1 docs(epics): Add EPIC-CCN-1 and…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@f2eb3cb19d2167691910496055556d55e4eb4227": "f2eb3cb [EPIC-CCN-13] ticket-05: extract HandleRealtime + AttachUiComponents --…" | kind=Commit | source=git | neighbors=[a1af247 [EPIC-CCN-13] ticket-04: extrac…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@f4bd3961df9fec68f9bdc9608d64aa744a251573": "f4bd396 test: Add 11 TDD tests for boolean helpers extraction (EPIC-CCN-18 T1)" | kind=Commit | source=git | neighbors=[2751004 refactor(EPIC-CCN-17-T3): Simpl…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@f601797804ec1a764c4baad45b25cc7c41a2ea46": "f601797 docs(agents): Add AGENTS.md to all directories for agent context" | kind=Commit | source=git | neighbors=[662aedb feat(infra): Add epic failure h…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@f756ef97a21ced44d79b4c9e0fe317874c0e9199": "f756ef9 fix: Change HydrateFromOpenPositions signature to ConcurrentDictionary …" | kind=Commit | source=git | neighbors=[c450dbd feat(epic-ccn-16-t5): extract H…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@f7b39e5296099ec9379678d43f66fe3ad627454d": "f7b39e5 Add pending notes" | kind=Commit | source=git | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@f8724be848ea7103623688014c393afe2b72b12a": "f8724be docs: update plugins and tooling documentation" | kind=Commit | source=git | neighbors=[06c181c docs(migration): add mandatory …, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@fa260284d1430cb489976c41747c66c401f9e3e8": "fa26028 EPIC-CCN-15 [T4]: Extract target handler (CYC 31->21)" | kind=Commit | source=git | neighbors=[d7fdfe1 EPIC-CCN-15 [T3]: Extract stop …, main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing]
- "eval_viewer_generate_review": "generate_review.py" | kind=code-symbol | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, build_run(), embed_file(), find_runs(), _find_runs_recursive()]
- "hooks_after_task": "after_task.py" | kind=code-symbol | source=.bob/hooks/after_task.py:L1 | neighbors=[09e0307 feat(infra): implement minimal …, 278fbf7 docs(gitbutler): restore workfl…, 39c9fb8 docs(gitbutler): restore workfl…, 5535f79 feat(infra): implement minimal …, 6040402 GitButler Workspace Commit, cc770c9 GitButler Workspace Commit]
- "hooks_after_task_complete": "after_task_complete.py" | kind=code-symbol | source=.bob/hooks/after_task_complete.py:L1 | neighbors=[2cc64b7 chore(workspace): merge main to…, 3cc6748 feat(protocol): GitButler integ…, 46e163d chore(workspace): merge main to…, 7a0625a Merge origin/main into workspac…, 80dba42 [INFRA] Tier 6 consolidation - …, a3ae570 GitButler Workspace Commit]
- "scripts_agent_bootstrap_agentbootstraploader": "AgentBootstrapLoader" | kind=code-symbol | source=scripts/agent_bootstrap.py:L43 | neighbors=[agent_bootstrap.py, ._extract_component_name(), ._extract_relevant_nodes(), ._generate_summary(), .__init__(), .load_all()]
- "scripts_epic_manifest_validationerror": "ValidationError" | kind=code-symbol | source=scripts/epic_manifest.py:L134 | neighbors=[epic_manifest.py, add_ticket_phases(), generate_manifest(), load_manifest(), Raised when manifest validation fails, update_manifest()]
- "scripts_orchestrate_full_epic_execution_epicwaveorchestrator": "EpicWaveOrchestrator" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L108 | neighbors=[orchestrate_full_epic_execution.py, .execute_all_phases(), .execute_phase_all_waves(), ._execute_phase_for_epic(), .execute_wave(), ._get_pending_epics()]
- "scripts_orchestrate_phase_execution_phaseorchestrator": "PhaseOrchestrator" | kind=code-symbol | source=scripts/orchestrate_phase_execution.py:L29 | neighbors=[orchestrate_phase_execution.py, main(), .execute_phase(), .execute_wave(), .generate_execution_plan(), .get_epic()]
- "scripts_test_v12_52": "test_v12_52.py" | kind=code-symbol | source=scripts/test_v12_52.py:L1 | neighbors=[3a92e1e feat: Wave 6 Phase 0 preparatio…, ceae42c feat: Wave 6 Phase 0 preparatio…, cleanup_test_data(), run_all_tests(), test_dependency_checking(), test_deterministic_workflow()]
- "scripts_wave_coordinator_wavecoordinator": "WaveCoordinator" | kind=code-symbol | source=scripts/wave_coordinator.py:L17 | neighbors=[wave_coordinator.py, main(), Coordinates wave-based epic execution t…, .execute_wave(), .generate_execution_plan(), ._generate_instructions()]
- "scripts_worker_agent_mcp_fastmcp": "worker_agent_mcp_fastmcp.py" | kind=code-symbol | source=scripts/worker_agent_mcp_fastmcp.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, claim_epic(), execute_epic()]
- "wave2_phase4_with_checkpoints_v2": "phase4_with_checkpoints_v2.py" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v2.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, build_phase4_script(), check_phase_status_with_healing()]
- "wave2_track_api_balances": "track_api_balances.py" | kind=code-symbol | source=scripts/wave2/track_api_balances.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, calculate_balances(), check_thresholds()]
- "card_board_main_b": "b()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, a(), e(), f(), r(), t()]
- "card_board_main_n": "N()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, g(), ir(), a(), s(), t()]
- "card_board_main_v": "v()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, a(), Bn(), c(), d(), e()]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@0adc411485ccd91f228cc190f52ea153311bd1db": "0adc411 docs: Wave 4 protocol hardening and special case detection" | kind=Commit | source=git | neighbors=[mcp.json, gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@29c4de041ebf1d61fd7890abf695a379abe4ba6d": "29c4de0 Merge branch 'feature/src-epic-ccn-51-reaper-restore' of https://github…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@5389bdedd0b4cf9cd631a82deef4293a454d0074": "5389bde [DOCS] Branch consolidation complete - final summary and deletion check…" | kind=Commit | source=git | neighbors=[wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2, wave7/s3-ui-photon-io]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@86a17596de6ba2c041a0601c45c6532ac376ee34": "86a1759 [DOCS] Extract Jane Street deviations + PR forensics from docs/jane-str…" | kind=Commit | source=git | neighbors=[80dba42 [INFRA] Tier 6 consolidation - …, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@a7edc51270620d84cf86cacfd776dc78b6c9a3c5": "a7edc51 Merge branch 'infra/epic-posinfo-phase1.5-docs'" | kind=Commit | source=git | neighbors=[86a1759 [DOCS] Extract Jane Street devi…, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@abc28fd2b532324262c7357afb07a5f6bb7c5343": "abc28fd [DOCS] GitHub branch cleanup strategy and deletion script" | kind=Commit | source=git | neighbors=[5389bde [DOCS] Branch consolidation com…, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@b727c6c2bfe03546824888ce3a010e8ab4406f64": "b727c6c [DOCS] PR #5 post-merge cleanup - add new docs, remove stale PR analysi…" | kind=Commit | source=git | neighbors=[main, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@d4ef3db1b6fd48227f869adab41c2a8f07a54fdc": "d4ef3db [DOCS] GitButler virtual branch workflow + merge conflict resolution st…" | kind=Commit | source=git | neighbors=[abc28fd [DOCS] GitHub branch cleanup st…, wave7/brain-docs-epics, wave7/orchestration-metadata, wave7/s1-sima-core, wave7/s2-trailing, wave7/s2-trailing-v2]
- "hooks_before_new_task": "before_new_task.py" | kind=code-symbol | source=.bob/hooks/before_new_task.py:L1 | neighbors=[2cc64b7 chore(workspace): merge main to…, 3cc6748 feat(protocol): GitButler integ…, 46e163d chore(workspace): merge main to…, 7a0625a Merge origin/main into workspac…, 80dba42 [INFRA] Tier 6 consolidation - …, a3ae570 GitButler Workspace Commit]
- "scripts_complexity_audit": "complexity_audit.py" | kind=code-symbol | source=scripts/complexity_audit.py:L1 | neighbors=[2cc64b7 chore(workspace): merge main to…, 46e163d chore(workspace): merge main to…, 7a0625a Merge origin/main into workspac…, 7f780c5 [MERGE] Epic CCN-14: PropagateM…, b12f440 [MERGE] Epic CCN-14: PropagateM…, ffe73a8 Merge branch 'build/1105-monoli…]
- "scripts_continue_session": "continue_session.py" | kind=code-symbol | source=scripts/continue_session.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, complete_task(), ensure_state_dir(), get_git_info(), get_minimal_context()]
- "scripts_epic_manifest_load_manifest": "load_manifest()" | kind=code-symbol | source=scripts/epic_manifest.py:L245 | neighbors=[epic_manifest.py, get_next_phases(), DependencyError, _detect_circular_dependencies(), _get_manifest_path(), _validate_phase_id()]
- "scripts_preflight_validation": "preflight_validation.py" | kind=code-symbol | source=scripts/preflight_validation.py:L1 | neighbors=[0adc411 docs: Wave 4 protocol hardening…, 7c00b3d docs: Wave 4 protocol hardening…, detect_already_complete(), detect_encoding_issues(), detect_invalid_target(), detect_test_requirements()]
- "scripts_test_phase_mcp_integration_integrationtester": "IntegrationTester" | kind=code-symbol | source=scripts/test_phase_mcp_integration.py:L29 | neighbors=[test_phase_mcp_integration.py, .create_test_epic(), .generate_summary(), .__init__(), .log(), .test_dependency_validation()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-006.json

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
