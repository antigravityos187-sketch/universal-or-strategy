# Node Description Batch 34 of 61

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
No marketing language.
Respond ONLY with a JSON object mapping each node id (as a string) to its
one-sentence description — no prose, no markdown fences.

- "validate_180_method_count_analyze_by_file": "analyze_by_file()" | kind=code-symbol | source=validate_180_method_count.py:L82 | neighbors=[validate_180_method_count.py, main(), Group methods by file.]
- "validate_180_method_count_analyze_distribution": "analyze_distribution()" | kind=code-symbol | source=validate_180_method_count.py:L63 | neighbors=[validate_180_method_count.py, main(), Analyze complexity distribution.]
- "validate_180_method_count_parse_complexity_audit": "parse_complexity_audit()" | kind=code-symbol | source=validate_180_method_count.py:L29 | neighbors=[validate_180_method_count.py, main(), Parse complexity audit and extract meth…]
- "validate_180_method_count_validate_count": "validate_count()" | kind=code-symbol | source=validate_180_method_count.py:L52 | neighbors=[validate_180_method_count.py, main(), Validate method count matches expected.]
- "validate_wave6_epic_structure": "validate_wave6_epic_structure.py" | kind=code-symbol | source=validate_wave6_epic_structure.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, validate_wave6_structure()]
- "wave2_api_balance_tracker_estimate_phase_budget": "estimate_phase_budget()" | kind=code-symbol | source=scripts/wave2/api_balance_tracker.py:L125 | neighbors=[api_balance_tracker.py, check_phase_feasibility(), Estimate bobcoin budget for a phase]
- "wave2_api_balance_tracker_print_summary": "print_summary()" | kind=code-symbol | source=scripts/wave2/api_balance_tracker.py:L83 | neighbors=[api_balance_tracker.py, load_tracker_state(), Print summary of all API balances]
- "wave2_api_balance_tracker_save_tracker_state": "save_tracker_state()" | kind=code-symbol | source=scripts/wave2/api_balance_tracker.py:L46 | neighbors=[api_balance_tracker.py, Save tracker state to file, record_usage()]
- "wave2_check_api_balances_check_balance": "check_balance()" | kind=code-symbol | source=scripts/wave2/check_api_balances.py:L16 | neighbors=[check_api_balances.py, main(), Check balance for a single API key usin…]
- "wave2_check_phase2_status_get_epic_status": "get_epic_status()" | kind=code-symbol | source=scripts/wave2/check_phase2_status.py:L12 | neighbors=[check_phase2_status.py, main(), Get Phase 2 status for an epic.]
- "wave2_generate_phase1_scripts_generate_scripts": "generate_scripts()" | kind=code-symbol | source=scripts/wave2/generate_phase1_scripts.py:L71 | neighbors=[generate_phase1_scripts.py, load_api_key(), Generate Phase 1 scripts for all epics …]
- "wave2_generate_phase1_scripts_load_api_key": "load_api_key()" | kind=code-symbol | source=scripts/wave2/generate_phase1_scripts.py:L23 | neighbors=[generate_phase1_scripts.py, generate_scripts(), Load API key from JSON file.]
- "wave2_generate_phase2_scripts_generate_launcher": "generate_launcher()" | kind=code-symbol | source=scripts/wave2/generate_phase2_scripts.py:L93 | neighbors=[generate_phase2_scripts.py, main(), Generate launcher script for all Phase …]
- "wave2_generate_phase2_scripts_generate_phase2_script": "generate_phase2_script()" | kind=code-symbol | source=scripts/wave2/generate_phase2_scripts.py:L33 | neighbors=[generate_phase2_scripts.py, main(), Generate Phase 2 script for given epic …]
- "wave2_generate_phase2_scripts_main": "main()" | kind=code-symbol | source=scripts/wave2/generate_phase2_scripts.py:L132 | neighbors=[generate_phase2_scripts.py, generate_launcher(), generate_phase2_script()]
- "wave2_generate_phase3_scripts_generate_launcher": "generate_launcher()" | kind=code-symbol | source=scripts/wave2/generate_phase3_scripts.py:L101 | neighbors=[generate_phase3_scripts.py, main(), Generate launcher script for all Phase …]
- "wave2_generate_phase3_scripts_generate_phase3_script": "generate_phase3_script()" | kind=code-symbol | source=scripts/wave2/generate_phase3_scripts.py:L36 | neighbors=[generate_phase3_scripts.py, main(), Generate Phase 3 script for given epic …]
- "wave2_generate_phase3_scripts_main": "main()" | kind=code-symbol | source=scripts/wave2/generate_phase3_scripts.py:L144 | neighbors=[generate_phase3_scripts.py, generate_launcher(), generate_phase3_script()]
- "wave2_generate_phase4_scripts_generate_launcher": "generate_launcher()" | kind=code-symbol | source=scripts/wave2/generate_phase4_scripts.py:L107 | neighbors=[generate_phase4_scripts.py, main(), Generate launcher script for all Phase …]
- "wave2_generate_phase4_scripts_generate_phase4_script": "generate_phase4_script()" | kind=code-symbol | source=scripts/wave2/generate_phase4_scripts.py:L36 | neighbors=[generate_phase4_scripts.py, main(), Generate Phase 4 script for given epic …]
- "wave2_generate_phase4_scripts_main": "main()" | kind=code-symbol | source=scripts/wave2/generate_phase4_scripts.py:L150 | neighbors=[generate_phase4_scripts.py, generate_launcher(), generate_phase4_script()]
- "wave2_generate_phase5_scripts_copy_and_modify_phase4_to_phase5_ticket": "copy_and_modify_phase4_to_phase5_ticket()" | kind=code-symbol | source=scripts/wave2/generate_phase5_scripts.py:L31 | neighbors=[generate_phase5_scripts.py, main(), Copy Phase 4 script and modify for Phas…]
- "wave2_generate_phase5_scripts_copy_and_modify_phase4_to_phase5_validator": "copy_and_modify_phase4_to_phase5_validator()" | kind=code-symbol | source=scripts/wave2/generate_phase5_scripts.py:L77 | neighbors=[generate_phase5_scripts.py, main(), Copy Phase 4 script and modify for Phas…]
- "wave2_generate_phase5_scripts_copy_and_modify_phase4_to_phase6_review": "copy_and_modify_phase4_to_phase6_review()" | kind=code-symbol | source=scripts/wave2/generate_phase5_scripts.py:L123 | neighbors=[generate_phase5_scripts.py, main(), Copy Phase 4 script and modify for Phas…]
- "wave2_generate_phase5_scripts_generate_gated_launcher": "generate_gated_launcher()" | kind=code-symbol | source=scripts/wave2/generate_phase5_scripts.py:L162 | neighbors=[generate_phase5_scripts.py, main(), Generate gated sequential launcher scri…]
- "wave2_launch_phase0_v4_shell_commands_main": "main()" | kind=code-symbol | source=scripts/wave2/launch_phase0_v4_shell_commands.py:L59 | neighbors=[launch_phase0_v4_shell_commands.py, create_script(), load_api_key()]
- "wave2_launch_wave_get_bob_api_key": "get_bob_api_key()" | kind=code-symbol | source=scripts/wave2/launch_wave.py:L51 | neighbors=[launch_wave.py, launch_wave(), Read Bob API key from local .env or env…]
- "wave2_launch_wave_now_build_wave_script": "build_wave_script()" | kind=code-symbol | source=scripts/wave2/launch_wave_now.py:L58 | neighbors=[launch_wave_now.py, main(), Generate the wave orchestrator script —…]
- "wave2_launch_wave_now_load_epics": "load_epics()" | kind=code-symbol | source=scripts/wave2/launch_wave_now.py:L44 | neighbors=[launch_wave_now.py, main(), Load epics from file. Returns list of (…]
- "wave2_launch_wave_run": "run()" | kind=code-symbol | source=scripts/wave2/launch_wave.py:L36 | neighbors=[launch_wave.py, gcloud(), Run a subprocess command. Paths with sp…]
- "wave2_launch_wave_v2_build_wave_script": "build_wave_script()" | kind=code-symbol | source=scripts/wave2/launch_wave_v2.py:L63 | neighbors=[launch_wave_v2.py, main(), Generate the wave orchestrator script f…]
- "wave2_launch_wave_v2_load_epics": "load_epics()" | kind=code-symbol | source=scripts/wave2/launch_wave_v2.py:L49 | neighbors=[launch_wave_v2.py, main(), Load epics from file. Returns list of (…]
- "wave2_launch_wave_v3_multi_api_build_wave_script": "build_wave_script()" | kind=code-symbol | source=scripts/wave2/launch_wave_v3_multi_api.py:L77 | neighbors=[launch_wave_v3_multi_api.py, main(), Generate orchestrator script with 1 API…]
- "wave2_launch_wave_v3_multi_api_load_api_keys": "load_api_keys()" | kind=code-symbol | source=scripts/wave2/launch_wave_v3_multi_api.py:L49 | neighbors=[launch_wave_v3_multi_api.py, main(), Load API keys from docs/API/*.json file…]
- "wave2_launch_wave_v3_multi_api_load_epics": "load_epics()" | kind=code-symbol | source=scripts/wave2/launch_wave_v3_multi_api.py:L63 | neighbors=[launch_wave_v3_multi_api.py, main(), Load epics from file. Returns list of (…]
- "wave2_launch_wave_v4_safe_budget_build_wave_script": "build_wave_script()" | kind=code-symbol | source=scripts/wave2/launch_wave_v4_safe_budget.py:L79 | neighbors=[launch_wave_v4_safe_budget.py, main(), Generate orchestrator script with 1 API…]
- "wave2_launch_wave_v4_safe_budget_load_api_keys": "load_api_keys()" | kind=code-symbol | source=scripts/wave2/launch_wave_v4_safe_budget.py:L51 | neighbors=[launch_wave_v4_safe_budget.py, main(), Load API keys from docs/API/*.json file…]
- "wave2_launch_wave_v4_safe_budget_load_epics": "load_epics()" | kind=code-symbol | source=scripts/wave2/launch_wave_v4_safe_budget.py:L65 | neighbors=[launch_wave_v4_safe_budget.py, main(), Load epics from file. Returns list of (…]
- "wave2_monitor_phase4_check_log_completion": "check_log_completion()" | kind=code-symbol | source=scripts/wave2/monitor_phase4.py:L41 | neighbors=[monitor_phase4.py, main(), Check if log shows DONE_EXIT]
- "wave2_monitor_phase4_check_screen_sessions": "check_screen_sessions()" | kind=code-symbol | source=scripts/wave2/monitor_phase4.py:L23 | neighbors=[monitor_phase4.py, main(), Check which screen sessions are still r…]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-033.json

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
