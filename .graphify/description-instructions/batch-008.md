# Node Description Batch 9 of 61

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

- "wave7_generate_phase0_scripts_fixed": "generate_phase0_scripts_fixed.py" | kind=code-symbol | source=scripts/wave7/generate_phase0_scripts_fixed.py:L1 | neighbors=[57d3230 fix(wave7): Add heredoc-free sc…, 65d3580 fix(wave7): Update generator to…, cbfa83c fix(wave7): Add heredoc-free sc…, ce4db7a fix(wave7): Update generator to…, edecfdb fix: Update generator to use 16…, fa998a7 fix: Update generator to use 16…]
- "card_board_main_an": "An()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, c(), Mn(), t(), cn(), Fn()]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@037ef9ce110859e473164dbf8087b1910f405193": "037ef9c Merge branch 'feature/src-epic-ccn-51-reaper-restore' of https://github…" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@0840dd77d2cfb2ea9c74c3b84cca841eba7f7354": "0840dd7 fix: Add Verify.Xunit package and resolve version conflicts" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@09e03071d6ceea0236d734c13f33de30902a37f6": "09e0307 feat(infra): implement minimal GitButler after_task hook for workspace …" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@142f19253c4631942536e97b7223698776ade092": "142f192 feat: Add parallel epic execution infrastructure" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@39c9fb81c83baa6861d5b6b51fda94458fdab0b4": "39c9fb8 docs(gitbutler): restore workflow documentation and hooks" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@3aab54fc47e8c002df77a95bc02cee6f4787a42f": "3aab54f Merge main into gitbutler/workspace - resolve conflicts" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@3bfeacdf7ca5e30f391a56011d2dc7870d112dbd": "3bfeacd fix: Add missing V12.52 top-level manifest fields (description, status,…" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@3d4c01211db810bd44b71bd4e826e83965910a71": "3d4c012 Merge branch 'dependabot/nuget/all-88f86858a1' into gitbutler/workspace" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@44582d8ee19ab8a0c54e6da9d7b175a4be718c9f": "44582d8 feat: V12 Epic Workflow Refactoring - Phase 1 Foundation" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@4458f9422af330c846b7f7a0ef87b11d92966fc2": "4458f94 docs: Wave 4 PR cluster analysis - 7 PRs, 7,712 lines, well-balanced" | kind=Commit | source=git | neighbors=[362fdb6 docs: add PR review cluster str…, gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@44b66eb25f9616ccc1ed69e3e794e1305d000633": "44b66eb feat(epic-ccn-16): phase 0 hotspot analysis complete" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@4ccbce81076cf5eabceca56db630363316ddfa20": "4ccbce8 fix: Remove broken V12_002.csproj reference from test project" | kind=Commit | source=git | neighbors=[14ea48b docs: Firebase key exposure inc…, gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@568177bdd33e77ca4a1fc45fbf5fc6a52d5606a6": "568177b Merge branch 'main' of https://github.com/antigravityos187-sketch/unive…" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@6c6231adcbbe5ddcb6036cad3e08e892be842b5a": "6c6231a Merge branch 'feature/src-fix-compilation-errors' into gitbutler/worksp…" | kind=Commit | source=git | neighbors=[16a25a6 [SRC] Fix 42 compilation errors…, gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@7bb0ac2a83888c5be8220f9db6d3c45fab5aafef": "7bb0ac2 docs: Add Git Hooks Consolidation documentation (Phases 1-3)" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@96e2f6db0be1a73909917d1106b791c60c190460": "96e2f6d feat(locking): Implement atomic epic locking to prevent duplicate work" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@ae6e384404d08318e7bf251ed7662f009c1096a8": "ae6e384 Merge branch 'main' into gitbutler/workspace" | kind=Commit | source=git | neighbors=[299811d docs(protocol): Filtered consol…, gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@b2cd31811d305eee22e869b636b3d6871f6190a4": "b2cd318 Merge branch 'feature/infra-fix-compilation-errors' into gitbutler/work…" | kind=Commit | source=git | neighbors=[3f42ed0 Merge branch 'feature/infra-pr1…, 52487a6 [INFRA] Fix 42 pre-existing com…, gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@b60fbdb7802cd63476670dc2a8265a43ead6d3a5": "b60fbdb [MERGE] PR #7: REAPER infrastructure restoration + Epic CCN-51 planning" | kind=Commit | source=git | neighbors=[037ef9c Merge branch 'feature/src-epic-…, 64d6605 [DOCS] GitButler virtual branch…, gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@c97f15b1e53b29b2bfa6261ef3f4616f1826b56d": "c97f15b feat(epic-ccn-16): phase 0 hotspot analysis complete" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@d125894c0eff3b75bc2fcf1ddc4bb5500fe6c872": "d125894 feat: V12 Epic Workflow Refactoring - Phase 1 Foundation" | kind=Commit | source=git | neighbors=[06fce0a EPIC-CCN-15 [T4]: Extract targe…, gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf]
- "scripts_capture_lesson": "capture_lesson.py" | kind=code-symbol | source=scripts/capture_lesson.py:L1 | neighbors=[283eb34 Merge documentation: EPIC-CCN-1…, 662aedb feat(infra): Add epic failure h…, 8fd8b93 Merge documentation: EPIC-CCN-1…, bd33127 feat(infra): Add epic failure h…, c6f6b0f feat(infra): Add epic failure h…, db71df4 feat(infra): Add epic failure h…]
- "scripts_epic_manifest_validate_dependencies": "validate_dependencies()" | kind=code-symbol | source=scripts/epic_manifest.py:L473 | neighbors=[epic_manifest.py, get_next_phases(), Check if all dependencies for a phase a…, DependencyError, _detect_circular_dependencies(), load_manifest()]
- "scripts_generate_epic_roadmap": "generate_epic_roadmap.py" | kind=code-symbol | source=scripts/generate_epic_roadmap.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 142f192 feat: Add parallel epic executi…, be6c8a1 docs: Wave 4 documentation merg…, d6f7a6f feat: Add parallel epic executi…, load_existing_roadmap(), main()]
- "scripts_jcodemunch_hook_jcodemunchhook": "JCodemunchHook" | kind=code-symbol | source=scripts/jcodemunch_hook.py:L42 | neighbors=[jcodemunch_hook.py, ._call_mcp_tool(), .index_file(), .index_folder(), .__init__(), .register_edit()]
- "scripts_linear_sync_v2_linearsync": "LinearSync" | kind=code-symbol | source=scripts/linear_sync_v2.py:L45 | neighbors=[linear_sync_v2.py, .find_project_by_name(), .get_or_create_project(), .__init__(), .parse_roadmap(), .sync_to_linear()]
- "scripts_orchestrate_full_epic_execution": "orchestrate_full_epic_execution.py" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, BobCoinBudgetManager, EpicWaveOrchestrator]
- "scripts_phase_4_5_ticket_review_mcp": "phase_4_5_ticket_review_mcp.py" | kind=code-symbol | source=scripts/phase_4_5_ticket_review_mcp.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 3bcfaac feat: Institutionalize building…, a24ed35 feat: Institutionalize building…, be6c8a1 docs: Wave 4 documentation merg…, execute_phase_4_5(), init_firestore()]
- "scripts_run_loop": "run_loop.py" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/run_loop.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, generate_report.py, improve_description.py, run_eval.py, main()]
- "scripts_session_snapshot_main": "main()" | kind=code-symbol | source=scripts/session_snapshot.py:L212 | neighbors=[session_snapshot.py, SessionSnapshot, .check_read(), .get_state(), .record_negative_evidence(), .record_read()]
- "scripts_session_snapshot_sessionsnapshot_load": ".load()" | kind=code-symbol | source=scripts/session_snapshot.py:L65 | neighbors=[Load existing session data., SessionSnapshot, .check_read(), .get_state(), .record_negative_evidence(), .record_read()]
- "scripts_validate_epic_main": "main()" | kind=code-symbol | source=scripts/validate_epic.py:L136 | neighbors=[validate_epic.py, claim_epic(), get_epic_details(), get_next_epic(), list_assigned_epics(), list_pending_epics()]
- "scripts_validate_phase_compliance_phasevalidator": "PhaseValidator" | kind=code-symbol | source=scripts/validate_phase_compliance.py:L87 | neighbors=[validate_phase_compliance.py, ._check_custom_mode_mentioned(), ._check_lamport_event(), ._check_manifest_updated(), ._check_mcp_usage(), ._check_output_files()]
- "scripts_verify_wave7_templates": "verify_wave7_templates.py" | kind=code-symbol | source=scripts/verify_wave7_templates.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, check_epic_naming(), check_file_exists(), check_temp_file_pattern(), fix_epic_naming()]
- "scripts_wave2_direct_executor": "wave2_direct_executor.py" | kind=code-symbol | source=scripts/wave2_direct_executor.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, create_bob_prompt_for_phase_1(), create_phase_0_artifacts()]
- "wave2_generate_phase5_scripts": "generate_phase5_scripts.py" | kind=code-symbol | source=scripts/wave2/generate_phase5_scripts.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, copy_and_modify_phase4_to_phase5_ticket…, copy_and_modify_phase4_to_phase5_valida…]
- "wave4_audit_and_remove_pr_references": "audit_and_remove_pr_references.py" | kind=code-symbol | source=scripts/wave4/audit_and_remove_pr_references.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…, audit_directory(), find_pr_references()]
- "wave4_execute_80_80_recovery": "execute_80_80_recovery.py" | kind=code-symbol | source=scripts/wave4/execute_80_80_recovery.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…, main(), monitor_recovery()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-008.json

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
