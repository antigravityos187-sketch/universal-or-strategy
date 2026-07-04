# Node Description Batch 55 of 61

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

- "scripts_validate_phase_compliance_rationale_98": "Run all validation checks. Returns (success, errors, warnings)." | kind=entity | source=scripts/validate_phase_compliance.py:L98 | neighbors=[.validate()]
- "scripts_verify_index_freshness_rationale_26": "Get timestamp of current git HEAD commit." | kind=entity | source=scripts/verify_index_freshness.py:L26 | neighbors=[get_git_head_timestamp()]
- "scripts_verify_index_freshness_rationale_37": "Get timestamp of graphify-out/graph.json." | kind=entity | source=scripts/verify_index_freshness.py:L37 | neighbors=[get_graphify_timestamp()]
- "scripts_verify_index_freshness_rationale_47": "Get list of files modified since given timestamp." | kind=entity | source=scripts/verify_index_freshness.py:L47 | neighbors=[get_modified_files_since()]
- "scripts_verify_index_freshness_rationale_61": "Verify jCodemunch index is fresh.\r     \r     Args:\r         max_age_days: Maximu" | kind=entity | source=scripts/verify_index_freshness.py:L61 | neighbors=[verify_index_freshness()]
- "scripts_verify_wave7_determinism_rationale_101": "Print verification results.\r     \r     Args:\r         total: Total epics checked" | kind=entity | source=scripts/verify_wave7_determinism.py:L101 | neighbors=[print_results()]
- "scripts_verify_wave7_determinism_rationale_22": "Get all Wave 7 epic IDs from event log.\r     \r     Returns:\r         List of uni" | kind=entity | source=scripts/verify_wave7_determinism.py:L22 | neighbors=[get_wave7_epics()]
- "scripts_verify_wave7_determinism_rationale_46": "Verify determinism for a single epic.\r     \r     Args:\r         epic_id: Epic id" | kind=entity | source=scripts/verify_wave7_determinism.py:L46 | neighbors=[verify_epic()]
- "scripts_verify_wave7_determinism_rationale_77": "Verify determinism for all Wave 7 epics.\r     \r     Returns:\r         (total_epi" | kind=entity | source=scripts/verify_wave7_determinism.py:L77 | neighbors=[verify_all_epics()]
- "scripts_verify_wave7_templates_rationale_107": "Verify a single template file." | kind=entity | source=scripts/verify_wave7_templates.py:L107 | neighbors=[verify_template()]
- "scripts_verify_wave7_templates_rationale_149": "Print verification summary." | kind=entity | source=scripts/verify_wave7_templates.py:L149 | neighbors=[print_summary()]
- "scripts_verify_wave7_templates_rationale_186": "Main verification routine." | kind=entity | source=scripts/verify_wave7_templates.py:L186 | neighbors=[main()]
- "scripts_verify_wave7_templates_rationale_56": "Check if all required template files exist." | kind=entity | source=scripts/verify_wave7_templates.py:L56 | neighbors=[check_file_exists()]
- "scripts_verify_wave7_templates_rationale_64": "Check if template uses temp file + command substitution pattern." | kind=entity | source=scripts/verify_wave7_templates.py:L64 | neighbors=[check_temp_file_pattern()]
- "scripts_verify_wave7_templates_rationale_85": "Check EPIC naming convention." | kind=entity | source=scripts/verify_wave7_templates.py:L85 | neighbors=[check_epic_naming()]
- "scripts_verify_wave7_templates_rationale_97": "Replace EPIC-CCN-XXX with EPIC-W7-XXX." | kind=entity | source=scripts/verify_wave7_templates.py:L97 | neighbors=[fix_epic_naming()]
- "scripts_wave_coordinator_rationale_103": "Generate human-readable instructions for executing MCP calls." | kind=entity | source=scripts/wave_coordinator.py:L103 | neighbors=[._generate_instructions()]
- "scripts_wave_coordinator_rationale_141": "Run a batch of epics through specified phases.\r         \r         Args:" | kind=entity | source=scripts/wave_coordinator.py:L141 | neighbors=[.run_wave_batch()]
- "scripts_wave_coordinator_rationale_165": "Save wave execution checkpoint." | kind=entity | source=scripts/wave_coordinator.py:L165 | neighbors=[._save_checkpoint()]
- "scripts_wave_coordinator_rationale_18": "Coordinates wave-based epic execution through all phases." | kind=entity | source=scripts/wave_coordinator.py:L18 | neighbors=[WaveCoordinator]
- "scripts_wave_coordinator_rationale_186": "Get next batch of pending epics.\r         \r         Args:\r             wave_numb" | kind=entity | source=scripts/wave_coordinator.py:L186 | neighbors=[.get_next_wave()]
- "scripts_wave_coordinator_rationale_212": "Generate complete execution plan for N waves.\r         \r         Args:" | kind=entity | source=scripts/wave_coordinator.py:L212 | neighbors=[.generate_execution_plan()]
- "scripts_wave_coordinator_rationale_252": "CLI entry point for wave coordinator." | kind=entity | source=scripts/wave_coordinator.py:L252 | neighbors=[main()]
- "scripts_wave_coordinator_rationale_34": "Initialize Wave Coordinator.\r         \r         Args:\r             wave_size: Nu" | kind=entity | source=scripts/wave_coordinator.py:L34 | neighbors=[.__init__()]
- "scripts_wave_coordinator_rationale_49": "Load epic roadmap and filter pending epics." | kind=entity | source=scripts/wave_coordinator.py:L49 | neighbors=[.load_roadmap()]
- "scripts_wave_coordinator_rationale_55": "Get phase configuration by ID." | kind=entity | source=scripts/wave_coordinator.py:L55 | neighbors=[.get_phase_config()]
- "scripts_wave_coordinator_rationale_62": "Execute one phase for multiple epics.\r         \r         This is a COORDINATOR f" | kind=entity | source=scripts/wave_coordinator.py:L62 | neighbors=[.execute_wave()]
- "scripts_wave2_bob_shell_executor_rationale_100": "Execute a phase for all epics in parallel" | kind=entity | source=scripts/wave2_bob_shell_executor.py:L100 | neighbors=[execute_phase_parallel()]
- "scripts_wave2_bob_shell_executor_rationale_130": "Execute all phases for Wave 2 epics" | kind=entity | source=scripts/wave2_bob_shell_executor.py:L130 | neighbors=[main()]
- "scripts_wave2_bob_shell_executor_rationale_31": "Execute a phase using Bob Shell API mode (non-interactive)" | kind=entity | source=scripts/wave2_bob_shell_executor.py:L31 | neighbors=[execute_phase_with_bob_shell()]
- "scripts_wave2_direct_executor_rationale_113": "Execute Phase 1 for all Wave 2 epics using Bob CLI" | kind=entity | source=scripts/wave2_direct_executor.py:L113 | neighbors=[execute_phase_1_all()]
- "scripts_wave2_direct_executor_rationale_27": "Create Phase 0 artifacts directly (no MCP)" | kind=entity | source=scripts/wave2_direct_executor.py:L27 | neighbors=[create_phase_0_artifacts()]
- "scripts_wave2_direct_executor_rationale_85": "Execute Phase 0 for all Wave 2 epics" | kind=entity | source=scripts/wave2_direct_executor.py:L85 | neighbors=[execute_phase_0_all()]
- "scripts_wave2_direct_executor_rationale_96": "Create Bob CLI prompt for Phase 1" | kind=entity | source=scripts/wave2_direct_executor.py:L96 | neighbors=[create_bob_prompt_for_phase_1()]
- "scripts_wave2_parallel_executor_rationale_147": "Generate Phase 1 prompt for an epic" | kind=entity | source=scripts/wave2_parallel_executor.py:L147 | neighbors=[phase_1_prompt()]
- "scripts_wave2_parallel_executor_rationale_174": "Generate Phase 1.5 prompt for an epic" | kind=entity | source=scripts/wave2_parallel_executor.py:L174 | neighbors=[phase_1_5_prompt()]
- "scripts_wave2_parallel_executor_rationale_195": "Generate Phase 2 prompt for an epic" | kind=entity | source=scripts/wave2_parallel_executor.py:L195 | neighbors=[phase_2_prompt()]
- "scripts_wave2_parallel_executor_rationale_215": "Generate Phase 3 prompt for an epic" | kind=entity | source=scripts/wave2_parallel_executor.py:L215 | neighbors=[phase_3_prompt()]
- "scripts_wave2_parallel_executor_rationale_233": "Generate Phase 4 prompt for an epic" | kind=entity | source=scripts/wave2_parallel_executor.py:L233 | neighbors=[phase_4_prompt()]
- "scripts_wave2_parallel_executor_rationale_250": "Generate Phase 5 prompt for an epic" | kind=entity | source=scripts/wave2_parallel_executor.py:L250 | neighbors=[phase_5_prompt()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-054.json

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
