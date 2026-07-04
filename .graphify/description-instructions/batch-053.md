# Node Description Batch 54 of 61

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

- "scripts_test_v12_52_rationale_229": "Test 7: Filesystem state verification." | kind=entity | source=scripts/test_v12_52.py:L229 | neighbors=[test_filesystem_state_verification()]
- "scripts_test_v12_52_rationale_246": "Test 8: Failure handling and recovery." | kind=entity | source=scripts/test_v12_52.py:L246 | neighbors=[test_failure_handling()]
- "scripts_test_v12_52_rationale_278": "Run all V12.52 tests." | kind=entity | source=scripts/test_v12_52.py:L278 | neighbors=[run_all_tests()]
- "scripts_test_v12_52_rationale_56": "Clean up test data from previous runs." | kind=entity | source=scripts/test_v12_52.py:L56 | neighbors=[cleanup_test_data()]
- "scripts_test_v12_52_rationale_71": "Test 1: Lamport clock increments monotonically." | kind=entity | source=scripts/test_v12_52.py:L71 | neighbors=[test_lamport_clock_monotonicity()]
- "scripts_test_v12_52_rationale_98": "Test 2: Event log maintains causal order." | kind=entity | source=scripts/test_v12_52.py:L98 | neighbors=[test_event_log_ordering()]
- "scripts_test_wave7_lamport_rationale_24": "Test Wave 7 Lamport clock implementation." | kind=entity | source=scripts/test_wave7_lamport.py:L24 | neighbors=[main()]
- "scripts_test_worker_mcp_client_rationale_103": "Test all 4 worker agents" | kind=entity | source=scripts/test_worker_mcp_client.py:L103 | neighbors=[test_all_workers()]
- "scripts_test_worker_mcp_client_rationale_124": "Test a single worker by ID" | kind=entity | source=scripts/test_worker_mcp_client.py:L124 | neighbors=[test_single_worker()]
- "scripts_test_worker_mcp_client_rationale_18": "Test all MCP tools for a single worker" | kind=entity | source=scripts/test_worker_mcp_client.py:L18 | neighbors=[test_worker_agent()]
- "scripts_trailing_split_extract": "extract()" | kind=code-symbol | source=scripts/trailing_split.py:L66 | neighbors=[trailing_split.py]
- "scripts_trailing_split_make_header_simple": "make_header_simple()" | kind=code-symbol | source=scripts/trailing_split.py:L53 | neighbors=[trailing_split.py]
- "scripts_trailing_split_make_header_wrapped": "make_header_wrapped()" | kind=code-symbol | source=scripts/trailing_split.py:L42 | neighbors=[trailing_split.py]
- "scripts_trailing_split_write_file": "write_file()" | kind=code-symbol | source=scripts/trailing_split.py:L69 | neighbors=[trailing_split.py]
- "scripts_ui_ipc_split_extract": "extract()" | kind=code-symbol | source=scripts/ui_ipc_split.py:L60 | neighbors=[ui_ipc_split.py]
- "scripts_ui_ipc_split_make_header": "make_header()" | kind=code-symbol | source=scripts/ui_ipc_split.py:L42 | neighbors=[ui_ipc_split.py]
- "scripts_ui_ipc_split_write_file": "write_file()" | kind=code-symbol | source=scripts/ui_ipc_split.py:L63 | neighbors=[ui_ipc_split.py]
- "scripts_utils_rationale_1": "Shared utilities for skill-creator scripts." | kind=entity | source=.bob/skills/skill-creator/scripts/utils.py:L1 | neighbors=[utils.py]
- "scripts_utils_rationale_8": "Parse a SKILL.md file, returning (name, description, full_content)." | kind=entity | source=.bob/skills/skill-creator/scripts/utils.py:L8 | neighbors=[parse_skill_md()]
- "scripts_v12_main_split_extract": "extract()" | kind=code-symbol | source=scripts/v12_main_split.py:L55 | neighbors=[v12_main_split.py]
- "scripts_v12_main_split_make_header": "make_header()" | kind=code-symbol | source=scripts/v12_main_split.py:L43 | neighbors=[v12_main_split.py]
- "scripts_v12_main_split_write_file": "write_file()" | kind=code-symbol | source=scripts/v12_main_split.py:L58 | neighbors=[v12_main_split.py]
- "scripts_v12_split_rationale_14": "Extract a method block from source lines.     Returns (method_lines, start_idx," | kind=entity | source=scripts/v12_split.py:L14 | neighbors=[extract_method_block()]
- "scripts_v12_split_rationale_65": "Split a method from source file.     If output_file is None, modifies source_fil" | kind=entity | source=scripts/v12_split.py:L65 | neighbors=[split_method()]
- "scripts_validate_epic_rationale_112": "Release epic lock (on completion or failure)" | kind=entity | source=scripts/validate_epic.py:L112 | neighbors=[release_epic()]
- "scripts_validate_epic_rationale_126": "List all currently assigned epics" | kind=entity | source=scripts/validate_epic.py:L126 | neighbors=[list_assigned_epics()]
- "scripts_validate_epic_rationale_131": "List pending epics (not complete, not assigned)" | kind=entity | source=scripts/validate_epic.py:L131 | neighbors=[list_pending_epics()]
- "scripts_validate_epic_rationale_23": "Load epic_roadmap.json" | kind=entity | source=scripts/validate_epic.py:L23 | neighbors=[load_roadmap()]
- "scripts_validate_epic_rationale_36": "Verify epic exists in roadmap" | kind=entity | source=scripts/validate_epic.py:L36 | neighbors=[validate_epic_exists()]
- "scripts_validate_epic_rationale_41": "Get epic details from roadmap" | kind=entity | source=scripts/validate_epic.py:L41 | neighbors=[get_epic_details()]
- "scripts_validate_epic_rationale_49": "Get next pending epic from roadmap (status != 'complete', not assigned)" | kind=entity | source=scripts/validate_epic.py:L49 | neighbors=[get_next_epic()]
- "scripts_validate_epic_rationale_57": "Atomically claim epic for worker using git pull + commit + push\r     Raises Valu" | kind=entity | source=scripts/validate_epic.py:L57 | neighbors=[claim_epic()]
- "scripts_validate_phase_compliance_phasevalidator_init": ".__init__()" | kind=code-symbol | source=scripts/validate_phase_compliance.py:L88 | neighbors=[PhaseValidator]
- "scripts_validate_phase_compliance_rationale_125": "Check that required output files exist." | kind=entity | source=scripts/validate_phase_compliance.py:L125 | neighbors=[._check_output_files()]
- "scripts_validate_phase_compliance_rationale_143": "Check that manifest.json was updated for this phase." | kind=entity | source=scripts/validate_phase_compliance.py:L143 | neighbors=[._check_manifest_updated()]
- "scripts_validate_phase_compliance_rationale_167": "Check that Lamport event was logged for this phase." | kind=entity | source=scripts/validate_phase_compliance.py:L167 | neighbors=[._check_lamport_event()]
- "scripts_validate_phase_compliance_rationale_192": "Heuristic check: Look for MCP tool names in output files." | kind=entity | source=scripts/validate_phase_compliance.py:L192 | neighbors=[._check_mcp_usage()]
- "scripts_validate_phase_compliance_rationale_225": "Heuristic check: Look for custom mode name in Agent Tracking section." | kind=entity | source=scripts/validate_phase_compliance.py:L225 | neighbors=[._check_custom_mode_mentioned()]
- "scripts_validate_phase_compliance_rationale_253": "Validate a single epic phase. Returns True if valid." | kind=entity | source=scripts/validate_phase_compliance.py:L253 | neighbors=[validate_epic_phase()]
- "scripts_validate_phase_compliance_rationale_285": "Validate all epics in docs/brain/EPIC-W7-*." | kind=entity | source=scripts/validate_phase_compliance.py:L285 | neighbors=[validate_all_epics()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-053.json

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
