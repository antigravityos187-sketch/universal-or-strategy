# Node Description Batch 8 of 61

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

- "scripts_verify_index_freshness": "verify_index_freshness.py" | kind=code-symbol | source=scripts/verify_index_freshness.py:L1 | neighbors=[283eb34 Merge documentation: EPIC-CCN-1…, 662aedb feat(infra): Add epic failure h…, 8fd8b93 Merge documentation: EPIC-CCN-1…, bd33127 feat(infra): Add epic failure h…, c6f6b0f feat(infra): Add epic failure h…, db71df4 feat(infra): Add epic failure h…]
- "sdk_microsoft_net_sdk": "Microsoft.NET.Sdk" | kind=entity | source=xunit-tests/W7-160/W7_160.Tests.csproj | neighbors=[SpscRing.Benchmarks.csproj, V12_Performance.Benchmarks.csproj, Linting.csproj, R28_MmioSpscRing.csproj, Testing.csproj, V12_Performance.Tests.csproj]
- "wave2_launch_wave": "launch_wave.py" | kind=code-symbol | source=scripts/wave2/launch_wave.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, collect_results(), gcloud()]
- "wave2_launch_wave_v3_multi_api": "launch_wave_v3_multi_api.py" | kind=code-symbol | source=scripts/wave2/launch_wave_v3_multi_api.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, build_wave_script(), clear_stale_ssh_key()]
- "wave2_launch_wave_v4_safe_budget": "launch_wave_v4_safe_budget.py" | kind=code-symbol | source=scripts/wave2/launch_wave_v4_safe_budget.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, build_wave_script(), clear_stale_ssh_key()]
- "wave2_update_obsidian_kanban": "update_obsidian_kanban.py" | kind=code-symbol | source=scripts/wave2/update_obsidian_kanban.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, generate_kanban_markdown(), get_all_status()]
- "card_board_main_d": "d()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, e(), f(), r(), t(), u()]
- "card_board_main_jn": "jn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, er(), An(), c(), e(), En()]
- "card_board_main_p": "p()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, a(), Eo(), Fo(), ko(), g()]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@06af2f13bcd2f4835b0329d8b3e34a0408727575": "06af2f1 feat: 4-worker parallel epic execution infrastructure" | kind=Commit | source=git | neighbors=[mcp.json, gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@7f780c5a9e83826200fea047ced4957bcce8cf5c": "7f780c5 [MERGE] Epic CCN-14: PropagateMaster refactoring (CYC 18->4) - conflict…" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@a14581b5b310d6b8237dbe9be814cf6e578612ef": "a14581b feat: Wave 5 preparation - protocol hardening complete" | kind=Commit | source=git | neighbors=[1076919 rollback: Wave 4 Phase 5-6 (78 …, gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@a193d48f40a60a90002fb2ca6a46389b31d5c822": "a193d48 docs: EPIC-CCN-16/17/18 documentation + parallel workflow setup" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@ae8e70bbf4c24758c1d61f8bc5e49ec8865c1d40": "ae8e70b docs: EPIC-CCN-16/17/18 documentation + parallel workflow setup" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@bb9723e5bbb7b0d0d95f99f90704534f918cad9c": "bb9723e Remove PAT file from tracking - security fix" | kind=Commit | source=git | neighbors=[180215d Wave 7 Phase 1 100% complete - …, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@cc770c9fd30fc3b692991673cd57fc4cd7ef747e": "cc770c9 GitButler Workspace Commit" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@e4fb52acbbc57e9414756a963d18daf607415810": "e4fb52a feat(epic-ccn-51-t4): extract AdoptMasterOrders - CYC 37->19" | kind=Commit | source=git | neighbors=[cf31d1a feat(epic-ccn-51): complete Tic…, gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@f0f7a14679e865fd7cc0541140f5b9827a36aae8": "f0f7a14 feat(protocol): register GitButler after_task hook" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "complete_wave_cross_reference": "complete_wave_cross_reference.py" | kind=code-symbol | source=complete_wave_cross_reference.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, analyze_jane_street_violations(), analyze_wave6_epics(), cross_reference_jane_street(), extract_baseline_methods()]
- "hooks_after_epic_failure": "after_epic_failure.py" | kind=code-symbol | source=.bob/hooks/after_epic_failure.py:L1 | neighbors=[283eb34 Merge documentation: EPIC-CCN-1…, 662aedb feat(infra): Add epic failure h…, 8fd8b93 Merge documentation: EPIC-CCN-1…, bd33127 feat(infra): Add epic failure h…, c6f6b0f feat(infra): Add epic failure h…, db71df4 feat(infra): Add epic failure h…]
- "hooks_pre_task_jane_street_kb": "pre_task_jane_street_kb.py" | kind=code-symbol | source=.bob/hooks/pre_task_jane_street_kb.py:L1 | neighbors=[2cc64b7 chore(workspace): merge main to…, 46e163d chore(workspace): merge main to…, 66d490b fix(wave7): OKF integration com…, 7f780c5 [MERGE] Epic CCN-14: PropagateM…, b12f440 [MERGE] Epic CCN-14: PropagateM…, extract_topics()]
- "scripts_amal_harness": "amal_harness.py" | kind=code-symbol | source=scripts/amal_harness.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, cleanup_orphaned_blocks(), extract_all_literals(), extract_named_ts_exports(), get_method_body()]
- "scripts_epic_manifest_update_manifest": "update_manifest()" | kind=code-symbol | source=scripts/epic_manifest.py:L334 | neighbors=[epic_manifest.py, complete_phase_execution(), fail_phase_execution(), Update phase status and outputs in mani…, start_phase_execution(), _get_manifest_path()]
- "scripts_epic_planner": "epic_planner.py" | kind=code-symbol | source=scripts/epic_planner.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, calculate_composite_score(), generate_epic_roadmap(), get_codescene_review(), get_jcodemunch_hotspots()]
- "scripts_linear_sync_linearsync": "LinearSync" | kind=code-symbol | source=scripts/linear_sync.py:L49 | neighbors=[linear_sync.py, .create_epic(), .create_issue(), .find_project_by_name(), .__init__(), .parse_roadmap()]
- "scripts_monitor_vm_progress": "monitor_vm_progress.py" | kind=code-symbol | source=scripts/monitor_vm_progress.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, check_vm_status(), create_epic_card()]
- "scripts_negative_evidence_check_negativeevidencecache": "NegativeEvidenceCache" | kind=code-symbol | source=scripts/negative_evidence_check.py:L23 | neighbors=[negative_evidence_check.py, main(), .check(), .clear(), .__init__(), .list_all()]
- "scripts_orchestrate_full_epic_execution_bobcoinbudgetmanager": "BobCoinBudgetManager" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L31 | neighbors=[orchestrate_full_epic_execution.py, .check_balance(), .get_average_cost_per_phase(), .__init__(), .needs_refill(), .predict_wave_cost()]
- "scripts_orchestrate_full_epic_execution_epicwaveorchestrator_execute_wave": ".execute_wave()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L166 | neighbors=[EpicWaveOrchestrator, .execute_phase_all_waves(), .check_balance(), .needs_refill(), .predict_wave_cost(), .prompt_refill()]
- "scripts_orchestrate_phase0_with_prep": "orchestrate_phase0_with_prep.py" | kind=code-symbol | source=scripts/orchestrate_phase0_with_prep.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, call_phase0_mcp(), execute_phase0_with_prep()]
- "scripts_phase_1_scope_mcp": "phase_1_scope_mcp.py" | kind=code-symbol | source=scripts/phase_1_scope_mcp.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, call_tool(), create_extraction_scope()]
- "scripts_query_codescene_codesceneclient": "CodeSceneClient" | kind=code-symbol | source=scripts/query_codescene.py:L34 | neighbors=[query_codescene.py, .get_code_health(), .get_file_health(), .get_hotspots(), .get_project_id(), .get_refactoring_targets()]
- "scripts_test_phase_mcp_servers_mcpservertester": "MCPServerTester" | kind=code-symbol | source=scripts/test_phase_mcp_servers.py:L27 | neighbors=[test_phase_mcp_servers.py, main(), .generate_report(), .__init__(), ._load_config(), ._log()]
- "scripts_validate_epic_load_roadmap": "load_roadmap()" | kind=code-symbol | source=scripts/validate_epic.py:L22 | neighbors=[validate_epic.py, claim_epic(), get_epic_details(), get_next_epic(), list_assigned_epics(), list_pending_epics()]
- "wave2_api_balance_tracker": "api_balance_tracker.py" | kind=code-symbol | source=scripts/wave2/api_balance_tracker.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, check_phase_feasibility(), estimate_phase_budget()]
- "wave2_launch_wave_now": "launch_wave_now.py" | kind=code-symbol | source=scripts/wave2/launch_wave_now.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, build_wave_script(), clear_stale_ssh_key()]
- "wave2_launch_wave_v2": "launch_wave_v2.py" | kind=code-symbol | source=scripts/wave2/launch_wave_v2.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, build_wave_script(), clear_stale_ssh_key()]
- "wave2_phase4_with_checkpoints": "phase4_with_checkpoints.py" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, build_phase4_script(), check_phase_status()]
- "wave2_test_single_epic_107": "test_single_epic_107.py" | kind=code-symbol | source=scripts/wave2/test_single_epic_107.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, generate_test_script(), get_epic_data()]
- "wave2_update_wave2_kanban": "update_wave2_kanban.py" | kind=code-symbol | source=scripts/wave2/update_wave2_kanban.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, find_obsidian_vaults(), get_all_phase5_status()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-007.json

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
