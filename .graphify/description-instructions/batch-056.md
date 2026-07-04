# Node Description Batch 57 of 61

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

- "wave2_check_api_balances_rationale_17": "Check balance for a single API key using bob CLI." | kind=entity | source=scripts/wave2/check_api_balances.py:L17 | neighbors=[check_balance()]
- "wave2_check_phase2_status_rationale_13": "Get Phase 2 status for an epic." | kind=entity | source=scripts/wave2/check_phase2_status.py:L13 | neighbors=[get_epic_status()]
- "wave2_check_phase4_local_rationale_14": "Check Phase 4 status for all Wave 2 epics." | kind=entity | source=scripts/wave2/check_phase4_local.py:L14 | neighbors=[check_phase4_status()]
- "wave2_generate_phase1_scripts_rationale_24": "Load API key from JSON file." | kind=entity | source=scripts/wave2/generate_phase1_scripts.py:L24 | neighbors=[load_api_key()]
- "wave2_generate_phase1_scripts_rationale_72": "Generate Phase 1 scripts for all epics with hardcoded API keys." | kind=entity | source=scripts/wave2/generate_phase1_scripts.py:L72 | neighbors=[generate_scripts()]
- "wave2_generate_phase2_scripts_rationale_34": "Generate Phase 2 script for given epic number" | kind=entity | source=scripts/wave2/generate_phase2_scripts.py:L34 | neighbors=[generate_phase2_script()]
- "wave2_generate_phase2_scripts_rationale_94": "Generate launcher script for all Phase 2 scripts" | kind=entity | source=scripts/wave2/generate_phase2_scripts.py:L94 | neighbors=[generate_launcher()]
- "wave2_generate_phase3_scripts_rationale_102": "Generate launcher script for all Phase 3 scripts" | kind=entity | source=scripts/wave2/generate_phase3_scripts.py:L102 | neighbors=[generate_launcher()]
- "wave2_generate_phase3_scripts_rationale_37": "Generate Phase 3 script for given epic number" | kind=entity | source=scripts/wave2/generate_phase3_scripts.py:L37 | neighbors=[generate_phase3_script()]
- "wave2_generate_phase4_scripts_rationale_108": "Generate launcher script for all Phase 4 scripts" | kind=entity | source=scripts/wave2/generate_phase4_scripts.py:L108 | neighbors=[generate_launcher()]
- "wave2_generate_phase4_scripts_rationale_37": "Generate Phase 4 script for given epic number" | kind=entity | source=scripts/wave2/generate_phase4_scripts.py:L37 | neighbors=[generate_phase4_script()]
- "wave2_generate_phase5_scripts_rationale_124": "Copy Phase 4 script and modify for Phase 6 epic review." | kind=entity | source=scripts/wave2/generate_phase5_scripts.py:L124 | neighbors=[copy_and_modify_phase4_to_phase6_review…]
- "wave2_generate_phase5_scripts_rationale_163": "Generate gated sequential launcher script." | kind=entity | source=scripts/wave2/generate_phase5_scripts.py:L163 | neighbors=[generate_gated_launcher()]
- "wave2_generate_phase5_scripts_rationale_32": "Copy Phase 4 script and modify for Phase 5 ticket execution." | kind=entity | source=scripts/wave2/generate_phase5_scripts.py:L32 | neighbors=[copy_and_modify_phase4_to_phase5_ticket…]
- "wave2_generate_phase5_scripts_rationale_78": "Copy Phase 4 script and modify for Phase 5 ticket validation." | kind=entity | source=scripts/wave2/generate_phase5_scripts.py:L78 | neighbors=[copy_and_modify_phase4_to_phase5_valida…]
- "wave2_launch_phase0_v4_shell_commands_load_epics_from_roadmap": "load_epics_from_roadmap()" | kind=code-symbol | source=scripts/wave2/launch_phase0_v4_shell_commands.py:L7 | neighbors=[launch_phase0_v4_shell_commands.py]
- "wave2_launch_wave_now_rationale_124": "Remove stale Plink key cache entry for the VM's IP." | kind=entity | source=scripts/wave2/launch_wave_now.py:L124 | neighbors=[clear_stale_ssh_key()]
- "wave2_launch_wave_now_rationale_45": "Load epics from file. Returns list of (epic_id, method_name, complexity) tuples." | kind=entity | source=scripts/wave2/launch_wave_now.py:L45 | neighbors=[load_epics()]
- "wave2_launch_wave_now_rationale_59": "Generate the wave orchestrator script — LF line endings, no $REPO in subshells." | kind=entity | source=scripts/wave2/launch_wave_now.py:L59 | neighbors=[build_wave_script()]
- "wave2_launch_wave_rationale_128": "Poll VM for orchestrator status and agent progress." | kind=entity | source=scripts/wave2/launch_wave.py:L128 | neighbors=[monitor_wave()]
- "wave2_launch_wave_rationale_150": "Pull log files from VM back to local machine." | kind=entity | source=scripts/wave2/launch_wave.py:L150 | neighbors=[collect_results()]
- "wave2_launch_wave_rationale_37": "Run a subprocess command. Paths with spaces are handled via list args." | kind=entity | source=scripts/wave2/launch_wave.py:L37 | neighbors=[run()]
- "wave2_launch_wave_rationale_52": "Read Bob API key from local .env or environment variable.          NOTE: Golden" | kind=entity | source=scripts/wave2/launch_wave.py:L52 | neighbors=[get_bob_api_key()]
- "wave2_launch_wave_rationale_70": "Launch a wave VM with orchestrator via startup script metadata." | kind=entity | source=scripts/wave2/launch_wave.py:L70 | neighbors=[launch_wave()]
- "wave2_launch_wave_v2_rationale_131": "Remove stale Plink key cache entry for the VM's IP." | kind=entity | source=scripts/wave2/launch_wave_v2.py:L131 | neighbors=[clear_stale_ssh_key()]
- "wave2_launch_wave_v2_rationale_50": "Load epics from file. Returns list of (epic_id, method_name, complexity) tuples." | kind=entity | source=scripts/wave2/launch_wave_v2.py:L50 | neighbors=[load_epics()]
- "wave2_launch_wave_v2_rationale_64": "Generate the wave orchestrator script for FULL epic-intake workflow." | kind=entity | source=scripts/wave2/launch_wave_v2.py:L64 | neighbors=[build_wave_script()]
- "wave2_launch_wave_v3_multi_api_rationale_142": "Remove stale Plink key cache entry for the VM's IP." | kind=entity | source=scripts/wave2/launch_wave_v3_multi_api.py:L142 | neighbors=[clear_stale_ssh_key()]
- "wave2_launch_wave_v3_multi_api_rationale_50": "Load API keys from docs/API/*.json files." | kind=entity | source=scripts/wave2/launch_wave_v3_multi_api.py:L50 | neighbors=[load_api_keys()]
- "wave2_launch_wave_v3_multi_api_rationale_64": "Load epics from file. Returns list of (epic_id, method_name, complexity) tuples." | kind=entity | source=scripts/wave2/launch_wave_v3_multi_api.py:L64 | neighbors=[load_epics()]
- "wave2_launch_wave_v3_multi_api_rationale_78": "Generate orchestrator script with 1 API key per agent." | kind=entity | source=scripts/wave2/launch_wave_v3_multi_api.py:L78 | neighbors=[build_wave_script()]
- "wave2_launch_wave_v4_safe_budget_rationale_165": "Remove stale Plink key cache entry for the VM's IP." | kind=entity | source=scripts/wave2/launch_wave_v4_safe_budget.py:L165 | neighbors=[clear_stale_ssh_key()]
- "wave2_launch_wave_v4_safe_budget_rationale_52": "Load API keys from docs/API/*.json files. Returns list of (filename, apikey) tup" | kind=entity | source=scripts/wave2/launch_wave_v4_safe_budget.py:L52 | neighbors=[load_api_keys()]
- "wave2_launch_wave_v4_safe_budget_rationale_66": "Load epics from file. Returns list of (epic_id, method_name, complexity) tuples." | kind=entity | source=scripts/wave2/launch_wave_v4_safe_budget.py:L66 | neighbors=[load_epics()]
- "wave2_launch_wave_v4_safe_budget_rationale_80": "Generate orchestrator script with 1 API key per agent + budget tracking." | kind=entity | source=scripts/wave2/launch_wave_v4_safe_budget.py:L80 | neighbors=[build_wave_script()]
- "wave2_monitor_phase4_rationale_24": "Check which screen sessions are still running" | kind=entity | source=scripts/wave2/monitor_phase4.py:L24 | neighbors=[check_screen_sessions()]
- "wave2_monitor_phase4_rationale_42": "Check if log shows DONE_EXIT" | kind=entity | source=scripts/wave2/monitor_phase4.py:L42 | neighbors=[check_log_completion()]
- "wave2_monitor_phase4_rationale_57": "Update manifest with completion status" | kind=entity | source=scripts/wave2/monitor_phase4.py:L57 | neighbors=[update_manifest_status()]
- "wave2_phase4_with_checkpoints_rationale_115": "Load API key from JSON file" | kind=entity | source=scripts/wave2/phase4_with_checkpoints.py:L115 | neighbors=[load_api_key()]
- "wave2_phase4_with_checkpoints_rationale_122": "Build bash script for Phase 4 execution with checkpoints" | kind=entity | source=scripts/wave2/phase4_with_checkpoints.py:L122 | neighbors=[build_phase4_script()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-056.json

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
