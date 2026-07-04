# Node Description Batch 20 of 61

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

- "wave2_launch_wave_launch_wave": "launch_wave()" | kind=code-symbol | source=scripts/wave2/launch_wave.py:L69 | neighbors=[launch_wave.py, gcloud(), get_bob_api_key(), main(), Launch a wave VM with orchestrator via …]
- "wave2_launch_wave_now_main": "main()" | kind=code-symbol | source=scripts/wave2/launch_wave_now.py:L161 | neighbors=[launch_wave_now.py, build_wave_script(), clear_stale_ssh_key(), gcloud(), load_epics()]
- "wave2_launch_wave_v2_main": "main()" | kind=code-symbol | source=scripts/wave2/launch_wave_v2.py:L168 | neighbors=[launch_wave_v2.py, build_wave_script(), clear_stale_ssh_key(), gcloud(), load_epics()]
- "wave2_phase4_with_checkpoints_v2_check_phase_status_with_healing": "check_phase_status_with_healing()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v2.py:L106 | neighbors=[phase4_with_checkpoints_v2.py, load_manifest(), save_manifest(), main(), Check phase status with self-healing fo…]
- "wave2_phase4_with_checkpoints_v2_update_manifest": "update_manifest()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v2.py:L94 | neighbors=[phase4_with_checkpoints_v2.py, main(), Update manifest with phase status, load_manifest(), save_manifest()]
- "wave2_phase4_with_checkpoints_v3_fixed_check_phase_status_with_healing": "check_phase_status_with_healing()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L116 | neighbors=[phase4_with_checkpoints_v3_fixed.py, load_manifest(), save_manifest(), main(), Check phase status with self-healing fo…]
- "wave2_phase4_with_checkpoints_v3_fixed_main": "main()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L262 | neighbors=[phase4_with_checkpoints_v3_fixed.py, check_phase_status_with_healing(), launch_agents_on_vm(), update_manifest(), validate_api_allocation()]
- "wave2_phase4_with_checkpoints_v3_fixed_update_manifest": "update_manifest()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L104 | neighbors=[phase4_with_checkpoints_v3_fixed.py, main(), Update manifest with phase status, load_manifest(), save_manifest()]
- "wave2_update_obsidian_kanban_get_all_status": "get_all_status()" | kind=code-symbol | source=scripts/wave2/update_obsidian_kanban.py:L82 | neighbors=[update_obsidian_kanban.py, get_epic_status(), get_ticket_status(), main(), Get status of all epics and tickets.]
- "wave3_generate_wave3_phase2_scripts": "generate_wave3_phase2_scripts.py" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase2_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 3bcfaac feat: Institutionalize building…, a24ed35 feat: Institutionalize building…, be6c8a1 docs: Wave 4 documentation merg…, generate_phase2_scripts()]
- "wave4_execute_80_80_recovery_main": "main()" | kind=code-symbol | source=scripts/wave4/execute_80_80_recovery.py:L219 | neighbors=[execute_80_80_recovery.py, monitor_recovery(), step1_fix_phase6_path_issue(), step2_upload_missing_phase5_scripts(), Execute recovery plan.]
- "wave4_execute_80_80_recovery_run_gcloud": "run_gcloud()" | kind=code-symbol | source=scripts/wave4/execute_80_80_recovery.py:L27 | neighbors=[execute_80_80_recovery.py, monitor_recovery(), Execute gcloud command and return exit …, step1_fix_phase6_path_issue(), step2_upload_missing_phase5_scripts()]
- "wave4_fix_phase6_prerequisite_v3": "fix_phase6_prerequisite_v3.py" | kind=code-symbol | source=scripts/wave4/fix_phase6_prerequisite_v3.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…, replace_check()]
- "wave4_generate_phase1_scripts": "generate_phase1_scripts.py" | kind=code-symbol | source=scripts/wave4/generate_phase1_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…, load_api_keys()]
- "wave6_generate_phase1_5_scripts": "generate_phase1_5_scripts.py" | kind=code-symbol | source=scripts/wave6/generate_phase1_5_scripts.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, generate_phase1_5_script(), get_agent_id(), main()]
- "wave7_fix_failed_epics_with_active_keys": "fix_failed_epics_with_active_keys.py" | kind=code-symbol | source=building-blocks/wave7/fix_failed_epics_with_active_keys.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, fix_epic_script(), load_active_api_keys(), main()]
- "wave7_generate_phase0_scripts_fixed_generate_scripts": "generate_scripts()" | kind=code-symbol | source=scripts/wave7/generate_phase0_scripts_fixed.py:L139 | neighbors=[generate_phase0_scripts_fixed.py, get_bob_message(), load_api_keys(), load_pending_epics(), Generate Phase 0 scripts for Wave 7 epi…]
- "wave7_launch_epic_with_fixed_env": "launch_epic_with_fixed_env.py" | kind=code-symbol | source=building-blocks/wave7/launch_epic_with_fixed_env.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, get_fixed_environment(), launch_epic(), launch_epic_batch()]
- "add_path_to_scripts": "add_path_to_scripts.py" | kind=code-symbol | source=add_path_to_scripts.py:L1 | neighbors=[add_path_to_script(), main(), 180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …]
- "analyze_complexity_audit": "analyze_complexity_audit.py" | kind=code-symbol | source=analyze_complexity_audit.py:L1 | neighbors=[analyze_complexity_audit(), main(), f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "benchmarks_spscring_benchmarks": "SpscRing.Benchmarks.csproj" | kind=code-symbol | source=benchmarks/SpscRing.Benchmarks.csproj:L1 | neighbors=[net6.0, Microsoft.NET.Sdk, 7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…]
- "bob_mcp_mcp_server_sequential_thinking": "sequential-thinking" | kind=code-symbol | source=.bob/mcp.json:L1 | neighbors=[mcp.json, PATH, npx, @modelcontextprotocol/server-sequential…]
- "calculate_phase3_bobcoins": "calculate_phase3_bobcoins.py" | kind=code-symbol | source=calculate_phase3_bobcoins.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "card_board_main_eo": "Eo()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, c(), p(), xo()]
- "card_board_main_fn": "Fn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, An(), c(), Un()]
- "card_board_main_fo": "Fo()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, Co(), c(), p()]
- "card_board_main_to": "To()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, Co(), p(), xo()]
- "card_board_main_wn": "wn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, qn(), s(), Zn()]
- "card_board_main_wr": "Wr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, qr(), Br(), Ur()]
- "card_board_main_x": "x()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, d(), m(), p()]
- "card_board_main_xr": "Xr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, d(), s(), sr()]
- "card_board_main_yn": "yn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, pn(), qn(), s()]
- "check_incomplete": "check_incomplete.py" | kind=code-symbol | source=check_incomplete.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "check_roadmap_epics": "check_roadmap_epics.py" | kind=code-symbol | source=check_roadmap_epics.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@3cc67485b31028ebd66572934c0402d70f19f09e": "3cc6748 feat(protocol): GitButler integration for Bob CLI - auto branch managem…" | kind=Commit | source=git | neighbors=[gitbutler/workspace, 500c4a9 docs(protocol): GitButler integ…, after_task_complete.py, before_new_task.py]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@803704cea2f6f3dade4963e7169682e791cf1ab9": "803704c Merge remote-tracking branch 'origin/main' into gitbutler/workspace" | kind=Commit | source=git | neighbors=[4427b8b [DOCS] EPIC-CCN-51 planning art…, gitbutler/workspace, 3f42ed0 Merge branch 'feature/infra-pr1…, ffe73a8 Merge branch 'build/1105-monoli…]
- "complete_wave_cross_reference_generate_report": "generate_report()" | kind=code-symbol | source=complete_wave_cross_reference.py:L197 | neighbors=[complete_wave_cross_reference.py, generate_markdown_summary(), main(), Generate comprehensive cross-reference …]
- "eval_viewer_generate_review_build_run": "build_run()" | kind=code-symbol | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L85 | neighbors=[generate_review.py, embed_file(), _find_runs_recursive(), Build a run dict with prompt, outputs, …]
- "eval_viewer_generate_review_embed_file": "embed_file()" | kind=code-symbol | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L149 | neighbors=[generate_review.py, build_run(), get_mime_type(), Read a file and return an embedded repr…]
- "eval_viewer_generate_review_generate_html": "generate_html()" | kind=code-symbol | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L250 | neighbors=[generate_review.py, main(), Generate the complete standalone HTML p…, .do_GET()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-019.json

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
